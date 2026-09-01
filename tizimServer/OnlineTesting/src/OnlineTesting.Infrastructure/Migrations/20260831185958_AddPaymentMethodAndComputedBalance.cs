using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodAndComputedBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "next_payment_due_date",
                table: "group_students");

            migrationBuilder.AddColumn<int>(
                name: "method",
                table: "tuition_payments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "frozen_at",
                table: "group_students",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "method",
                table: "tuition_payments");

            migrationBuilder.DropColumn(
                name: "frozen_at",
                table: "group_students");

            migrationBuilder.AddColumn<DateOnly>(
                name: "next_payment_due_date",
                table: "group_students",
                type: "date",
                nullable: true);
        }
    }
}
