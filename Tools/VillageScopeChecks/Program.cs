using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Service.TanAn.Infrastructure.Persistence;
using Service.TanAn.Infrastructure.Services;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Application.Services;
await using var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
var options = new DbContextOptionsBuilder<TanAnDbContext>().UseSqlite(connection).Options;
var user = new User {UserName="village-test",Role=RoleEnum.CanBoThon};
var a = new ApThon {Ma="A",Ten="Thôn A"}; var b = new ApThon {Ma="B",Ten="Thôn B"};
var h1 = new HoGiaDinh {MaSoHo="H1",DiaBan=a}; var h2 = new HoGiaDinh {MaSoHo="H2",DiaBan=b};
var n1 = new NhanKhau {HoGiaDinh=h1,HoTen="Nguyễn Văn An",CCCD="TEST1"};
var n2 = new NhanKhau {HoGiaDinh=h2,HoTen="Trần Thị Bình",CCCD="TEST2"};
await using(var seed = new TanAnDbContext(options)) {
 await seed.Database.EnsureCreatedAsync(); seed.Users.Add(user);seed.NhanKhaus.AddRange(n1,n2);
 seed.PhuTrachThons.Add(new(){User=user,Thon=a});
 seed.YeuCauNguoiDans.AddRange(new(){MaYeuCau="Y1",Thon=a},new(){MaYeuCau="Y2",Thon=b});
 seed.DoiTuongAnSinhs.AddRange(new(){NhanKhau=n1},new(){NhanKhau=n2}); await seed.SaveChangesAsync();
}
var actor = new DataActor(new HttpContextAccessor());
actor.Principal = new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())},"test"));
await using(var db = new TanAnDbContext(options,actor)) {
 if(await db.HoGiaDinhs.CountAsync()!=1 || await db.NhanKhaus.CountAsync()!=1 || await db.DoiTuongAnSinhs.CountAsync()!=1 || await db.YeuCauNguoiDans.CountAsync()!=1) throw new Exception("Cross-village read leak");
 if(await db.HoGiaDinhs.SingleOrDefaultAsync(x=>x.Id==h2.Id)!=null) throw new Exception("ID bypass");
 db.HoGiaDinhs.Add(new(){MaSoHo="BAD",ApThonId=b.Id});
 try {await db.SaveChangesAsync();throw new Exception("Cross-village insert allowed");}catch(UnauthorizedAccessException) {db.ChangeTracker.Clear();}
 db.Attach(new HoGiaDinh {Id=h2.Id,ApThonId=a.Id,MaSoHo="H2"}).State=EntityState.Modified;
 try {await db.SaveChangesAsync();throw new Exception("Attached update bypass");}catch(UnauthorizedAccessException) {db.ChangeTracker.Clear();}
 var own = await db.HoGiaDinhs.SingleAsync();own.DiaChi="Updated";await db.SaveChangesAsync();
 var service = new PopulationService(db,new AuditLogService(db));
 var created = await service.CreateHoGiaDinhAsync(new(){MaSoHo="NEW",TenChuHo="Nguyễn Văn Nam",CCCDChuHo="TEST3",ApThon=a.Ten,DiaChi="Thôn A",NgaySinhChuHo=new DateTime(1980,1,1)},user.UserName);
 if(!created.Success) throw new Exception(created.Message);
 var newId=created.Data!.Id;
 if(await db.ThanhVienHos.CountAsync(x=>x.HoGiaDinhId==newId && x.LaChuHo && x.DenNgay==null)!=1) throw new Exception("New head missing");
 var member=await service.CreateNhanKhauAsync(new(){MaHoGiaDinh=newId,HoTen="Trần Thị Mai",CCCD="TEST4",NgaySinh=new DateTime(1985,1,1),QuanHeVoiChuHo="Vợ"},user.UserName);
 if(!member.Success) throw new Exception(member.Message);
 await service.SetChuHoAsync(newId,member.Data!.Id,user.UserName);
 if(await db.ThanhVienHos.CountAsync(x=>x.HoGiaDinhId==newId && x.LaChuHo && x.DenNgay==null)!=1 || (await db.HoGiaDinhs.SingleAsync(x=>x.Id==newId)).TenChuHo!="Trần Thị Mai") throw new Exception("Head change inconsistent");
 try {await service.SetChuHoAsync(newId,n2.Id,user.UserName);throw new Exception("Cross-house head allowed");}catch(ArgumentException) {}
 var requests = new CitizenRequestService(db,new AuditLogService(db));
 var request=await requests.CreateYeuCauAsync(new(){ApThonId=a.Id,HoTenNguoiYeuCau="Test",CCCDNguoiYeuCau="TEST1",LoaiYeuCau="Test",NoiDung="Test"});
 if(!request.Success || await db.ThongBaoThons.CountAsync()!=1) throw new Exception("Missing scoped notification");
 var notice=await db.ThongBaoThons.SingleAsync();notice.DaDocLuc=DateTime.UtcNow;await db.SaveChangesAsync();
 await using(var admin = new TanAnDbContext(options)) { admin.PhuTrachThons.Remove(await admin.PhuTrachThons.SingleAsync());await admin.SaveChangesAsync(); }
 if(await db.HoGiaDinhs.CountAsync()!=0) throw new Exception("Revocation failed");
 own.DiaChi="No longer allowed";
 try {await db.SaveChangesAsync();throw new Exception("Revoked tracked update allowed");}catch(UnauthorizedAccessException) {}
}
Console.WriteLine("PASS: scoped reads across population/welfare/requests, ID lookup, insert/update guards, own write and immediate revocation.");
