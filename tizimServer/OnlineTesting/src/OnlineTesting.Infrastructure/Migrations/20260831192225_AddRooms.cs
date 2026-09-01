using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "room_id",
                table: "groups",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rooms", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_groups_room_id",
                table: "groups",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_branch_id",
                table: "rooms",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_organization_id",
                table: "rooms",
                column: "organization_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropIndex(
                name: "ix_groups_room_id",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "room_id",
                table: "groups");
        }
    }
}
