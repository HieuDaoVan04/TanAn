using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.TanAn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseBirthModerationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModerationStatus",
                table: "HoSoKhaiSinhs",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Giữ trạng thái duyệt đang có trước khi bỏ cột bool.
            migrationBuilder.Sql("""
                UPDATE "HoSoKhaiSinhs"
                SET "ModerationStatus" = CASE WHEN "DaDuyet" THEN 0 ELSE 1 END;
                """);

            migrationBuilder.DropColumn(
                name: "DaDuyet",
                table: "HoSoKhaiSinhs");

            migrationBuilder.AddCheckConstraint(
                name: "CK_HoSoKhaiSinhs_ModerationStatus",
                table: "HoSoKhaiSinhs",
                sql: "\"ModerationStatus\" IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_HoSoKhaiSinhs_ModerationStatus",
                table: "HoSoKhaiSinhs");

            migrationBuilder.AddColumn<bool>(
                name: "DaDuyet",
                table: "HoSoKhaiSinhs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE "HoSoKhaiSinhs"
                SET "DaDuyet" = ("ModerationStatus" = 0);
                """);

            migrationBuilder.DropColumn(
                name: "ModerationStatus",
                table: "HoSoKhaiSinhs");
        }
    }
}
