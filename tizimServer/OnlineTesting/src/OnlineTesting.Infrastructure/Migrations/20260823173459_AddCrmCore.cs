using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    source = table.Column<int>(type: "integer", nullable: false),
                    assigned_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "crm_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    due_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    assigned_to_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_crm_tasks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "deals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    course_id = table.Column<Guid>(type: "uuid", nullable: true),
                    stage = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    assigned_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    expected_close_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lost_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_deals", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_clients_assigned_manager_id",
                table: "clients",
                column: "assigned_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_clients_organization_id",
                table: "clients",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_courses_organization_id",
                table: "courses",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_tasks_assigned_to_user_id",
                table: "crm_tasks",
                column: "assigned_to_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_tasks_client_id",
                table: "crm_tasks",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_tasks_deal_id",
                table: "crm_tasks",
                column: "deal_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_tasks_due_at",
                table: "crm_tasks",
                column: "due_at");

            migrationBuilder.CreateIndex(
                name: "ix_crm_tasks_organization_id",
                table: "crm_tasks",
                column: "organization_id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "crm_tasks");

            migrationBuilder.DropTable(
                name: "deals");
        }
    }
}
