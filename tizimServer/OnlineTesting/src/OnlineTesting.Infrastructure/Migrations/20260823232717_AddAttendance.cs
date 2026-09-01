using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "attendance",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    marked_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    marked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendance", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_attendance_organization_id",
                table: "attendance",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ux_attendance_group_student_date",
                table: "attendance",
                columns: new[] { "group_id", "student_id", "lesson_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "attendance");
        }
    }
}
