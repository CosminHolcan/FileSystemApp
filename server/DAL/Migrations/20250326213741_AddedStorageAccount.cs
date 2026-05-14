using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddedStorageAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageAccount",
                table: "AppFiles");

            migrationBuilder.AddColumn<Guid>(
                name: "StorageAccountId",
                table: "AppFiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "StorageAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<int>(type: "int", nullable: false),
                    Redundancy = table.Column<int>(type: "int", nullable: false),
                    Versioning = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageAccounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppFiles_StorageAccountId",
                table: "AppFiles",
                column: "StorageAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppFiles_StorageAccounts_StorageAccountId",
                table: "AppFiles",
                column: "StorageAccountId",
                principalTable: "StorageAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppFiles_StorageAccounts_StorageAccountId",
                table: "AppFiles");

            migrationBuilder.DropTable(
                name: "StorageAccounts");

            migrationBuilder.DropIndex(
                name: "IX_AppFiles_StorageAccountId",
                table: "AppFiles");

            migrationBuilder.DropColumn(
                name: "StorageAccountId",
                table: "AppFiles");

            migrationBuilder.AddColumn<string>(
                name: "StorageAccount",
                table: "AppFiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
