using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RobotManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMissionLogUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MissionLogs_UserAccountDto_UserId",
                table: "MissionLogs");

            migrationBuilder.DropTable(
                name: "UserAccountDto");

            migrationBuilder.DropIndex(
                name: "IX_MissionLogs_UserId",
                table: "MissionLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccountDto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountDto", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MissionLogs_UserId",
                table: "MissionLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MissionLogs_UserAccountDto_UserId",
                table: "MissionLogs",
                column: "UserId",
                principalTable: "UserAccountDto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
