using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.TanAn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DaDuyet",
                table: "HoSoKhaiSinhs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDuyet",
                table: "HoSoKhaiSinhs",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiDuyet",
                table: "HoSoKhaiSinhs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "NguoiDuyetId",
                table: "HoSoKhaiSinhs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKhaiSinhs_NguoiDuyetId",
                table: "HoSoKhaiSinhs",
                column: "NguoiDuyetId");

            migrationBuilder.AddForeignKey(
                name: "FK_HoSoKhaiSinhs_Users_NguoiDuyetId",
                table: "HoSoKhaiSinhs",
                column: "NguoiDuyetId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HoSoKhaiSinhs_Users_NguoiDuyetId",
                table: "HoSoKhaiSinhs");

            migrationBuilder.DropIndex(
                name: "IX_HoSoKhaiSinhs_NguoiDuyetId",
                table: "HoSoKhaiSinhs");

            migrationBuilder.DropColumn(
                name: "DaDuyet",
                table: "HoSoKhaiSinhs");

            migrationBuilder.DropColumn(
                name: "NgayDuyet",
                table: "HoSoKhaiSinhs");

            migrationBuilder.DropColumn(
                name: "NguoiDuyet",
                table: "HoSoKhaiSinhs");

            migrationBuilder.DropColumn(
                name: "NguoiDuyetId",
                table: "HoSoKhaiSinhs");
        }
    }
}
