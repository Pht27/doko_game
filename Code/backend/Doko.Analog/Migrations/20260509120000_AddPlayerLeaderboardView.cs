using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doko.Analog.Migrations
{
    public partial class AddPlayerLeaderboardView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS analytics;");

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW analytics.player_leaderboard AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                player_round_results AS (
                    SELECT
                        tm."PlayerId"                                                         AS player_id,
                        t."RoundId"                                                           AS round_id,
                        (t."Party" = r."WinningParty")                                        AS won,
                        CASE WHEN gm."IsSolo" AND gm."SoloParty" = t."Party" THEN 3 ELSE 1 END
                            * r."Points"::decimal / ts.team_size
                            * CASE WHEN t."Party" = r."WinningParty" THEN 1 ELSE -1 END       AS point_delta
                    FROM analog.team_member tm
                    JOIN analog.team      t  ON t."Id"         = tm."TeamId"
                    JOIN analog.round     r  ON r."Id"         = t."RoundId"
                    JOIN analog.game_mode gm ON gm."Id"        = r."GameModeId"
                    JOIN team_sizes       ts ON ts.team_id     = tm."TeamId"
                )
                SELECT
                    p."Id"                                                                    AS player_id,
                    p."Name"                                                                  AS name,
                    p."IsActive"                                                              AS is_active,
                    round(p."StartingPoints" + coalesce(sum(r.point_delta), 0), 2)            AS total_points,
                    count(r.round_id)                                                         AS games_played,
                    count(r.round_id) FILTER (WHERE r.won)                                    AS wins,
                    count(r.round_id) FILTER (WHERE NOT r.won)                                AS losses,
                    CASE
                        WHEN count(r.round_id) > 0
                        THEN round(count(r.round_id) FILTER (WHERE r.won)::decimal / count(r.round_id), 3)
                        ELSE 0
                    END                                                                       AS win_rate,
                    CASE
                        WHEN count(r.round_id) > 0
                        THEN round(sum(r.point_delta) / count(r.round_id), 2)
                        ELSE 0
                    END                                                                       AS avg_points_per_game
                FROM analog.player p
                LEFT JOIN player_round_results r ON r.player_id = p."Id"
                GROUP BY p."Id", p."Name", p."IsActive", p."StartingPoints";
                """
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_leaderboard;");
            migrationBuilder.Sql("DROP SCHEMA IF EXISTS analytics;");
        }
    }
}
