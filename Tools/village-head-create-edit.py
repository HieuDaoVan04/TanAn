from pathlib import Path
def edit(p,a,b):
 p=Path(p);s=p.read_text(encoding='utf-8-sig');assert a in s,p;p.write_text(s.replace(a,b),encoding='utf-8')
p='src/Service.Shared/Service.Shared.Contracts/DTOs/PopulationDtos.cs'
edit(p,'    public class CreateHoGiaDinhForm\n    {','    public class CreateHoGiaDinhForm\n    {\n        public DateTime? NgaySinhChuHo { get; set; }\n        public GioiTinhEnum GioiTinhChuHo { get; set; } = GioiTinhEnum.Nam;')
p='src/Service.TanAn/Service.TanAn.Application/Services/PopulationService.cs'
edit(p,'            var ho = new HoGiaDinh','''            if (form.NgaySinhChuHo == null || form.NgaySinhChuHo > DateTime.Today || form.NgaySinhChuHo < new DateTime(1900,1,1)) return ApiResult<HoGiaDinhDto>.Fail("Nhập ngày sinh hợp lệ của chủ hộ.");
            var villageId = await _db.ApThons.Where(x=>x.Ten==form.ApThon && x.DangHoatDong).Select(x=>(Guid?)x.Id).SingleOrDefaultAsync();
            if(villageId == null) return ApiResult<HoGiaDinhDto>.Fail("Hãy chọn thôn trong danh mục.");
            var ho = new HoGiaDinh''')
edit(p,'ApThonId = await _db.ApThons.Where(x => x.Ten == form.ApThon).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(),','ApThonId = villageId,')
edit(p,'            await _db.HoGiaDinhs.AddAsync(ho);','''            var head = new NhanKhau {HoGiaDinh=ho,HoTen=form.TenChuHo,CCCD=form.CCCDChuHo,NgaySinh=DateTime.SpecifyKind(form.NgaySinhChuHo.Value,DateTimeKind.Utc),GioiTinh=form.GioiTinhChuHo,QuanHeVoiChuHo="Chủ hộ",ThuongTru=form.DiaChi};
            _db.NhanKhaus.Add(head);
            _db.ThanhVienHos.Add(new ThanhVienHo {HoGiaDinh=ho,NhanKhau=head,LaChuHo=true,QuanHeVoiChuHo="Chủ hộ",TuNgay=DateOnly.FromDateTime(DateTime.Today)});
            await _db.HoGiaDinhs.AddAsync(ho);''')
edit(p,'            var nk = new NhanKhau','''            if(!await _db.HoGiaDinhs.AnyAsync(x=>x.Id==form.MaHoGiaDinh)) return ApiResult<NhanKhauDto>.Fail("Hộ không tồn tại hoặc ngoài thôn được giao.");
            if(form.QuanHeVoiChuHo=="Chủ hộ") return ApiResult<NhanKhauDto>.Fail("Thêm thành viên trước rồi dùng chức năng chọn chủ hộ.");
            var nk = new NhanKhau''')
edit(p,'            await _db.NhanKhaus.AddAsync(nk);','''            await _db.NhanKhaus.AddAsync(nk);
            _db.ThanhVienHos.Add(new ThanhVienHo {HoGiaDinhId=form.MaHoGiaDinh,NhanKhau=nk,QuanHeVoiChuHo=form.QuanHeVoiChuHo,TuNgay=DateOnly.FromDateTime(DateTime.Today)});''')
edit(p,'            nk.HoTen = form.HoTen;','''            if(nk.QuanHeVoiChuHo != form.QuanHeVoiChuHo && (nk.QuanHeVoiChuHo=="Chủ hộ" || form.QuanHeVoiChuHo=="Chủ hộ")) return ApiResult<NhanKhauDto>.Fail("Dùng chức năng chọn chủ hộ để thay đổi chủ hộ.");
            var house = await _db.HoGiaDinhs.SingleAsync(x=>x.Id==nk.MaHoGiaDinh);
            if(await _db.ThanhVienHos.AnyAsync(x=>x.NhanKhauId==nk.Id && x.DenNgay==null && x.LaChuHo)) { house.TenChuHo=form.HoTen;house.CCCDChuHo=form.CCCD; }
            nk.HoTen = form.HoTen;''')
p='src/Service.TanAn/Service.TanAn.Infrastructure/Persistence/TanAnDbContext.Scope.cs'
edit(p,'&& await NhanKhaus.AsNoTracking().AnyAsync(n=>n.Id==personId && n.MaHoGiaDinh==houseId,ct),','&& (await NhanKhaus.AsNoTracking().AnyAsync(n=>n.Id==personId && n.MaHoGiaDinh==houseId,ct) || ChangeTracker.Entries<NhanKhau>().Any(e=>e.State==EntityState.Added && e.Entity.Id==personId && e.Entity.MaHoGiaDinh==houseId)),')
p='src/Service.UI/Service.UI.CMS.Blazor/Components/Shared/BusinessCreateDialog.razor'
edit(p,'            <label>CCCD chủ hộ *','            <label>Ngày sinh chủ hộ *<InputDate @bind-Value="household.NgaySinhChuHo" required /></label>\n            <label>Giới tính chủ hộ<InputSelect @bind-Value="household.GioiTinhChuHo">@foreach(var gender in Enum.GetValues<GioiTinhEnum>()) { <option value="@gender">@gender</option> }</InputSelect></label>\n            <label>CCCD chủ hộ *')
