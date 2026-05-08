using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BldLeague.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchSubmissionTimestamps_RemoveRoundSubmissionFormUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "submission_form_url",
                table: "rounds");

            migrationBuilder.AddColumn<DateTime>(
                name: "user_a_submitted_at",
                table: "matches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "user_b_submitted_at",
                table: "matches",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_a_submitted_at",
                table: "matches");

            migrationBuilder.DropColumn(
                name: "user_b_submitted_at",
                table: "matches");

            migrationBuilder.AddColumn<string>(
                name: "submission_form_url",
                table: "rounds",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}
