using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doko.Analog.Migrations
{
    /// <inheritdoc />
    public partial class AddReKontraToSpecialCardExtraPointStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_special_card_stats;");
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics.player_special_card_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                team_round_results AS (
                    SELECT
                        t."Id"                                                                        AS team_id,
                        t."Party"                                                                     AS party,
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
                    tr.party,
                    count(*)                                                                           AS occurrences,
                    count(*) FILTER (WHERE tr.won)                                                     AS wins,
                    round(count(*) FILTER (WHERE tr.won)::decimal / count(*), 3)                      AS win_rate,
                    round(avg(tr.game_value), 2)                                                      AS avg_game_value,
                    round(avg(tr.game_value * tr.solo_multiplier), 2)                                 AS avg_points_won_lost
                FROM analog.round_special_card rsc
                JOIN analog.special_card   sc ON sc."Id"    = rsc."SpecialCardId"
                JOIN analog.team_member    tm ON tm."TeamId" = rsc."TeamId"
                JOIN team_round_results    tr ON tr.team_id  = rsc."TeamId"
                GROUP BY tm."PlayerId", sc."Id", sc."Name", tr.party;
                """
            );

            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_extra_point_stats;");
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics.player_extra_point_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                team_round_results AS (
                    SELECT
                        t."Id"                                                                        AS team_id,
                        t."Party"                                                                     AS party,
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
                    tr.party,
                    count(*)                                                                           AS occurrences,
                    sum(rep."Count")                                                                   AS total_count,
                    count(*) FILTER (WHERE tr.won)                                                     AS wins,
                    round(count(*) FILTER (WHERE tr.won)::decimal / count(*), 3)                      AS win_rate,
                    round(avg(tr.game_value), 2)                                                      AS avg_game_value
                FROM analog.round_extra_point rep
                JOIN analog.extra_point    ep ON ep."Id"    = rep."ExtraPointId"
                JOIN analog.team_member    tm ON tm."TeamId" = rep."TeamId"
                JOIN team_round_results    tr ON tr.team_id  = rep."TeamId"
                GROUP BY tm."PlayerId", ep."Id", ep."Name", tr.party;
                """
            );

            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.special_card_stats;");
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics.special_card_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                team_round_results AS (
                    SELECT
                        t."Id"                                                                        AS team_id,
                        t."Party"                                                                     AS party,
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
                    round(avg(tr.game_value * tr.solo_multiplier), 2)                                 AS avg_points_won_lost,
                    CASE WHEN count(*) FILTER (WHERE tr.party = 0) > 0
                         THEN round(
                             count(*) FILTER (WHERE tr.party = 0 AND tr.won)::decimal
                             / count(*) FILTER (WHERE tr.party = 0), 3)
                         ELSE NULL END                                                                 AS re_win_rate,
                    CASE WHEN count(*) FILTER (WHERE tr.party = 0) > 0
                         THEN round(avg(tr.game_value) FILTER (WHERE tr.party = 0), 2)
                         ELSE NULL END                                                                 AS re_avg_game_value
                FROM analog.round_special_card rsc
                JOIN analog.special_card   sc ON sc."Id"    = rsc."SpecialCardId"
                JOIN team_round_results    tr ON tr.team_id  = rsc."TeamId"
                GROUP BY sc."Id", sc."Name";
                """
            );

            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.extra_point_stats;");
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics.extra_point_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                team_round_results AS (
                    SELECT
                        t."Id"                                                                        AS team_id,
                        t."Party"                                                                     AS party,
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
                    round(avg(tr.game_value * tr.solo_multiplier), 2)                                 AS avg_points_won_lost,
                    CASE WHEN count(*) FILTER (WHERE tr.party = 0) > 0
                         THEN round(
                             count(*) FILTER (WHERE tr.party = 0 AND tr.won)::decimal
                             / count(*) FILTER (WHERE tr.party = 0), 3)
                         ELSE NULL END                                                                 AS re_win_rate,
                    CASE WHEN count(*) FILTER (WHERE tr.party = 0) > 0
                         THEN round(avg(tr.game_value) FILTER (WHERE tr.party = 0), 2)
                         ELSE NULL END                                                                 AS re_avg_game_value
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
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_special_card_stats;");
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics.player_special_card_stats AS
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

            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_extra_point_stats;");
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics.player_extra_point_stats AS
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

            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.special_card_stats;");
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics.special_card_stats AS
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

            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.extra_point_stats;");
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics.extra_point_stats AS
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
    }
}
