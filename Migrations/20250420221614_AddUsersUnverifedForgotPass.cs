using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CipherJourney.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersUnverifedForgotPass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "UsersUnverified",
                newName: "UserVerificationTokens"
            );

            migrationBuilder.RenameColumn(
                name: "DateOfCreation",
                table: "UserVerificationTokens",
                newName: "ExpirationDate"
            );

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "UserVerificationTokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "email_verification"  // Optional default if you want
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
