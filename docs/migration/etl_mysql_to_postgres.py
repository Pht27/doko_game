#!/usr/bin/env python3
"""
ETL: MySQL doko dump → Postgres analog schema SQL

MySQL table → Postgres table mapping:
  player                       → analog.player        (column rename + bool + no CreatedAt)
  round + round_is_game_mode   → analog.round         (winning_party string→int, join GameModeId)
  team + round_has_team        → analog.team          (one Postgres team per round×seat; new IDs)
  team_has_member + round_has_team → analog.team_member
  team_in_round_has_special_card   → analog.round_special_card
  team_in_round_has_extra_point    → analog.round_extra_point
  comment + round_has_comment      → analog.comment
"""

import re, sys

DUMP = '/tmp/mysql_dump.sql'

with open(DUMP, encoding='utf-8', errors='replace') as f:
    content = f.read()

# ── Column order from CREATE TABLE ────────────────────────────────────────────
def parse_col_order(content):
    tables = {}
    for m in re.finditer(r'CREATE TABLE `(\w+)` \((.*?)\n\) ENGINE', content, re.DOTALL):
        cols = [c.group(1) for c in re.finditer(r'^\s+`(\w+)`', m.group(2), re.MULTILINE)]
        tables[m.group(1)] = cols
    return tables

col_order = parse_col_order(content)

# ── Value parser ──────────────────────────────────────────────────────────────
def parse_val_list(s):
    vals, buf, i, n = [], [], 0, len(s)
    while i < n:
        c = s[i]
        if c == "'":
            start = i; i += 1
            while i < n:
                if s[i] == '\\' and i+1 < n: i += 2
                elif s[i] == "'": i += 1; break
                else: i += 1
            buf.append(s[start:i])
        elif c == ',':
            vals.append(''.join(buf).strip()); buf = []; i += 1
        else:
            buf.append(c); i += 1
    if buf: vals.append(''.join(buf).strip())
    return vals

def parse_rows(values_str):
    rows, i, n = [], 0, len(values_str)
    while i < n:
        if values_str[i] == '(':
            j = i+1
            while j < n:
                if values_str[j] == "'":
                    j += 1
                    while j < n:
                        if values_str[j] == '\\' and j+1 < n: j += 2
                        elif values_str[j] == "'": j += 1; break
                        else: j += 1
                elif values_str[j] == ')': break
                else: j += 1
            rows.append(parse_val_list(values_str[i+1:j]))
            i = j+1
        else: i += 1
    return rows

def load(table):
    cols = col_order.get(table, [])
    m = re.search(r'^INSERT INTO `' + re.escape(table) + r'` VALUES (.+?);$',
                  content, re.MULTILINE)
    if not m: return []
    rows_raw = parse_rows(m.group(1))
    out = []
    for row in rows_raw:
        if len(row) != len(cols):
            print(f'WARN {table}: {len(row)} vals vs {len(cols)} cols', file=sys.stderr)
        out.append(dict(zip(cols, row)))
    return out

# ── Load tables ───────────────────────────────────────────────────────────────
players   = load('player')
rounds    = load('round')
teams_raw = load('team')
rht       = load('round_has_team')          # round_id, team_id, position, party
rig       = load('round_is_game_mode')      # round_id, game_mode_id
thm       = load('team_has_member')         # team_id, player_id
tirsc     = load('team_in_round_has_special_card')   # round_id, team_id, special_card_id
tirep     = load('team_in_round_has_extra_point')    # round_id, team_id, extra_point_id, count
comments  = load('comment')                 # id, text
rhc       = load('round_has_comment')       # round_id, comment_id

print(f'Loaded  players={len(players)} rounds={len(rounds)} teams={len(teams_raw)} '
      f'round_has_team={len(rht)} team_has_member={len(thm)} '
      f'special_cards={len(tirsc)} extra_points={len(tirep)} comments={len(comments)}',
      file=sys.stderr)

# ── Helpers ───────────────────────────────────────────────────────────────────
def pg_str(val):
    """MySQL string literal → Postgres string literal (NULL stays NULL)."""
    if val == 'NULL': return 'NULL'
    inner = val[1:-1].replace("\\'", "''").replace('\\\\', '\\')
    return "'" + inner + "'"

def party_int(val):
    """'Re' → 0, 'Kontra' → 1  (input is MySQL string literal with quotes)."""
    return '0' if val.strip("'") == 'Re' else '1'

# ── Build lookup maps ─────────────────────────────────────────────────────────
round_gm      = {r['round_id']: r['game_mode_id'] for r in rig}
team_name_map = {t['id']: t['name'] for t in teams_raw}

# members per MySQL team_id
team_members = {}
for row in thm:
    team_members.setdefault(row['team_id'], []).append(row['player_id'])

# comment_id → round_id
comment_round = {r['comment_id']: r['round_id'] for r in rhc}

# Assign new sequential Postgres team IDs for each (round_id, mysql_team_id) in round_has_team.
# Sort by round then seat position so IDs are deterministic.
pg_team_map = {}   # (round_id, mysql_team_id) → pg_team_id
pg_team_ctr = 1
sorted_rht = sorted(rht, key=lambda r: (int(r['round_id']), int(r['position'])))
for row in sorted_rht:
    key = (row['round_id'], row['team_id'])
    if key not in pg_team_map:
        pg_team_map[key] = pg_team_ctr
        pg_team_ctr += 1

print(f'Postgres teams to create: {len(pg_team_map)}', file=sys.stderr)

# ── Generate SQL ──────────────────────────────────────────────────────────────
out = []

out.append('BEGIN;\n')
out.append(
    'TRUNCATE\n'
    '    analog.round_special_card,\n'
    '    analog.round_extra_point,\n'
    '    analog.comment,\n'
    '    analog.team_member,\n'
    '    analog.team,\n'
    '    analog.round,\n'
    '    analog.player\n'
    '    RESTART IDENTITY CASCADE;\n'
)

# Players
vals = []
for p in players:
    vals.append(
        f"({p['id']},{pg_str(p['name'])},"
        f"{'true' if p['active']=='1' else 'false'},"
        f"{p['start_points']},{pg_str(p['picture_name'])})"
    )
out.append(
    '\n-- players\n'
    'INSERT INTO analog."player" ("Id","Name","IsActive","StartingPoints","HeroCard") VALUES\n'
    + ',\n'.join(vals) + ';\n'
)

# Rounds
vals = []
for r in rounds:
    gm = round_gm.get(r['id'], 'NULL')
    vals.append(
        f"({r['id']},{party_int(r['winning_party'])},{r['points']},"
        f"{pg_str(r['time_stamp'])},{gm})"
    )
out.append(
    '\n-- rounds\n'
    'INSERT INTO analog."round" ("Id","WinningParty","Points","PlayedAt","GameModeId") VALUES\n'
    + ',\n'.join(vals) + ';\n'
)

# Teams
vals = []
for row in sorted_rht:
    key = (row['round_id'], row['team_id'])
    pg_tid = pg_team_map[key]
    name = pg_str(team_name_map.get(row['team_id'], 'NULL'))
    vals.append(f"({pg_tid},{row['round_id']},{name},{party_int(row['party'])})")
out.append(
    '\n-- teams\n'
    'INSERT INTO analog."team" ("Id","RoundId","Name","Party") VALUES\n'
    + ',\n'.join(vals) + ';\n'
)

# Team members
vals = []
seen = set()
for row in sorted_rht:
    key = (row['round_id'], row['team_id'])
    pg_tid = pg_team_map[key]
    pos = row['position']
    for pid in team_members.get(row['team_id'], []):
        tm_key = (pg_tid, pid)
        if tm_key not in seen:
            seen.add(tm_key)
            vals.append(f"({pg_tid},{pid},{pos})")
if vals:
    out.append(
        '\n-- team_member\n'
        'INSERT INTO analog."team_member" ("TeamId","PlayerId","Position") VALUES\n'
        + ',\n'.join(vals) + ';\n'
    )

# Round special cards
vals, warnings = [], 0
for row in tirsc:
    key = (row['round_id'], row['team_id'])
    pg_tid = pg_team_map.get(key)
    if pg_tid is None:
        print(f'WARN no team for special_card {key}', file=sys.stderr); warnings += 1; continue
    vals.append(f"({row['round_id']},{row['special_card_id']},{pg_tid})")
if vals:
    out.append(
        '\n-- round_special_card\n'
        'INSERT INTO analog."round_special_card" ("RoundId","SpecialCardId","TeamId") VALUES\n'
        + ',\n'.join(vals) + ';\n'
    )

# Round extra points
for row in tirep:
    key = (row['round_id'], row['team_id'])
    pg_tid = pg_team_map.get(key)
    if pg_tid is None:
        print(f'WARN no team for extra_point {key}', file=sys.stderr); warnings += 1; continue
    vals.append(f"({row['round_id']},{row['extra_point_id']},{pg_tid},{row['count']})")
# Note: vals still has special_card entries — reset for extra_points
ep_vals = []
for row in tirep:
    key = (row['round_id'], row['team_id'])
    pg_tid = pg_team_map.get(key)
    if pg_tid is None: continue
    ep_vals.append(f"({row['round_id']},{row['extra_point_id']},{pg_tid},{row['count']})")
if ep_vals:
    out.append(
        '\n-- round_extra_point\n'
        'INSERT INTO analog."round_extra_point" ("RoundId","ExtraPointId","TeamId","Count") VALUES\n'
        + ',\n'.join(ep_vals) + ';\n'
    )

# Comments
vals = []
for c in comments:
    rid = comment_round.get(c['id'])
    if rid is None:
        print(f'WARN comment {c["id"]} has no round', file=sys.stderr); continue
    vals.append(f"({c['id']},{rid},{pg_str(c['text'])})")
if vals:
    out.append(
        '\n-- comment\n'
        'INSERT INTO analog."comment" ("Id","RoundId","Text") VALUES\n'
        + ',\n'.join(vals) + ';\n'
    )

# Reset sequences
out.append('\n-- Reset sequences')
for table in ['player', 'round', 'team', 'comment']:
    out.append(
        f"\nSELECT setval(pg_get_serial_sequence('analog.{table}', 'Id'),"
        f' COALESCE((SELECT MAX("Id") FROM analog.{table}), 1));'
    )

out.append('\n\nCOMMIT;\n')

if warnings:
    print(f'{warnings} mapping warnings — check output', file=sys.stderr)

print('\n'.join(out))
