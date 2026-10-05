using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.TanAn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VillageAdministrationScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApThonId",
                table: "YeuCauNguoiDans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "XaId",
                table: "ApThons",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PhuTrachThons",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApThonId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuTrachThons", x => new { x.UserId, x.ApThonId });
                    table.ForeignKey(
                        name: "FK_PhuTrachThons_ApThons_ApThonId",
                        column: x => x.ApThonId,
                        principalTable: "ApThons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhuTrachThons_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongBaoThons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NguoiNhanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApThonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TieuDe = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NoiDung = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DaDocLuc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NgayTao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    NguoiTaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NguoiSuaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThongBaoThons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongBaoThons_ApThons_ApThonId",
                        column: x => x.ApThonId,
                        principalTable: "ApThons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThongBaoThons_Users_NguoiNhanId",
                        column: x => x.NguoiNhanId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Xas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ma = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Ten = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NgayTao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    NguoiTaoId = table.Column<Guid>(type: "uuid", nullable: true),
                    NgaySua = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    NguoiSuaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Xas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauNguoiDans_ApThonId",
                table: "YeuCauNguoiDans",
                column: "ApThonId");

            migrationBuilder.CreateIndex(
                name: "IX_ApThons_XaId",
                table: "ApThons",
                column: "XaId");

            migrationBuilder.CreateIndex(
                name: "IX_PhuTrachThons_ApThonId",
                table: "PhuTrachThons",
                column: "ApThonId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaoThons_ApThonId",
                table: "ThongBaoThons",
                column: "ApThonId");

            migrationBuilder.CreateIndex(
                name: "IX_ThongBaoThons_NguoiNhanId_NgayTao",
                table: "ThongBaoThons",
                columns: new[] { "NguoiNhanId", "NgayTao" });

            migrationBuilder.CreateIndex(
                name: "IX_Xas_Ma",
                table: "Xas",
                column: "Ma",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ApThons_Xas_XaId",
                table: "ApThons",
                column: "XaId",
                principalTable: "Xas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YeuCauNguoiDans_ApThons_ApThonId",
                table: "YeuCauNguoiDans",
                column: "ApThonId",
                principalTable: "ApThons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.Sql("""
    INSERT INTO "Xas" ("Id","Ma","Ten","NgayTao")
    SELECT
        'b1463302-d74c-4a98-ab01-7772ef157270'::uuid,
        'TA-XA',
        'Xã Tân An',
        CURRENT_TIMESTAMP
    WHERE NOT EXISTS (
        SELECT 1 FROM "Xas" WHERE "Ma" = 'TA-XA'
    );

    UPDATE "ApThons"
    SET "XaId" = (
        SELECT "Id" FROM "Xas" WHERE "Ma" = 'TA-XA'
    )
    WHERE "Ma" LIKE 'TA-THON-%'
      AND "XaId" IS NULL;

    INSERT INTO "Groups"
        ("Id","GroupCode","GroupName","Name","Description","IsActive","CreatedDate","UnitType")
    SELECT
        '1a9de9fb-cd74-449c-9fd8-079a21b03d22'::uuid,
        'TA-XA',
        'UBND xã Tân An',
        'UBND xã Tân An',
        'Đơn vị cấp xã quản lý các thôn',
        TRUE,
        CURRENT_TIMESTAMP,
        0
    WHERE NOT EXISTS (
        SELECT 1 FROM "Groups" WHERE "GroupCode" = 'TA-XA'
    );

    UPDATE "Groups"
    SET "ParentId" = (
        SELECT "Id" FROM "Groups" WHERE "GroupCode" = 'TA-XA'
    )
    WHERE "GroupCode" LIKE 'TA-THON-%'
      AND "ParentId" IS NULL;

    UPDATE "YeuCauNguoiDans" AS y
    SET "ApThonId" = h."ApThonId"
    FROM "NhanKhaus" n
    JOIN "HoGiaDinhs" h ON n."MaHoGiaDinh" = h."Id"
    WHERE y."ApThonId" IS NULL
      AND y."CCCDNguoiYeuCau" <> ''
      AND y."CCCDNguoiYeuCau" = n."CCCD";
    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApThons_Xas_XaId",
                table: "ApThons");

            migrationBuilder.DropForeignKey(
                name: "FK_YeuCauNguoiDans_ApThons_ApThonId",
                table: "YeuCauNguoiDans");

            migrationBuilder.DropTable(
                name: "PhuTrachThons");

            migrationBuilder.DropTable(
                name: "ThongBaoThons");

            migrationBuilder.DropTable(
                name: "Xas");

            migrationBuilder.DropIndex(
                name: "IX_YeuCauNguoiDans_ApThonId",
                table: "YeuCauNguoiDans");

            migrationBuilder.DropIndex(
                name: "IX_ApThons_XaId",
                table: "ApThons");

            migrationBuilder.DropColumn(
                name: "ApThonId",
                table: "YeuCauNguoiDans");

            migrationBuilder.DropColumn(
                name: "XaId",
                table: "ApThons");
        }
    }
}
