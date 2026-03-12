using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationNoise.Insights.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitInsights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_stats",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Total = table.Column<int>(type: "integer", nullable: false),
                    Noise = table.Column<int>(type: "integer", nullable: false),
                    Useful = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_stats", x => new { x.UserId, x.Date });
                });

            migrationBuilder.CreateTable(
                name: "recommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Target = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Why = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ImpactScore = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recommendations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sender_stats",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SenderKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TotalCount = table.Column<int>(type: "integer", nullable: false),
                    NoiseCount = table.Column<int>(type: "integer", nullable: false),
                    UsefulCount = table.Column<int>(type: "integer", nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sender_stats", x => new { x.UserId, x.SenderKey });
                });

            migrationBuilder.CreateIndex(
                name: "IX_recommendations_UserId_Status",
                table: "recommendations",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_stats");

            migrationBuilder.DropTable(
                name: "recommendations");

            migrationBuilder.DropTable(
                name: "sender_stats");
        }
    }
}
