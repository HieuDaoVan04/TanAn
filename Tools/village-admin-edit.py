from pathlib import Path
def edit(p,a,b):
 p=Path(p);s=p.read_text(encoding='utf-8-sig');assert a in s,p;p.write_text(s.replace(a,b),encoding='utf-8')
edit('src/Service.Shared/Service.Shared.Contracts/DTOs/AdministrationDtos.cs','    public List<Guid> AssignedIds','    public Service.TanAn.Domain.Enums.RoleEnum AccountRole { get; set; } = Service.TanAn.Domain.Enums.RoleEnum.CanBoXa;\n    public List<Guid> VillageIds { get; set; } = new();\n    public List<Guid> AssignedIds')
p='src/Service.TanAn/Service.TanAn.Application/Services/Core/AdministrationService.cs'
edit(p,'Phone = x.PhoneNumber, ModerationStatus','Phone = x.PhoneNumber, AccountRole = x.Role, VillageIds = db.PhuTrachThons.Where(p => p.UserId == x.Id).Select(p => p.ApThonId).ToList(), ModerationStatus')
edit(p,'                var old = await db.UserRoles.Where(r => r.UserId == id).ToListAsync();','''                if (!Enum.IsDefined(form.AccountRole)) throw new ArgumentException("Vai trò tài khoản không hợp lệ.");
                if (id == actor.UserId && form.AccountRole != x.Role) throw new ArgumentException("Không được tự thay đổi cấp quyền của tài khoản đang đăng nhập.");
                var villages = form.VillageIds.Distinct().ToList();
                if (form.AccountRole != Service.TanAn.Domain.Enums.RoleEnum.CanBoThon) villages.Clear();
                if (form.AccountRole == Service.TanAn.Domain.Enums.RoleEnum.CanBoThon && villages.Count == 0) throw new ArgumentException("Hãy chọn ít nhất một thôn phụ trách.");
                if (await db.ApThons.CountAsync(t => villages.Contains(t.Id) && t.DangHoatDong && t.XaId != null) != villages.Count) throw new ArgumentException("Thôn không tồn tại, chưa thuộc xã hoặc chưa duyệt.");
                x.Role = form.AccountRole;
                var oldVillages = await db.PhuTrachThons.Where(p => p.UserId == id).ToListAsync();
                db.PhuTrachThons.RemoveRange(oldVillages.Where(p => !villages.Contains(p.ApThonId)));
                foreach (var villageId in villages.Except(oldVillages.Select(p => p.ApThonId)))
                {
                    db.PhuTrachThons.Add(new PhuTrachThon { UserId = id, ApThonId = villageId });
                    db.ThongBaoThons.Add(new ThongBaoThon { NguoiNhanId = id, ApThonId = villageId, TieuDe = "Phân công phụ trách thôn", NoiDung = "Bạn được giao quản lý dân cư, an sinh và hồ sơ trong thôn này.", NguoiTaoId = actor.UserId });
                }
                var old = await db.UserRoles.Where(r => r.UserId == id).ToListAsync();''')
p='src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/QuanTriHeThong/DanhMuc/Index.razor.cs'
edit(p,'    private List<AdminRecord> roleOptions = new();','    private List<AdminRecord> roleOptions = new();\n    private List<Service.TanAn.Domain.Entities.ApThon> villageOptions = new();\n    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;')
edit(p,'            form = JsonSerializer.Deserialize<AdminRecord>', '''            if (kind == AdminCatalog.Users) {
                using var scope = Scopes.CreateScope();
                villageOptions = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(scope.ServiceProvider.GetRequiredService<ITanAnDbContext>().ApThons.Where(x=>x.DangHoatDong && x.XaId != null).OrderBy(x=>x.Ten));
            }
            form = JsonSerializer.Deserialize<AdminRecord>''')
edit(p,'    private void SetAssignment(', '    private void SetVillage(Guid id, bool selected) { if(selected && !form!.VillageIds.Contains(id)) form.VillageIds.Add(id); else if(!selected) form!.VillageIds.Remove(id); }\n    private void SetAssignment(')
p='src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/QuanTriHeThong/DanhMuc/Index.razor'
edit(p,'                    @if (kind != AdminCatalog.Parameters) { <label>Tên', '''                    @if (kind == AdminCatalog.Users)
                    {
                        <label>Cấp quyền<InputSelect @bind-Value="form.AccountRole"><option value="Admin">Quản trị xã</option><option value="CanBoXa">Cán bộ xã</option><option value="CanBoThon">Phụ trách thôn</option><option value="NguoiDan">Người dân</option></InputSelect></label>
                        @if (form.AccountRole == Service.TanAn.Domain.Enums.RoleEnum.CanBoThon)
                        {
                            <div class="full-width"><strong>Thôn được giao quản lý</strong><p>Có quyền nghiệp vụ dân cư, an sinh và hồ sơ trong các thôn được chọn.</p>
                            @foreach(var village in villageOptions) { <label><input type="checkbox" checked="@form.VillageIds.Contains(village.Id)" @onchange="e => SetVillage(village.Id, (bool)e.Value!)" />@village.Ten</label> }
                            </div>
                        }
                    }
                    @if (kind != AdminCatalog.Parameters) { <label>Tên''')
p='src/Service.Shared/Service.Shared.Commons/Models/CurrentUserDto.cs'
edit(p,'        public string ApThon {','        public List<Guid> VillageIds { get; set; } = new();\n        public string ApThon {')
p='src/Service.UI/Service.UI.CMS.Blazor/Applications/AccountService.cs'
edit(p,'        if (user.Role != RoleEnum.Admin)','''        var villageIds = await db.PhuTrachThons.Where(p => p.UserId == user.Id && p.Thon.DangHoatDong).Select(p => p.ApThonId).ToListAsync();
        if (user.Role == RoleEnum.CanBoThon)
        {
            var paths = new[] { "/ban-lam-viec", "/ho-khau", "/bien-dong", "/an-sinh", "/an-sinh/ho-ngheo", "/an-sinh/can-ngheo", "/an-sinh/nguoi-cao-tuoi", "/dich-vu-cong" };
            menus = menus.Where(x => villageIds.Count > 0 && paths.Contains(x.LienKet)).ToList();
        }
        else if (user.Role != RoleEnum.Admin)''')
edit(p,'Role = user.Role.ToString(), IsAuthenticated','VillageIds = villageIds, Role = user.Role.ToString(), IsAuthenticated')
