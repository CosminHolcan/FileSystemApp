using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class AddedReplicaProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReplica",
                table: "AppFiles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReplicaId",
                table: "AppFiles",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReplica",
                table: "AppFiles");

            migrationBuilder.DropColumn(
                name: "ReplicaId",
                table: "AppFiles");
        }
    }
}
