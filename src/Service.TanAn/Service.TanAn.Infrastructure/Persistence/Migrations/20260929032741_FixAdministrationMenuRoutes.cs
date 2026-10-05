using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Service.TanAn.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixAdministrationMenuRoutes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Modules"
                SET "LienKet" = CASE "LienKet"
                    WHEN '/quan-tri-he-thong/danh-muc/users' THEN '/quan-tri-he-thong/nguoi-dung'
                    WHEN '/quan-tri-he-thong/danh-muc/groups' THEN '/quan-tri-he-thong/don-vi-he-thong'
                    WHEN '/quan-tri-he-thong/danh-muc/roles' THEN '/quan-tri-he-thong/vai-tro'
                    WHEN '/quan-tri-he-thong/danh-muc/parameters' THEN '/quan-ly-tham-so-ht'
                    WHEN '/quan-tri-he-thong/danh-muc/modules' THEN '/quan-tri-he-thong/module'
                    WHEN '/quan-tri-he-thong/danh-muc/menus' THEN '/quan-tri-he-thong/quan-tri-menu'
                    ELSE "LienKet"
                END
                WHERE "LienKet" IN (
                    '/quan-tri-he-thong/danh-muc/users',
                    '/quan-tri-he-thong/danh-muc/groups',
                    '/quan-tri-he-thong/danh-muc/roles',
                    '/quan-tri-he-thong/danh-muc/parameters',
                    '/quan-tri-he-thong/danh-muc/modules',
                    '/quan-tri-he-thong/danh-muc/menus'
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Modules"
                SET "LienKet" = CASE "LienKet"
                    WHEN '/quan-tri-he-thong/nguoi-dung' THEN '/quan-tri-he-thong/danh-muc/users'
                    WHEN '/quan-tri-he-thong/don-vi-he-thong' THEN '/quan-tri-he-thong/danh-muc/groups'
                    WHEN '/quan-tri-he-thong/vai-tro' THEN '/quan-tri-he-thong/danh-muc/roles'
                    WHEN '/quan-ly-tham-so-ht' THEN '/quan-tri-he-thong/danh-muc/parameters'
                    WHEN '/quan-tri-he-thong/module' THEN '/quan-tri-he-thong/danh-muc/modules'
                    WHEN '/quan-tri-he-thong/quan-tri-menu' THEN '/quan-tri-he-thong/danh-muc/menus'
                    ELSE "LienKet"
                END
                WHERE "LienKet" IN (
                    '/quan-tri-he-thong/nguoi-dung',
                    '/quan-tri-he-thong/don-vi-he-thong',
                    '/quan-tri-he-thong/vai-tro',
                    '/quan-ly-tham-so-ht',
                    '/quan-tri-he-thong/module',
                    '/quan-tri-he-thong/quan-tri-menu'
                );
                """);
        }
    }
}
