using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Network.Management.App.Migrations
{
    /// <inheritdoc />
    public partial class AddVlansPortsRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VlanId",
                table: "devices",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    route_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    destination = table.Column<string>(type: "text", nullable: false),
                    gateway = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    device_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routes", x => x.route_id);
                    table.ForeignKey(
                        name: "FK_routes_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vlans",
                columns: table => new
                {
                    vlan_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vlan_number = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vlans", x => x.vlan_id);
                });

            migrationBuilder.CreateTable(
                name: "ports",
                columns: table => new
                {
                    port_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    is_up = table.Column<bool>(type: "boolean", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: false),
                    vlan_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ports", x => x.port_id);
                    table.ForeignKey(
                        name: "FK_ports_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ports_vlans_vlan_id",
                        column: x => x.vlan_id,
                        principalTable: "vlans",
                        principalColumn: "vlan_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_devices_VlanId",
                table: "devices",
                column: "VlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ports_device_id",
                table: "ports",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_ports_vlan_id",
                table: "ports",
                column: "vlan_id");

            migrationBuilder.CreateIndex(
                name: "IX_routes_device_id",
                table: "routes",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_vlans_vlan_number",
                table: "vlans",
                column: "vlan_number",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_devices_vlans_VlanId",
                table: "devices",
                column: "VlanId",
                principalTable: "vlans",
                principalColumn: "vlan_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_devices_vlans_VlanId",
                table: "devices");

            migrationBuilder.DropTable(
                name: "ports");

            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "vlans");

            migrationBuilder.DropIndex(
                name: "IX_devices_VlanId",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "VlanId",
                table: "devices");
        }
    }
}
