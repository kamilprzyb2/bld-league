using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BldLeague.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubleagueGroupToLeagueSeasonUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "subleague_group",
                table: "league_season_users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "subleague_group",
                table: "league_season_users");
        }
    }
}
