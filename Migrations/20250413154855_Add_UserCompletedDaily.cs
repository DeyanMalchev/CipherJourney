using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CipherJourney.Migrations
{
    /// <inheritdoc />
    public partial class Add_UserCompletedDaily : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "DailyConfiguration",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // Create new table
            migrationBuilder.CreateTable(
                name: "UsersCompletedDaily",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersCompletedDaily", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
