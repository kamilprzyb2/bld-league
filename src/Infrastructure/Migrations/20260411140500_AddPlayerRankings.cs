using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BldLeague.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerRankings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerRankings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    best_single = table.Column<int>(type: "integer", nullable: true),
                    single_rank = table.Column<int>(type: "integer", nullable: true),
                    single_round_id = table.Column<Guid>(type: "uuid", nullable: true),
                    best_average = table.Column<int>(type: "integer", nullable: true),
                    average_rank = table.Column<int>(type: "integer", nullable: true),
                    average_round_id = table.Column<Guid>(type: "uuid", nullable: true),
                    average_solve1 = table.Column<int>(type: "integer", nullable: true),
                    average_solve2 = table.Column<int>(type: "integer", nullable: true),
                    average_solve3 = table.Column<int>(type: "integer", nullable: true),
                    average_solve4 = table.Column<int>(type: "integer", nullable: true),
                    average_solve5 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_rankings", x => x.id);
                    table.ForeignKey(
                        name: "fk_player_rankings_round_average_round_id",
                        column: x => x.average_round_id,
                        principalTable: "rounds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_player_rankings_round_single_round_id",
                        column: x => x.single_round_id,
                        principalTable: "rounds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_player_rankings_user_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_player_rankings_average_round_id",
                table: "PlayerRankings",
                column: "average_round_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_rankings_single_round_id",
                table: "PlayerRankings",
                column: "single_round_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_rankings_user_id",
                table: "PlayerRankings",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerRankings");
        }
    }
}
