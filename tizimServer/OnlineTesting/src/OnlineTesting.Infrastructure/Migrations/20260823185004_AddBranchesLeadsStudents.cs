using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchesLeadsStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.RenameColumn(
                name: "client_id",
                table: "crm_tasks",
                newName: "lead_id");

            migrationBuilder.RenameIndex(
                name: "ix_crm_tasks_client_id",
                table: "crm_tasks",
                newName: "ix_crm_tasks_lead_id");

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    source = table.Column<int>(type: "integer", nullable: false),
                    stage = table.Column<int>(type: "integer", nullable: false),
                    assigned_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    lost_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_leads", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    branch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lead_id = table.Column<Guid>(type: "uuid", nullable: true),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_students", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_branches_organization_id",
                table: "branches",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_leads_assigned_manager_id",
                table: "leads",
                column: "assigned_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_leads_branch_id",
                table: "leads",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_leads_organization_id",
                table: "leads",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_leads_stage",
                table: "leads",
                column: "stage");

            migrationBuilder.CreateIndex(
                name: "ix_students_branch_id",
                table: "students",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_students_lead_id",
                table: "students",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "ix_students_organization_id",
                table: "students",
                column: "organization_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "leads");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.RenameColumn(
                name: "lead_id",
                table: "crm_tasks",
                newName: "client_id");

            migrationBuilder.RenameIndex(
                name: "ix_crm_tasks_lead_id",
                table: "crm_tasks",
                newName: "ix_crm_tasks_client_id");

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    source = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_clients_assigned_manager_id",
                table: "clients",
                column: "assigned_manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_clients_organization_id",
                table: "clients",
                column: "organization_id");
        }
    }
}
