using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddMissionLogId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccountDto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountDto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MissionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Command = table.Column<int>(type: "INTEGER", nullable: false),
                    CommandResult = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MissionLogs_UserAccountDto_UserId",
                        column: x => x.UserId,
                        principalTable: "UserAccountDto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MissionLogs_UserId",
                table: "MissionLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MissionLogs");

            migrationBuilder.DropTable(
                name: "UserAccountDto");
        }
    }
}
