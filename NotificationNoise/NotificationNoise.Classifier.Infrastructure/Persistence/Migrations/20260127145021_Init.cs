using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationNoise.Classifier.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "classifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Provider = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExternalMessageId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    ReasonsJson = table.Column<string>(type: "jsonb", nullable: false),
                    ClassifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_classifications_NotificationId",
                table: "classifications",
                column: "NotificationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_classifications_UserId_Provider_ExternalMessageId",
                table: "classifications",
                columns: new[] { "UserId", "Provider", "ExternalMessageId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "classifications");
        }
    }
}
