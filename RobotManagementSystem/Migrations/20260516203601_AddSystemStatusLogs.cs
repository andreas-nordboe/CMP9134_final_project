using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemStatusLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemStatusLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    CurrentStatus = table.Column<string>(type: "TEXT", nullable: true),
                    RobotX = table.Column<int>(type: "INTEGER", nullable: true),
                    RobotY = table.Column<int>(type: "INTEGER", nullable: true),
                    Battery = table.Column<double>(type: "REAL", nullable: true),
                    RobotState = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemStatusLogs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemStatusLogs");
        }
    }
}
