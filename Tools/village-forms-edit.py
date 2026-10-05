from pathlib import Path
def edit(p,a,b):
 p=Path(p);s=p.read_text(encoding='utf-8-sig');assert a in s,p;p.write_text(s.replace(a,b),encoding='utf-8')
p='src/Service.Shared/Service.Shared.Contracts/DTOs/CitizenRequestDtos.cs'
edit(p,'    public class CreateYeuCauForm\n    {','    public class CreateYeuCauForm\n    {\n        public Guid? ApThonId { get; set; }')
p='src/Service.TanAn/Service.TanAn.Application/Services/CitizenRequestService.cs'
edit(p,'            var req = new YeuCauNguoiDan','''            if (!form.ApThonId.HasValue || !await _db.ApThons.AnyAsync(x=>x.Id==form.ApThonId && x.DangHoatDong)) return ApiResult<YeuCauDto>.Fail("Hãy chọn thôn tiếp nhận hồ sơ.");
            var req = new YeuCauNguoiDan''')
edit(p,'                MaYeuCau = $"YC{DateTime.Now:yyyyMMddHHmmss}",','                ApThonId = form.ApThonId,\n                MaYeuCau = $"YC{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..8]}",')
edit(p,'            await _db.YeuCauNguoiDans.AddAsync(req);','''            await _db.YeuCauNguoiDans.AddAsync(req);
            var recipients = await _db.PhuTrachThons.Where(p=>p.ApThonId==form.ApThonId && p.User.Role==RoleEnum.CanBoThon && p.User.ModerationStatus==Service.Shared.Commons.Model.SQL.ModerationStatus.Approved).Select(p=>p.UserId).ToListAsync();
            foreach(var recipient in recipients) _db.ThongBaoThons.Add(new ThongBaoThon {NguoiNhanId=recipient,ApThonId=form.ApThonId.Value,TieuDe="Hồ sơ mới cần xử lý",NoiDung=$"Hồ sơ {req.MaYeuCau}: {req.LoaiYeuCau}."});''')
p='src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/DichVuCong/Index.razor.cs'
edit(p,'    private void OpenCreate()','''    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
    private List<Service.TanAn.Domain.Entities.ApThon> villages = new();
    private async Task OpenCreate()''')
edit(p,'        form = new() { LoaiYeuCau = loaiList[0] };','''        using var scope = Scopes.CreateScope();
        var user = await Users.GetCurrentUserAsync();
        var query = scope.ServiceProvider.GetRequiredService<ITanAnDbContext>().ApThons.Where(x=>x.DangHoatDong);
        if(user.Role == "CanBoThon") query = query.Where(x=>user.VillageIds.Contains(x.Id));
        villages = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query.OrderBy(x=>x.Ten));
        form = new() { LoaiYeuCau = loaiList[0], ApThonId = villages.Count == 1 ? villages[0].Id : null };''')
p='src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/DichVuCong/Index.razor'
edit(p,'                <FluentTextField Label="Họ Tên Người Nộp"','                <label>Thôn tiếp nhận *<select @bind="form.ApThonId"><option value="">— Chọn thôn —</option>@foreach(var village in villages) { <option value="@village.Id">@village.Ten</option> }</select></label>\n                <FluentTextField Label="Họ Tên Người Nộp"')
p='src/Service.UI/Service.UI.CMS.Blazor/Components/Shared/BusinessCreateDialog.razor.cs'
edit(p,'    protected override void OnInitialized()','    private List<string> villageNames = new();\n    protected override async Task OnInitializedAsync()')
edit(p,'        welfare.LoaiDoiTuong = Category','''        var user = await Users.GetCurrentUserAsync();
        using var scope = Scopes.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ITanAnDbContext>().ApThons.Where(x=>x.DangHoatDong);
        if(user.Role == "CanBoThon") query = query.Where(x=>user.VillageIds.Contains(x.Id));
        villageNames = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query.OrderBy(x=>x.Ten).Select(x=>x.Ten));
        if(villageNames.Count==1) household.ApThon = villageNames[0];
        welfare.LoaiDoiTuong = Category''')
p='src/Service.UI/Service.UI.CMS.Blazor/Components/Shared/BusinessCreateDialog.razor'
edit(p,'Service.Shared.Commons.Models.TanAnLocalities.Villages','villageNames')
