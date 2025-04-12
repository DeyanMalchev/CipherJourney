using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CipherJourney.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserPointsAndDailyConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeeklyScore",
                table: "UserPoints");

            migrationBuilder.DropColumn(
                name: "WeekliesDone",
                table: "UserPoints");

            // Rename columns
            migrationBuilder.RenameColumn(
                name: "DailiesDone",
                table: "UserPoints",
                newName: "RiddlesSolved");

            migrationBuilder.RenameColumn(
                name: "DailyScore",
                table: "UserPoints",
                newName: "Score");

            // Add GuessCount column
            migrationBuilder.AddColumn<int>(
                name: "GuessCount",
                table: "UserPoints",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
            name: "DailyConfiguration",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Sentence = table.Column<string>(nullable: false),
                Cipher = table.Column<string>(nullable: false),
                Key = table.Column<string>(nullable: true),
                Date = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_DailyConfiguration", x => x.Id);
            });


        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
