using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OnlineTesting.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPhoneAndStudentLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                table: "users",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "students",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_phone",
                table: "users",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_user_id",
                table: "students",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_phone",
                table: "users");

            migrationBuilder.DropIndex(
                name: "ix_students_user_id",
                table: "students");

            migrationBuilder.DropColumn(
                name: "phone",
                table: "users");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "students");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
