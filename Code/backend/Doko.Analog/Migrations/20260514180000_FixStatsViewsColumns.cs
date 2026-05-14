using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doko.Analog.Migrations
{
    /// <inheritdoc />
    public partial class FixStatsViewsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_game_mode_stats;");
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics.player_game_mode_stats AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                player_round_results AS (
                    SELECT
                        tm."PlayerId"                                                                 AS player_id,
                        t."RoundId"                                                                   AS round_id,
                        r."GameModeId"                                                                AS game_mode_id,
                        t."Party"                                                                     AS party,
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
                    r.party,
                    count(*)                                                                           AS games,
                    count(*) FILTER (WHERE r.won)                                                      AS wins,
                    round(count(*) FILTER (WHERE r.won)::decimal / count(*), 3)                       AS win_rate,
                    round(avg(r.game_value), 2)                                                       AS avg_game_value,
                    round(avg(r.game_value * r.solo_multiplier), 2)                                   AS avg_points_won_lost,
                    round(avg(r.game_value * r.solo_multiplier / r.team_size), 2)                     AS avg_points_earned
                FROM player_round_results r
                JOIN analog.game_mode gm ON gm."Id" = r.game_mode_id
                GROUP BY r.player_id, r.game_mode_id, gm."Name", r.party;
                """
            );

            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.game_mode_stats;");
            migrationBuilder.Sql(
                """
                CREATE VIEW analytics.game_mode_stats AS
                SELECT
                    gm."Id"                                                                            AS game_mode_id,
                    gm."Name"                                                                          AS game_mode_name,
                    count(r."Id")                                                                      AS total_rounds,
                    round(avg(r."Points"::decimal), 2)                                                AS avg_game_value,
                    CASE WHEN count(r."Id") > 0
                         THEN round(
                             count(r."Id") FILTER (WHERE r."WinningParty" = 0)::decimal
                             / count(r."Id"), 3)
                         ELSE NULL END                                                                 AS re_win_rate,
                    CASE WHEN count(r."Id") > 0
                         THEN round(avg(
                             CASE WHEN r."WinningParty" = 0
                                  THEN  r."Points"::decimal
                                  ELSE -r."Points"::decimal
                             END), 2)
                         ELSE NULL END                                                                 AS re_avg_game_value
                FROM analog.game_mode gm
                LEFT JOIN analog.round r ON r."GameModeId" = gm."Id"
                GROUP BY gm."Id", gm."Name";
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_game_mode_stats;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.game_mode_stats;");
        }
    }
}
