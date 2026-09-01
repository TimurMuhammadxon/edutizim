using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovePaymeClickSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "click_transactions");

            migrationBuilder.DropTable(
                name: "payme_transactions");

            migrationBuilder.DropTable(
                name: "payment_orders");

            migrationBuilder.DropTable(
                name: "subscription_plans");

            migrationBuilder.DropTable(
                name: "subscriptions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "click_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    click_transaction_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    complete_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<int>(type: "integer", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    prepare_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    prepare_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_click_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payme_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    cancel_reason = table.Column<int>(type: "integer", nullable: true),
                    cancel_time = table.Column<long>(type: "bigint", nullable: true),
                    create_time = table.Column<long>(type: "bigint", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payme_transaction_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    perform_time = table.Column<long>(type: "bigint", nullable: true),
                    state = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payme_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payment_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_tiyin = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    order_number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_orders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscription_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscriptions", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_click_transactions_order_id",
                table: "click_transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ux_click_transactions_click_id",
                table: "click_transactions",
                column: "click_transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_click_transactions_prepare_id",
                table: "click_transactions",
                column: "prepare_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payme_transactions_order_id",
                table: "payme_transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ux_payme_transactions_payme_id",
                table: "payme_transactions",
                column: "payme_transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_orders_status",
                table: "payment_orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_payment_orders_user_id",
                table: "payment_orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ux_payment_orders_order_number",
                table: "payment_orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_subscription_plans_type_duration",
                table: "subscription_plans",
                columns: new[] { "type", "duration" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_expires_at",
                table: "subscriptions",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "ux_subscriptions_user_id",
                table: "subscriptions",
                column: "user_id",
                unique: true);
        }
    }
}
