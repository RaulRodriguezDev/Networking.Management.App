using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Network.Management.App.Migrations
{
    /// <inheritdoc />
    public partial class FixLogUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_logs_log_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_log_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "log_id",
                table: "users");

            migrationBuilder.AddColumn<int>(
                name: "user_id",
                table: "logs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_logs_user_id",
                table: "logs",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_logs_users_user_id",
                table: "logs",
                column: "user_id",
                principalTable: "users",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_logs_users_user_id",
                table: "logs");

            migrationBuilder.DropIndex(
                name: "IX_logs_user_id",
                table: "logs");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "logs");

            migrationBuilder.AddColumn<int>(
                name: "log_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_log_id",
                table: "users",
                column: "log_id");

            migrationBuilder.AddForeignKey(
                name: "FK_users_logs_log_id",
                table: "users",
                column: "log_id",
                principalTable: "logs",
                principalColumn: "log_id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
