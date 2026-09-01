using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "click_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prepare_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    click_transaction_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    prepare_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    complete_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_click_transactions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    invite_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organizations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organizations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "payme_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payme_transaction_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<long>(type: "bigint", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    create_time = table.Column<long>(type: "bigint", nullable: false),
                    perform_time = table.Column<long>(type: "bigint", nullable: true),
                    cancel_time = table.Column<long>(type: "bigint", nullable: true),
                    cancel_reason = table.Column<int>(type: "integer", nullable: true)
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
                    order_number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_tiyin = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    type = table.Column<int>(type: "integer", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
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
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    starts_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "teacher_applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    telegram_username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    organization_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    experience_text = table.Column<string>(type: "text", nullable: true),
                    additional_notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reviewed_by = table.Column<Guid>(type: "uuid", nullable: true),
                    rejection_reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
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
                name: "external_logins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<int>(type: "integer", nullable: false),
                    external_user_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    external_username = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    linked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    telegram_blocked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_logins", x => x.id);
                    table.ForeignKey(
                        name: "fk_external_logins_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    replaced_by_token_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    created_by_ip = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "ix_external_logins_provider_external_user_id",
                table: "external_logins",
                columns: new[] { "provider", "external_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_logins_user_id_provider",
                table: "external_logins",
                columns: new[] { "user_id", "provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_group_members_user_id",
                table: "group_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_organization_id",
                table: "groups",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "ix_groups_teacher_id",
                table: "groups",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "ux_groups_invite_code",
                table: "groups",
                column: "invite_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_organizations_slug",
                table: "organizations",
                column: "slug",
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
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash");

            migrationBuilder.CreateIndex(
                name: "ix_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_organization_id",
                table: "users",
                column: "organization_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "click_transactions");

            migrationBuilder.DropTable(
                name: "external_logins");

            migrationBuilder.DropTable(
                name: "group_members");

            migrationBuilder.DropTable(
                name: "organizations");

            migrationBuilder.DropTable(
                name: "payme_transactions");

            migrationBuilder.DropTable(
                name: "payment_orders");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "subscription_plans");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DropTable(
                name: "teacher_applications");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
