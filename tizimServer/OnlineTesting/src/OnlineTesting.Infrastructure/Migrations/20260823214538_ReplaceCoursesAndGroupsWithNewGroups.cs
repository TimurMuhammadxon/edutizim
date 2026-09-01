using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCoursesAndGroupsWithNewGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "group_members");

            migrationBuilder.DropTable(
                name: "teacher_applications");

            migrationBuilder.DropIndex(
                name: "ux_groups_invite_code",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "invite_code",
                table: "groups");

            migrationBuilder.AlterColumn<Guid>(
                name: "teacher_id",
                table: "groups",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "branch_id",
                table: "groups",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "groups",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "price",
                table: "groups",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "group_schedule_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_schedule_slots", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "group_students",
                columns: table => new
                {
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_students", x => new { x.group_id, x.student_id });
                });

            migrationBuilder.CreateIndex(
                name: "ix_groups_branch_id",
                table: "groups",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_schedule_slots_group_id",
                table: "group_schedule_slots",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_schedule_slots_organization_id",
                table: "group_schedule_slots",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_students_student_id",
                table: "group_students",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "group_schedule_slots");

            migrationBuilder.DropTable(
                name: "group_students");

            migrationBuilder.DropIndex(
                name: "ix_groups_branch_id",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "branch_id",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "description",
                table: "groups");

            migrationBuilder.DropColumn(
                name: "price",
                table: "groups");

            migrationBuilder.AlterColumn<Guid>(
                name: "teacher_id",
                table: "groups",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "invite_code",
                table: "groups",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "group_members",
                columns: table => new
                {
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_members", x => new { x.group_id, x.user_id });
                    table.ForeignKey(
                        name: "fk_group_members_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teacher_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    additional_notes = table.Column<string>(type: "text", nullable: true),
                    experience_text = table.Column<string>(type: "text", nullable: true),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    rejection_reason = table.Column<string>(type: "text", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    telegram_username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_applications", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_groups_invite_code",
                table: "groups",
                column: "invite_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_courses_organization_id",
                table: "courses",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_members_user_id",
                table: "group_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_applications_organization_id",
                table: "teacher_applications",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_applications_status",
                table: "teacher_applications",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ux_teacher_applications_user_pending",
                table: "teacher_applications",
                column: "user_id",
                unique: true,
                filter: "status = 0");
        }
    }
}
