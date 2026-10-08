using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.TanAn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LinkLocalityToUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "ApThons",
                type: "uuid",
                nullable: true);

            // Match only unique unit codes; never infer ownership from display names.
            migrationBuilder.Sql("""
                UPDATE "ApThons" AS a SET "GroupId" = g."Id"
                FROM "Groups" AS g
                WHERE a."Ma" = g."GroupCode" AND g."UnitType" = 0
                  AND (SELECT count(*) FROM "Groups" x WHERE x."GroupCode" = a."Ma") = 1;
                UPDATE "HoGiaDinhs" AS h SET "ApThonId" = a."Id"
                FROM "ApThons" AS a
                WHERE h."ApThonId" IS NULL AND h."ApThon" = a."Ten"
                  AND (SELECT count(*) FROM "ApThons" x WHERE x."Ten" = h."ApThon") = 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ApThons_GroupId",
                table: "ApThons",
                column: "GroupId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ApThons_Groups_GroupId",
                table: "ApThons",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApThons_Groups_GroupId",
                table: "ApThons");

            migrationBuilder.DropIndex(
                name: "IX_ApThons_GroupId",
                table: "ApThons");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "ApThons");
        }
    }
}
