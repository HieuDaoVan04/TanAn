from pathlib import Path
def edit(p,a,b):
 p=Path(p);s=p.read_text(encoding='utf-8-sig');assert a in s,p;p.write_text(s.replace(a,b),encoding='utf-8')
p='src/Service.TanAn/Service.TanAn.Application/Interfaces/IPopulationService.cs'
edit(p,'        Task<ApiResult<HoGiaDinhDto>> GetHoGiaDinhByIdAsync','        Task SetChuHoAsync(Guid householdId, Guid residentId, string username);\n        Task<ApiResult<HoGiaDinhDto>> GetHoGiaDinhByIdAsync')
p='src/Service.TanAn/Service.TanAn.Application/Services/PopulationService.cs'
edit(p,'        public async Task<ApiResult<HoGiaDinhDto>> GetHoGiaDinhByIdAsync','''        public async Task SetChuHoAsync(Guid householdId, Guid residentId, string username)
        {
            var context = (DbContext)_db;
            await using var transaction = await context.Database.BeginTransactionAsync();
            var house = await _db.HoGiaDinhs.SingleOrDefaultAsync(x=>x.Id==householdId) ?? throw new UnauthorizedAccessException("Không có quyền quản lý hộ này.");
            var resident = await _db.NhanKhaus.SingleOrDefaultAsync(x=>x.Id==residentId && x.MaHoGiaDinh==householdId) ?? throw new ArgumentException("Chủ hộ phải là thành viên của hộ.");
            var active = await _db.ThanhVienHos.Where(x=>x.HoGiaDinhId==householdId && x.DenNgay==null && (x.LaChuHo || x.NhanKhauId==residentId)).ToListAsync();
            if(active.Any(x=>x.LaChuHo && x.NhanKhauId==residentId)) return;
            var today = DateOnly.FromDateTime(DateTime.Today);
            if(active.Any(x=>x.TuNgay>today)) throw new InvalidOperationException("Ngày hiệu lực thành viên đang nằm trong tương lai.");
            foreach(var item in active) item.DenNgay=today;
            await _db.SaveChangesAsync(); // close old head before inserting the new unique active head
            foreach(var old in active.Where(x=>x.LaChuHo && x.NhanKhauId!=residentId))
            {
                var prior = await _db.NhanKhaus.SingleAsync(x=>x.Id==old.NhanKhauId);
                prior.QuanHeVoiChuHo="Thành viên";
                _db.ThanhVienHos.Add(new ThanhVienHo {HoGiaDinhId=householdId,NhanKhauId=prior.Id,QuanHeVoiChuHo="Thành viên",TuNgay=today});
            }
            resident.QuanHeVoiChuHo="Chủ hộ";
            house.TenChuHo=resident.HoTen; house.CCCDChuHo=resident.CCCD;
            _db.ThanhVienHos.Add(new ThanhVienHo {HoGiaDinhId=householdId,NhanKhauId=residentId,QuanHeVoiChuHo="Chủ hộ",LaChuHo=true,TuNgay=today});
            await _db.SaveChangesAsync();
            await _auditLog.LogAsync(username,"Thay đổi chủ hộ","HoGiaDinh",householdId.ToString(),null,residentId.ToString());
            await transaction.CommitAsync();
        }

        public async Task<ApiResult<HoGiaDinhDto>> GetHoGiaDinhByIdAsync''')
p='src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/HoKhau/Index.razor.cs'
edit(p,'    private HoGiaDinhDto? detail;','''    private HoGiaDinhDto? detail;
    private Guid selectedHead;
    private bool headBusy;
    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] public IUserService Users { get; set; } = default!;
    private async Task SaveHead()
    {
        if(detail == null || selectedHead == Guid.Empty || headBusy) return;
        headBusy=true;
        try {
            var user=await Users.GetCurrentUserAsync();
            if(!user.IsAuthenticated) throw new UnauthorizedAccessException("Hãy đăng nhập lại.");
            using var scope=Scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IPopulationService>().SetChuHoAsync(detail.Id,selectedHead,user.UserName);
            await View(detail); await LoadData();
        } catch(Exception ex) { await DialogService.ShowErrorAsync(ex.Message); }
        finally {headBusy=false;}
    }''')
edit(p,'if (result.Success) detail = result.Data;','if (result.Success) { detail = result.Data; selectedHead = detail?.ThanhVien?.FirstOrDefault(x=>x.QuanHeVoiChuHo=="Chủ hộ")?.Id ?? Guid.Empty; }')
p='src/Service.UI/Service.UI.CMS.Blazor/Components/Pages/HoKhau/Index.razor'
edit(p,'        <h4>Thành viên trong hộ</h4>','''        <h4>Thành viên trong hộ</h4>
        <label>Chủ hộ<select @bind="selectedHead" disabled="headBusy"><option value="@Guid.Empty">— Chọn thành viên —</option>@foreach(var member in detail.ThanhVien ?? new()) { <option value="@member.Id">@member.HoTen — @member.CCCD</option> }</select></label>
        <FluentButton OnClick="SaveHead" Disabled="@(headBusy || selectedHead == Guid.Empty)">Lưu chủ hộ</FluentButton>''')
