using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pulsar.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "MonitoredEndpoints",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MonitoredEndpoints_UserId1",
                table: "MonitoredEndpoints",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MonitoredEndpoints_Users_UserId1",
                table: "MonitoredEndpoints",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MonitoredEndpoints_Users_UserId1",
                table: "MonitoredEndpoints");

            migrationBuilder.DropIndex(
                name: "IX_MonitoredEndpoints_UserId1",
                table: "MonitoredEndpoints");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "MonitoredEndpoints");
        }
    }
}
