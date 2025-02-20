using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CipherJourney.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyGameTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_Users_VerificationToken",
            //    table: "Users"
            //    );

            migrationBuilder.DropColumn(
                name: "VerificationToken",
                table: "Users");


            migrationBuilder.DropColumn(
                name: "Score",
                table: "UserPoints"
                );

            migrationBuilder.AddColumn<int>(
                name: "DailyScore",
                table: "UserPoints",
                defaultValue: 0
                );

            migrationBuilder.AddColumn<int>(
                name: "WeeklyScore",
                table: "UserPoints",
                defaultValue: 0
                );

            migrationBuilder.RenameColumn(
                name: "DailyAmountDone",
                table: "UserPoints",
                newName: "DailiesDone"
                );
            migrationBuilder.RenameColumn(
                name: "WeeklyAmountDone",
                table: "UserPoints",
                newName: "WeekliesDone"
                );

            migrationBuilder.CreateTable(
                name: "Ciphers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cipher = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ciphers", x => x.Id);

                });

            migrationBuilder.CreateTable(
                name: "Leaderboard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalPoints = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaderboard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaderboard_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SentencesDaily",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sentence = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentencesDaily", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsersUnverified",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    VerificationToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfCreation = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersUnverified", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsersUnverified_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leaderboard_UserId",
                table: "Leaderboard",
                column: "UserId");


            migrationBuilder.CreateIndex(
                name: "IX_UsersUnverified_UserID",
                table: "UsersUnverified",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
