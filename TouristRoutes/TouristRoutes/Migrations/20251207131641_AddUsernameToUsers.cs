using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TouristRoutes.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "users",
                newName: "Username");

            migrationBuilder.AddColumn<string>(
                name: "RouteSummary",
                table: "routes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "routes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "TotalDistanceKm",
                table: "routes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TotalDuration",
                table: "routes",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "TotalTravelTime",
                table: "routes",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RouteSummary",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "TotalDistanceKm",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "TotalDuration",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "TotalTravelTime",
                table: "routes");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "users",
                newName: "UserName");
        }
    }
}
