using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BldLeague.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "leagues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    league_identifier = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_leagues", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "seasons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_number = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_seasons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    wca_id = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    avatar_thumbnail_url = table.Column<string>(type: "text", nullable: true),
                    is_admin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "league_seasons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    league_id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_league_seasons", x => x.id);
                    table.ForeignKey(
                        name: "fk_league_seasons_leagues_league_id",
                        column: x => x.league_id,
                        principalTable: "leagues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_league_seasons_season_season_id",
                        column: x => x.season_id,
                        principalTable: "seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rounds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    round_number = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rounds", x => x.id);
                    table.ForeignKey(
                        name: "fk_rounds_season_season_id",
                        column: x => x.season_id,
                        principalTable: "seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "league_season_standings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    league_season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    place = table.Column<int>(type: "integer", nullable: false),
                    matches_played = table.Column<int>(type: "integer", nullable: false),
                    matches_won = table.Column<int>(type: "integer", nullable: false),
                    matches_tied = table.Column<int>(type: "integer", nullable: false),
                    matches_lost = table.Column<int>(type: "integer", nullable: false),
                    big_points = table.Column<int>(type: "integer", nullable: false),
                    bonus_points = table.Column<int>(type: "integer", nullable: false),
                    small_points_gained = table.Column<int>(type: "integer", nullable: false),
                    small_points_lost = table.Column<int>(type: "integer", nullable: false),
                    small_points_balance = table.Column<int>(type: "integer", nullable: false),
                    best = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_league_season_standings", x => x.id);
                    table.ForeignKey(
                        name: "fk_league_season_standings_league_seasons_league_season_id",
                        column: x => x.league_season_id,
                        principalTable: "league_seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_league_season_standings_user_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "league_season_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    league_season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_league_season_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_league_season_users_league_seasons_league_season_id",
                        column: x => x.league_season_id,
                        principalTable: "league_seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_league_season_users_user_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    league_season_id = table.Column<Guid>(type: "uuid", nullable: false),
                    round_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_a_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_b_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_a_score = table.Column<int>(type: "integer", nullable: false),
                    user_b_score = table.Column<int>(type: "integer", nullable: false),
                    user_a_best = table.Column<int>(type: "integer", nullable: false),
                    user_b_best = table.Column<int>(type: "integer", nullable: false),
                    user_a_average = table.Column<int>(type: "integer", nullable: false),
                    user_b_average = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_matches", x => x.id);
                    table.ForeignKey(
                        name: "fk_matches_league_seasons_league_season_id",
                        column: x => x.league_season_id,
                        principalTable: "league_seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_matches_round_round_id",
                        column: x => x.round_id,
                        principalTable: "rounds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_matches_user_user_a_id",
                        column: x => x.user_a_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_matches_user_user_b_id",
                        column: x => x.user_b_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "round_standings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    round_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    league_id = table.Column<Guid>(type: "uuid", nullable: false),
                    points = table.Column<int>(type: "integer", nullable: false),
                    place = table.Column<int>(type: "integer", nullable: false),
                    solve1 = table.Column<int>(type: "integer", nullable: false),
                    solve2 = table.Column<int>(type: "integer", nullable: false),
                    solve3 = table.Column<int>(type: "integer", nullable: false),
                    solve4 = table.Column<int>(type: "integer", nullable: false),
                    solve5 = table.Column<int>(type: "integer", nullable: false),
                    best = table.Column<int>(type: "integer", nullable: false),
                    average = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_round_standings", x => x.id);
                    table.ForeignKey(
                        name: "fk_round_standings_leagues_league_id",
                        column: x => x.league_id,
                        principalTable: "leagues",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_round_standings_rounds_round_id",
                        column: x => x.round_id,
                        principalTable: "rounds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_round_standings_user_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scrambles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    round_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scramble_number = table.Column<int>(type: "integer", nullable: false),
                    notation = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scrambles", x => x.id);
                    table.ForeignKey(
                        name: "fk_scrambles_rounds_round_id",
                        column: x => x.round_id,
                        principalTable: "rounds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "solves",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    result = table.Column<int>(type: "integer", nullable: false),
                    index = table.Column<int>(type: "integer", nullable: false),
                    match_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_solves", x => x.id);
                    table.ForeignKey(
                        name: "fk_solves_matches_match_id",
                        column: x => x.match_id,
                        principalTable: "matches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_solves_user_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_league_season_standings_league_season_id",
                table: "league_season_standings",
                column: "league_season_id");

            migrationBuilder.CreateIndex(
                name: "ix_league_season_standings_user_id",
                table: "league_season_standings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_league_season_users_league_season_id_user_id",
                table: "league_season_users",
                columns: new[] { "league_season_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_league_season_users_user_id",
                table: "league_season_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_league_seasons_league_id_season_id",
                table: "league_seasons",
                columns: new[] { "league_id", "season_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_league_seasons_season_id",
                table: "league_seasons",
                column: "season_id");

            migrationBuilder.CreateIndex(
                name: "ix_leagues_league_identifier",
                table: "leagues",
                column: "league_identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_matches_league_season_id",
                table: "matches",
                column: "league_season_id");

            migrationBuilder.CreateIndex(
                name: "ix_matches_round_id",
                table: "matches",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "ix_matches_user_a_id",
                table: "matches",
                column: "user_a_id");

            migrationBuilder.CreateIndex(
                name: "ix_matches_user_b_id",
                table: "matches",
                column: "user_b_id");

            migrationBuilder.CreateIndex(
                name: "ix_round_standings_league_id",
                table: "round_standings",
                column: "league_id");

            migrationBuilder.CreateIndex(
                name: "ix_round_standings_round_id",
                table: "round_standings",
                column: "round_id");

            migrationBuilder.CreateIndex(
                name: "ix_round_standings_user_id",
                table: "round_standings",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_rounds_season_id_round_number",
                table: "rounds",
                columns: new[] { "season_id", "round_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scrambles_round_id_scramble_number",
                table: "scrambles",
                columns: new[] { "round_id", "scramble_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_seasons_season_number",
                table: "seasons",
                column: "season_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_solves_match_id",
                table: "solves",
                column: "match_id");

            migrationBuilder.CreateIndex(
                name: "ix_solves_user_id",
                table: "solves",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_wca_id",
                table: "users",
                column: "wca_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "league_season_standings");

            migrationBuilder.DropTable(
                name: "league_season_users");

            migrationBuilder.DropTable(
                name: "round_standings");

            migrationBuilder.DropTable(
                name: "scrambles");

            migrationBuilder.DropTable(
                name: "solves");

            migrationBuilder.DropTable(
                name: "matches");

            migrationBuilder.DropTable(
                name: "league_seasons");

            migrationBuilder.DropTable(
                name: "rounds");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "leagues");

            migrationBuilder.DropTable(
                name: "seasons");
        }
    }
}
