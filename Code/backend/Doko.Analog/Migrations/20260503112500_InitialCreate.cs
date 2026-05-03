using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Doko.Analog.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "analog");

            migrationBuilder.CreateTable(
                name: "extra_point",
                schema: "analog",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    Name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extra_point", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "game_mode",
                schema: "analog",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    Name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    IsSolo = table.Column<bool>(type: "boolean", nullable: false),
                    SoloParty = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_mode", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "player",
                schema: "analog",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    Name = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    StartingPoints = table.Column<decimal>(type: "numeric(10,1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "now()"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "special_card",
                schema: "analog",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    Name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_special_card", x => x.Id);
                }
            );

            migrationBuilder.CreateTable(
                name: "round",
                schema: "analog",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    WinningParty = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    PlayedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    GameModeId = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_round", x => x.Id);
                    table.ForeignKey(
                        name: "FK_round_game_mode_GameModeId",
                        column: x => x.GameModeId,
                        principalSchema: "analog",
                        principalTable: "game_mode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "comment",
                schema: "analog",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    RoundId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comment_round_RoundId",
                        column: x => x.RoundId,
                        principalSchema: "analog",
                        principalTable: "round",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "team",
                schema: "analog",
                columns: table => new
                {
                    Id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    RoundId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: true
                    ),
                    Party = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team", x => x.Id);
                    table.ForeignKey(
                        name: "FK_team_round_RoundId",
                        column: x => x.RoundId,
                        principalSchema: "analog",
                        principalTable: "round",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "round_extra_point",
                schema: "analog",
                columns: table => new
                {
                    RoundId = table.Column<int>(type: "integer", nullable: false),
                    ExtraPointId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_round_extra_point",
                        x => new
                        {
                            x.RoundId,
                            x.ExtraPointId,
                            x.TeamId,
                        }
                    );
                    table.ForeignKey(
                        name: "FK_round_extra_point_extra_point_ExtraPointId",
                        column: x => x.ExtraPointId,
                        principalSchema: "analog",
                        principalTable: "extra_point",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_round_extra_point_round_RoundId",
                        column: x => x.RoundId,
                        principalSchema: "analog",
                        principalTable: "round",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_round_extra_point_team_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "analog",
                        principalTable: "team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "round_special_card",
                schema: "analog",
                columns: table => new
                {
                    RoundId = table.Column<int>(type: "integer", nullable: false),
                    SpecialCardId = table.Column<int>(type: "integer", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_round_special_card",
                        x => new
                        {
                            x.RoundId,
                            x.SpecialCardId,
                            x.TeamId,
                        }
                    );
                    table.ForeignKey(
                        name: "FK_round_special_card_round_RoundId",
                        column: x => x.RoundId,
                        principalSchema: "analog",
                        principalTable: "round",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_round_special_card_special_card_SpecialCardId",
                        column: x => x.SpecialCardId,
                        principalSchema: "analog",
                        principalTable: "special_card",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_round_special_card_team_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "analog",
                        principalTable: "team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "team_member",
                schema: "analog",
                columns: table => new
                {
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_member", x => new { x.TeamId, x.PlayerId });
                    table.ForeignKey(
                        name: "FK_team_member_player_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "analog",
                        principalTable: "player",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_team_member_team_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "analog",
                        principalTable: "team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.InsertData(
                schema: "analog",
                table: "extra_point",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Agathe" },
                    { 2, "Doppelkopf" },
                    { 3, "Fischauge" },
                    { 4, "Fuchs gefangen" },
                    { 5, "Gans gefangen" },
                    { 6, "Kaffeekränzchen" },
                    { 7, "Karlchen" },
                    { 8, "Klabautermann gefangen" },
                }
            );

            migrationBuilder.InsertData(
                schema: "analog",
                table: "game_mode",
                columns: new[] { "Id", "IsSolo", "Name", "SoloParty" },
                values: new object[,]
                {
                    { 1, false, "Armut", null },
                    { 2, false, "Hochzeit", null },
                    { 3, false, "Normal", null },
                    { 4, true, "Bubensolo", 0 },
                    { 5, true, "Damensolo", 0 },
                    { 6, true, "Farbsolo", 0 },
                    { 7, true, "Fleischloses", 0 },
                    { 8, true, "Knochenloses", 0 },
                    { 9, true, "Kontrasolo", 1 },
                    { 10, true, "Schlanker Martin", 0 },
                    { 11, true, "Schwarze Sau", 0 },
                    { 12, true, "Stille Hochzeit", 0 },
                }
            );

            migrationBuilder.InsertData(
                schema: "analog",
                table: "special_card",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Gegengenscherdamen" },
                    { 2, "Genscherdamen" },
                    { 3, "Heidfrau" },
                    { 4, "Heidmann" },
                    { 5, "Hyperschweinchen" },
                    { 6, "Kemmerich" },
                    { 7, "Linksdrehender Gehängter" },
                    { 8, "Schweinchen" },
                    { 9, "Superschweinchen" },
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_comment_RoundId",
                schema: "analog",
                table: "comment",
                column: "RoundId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_round_GameModeId",
                schema: "analog",
                table: "round",
                column: "GameModeId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_round_extra_point_ExtraPointId",
                schema: "analog",
                table: "round_extra_point",
                column: "ExtraPointId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_round_extra_point_TeamId",
                schema: "analog",
                table: "round_extra_point",
                column: "TeamId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_round_special_card_SpecialCardId",
                schema: "analog",
                table: "round_special_card",
                column: "SpecialCardId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_round_special_card_TeamId",
                schema: "analog",
                table: "round_special_card",
                column: "TeamId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_team_RoundId",
                schema: "analog",
                table: "team",
                column: "RoundId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_team_member_PlayerId",
                schema: "analog",
                table: "team_member",
                column: "PlayerId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "comment", schema: "analog");

            migrationBuilder.DropTable(name: "round_extra_point", schema: "analog");

            migrationBuilder.DropTable(name: "round_special_card", schema: "analog");

            migrationBuilder.DropTable(name: "team_member", schema: "analog");

            migrationBuilder.DropTable(name: "extra_point", schema: "analog");

            migrationBuilder.DropTable(name: "special_card", schema: "analog");

            migrationBuilder.DropTable(name: "player", schema: "analog");

            migrationBuilder.DropTable(name: "team", schema: "analog");

            migrationBuilder.DropTable(name: "round", schema: "analog");

            migrationBuilder.DropTable(name: "game_mode", schema: "analog");
        }
    }
}
