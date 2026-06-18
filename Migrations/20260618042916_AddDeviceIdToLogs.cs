using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Network.Management.App.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceIdToLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "device_id",
                table: "logs",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_logs_device_id",
                table: "logs",
                column: "device_id");

            migrationBuilder.AddForeignKey(
                name: "FK_logs_devices_device_id",
                table: "logs",
                column: "device_id",
                principalTable: "devices",
                principalColumn: "device_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_logs_devices_device_id",
                table: "logs");

            migrationBuilder.DropIndex(
                name: "IX_logs_device_id",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "device_id",
                table: "logs");
        }
    }
}
