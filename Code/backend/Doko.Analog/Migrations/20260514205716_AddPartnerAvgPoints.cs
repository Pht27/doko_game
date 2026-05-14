using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doko.Analog.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerAvgPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                        t."Id"                                                                         AS team_id,
                        (t."Party" = r."WinningParty")                                                 AS won,
                        CASE WHEN gm."IsSolo" AND gm."SoloParty" = t."Party" THEN 3 ELSE 1 END        AS solo_multiplier,
                        CASE WHEN t."Party" = r."WinningParty"
                             THEN r."Points"::decimal
                             ELSE -r."Points"::decimal
                        END                                                                            AS game_value
                    FROM analog.team      t
                    JOIN analog.round     r  ON r."Id"         = t."RoundId"
                    JOIN analog.game_mode gm ON gm."Id"        = r."GameModeId"
                    JOIN team_sizes       ts ON ts.team_id     = t."Id"
                )
                SELECT
                    tm1."PlayerId"                                                                      AS player_id,
                    tm2."PlayerId"                                                                      AS partner_id,
                    p."Name"                                                                            AS partner_name,
                    count(*)                                                                            AS games_together,
                    count(*) FILTER (WHERE tr.won)                                                      AS wins_together,
                    round(count(*) FILTER (WHERE tr.won)::decimal / count(*), 3)                       AS win_rate_together,
                    round(avg(tr.game_value * tr.solo_multiplier), 2)                                  AS avg_points_won_lost
                FROM analog.team_member tm1
                JOIN analog.team_member    tm2 ON tm2."TeamId"  = tm1."TeamId"
                                               AND tm2."PlayerId" != tm1."PlayerId"
                JOIN analog.player         p   ON p."Id"         = tm2."PlayerId"
                JOIN team_round_results    tr  ON tr.team_id     = tm1."TeamId"
                GROUP BY tm1."PlayerId", tm2."PlayerId", p."Name";
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                        t."Id"    AS team_id,
                        (t."Party" = r."WinningParty") AS won
                    FROM analog.team      t
                    JOIN analog.round     r  ON r."Id"         = t."RoundId"
                    JOIN analog.game_mode gm ON gm."Id"        = r."GameModeId"
                    JOIN team_sizes       ts ON ts.team_id     = t."Id"
                )
                SELECT
                    tm1."PlayerId"  AS player_id,
                    tm2."PlayerId"  AS partner_id,
                    p."Name"        AS partner_name,
                    count(*)        AS games_together,
                    count(*) FILTER (WHERE tr.won) AS wins_together,
                    round(count(*) FILTER (WHERE tr.won)::decimal / count(*), 3) AS win_rate_together
                FROM analog.team_member tm1
                JOIN analog.team_member    tm2 ON tm2."TeamId"  = tm1."TeamId"
                                               AND tm2."PlayerId" != tm1."PlayerId"
                JOIN analog.player         p   ON p."Id"         = tm2."PlayerId"
                JOIN team_round_results    tr  ON tr.team_id     = tm1."TeamId"
                GROUP BY tm1."PlayerId", tm2."PlayerId", p."Name";
                """
            );
        }
    }
}
