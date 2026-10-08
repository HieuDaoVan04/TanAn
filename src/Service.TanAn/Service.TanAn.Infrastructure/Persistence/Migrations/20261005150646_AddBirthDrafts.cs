using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.TanAn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBirthDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoSoKhaiSinhs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaHoSo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    HoTenTre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HoTenNguoiYeuCau = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MaSoHo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NgaySinh = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    HoGiaDinhId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApThonId = table.Column<Guid>(type: "uuid", nullable: true),
                    NoiDungJson = table.Column<string>(type: "text", nullable: false),
                    PhienBan = table.Column<int>(type: "integer", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    NguoiTaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NguoiSuaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoSoKhaiSinhs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoSoKhaiSinhs_ApThons_ApThonId",
                        column: x => x.ApThonId,
                        principalTable: "ApThons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoSoKhaiSinhs_HoGiaDinhs_HoGiaDinhId",
                        column: x => x.HoGiaDinhId,
                        principalTable: "HoGiaDinhs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HoSoKhaiSinhs_Users_NguoiTaoId",
                        column: x => x.NguoiTaoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKhaiSinhs_ApThonId_NgayTao",
                table: "HoSoKhaiSinhs",
                columns: new[] { "ApThonId", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKhaiSinhs_HoGiaDinhId",
                table: "HoSoKhaiSinhs",
                column: "HoGiaDinhId");

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKhaiSinhs_MaHoSo",
                table: "HoSoKhaiSinhs",
                column: "MaHoSo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HoSoKhaiSinhs_NguoiTaoId_NgayTao",
                table: "HoSoKhaiSinhs",
                columns: new[] { "NguoiTaoId", "NgayTao" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoSoKhaiSinhs");
        }
    }
}
