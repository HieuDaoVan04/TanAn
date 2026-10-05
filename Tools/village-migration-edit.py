from pathlib import Path
p=next(Path('src/Service.TanAn/Service.TanAn.Infrastructure/Persistence/Migrations').glob('*_VillageAdministrationScope.cs'))
s=p.read_text(encoding='utf-8-sig');needle='        }\n\n        /// <inheritdoc />\n        protected override void Down'
assert needle in s
sql='''            migrationBuilder.Sql("""
                INSERT INTO "Xas" ("Id","Ma","Ten","NgayTao")
                SELECT 'b1463302-d74c-4a98-ab01-7772ef157270'::uuid, 'TA-XA', 'Xã Tân An', CURRENT_TIMESTAMP
                WHERE NOT EXISTS (SELECT 1 FROM "Xas" WHERE "Ma"='TA-XA');
                UPDATE "ApThons" SET "XaId"=(SELECT "Id" FROM "Xas" WHERE "Ma"='TA-XA')
                WHERE "Ma" LIKE 'TA-THON-%' AND "XaId" IS NULL;
                INSERT INTO "Groups" ("Id","GroupCode","GroupName","Description","IsActive","CreatedDate","UnitType")
                SELECT '1a9de9fb-cd74-449c-9fd8-079a21b03d22'::uuid, 'TA-XA', 'UBND xã Tân An', 'Đơn vị cấp xã quản lý các thôn', TRUE, CURRENT_TIMESTAMP, 0
                WHERE NOT EXISTS (SELECT 1 FROM "Groups" WHERE "GroupCode"='TA-XA');
                UPDATE "Groups" SET "ParentId"=(SELECT "Id" FROM "Groups" WHERE "GroupCode"='TA-XA')
                WHERE "GroupCode" LIKE 'TA-THON-%' AND "ParentId" IS NULL;
                UPDATE "YeuCauNguoiDans" AS y SET "ApThonId"=h."ApThonId"
                FROM "NhanKhaus" n JOIN "HoGiaDinhs" h ON n."MaHoGiaDinh"=h."Id"
                WHERE y."ApThonId" IS NULL AND y."CCCDNguoiYeuCau"<>'' AND y."CCCDNguoiYeuCau"=n."CCCD";
                """);
'''
s=s.replace(needle,sql+needle);p.write_text(s,encoding='utf-8')
p=Path('Tools/DemoPopulation/Program.cs');s=p.read_text(encoding='utf-8');s=s.replace('var linkOnly = args.Contains("--link-units");','var villageScope = args.Contains("--village-scope");\nvar linkOnly = args.Contains("--link-units") || villageScope;');s=s.replace('!x.EndsWith("_LinkLocalityToUnit")','!x.EndsWith(villageScope ? "_VillageAdministrationScope" : "_LinkLocalityToUnit")');s=s.replace('    Console.WriteLine($"PASS: {linked}', '    if(villageScope && await db.ApThons.CountAsync(x=>x.Ma.StartsWith("TA-THON-") && x.XaId!=null)!=11) throw new Exception("Commune hierarchy check failed.");\n    Console.WriteLine($"PASS: {linked}');p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/HoKhau/Index.razor');s=p.read_text(encoding='utf-8-sig').replace('Items="@householdsQuery"','@ref="grid" ItemsProvider="LoadHouseholds"').replace('Sortable="true"','Sortable="false"');p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/HoKhau/Index.razor.cs');s=p.read_text(encoding='utf-8-sig');start=s.index('    private async Task LoadData()');end=s.index('    private async Task ClearSearch()',start)
s=s[:start]+'''    private FluentDataGrid<HoGiaDinhDto>? grid;
    private async ValueTask<GridItemsProviderResult<HoGiaDinhDto>> LoadHouseholds(GridItemsProviderRequest<HoGiaDinhDto> request)
    {
        var size=request.Count ?? pagination.ItemsPerPage;
        string? ap = selectedApThon == TanAnLocalities.All || string.IsNullOrWhiteSpace(selectedApThon) ? null : selectedApThon;
        using var scope=Scopes.CreateScope();
        var result=await scope.ServiceProvider.GetRequiredService<IPopulationService>().GetHoGiaDinhsAsync(searchKeyword,ap,request.StartIndex/size+1,size);
        var items=result.Data?.Items ?? new(); householdsQuery=items.AsQueryable();
        return GridItemsProviderResult.From(items, result.Data?.TotalCount ?? 0);
    }
    private async Task LoadData()
    {
        await pagination.SetCurrentPageIndexAsync(0);
        if(grid!=null) await grid.RefreshDataAsync();
    }

'''+s[end:];p.write_text(s,encoding='utf-8')
