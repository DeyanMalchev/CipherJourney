using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CipherJourney.Migrations
{
    /// <inheritdoc />
    public partial class AddSentenceAndCipherData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
            table: "Ciphers",
            columns: new[] { "Cipher" },
            values: new object[,]
            {
                    { "Caesar" }
            });

            migrationBuilder.InsertData(
                table: "SentencesDaily",
                columns: new[] { "Sentence" },
                values: new object[,]
                {
                    { "The quick brown fox jumps over the lazy dog." },
                    { "Knowledge is power!" },
                    { "Encryption makes data secure!" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
