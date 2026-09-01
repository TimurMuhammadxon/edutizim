using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceAndMembershipStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "activated_at",
                table: "group_students",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "discount_end_date",
                table: "group_students",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "discount_start_date",
                table: "group_students",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "discounted_price",
                table: "group_students",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "next_payment_due_date",
                table: "group_students",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "group_students",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "tuition_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    paid_at = table.Column<DateOnly>(type: "date", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tuition_payments", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_tuition_payments_group_id",
                table: "tuition_payments",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_tuition_payments_organization_id",
                table: "tuition_payments",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_tuition_payments_student_id",
                table: "tuition_payments",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tuition_payments");

            migrationBuilder.DropColumn(
                name: "activated_at",
                table: "group_students");

            migrationBuilder.DropColumn(
                name: "discount_end_date",
                table: "group_students");

            migrationBuilder.DropColumn(
                name: "discount_start_date",
                table: "group_students");

            migrationBuilder.DropColumn(
                name: "discounted_price",
                table: "group_students");

            migrationBuilder.DropColumn(
                name: "next_payment_due_date",
                table: "group_students");

            migrationBuilder.DropColumn(
                name: "status",
                table: "group_students");
        }
    }
}
