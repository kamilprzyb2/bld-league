using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BldLeague.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeagueSeasonPlayoffCounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "playoff_promotion_count",
                table: "league_seasons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "playoff_relegation_count",
                table: "league_seasons",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "playoff_promotion_count",
                table: "league_seasons");

            migrationBuilder.DropColumn(
                name: "playoff_relegation_count",
                table: "league_seasons");
        }
    }
}
