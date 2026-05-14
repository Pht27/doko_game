using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doko.Analog.Migrations
{
    public partial class AddPlayerRoundHistoryView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE VIEW analytics.player_round_history AS
                WITH team_sizes AS (
                    SELECT "TeamId" AS team_id, count(*) AS team_size
                    FROM analog.team_member
                    GROUP BY "TeamId"
                ),
                player_round_results AS (
                    SELECT
                        tm."PlayerId"                                                         AS player_id,
                        t."RoundId"                                                           AS round_id,
                        r."PlayedAt"                                                          AS played_at,
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
                    r.player_id,
                    r.round_id,
                    r.played_at,
                    r.point_delta,
                    r.won,
                    round(
                        p."StartingPoints" + sum(r.point_delta) OVER (
                            PARTITION BY r.player_id
                            ORDER BY r.played_at, r.round_id
                            ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                        ),
                        2
                    ) AS cumulative_points
                FROM player_round_results r
                JOIN analog.player p ON p."Id" = r.player_id;
                """
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS analytics.player_round_history;");
        }
    }
}
