using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Network.Management.App.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceMacAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mac_address",
                table: "devices",
                type: "character varying(17)",
                maxLength: 17,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mac_address",
                table: "devices");
        }
    }
}
