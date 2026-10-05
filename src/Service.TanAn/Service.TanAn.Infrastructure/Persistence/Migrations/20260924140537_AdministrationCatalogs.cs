using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.TanAn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AdministrationCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PhanHeId",
                table: "Modules",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PhanHes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ma = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Ten = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "text", nullable: true),
                    HoatDong = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhanHes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_PhanHeId",
                table: "Modules",
                column: "PhanHeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhanHes_Ma",
                table: "PhanHes",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserId_RoleId",
                table: "UserRoles",
                columns: new[] { "UserId", "RoleId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_PhanHes_PhanHeId",
                table: "Modules",
                column: "PhanHeId",
                principalTable: "PhanHes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_PhanHes_PhanHeId",
                table: "Modules");

            migrationBuilder.DropTable(
                name: "PhanHes");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_Modules_PhanHeId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "PhanHeId",
                table: "Modules");
        }
    }
}
