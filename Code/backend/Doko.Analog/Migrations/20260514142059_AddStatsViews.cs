using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doko.Analog.Migrations
{
    /// <inheritdoc />
    public partial class AddStatsViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW analytics.player_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                player_round_results AS (
                    SELECT
                        tm."PlayerId"                                                                 AS player_id,
                        t."RoundId"                                                                   AS round_id,
                        (t."Party" = r."WinningParty")                                                AS won,
                        ts.team_size,
                        (ts.team_size = 1)                                                            AS is_alone,
                        (gm."IsSolo" AND gm."SoloParty" = t."Party")                                  AS is_solo,
                        CASE WHEN gm."IsSolo" AND gm."SoloParty" = t."Party" THEN 3 ELSE 1 END       AS solo_multiplier,
                        CASE WHEN t."Party" = r."WinningParty"
                             THEN r."Points"::decimal
                             ELSE -r."Points"::decimal
                        END                                                                           AS game_value
                    FROM analog.team_member tm
                    JOIN analog.team      t  ON t."Id"         = tm."TeamId"
                    JOIN analog.round     r  ON r."Id"         = t."RoundId"
                    JOIN analog.game_mode gm ON gm."Id"        = r."GameModeId"
                    JOIN team_sizes       ts ON ts.team_id     = tm."TeamId"
                )
                SELECT
                    p."Id"                                                                             AS player_id,
                    p."Name"                                                                           AS name,
                    p."IsActive"                                                                       AS is_active,
                    count(r.round_id)                                                                  AS total_games,
                    count(r.round_id) FILTER (WHERE r.won)                                             AS total_wins,
                    CASE WHEN count(r.round_id) > 0
                         THEN round(count(r.round_id) FILTER (WHERE r.won)::decimal / count(r.round_id), 3)
                         ELSE 0 END                                                                    AS total_win_rate,
                    CASE WHEN count(r.round_id) > 0
                         THEN round(avg(r.game_value), 2)
                         ELSE 0 END                                                                    AS total_avg_game_value,
                    CASE WHEN count(r.round_id) > 0
                         THEN round(avg(r.game_value * r.solo_multiplier), 2)
                         ELSE 0 END                                                                    AS total_avg_points_won_lost,
                    CASE WHEN count(r.round_id) > 0
                         THEN round(avg(r.game_value * r.solo_multiplier / r.team_size), 2)
                         ELSE 0 END                                                                    AS total_avg_points_earned,
                    count(r.round_id) FILTER (WHERE r.is_solo)                                         AS solo_games,
                    count(r.round_id) FILTER (WHERE r.is_solo AND r.won)                               AS solo_wins,
                    CASE WHEN count(r.round_id) FILTER (WHERE r.is_solo) > 0
                         THEN round(count(r.round_id) FILTER (WHERE r.is_solo AND r.won)::decimal
                              / count(r.round_id) FILTER (WHERE r.is_solo), 3)
                         ELSE 0 END                                                                    AS solo_win_rate,
                    CASE WHEN count(r.round_id) FILTER (WHERE r.is_solo) > 0
                         THEN round(avg(r.game_value) FILTER (WHERE r.is_solo), 2)
                         ELSE 0 END                                                                    AS solo_avg_game_value,
                    CASE WHEN count(r.round_id) FILTER (WHERE r.is_solo) > 0
                         THEN round(avg(r.game_value * r.solo_multiplier) FILTER (WHERE r.is_solo), 2)
                         ELSE 0 END                                                                    AS solo_avg_points_won_lost,
                    count(r.round_id) FILTER (WHERE r.is_alone)                                        AS alone_games,
                    count(r.round_id) FILTER (WHERE r.is_alone AND r.won)                              AS alone_wins,
                    CASE WHEN count(r.round_id) FILTER (WHERE r.is_alone) > 0
                         THEN round(count(r.round_id) FILTER (WHERE r.is_alone AND r.won)::decimal
                              / count(r.round_id) FILTER (WHERE r.is_alone), 3)
                         ELSE 0 END                                                                    AS alone_win_rate,
                    CASE WHEN count(r.round_id) FILTER (WHERE r.is_alone) > 0
                         THEN round(avg(r.game_value * r.solo_multiplier / r.team_size) FILTER (WHERE r.is_alone), 2)
                         ELSE 0 END                                                                    AS alone_avg_points_earned
                FROM analog.player p
                LEFT JOIN player_round_results r ON r.player_id = p."Id"
                GROUP BY p."Id", p."Name", p."IsActive";
                """
            );

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW analytics.player_game_mode_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                player_round_results AS (
                    SELECT
                        tm."PlayerId"                                                                 AS player_id,
                        r."GameModeId"                                                                AS game_mode_id,
                        (t."Party" = r."WinningParty")                                                AS won,
                        ts.team_size,
                        CASE WHEN gm."IsSolo" AND gm."SoloParty" = t."Party" THEN 3 ELSE 1 END       AS solo_multiplier,
                        CASE WHEN t."Party" = r."WinningParty"
                             THEN r."Points"::decimal
                             ELSE -r."Points"::decimal
                        END                                                                           AS game_value
                    FROM analog.team_member tm
                    JOIN analog.team      t  ON t."Id"         = tm."TeamId"
                    JOIN analog.round     r  ON r."Id"         = t."RoundId"
                    JOIN analog.game_mode gm ON gm."Id"        = r."GameModeId"
                    JOIN team_sizes       ts ON ts.team_id     = tm."TeamId"
                )
                SELECT
                    r.player_id,
                    r.game_mode_id,
                    gm."Name"                                                                          AS game_mode_name,
                    count(*)                                                                           AS games,
                    count(*) FILTER (WHERE r.won)                                                      AS wins,
                    round(count(*) FILTER (WHERE r.won)::decimal / count(*), 3)                       AS win_rate,
                    round(avg(r.game_value), 2)                                                       AS avg_game_value,
                    round(avg(r.game_value * r.solo_multiplier), 2)                                   AS avg_points_won_lost,
                    round(avg(r.game_value * r.solo_multiplier / r.team_size), 2)                     AS avg_points_earned
                FROM player_round_results r
                JOIN analog.game_mode gm ON gm."Id" = r.game_mode_id
                GROUP BY r.player_id, r.game_mode_id, gm."Name";
                """
            );

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW analytics.player_special_card_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                team_round_results AS (
                    SELECT
                        t."Id"                                                                        AS team_id,
                        (t."Party" = r."WinningParty")                                                AS won,
                        CASE WHEN gm."IsSolo" AND gm."SoloParty" = t."Party" THEN 3 ELSE 1 END       AS solo_multiplier,
                        CASE WHEN t."Party" = r."WinningParty"
                             THEN r."Points"::decimal
                             ELSE -r."Points"::decimal
                        END                                                                           AS game_value
                    FROM analog.team      t
                    JOIN analog.round     r  ON r."Id"         = t."RoundId"
                    JOIN analog.game_mode gm ON gm."Id"        = r."GameModeId"
                    JOIN team_sizes       ts ON ts.team_id     = t."Id"
                )
                SELECT
                    tm."PlayerId"                                                                      AS player_id,
                    sc."Id"                                                                            AS special_card_id,
                    sc."Name"                                                                          AS special_card_name,
                    count(*)                                                                           AS occurrences,
                    count(*) FILTER (WHERE tr.won)                                                     AS wins,
                    round(count(*) FILTER (WHERE tr.won)::decimal / count(*), 3)                      AS win_rate,
                    round(avg(tr.game_value), 2)                                                      AS avg_game_value,
                    round(avg(tr.game_value * tr.solo_multiplier), 2)                                 AS avg_points_won_lost
                FROM analog.round_special_card rsc
                JOIN analog.special_card   sc ON sc."Id"    = rsc."SpecialCardId"
                JOIN analog.team_member    tm ON tm."TeamId" = rsc."TeamId"
                JOIN team_round_results    tr ON tr.team_id  = rsc."TeamId"
                GROUP BY tm."PlayerId", sc."Id", sc."Name";
                """
            );

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW analytics.player_extra_point_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                team_round_results AS (
                    SELECT
                        t."Id"                                                                        AS team_id,
                        (t."Party" = r."WinningParty")                                                AS won,
                        CASE WHEN t."Party" = r."WinningParty"
                             THEN r."Points"::decimal
                             ELSE -r."Points"::decimal
                        END                                                                           AS game_value
                    FROM analog.team      t
                    JOIN analog.round     r  ON r."Id"         = t."RoundId"
                    JOIN analog.game_mode gm ON gm."Id"        = r."GameModeId"
                    JOIN team_sizes       ts ON ts.team_id     = t."Id"
                )
                SELECT
                    tm."PlayerId"                                                                      AS player_id,
                    ep."Id"                                                                            AS extra_point_id,
                    ep."Name"                                                                          AS extra_point_name,
                    count(*)                                                                           AS occurrences,
                    sum(rep."Count")                                                                   AS total_count,
                    count(*) FILTER (WHERE tr.won)                                                     AS wins,
                    round(count(*) FILTER (WHERE tr.won)::decimal / count(*), 3)                      AS win_rate,
                    round(avg(tr.game_value), 2)                                                      AS avg_game_value
                FROM analog.round_extra_point rep
                JOIN analog.extra_point    ep ON ep."Id"    = rep."ExtraPointId"
                JOIN analog.team_member    tm ON tm."TeamId" = rep."TeamId"
                JOIN team_round_results    tr ON tr.team_id  = rep."TeamId"
                GROUP BY tm."PlayerId", ep."Id", ep."Name";
                """
            );

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW analytics.player_partner_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                team_round_results AS (
                    SELECT
                        t."Id"                                                                        AS team_id,
                        (t."Party" = r."WinningParty")                                                AS won
                    FROM analog.team      t
                    JOIN analog.round     r  ON r."Id"         = t."RoundId"
                    JOIN analog.game_mode gm ON gm."Id"        = r."GameModeId"
                    JOIN team_sizes       ts ON ts.team_id     = t."Id"
                )
                SELECT
                    tm1."PlayerId"                                                                     AS player_id,
                    tm2."PlayerId"                                                                     AS partner_id,
                    p."Name"                                                                           AS partner_name,
                    count(*)                                                                           AS games_together,
                    count(*) FILTER (WHERE tr.won)                                                     AS wins_together,
                    round(count(*) FILTER (WHERE tr.won)::decimal / count(*), 3)                      AS win_rate_together
                FROM analog.team_member tm1
                JOIN analog.team_member    tm2 ON tm2."TeamId"  = tm1."TeamId"
                                               AND tm2."PlayerId" != tm1."PlayerId"
                JOIN analog.player         p   ON p."Id"         = tm2."PlayerId"
                JOIN team_round_results    tr  ON tr.team_id     = tm1."TeamId"
                GROUP BY tm1."PlayerId", tm2."PlayerId", p."Name";
                """
            );

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW analytics.game_mode_stats AS
                SELECT
                    gm."Id"                                                                            AS game_mode_id,
                    gm."Name"                                                                          AS game_mode_name,
                    count(r."Id")                                                                      AS total_rounds,
                    round(avg(r."Points"::decimal), 2)                                                AS avg_game_value
                FROM analog.game_mode gm
                LEFT JOIN analog.round r ON r."GameModeId" = gm."Id"
                GROUP BY gm."Id", gm."Name";
                """
            );

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW analytics.special_card_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                team_round_results AS (
                    SELECT
                        t."Id"                                                                        AS team_id,
                        (t."Party" = r."WinningParty")                                                AS won,
                        CASE WHEN gm."IsSolo" AND gm."SoloParty" = t."Party" THEN 3 ELSE 1 END       AS solo_multiplier,
                        CASE WHEN t."Party" = r."WinningParty"
                             THEN r."Points"::decimal
                             ELSE -r."Points"::decimal
                        END                                                                           AS game_value
                    FROM analog.team      t
                    JOIN analog.round     r  ON r."Id"         = t."RoundId"
                    JOIN analog.game_mode gm ON gm."Id"        = r."GameModeId"
                    JOIN team_sizes       ts ON ts.team_id     = t."Id"
                )
                SELECT
                    sc."Id"                                                                            AS special_card_id,
                    sc."Name"                                                                          AS name,
                    count(*)                                                                           AS occurrences,
                    count(*) FILTER (WHERE tr.won)                                                     AS wins,
                    round(count(*) FILTER (WHERE tr.won)::decimal / count(*), 3)                      AS win_rate,
                    round(avg(tr.game_value), 2)                                                      AS avg_game_value,
                    round(avg(tr.game_value * tr.solo_multiplier), 2)                                 AS avg_points_won_lost
                FROM analog.round_special_card rsc
                JOIN analog.special_card   sc ON sc."Id"    = rsc."SpecialCardId"
                JOIN team_round_results    tr ON tr.team_id  = rsc."TeamId"
                GROUP BY sc."Id", sc."Name";
                """
            );

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW analytics.extra_point_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                team_round_results AS (
                    SELECT
                        t."Id"                                                                        AS team_id,
                        (t."Party" = r."WinningParty")                                                AS won,
                        CASE WHEN gm."IsSolo" AND gm."SoloParty" = t."Party" THEN 3 ELSE 1 END       AS solo_multiplier,
                        CASE WHEN t."Party" = r."WinningParty"
                             THEN r."Points"::decimal
                             ELSE -r."Points"::decimal
                        END                                                                           AS game_value
                    FROM analog.team      t
                    JOIN analog.round     r  ON r."Id"         = t."RoundId"
                    JOIN analog.game_mode gm ON gm."Id"        = r."GameModeId"
                    JOIN team_sizes       ts ON ts.team_id     = t."Id"
                )
                SELECT
                    ep."Id"                                                                            AS extra_point_id,
                    ep."Name"                                                                          AS name,
                    count(*)                                                                           AS occurrences,
                    sum(rep."Count")                                                                   AS total_count,
                    count(*) FILTER (WHERE tr.won)                                                     AS wins,
                    round(count(*) FILTER (WHERE tr.won)::decimal / count(*), 3)                      AS win_rate,
                    round(avg(tr.game_value), 2)                                                      AS avg_game_value,
                    round(avg(tr.game_value * tr.solo_multiplier), 2)                                 AS avg_points_won_lost
                FROM analog.round_extra_point rep
                JOIN analog.extra_point    ep ON ep."Id"    = rep."ExtraPointId"
                JOIN team_round_results    tr ON tr.team_id  = rep."TeamId"
                GROUP BY ep."Id", ep."Name";
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_stats;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_game_mode_stats;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_special_card_stats;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_extra_point_stats;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_partner_stats;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.game_mode_stats;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.special_card_stats;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.extra_point_stats;");
        }
    }
}
