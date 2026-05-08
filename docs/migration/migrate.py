#!/usr/bin/env python3
"""
MySQL → PostgreSQL migration for Doppelkopf analog data.

Reads a mysqldump file and migrates all dynamic data into the new PostgreSQL schema.
Static tables (game_mode, special_card, extra_point) are seeded via EF Core and skipped.

Each round_has_team entry becomes one analog.team row (4 teams per round, same as
the old DB). The old DB reused team rows across rounds; the new DB creates fresh
round-specific teams, which is sufficient for all queries.

Usage:
    python3 migrate.py <dump_file> [pg_conn_string]

    pg_conn_string defaults to:
        host=localhost dbname=doko_analog user=doko_app password=doko_app

Example:
    python3 migrate.py ~/db_backups/db_backup_2026-05-08.sql \\
        "host=prod-server dbname=doko_analog user=doko_app password=secret"
"""

import re
import sys
from collections import defaultdict

import psycopg2

RE = 0
KONTRA = 1
PARTY_MAP = {"Re": RE, "Kontra": KONTRA}


# ─── Dump parser ──────────────────────────────────────────────────────────────


def parse_values(s: str) -> list[tuple]:
    """
    Parse a MySQL VALUES string like (1,'foo',NULL),(2,'bar',0) into Python tuples.
    Handles NULL, integers, decimals, and single-quoted strings with MySQL escapes.
    """
    rows = []
    i = 0
    n = len(s)

    while i < n:
        while i < n and s[i] in " \t\n,":
            i += 1
        if i >= n:
            break
        if s[i] != "(":
            i += 1
            continue

        i += 1  # skip '('
        row = []

        while i < n and s[i] != ")":
            while i < n and s[i] in " \t\n,":
                i += 1
            if i >= n or s[i] == ")":
                break

            if s[i] == "'":
                i += 1
                chars = []
                while i < n:
                    c = s[i]
                    if c == "\\":
                        i += 1
                        esc = {
                            "n": "\n", "t": "\t", "r": "\r",
                            "'": "'", "\\": "\\", '"': '"', "0": "\0",
                        }
                        chars.append(esc.get(s[i], s[i]))
                        i += 1
                    elif c == "'":
                        i += 1
                        break
                    else:
                        chars.append(c)
                        i += 1
                row.append("".join(chars))
            elif s[i : i + 4].upper() == "NULL":
                row.append(None)
                i += 4
            else:
                j = i
                if j < n and s[j] == "-":
                    j += 1
                while j < n and (s[j].isdigit() or s[j] == "."):
                    j += 1
                num = s[i:j]
                row.append(float(num) if "." in num else int(num))
                i = j

        if i < n and s[i] == ")":
            i += 1
        rows.append(tuple(row))

    return rows


def parse_dump(path: str) -> dict[str, list[tuple]]:
    """Extract INSERT data from a mysqldump file, keyed by table name."""
    result: dict[str, list[tuple]] = {}
    pattern = re.compile(r"INSERT INTO `(\w+)` VALUES (.*?);", re.DOTALL)
    with open(path, encoding="utf-8") as f:
        content = f.read()
    for m in pattern.finditer(content):
        rows = parse_values(m.group(2))
        result.setdefault(m.group(1), []).extend(rows)
    return result


# ─── Migration ────────────────────────────────────────────────────────────────


def clear_data(cur) -> None:
    print("  Clearing existing data...")
    for table in [
        "analog.round_special_card",
        "analog.round_extra_point",
        "analog.comment",
        "analog.team_member",
        "analog.team",
        "analog.round",
        "analog.player",
    ]:
        cur.execute(f"DELETE FROM {table}")
        print(f"    {table}: cleared")


def reset_sequences(cur) -> None:
    print("  Resetting sequences...")
    for table, col in [
        ("analog.player", "Id"),
        ("analog.round", "Id"),
        ("analog.comment", "Id"),
    ]:
        cur.execute(
            f"""SELECT setval(
                    pg_get_serial_sequence('{table}', '{col}'),
                    COALESCE((SELECT MAX("{col}") FROM {table}), 0)
                )"""
        )


def migrate(conn, data: dict[str, list[tuple]]) -> None:
    cur = conn.cursor()

    clear_data(cur)

    # ── Build lookups ──────────────────────────────────────────────────────────

    # round_id → game_mode_id  (from round_is_game_mode)
    round_game_mode: dict[int, int] = {
        int(r[0]): int(r[1]) for r in data.get("round_is_game_mode", [])
    }

    # old_team_id → [player_id, ...]
    team_players: dict[int, list[int]] = defaultdict(list)
    for team_id, player_id in data.get("team_has_member", []):
        team_players[int(team_id)].append(int(player_id))

    # (round_id, old_team_id) → new analog.team "Id"  (populated during team insert)
    new_team_id: dict[tuple[int, int], int] = {}

    # ── Players ───────────────────────────────────────────────────────────────
    print("  Inserting players...")
    players = data.get("player", [])
    for row in players:
        # old columns: (id, name, active, start_points, picture_name)
        cur.execute(
            """INSERT INTO analog.player ("Id", "Name", "IsActive", "StartingPoints")
               OVERRIDING SYSTEM VALUE VALUES (%s, %s, %s, %s)""",
            (int(row[0]), row[1], bool(int(row[2])), float(row[3])),
        )
    print(f"    {len(players)} players")

    # ── Rounds ────────────────────────────────────────────────────────────────
    print("  Inserting rounds...")
    rounds = data.get("round", [])
    for row in rounds:
        # old columns: (id, winning_party, points, time_stamp)
        # Dump sets TIME_ZONE='+00:00', so timestamps are UTC.
        round_id = int(row[0])
        cur.execute(
            """INSERT INTO analog.round
                   ("Id", "WinningParty", "Points", "PlayedAt", "GameModeId")
               OVERRIDING SYSTEM VALUE
               VALUES (%s, %s, %s, %s::timestamp AT TIME ZONE 'UTC', %s)""",
            (
                round_id,
                PARTY_MAP[row[1]],
                int(row[2]),
                str(row[3]),
                round_game_mode.get(round_id, 3),  # fallback: Normal (id=3)
            ),
        )
    print(f"    {len(rounds)} rounds")

    # ── Teams + team members ──────────────────────────────────────────────────
    print("  Inserting teams and members...")
    team_count = 0
    member_count = 0
    for row in data.get("round_has_team", []):
        # old columns: (round_id, team_id, position, party)
        # position (1–4) is the seat at the table; we store it as team_member.Position.
        round_id, old_tid, position, party_str = (
            int(row[0]), int(row[1]), int(row[2]), row[3]
        )

        cur.execute(
            """INSERT INTO analog.team ("RoundId", "Party") VALUES (%s, %s)
               RETURNING "Id" """,
            (round_id, PARTY_MAP[party_str]),
        )
        tid = cur.fetchone()[0]
        new_team_id[(round_id, old_tid)] = tid
        team_count += 1

        for member_pos, player_id in enumerate(team_players.get(old_tid, [])):
            cur.execute(
                """INSERT INTO analog.team_member ("TeamId", "PlayerId", "Position")
                   VALUES (%s, %s, %s)""",
                (tid, player_id, member_pos),
            )
            member_count += 1

    print(f"    {team_count} teams, {member_count} members")

    # ── Special cards ─────────────────────────────────────────────────────────
    print("  Inserting special cards...")
    sc_count = 0
    for row in data.get("team_in_round_has_special_card", []):
        # old columns: (round_id, team_id, special_card_id)
        round_id, old_tid, sc_id = int(row[0]), int(row[1]), int(row[2])
        tid = new_team_id.get((round_id, old_tid))
        if tid is None:
            print(f"    WARN: no team for round={round_id} old_team={old_tid}")
            continue
        cur.execute(
            """INSERT INTO analog.round_special_card
                   ("RoundId", "SpecialCardId", "TeamId") VALUES (%s, %s, %s)""",
            (round_id, sc_id, tid),
        )
        sc_count += 1
    print(f"    {sc_count} special card entries")

    # ── Extra points ──────────────────────────────────────────────────────────
    print("  Inserting extra points...")
    ep_count = 0
    for row in data.get("team_in_round_has_extra_point", []):
        # old columns: (round_id, team_id, extra_point_id, count)
        round_id, old_tid, ep_id, count = (
            int(row[0]), int(row[1]), int(row[2]), int(row[3])
        )
        tid = new_team_id.get((round_id, old_tid))
        if tid is None:
            print(f"    WARN: no team for round={round_id} old_team={old_tid}")
            continue
        cur.execute(
            """INSERT INTO analog.round_extra_point
                   ("RoundId", "ExtraPointId", "TeamId", "Count") VALUES (%s, %s, %s, %s)""",
            (round_id, ep_id, tid, count),
        )
        ep_count += 1
    print(f"    {ep_count} extra point entries")

    # ── Comments ──────────────────────────────────────────────────────────────
    print("  Inserting comments...")
    comment_text: dict[int, str] = {
        int(r[0]): r[1] for r in data.get("comment", [])
    }
    round_comments: dict[int, list[str]] = defaultdict(list)
    for row in data.get("round_has_comment", []):
        round_id, comment_id = int(row[0]), int(row[1])
        text = comment_text.get(comment_id, "")
        if text:
            round_comments[round_id].append(text)
    for round_id, texts in round_comments.items():
        cur.execute(
            """INSERT INTO analog.comment ("RoundId", "Text") VALUES (%s, %s)""",
            (round_id, " / ".join(texts)),
        )
    print(f"    {len(round_comments)} comments")

    reset_sequences(cur)

    # ── Validation ────────────────────────────────────────────────────────────
    # team_member count = sum of players-per-team across all round_has_team entries
    # (old teams are reused across rounds, so this exceeds len(team_has_member))
    expected_members = sum(
        len(team_players.get(int(row[1]), []))
        for row in data.get("round_has_team", [])
    )

    print("\n  Validation (row counts):")
    checks = [
        ("analog.player",      len(data.get("player", []))),
        ("analog.round",       len(data.get("round", []))),
        ("analog.team",        len(data.get("round_has_team", []))),
        ("analog.team_member", expected_members),
    ]
    all_ok = True
    for pg_table, expected in checks:
        cur.execute(f'SELECT COUNT(*) FROM {pg_table}')
        actual = cur.fetchone()[0]
        ok = actual == expected
        status = "OK" if ok else f"MISMATCH (expected {expected})"
        print(f"    {pg_table}: {actual}  [{status}]")
        if not ok:
            all_ok = False

    if not all_ok:
        raise RuntimeError("Row count mismatch — rolling back")

    print("\n  All checks passed.")


# ─── Entry point ──────────────────────────────────────────────────────────────


def main() -> None:
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    dump_path = sys.argv[1]
    pg_conn = (
        sys.argv[2]
        if len(sys.argv) > 2
        else "host=localhost dbname=doko_analog user=doko_app password=doko_app"
    )

    print(f"Parsing dump: {dump_path}")
    data = parse_dump(dump_path)
    summary = ", ".join(f"{t}({len(rows)})" for t, rows in sorted(data.items()))
    print(f"  Tables: {summary}\n")

    print("Connecting to PostgreSQL...")
    conn = psycopg2.connect(pg_conn)
    try:
        migrate(conn, data)
        conn.commit()
        print("\nMigration completed successfully.")
    except Exception as e:
        conn.rollback()
        print(f"\nMigration FAILED: {e}")
        raise
    finally:
        conn.close()


if __name__ == "__main__":
    main()
