using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class ImproveMissionLogPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Battery",
                table: "MissionLogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectionStatus",
                table: "MissionLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "MissionLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RobotX",
                table: "MissionLogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RobotY",
                table: "MissionLogs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MissionLogs_UserId",
                table: "MissionLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MissionLogs_Users_UserId",
                table: "MissionLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MissionLogs_Users_UserId",
                table: "MissionLogs");

            migrationBuilder.DropIndex(
                name: "IX_MissionLogs_UserId",
                table: "MissionLogs");

            migrationBuilder.DropColumn(
                name: "Battery",
                table: "MissionLogs");

            migrationBuilder.DropColumn(
                name: "ConnectionStatus",
                table: "MissionLogs");

            migrationBuilder.DropColumn(
                name: "Details",
                table: "MissionLogs");

            migrationBuilder.DropColumn(
                name: "RobotX",
                table: "MissionLogs");

            migrationBuilder.DropColumn(
                name: "RobotY",
                table: "MissionLogs");
        }
    }
}
