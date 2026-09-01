using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deals");

            migrationBuilder.DropIndex(
                name: "ix_crm_tasks_deal_id",
                table: "crm_tasks");

            migrationBuilder.DropColumn(
                name: "deal_id",
                table: "crm_tasks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "deal_id",
                table: "crm_tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "deals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    assigned_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    course_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expected_close_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lost_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deals", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_crm_tasks_deal_id",
                table: "crm_tasks",
                column: "deal_id");

            migrationBuilder.CreateIndex(
                name: "ix_deals_assigned_manager_id",
                table: "deals",
                column: "assigned_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_deals_client_id",
                table: "deals",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_deals_course_id",
                table: "deals",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "ix_deals_organization_id",
                table: "deals",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_deals_stage",
                table: "deals",
                column: "stage");
        }
    }
}
