using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentForMonth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "for_month",
                table: "tuition_payments",
                type: "date",
                nullable: true);

            // Backfill existing rows: assume each payment was for the calendar month it was
            // actually recorded in (the only reasonable default — we have no other signal).
            migrationBuilder.Sql(
                "UPDATE tuition_payments SET for_month = date_trunc('month', paid_at)::date WHERE for_month IS NULL;");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "for_month",
                table: "tuition_payments",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "for_month",
                table: "tuition_payments");
        }
    }
}
