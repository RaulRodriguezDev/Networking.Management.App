using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Network.Management.App.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceAddressingMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "addressing_mode",
                table: "devices",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "Static");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "addressing_mode",
                table: "devices");
        }
    }
}
