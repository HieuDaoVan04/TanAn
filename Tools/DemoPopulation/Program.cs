using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;
using Npgsql;
using Service.Shared.Commons.Models;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.TanAn.Infrastructure.Persistence;

const string marker = "DEMO-TANAN-20260926: Dữ liệu giả phục vụ đồ án, không phải dân cư thực tế.";
var counts = new[] {719,728,939,761,772,612,684,705,658,743,626};
var villages = TanAnLocalities.Villages;
var villageScope = args.Contains("--village-scope");
var linkOnly = args.Contains("--link-units") || villageScope;
var apply = args.Contains("--apply") || linkOnly;
var selfTest = args.Contains("--self-test");
if (!apply && !selfTest) { Console.WriteLine("Use --self-test or --apply <workspace>."); return; }
if(linkOnly) AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var options = new DbContextOptionsBuilder<TanAnDbContext>();
SqliteConnection? memory = null;
if (selfTest) { memory = new("Data Source=:memory:"); await memory.OpenAsync(); options.UseSqlite(memory); }
else
{
    var workspace = args.Last();
    var config = new ConfigurationBuilder().SetBasePath(Path.Combine(workspace,"src/Service.UI/Service.UI.CMS.Blazor"))
        .AddJsonFile("appsettings.json").AddJsonFile("appsettings.Development.json",optional:true)
        .AddUserSecrets("449971bb-141b-4bb9-a0ab-bb1e91a1b220").Build();
    var connection = config.GetConnectionString("DefaultConnection") ?? throw new Exception("Missing DB configuration.");
    var target = new NpgsqlConnectionStringBuilder(connection);
    if (!target.Host.EndsWith(".neon.tech",StringComparison.OrdinalIgnoreCase)) throw new Exception("Refusing non-personal-Neon target.");
    options.UseNpgsql(connection, o => o.CommandTimeout(180));
    Console.WriteLine("Target verified: configured personal Neon database (credentials hidden).");
}
await using var db = new TanAnDbContext(options.Options);
if(selfTest) await db.Database.EnsureCreatedAsync();
if(linkOnly)
{
    var beforeHomes = await db.HoGiaDinhs.CountAsync();
    var beforePeople = await db.NhanKhaus.CountAsync();
    var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
    if(pending.Any(x => !x.EndsWith(villageScope ? "_VillageAdministrationScope" : "_LinkLocalityToUnit"))) throw new Exception("Unrelated pending migrations; refusing to apply.");
    await db.Database.MigrateAsync();
    var linked = await db.ApThons.CountAsync(x => x.GroupId != null && x.Ma.StartsWith("TA-THON-"));
    var homes = await db.HoGiaDinhs.CountAsync(x => x.DiaBan != null && x.DiaBan.GroupId != null && x.GhiChu == marker);
    var people = await db.NhanKhaus.CountAsync(x => x.HoGiaDinh != null && x.HoGiaDinh.DiaBan != null && x.HoGiaDinh.DiaBan.GroupId != null && x.GhiChu == marker);
    if(linked != 11 || homes != 7947 || people != 31793 || beforeHomes != await db.HoGiaDinhs.CountAsync() || beforePeople != await db.NhanKhaus.CountAsync()) throw new Exception("Unit link verification failed.");
    if(villageScope && await db.ApThons.CountAsync(x=>x.Ma.StartsWith("TA-THON-") && x.XaId!=null)!=11) throw new Exception("Commune hierarchy check failed.");
    Console.WriteLine($"PASS: {linked} linked units; {homes} demo households; {people} demo residents; existing row counts preserved.");
    return;
}
await Seed();
if(selfTest)
{
    await Seed();
    var unit = await db.Groups.FirstAsync(x => x.GroupCode == "TA-THON-01");
    var service = new Service.TanAn.Application.Services.PopulationService(db, null!);
    var first = (await service.GetHoGiaDinhsAsync(null, null, 1, 2, unit.Id)).Data!;
    var second = (await service.GetHoGiaDinhsAsync(null, null, 2, 2, unit.Id)).Data!;
    var residents = (await service.GetNhanKhausAsync(null, null, 1, 20, unit.Id)).Data!;
    var empty = (await service.GetHoGiaDinhsAsync(null, null, 1, 20, Guid.NewGuid())).Data!;
    if(first.TotalCount != 3 || first.Items.Count != 2 || second.Items.Count != 1 || first.Items.Any(x => second.Items.Any(y => x.Id == y.Id)) || residents.TotalCount != 12 || empty.TotalCount != 0)
        throw new Exception("Unit filtering/pagination failed.");
    unit.GroupName = "Renamed unit"; await db.SaveChangesAsync();
    if((await service.GetHoGiaDinhsAsync(null, null, 1, 20, unit.Id)).Data!.TotalCount != 3) throw new Exception("Stable relationship failed after rename.");
    Console.WriteLine("PASS: idempotency, unit isolation, population queries, pagination, rename preserves relationships.");
}

async Task Seed()
{
    await using var transaction = await db.Database.BeginTransactionAsync();
    if(!selfTest) await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(2026092611)");
    var before = await db.HoGiaDinhs.CountAsync();
    for(int v=0;v<villages.Count;v++)
    {
        var code = $"TA-THON-{v+1:00}";
        var name = villages[v];
        var unit = await db.Groups.SingleOrDefaultAsync(x=>x.GroupCode==code || x.GroupName==name);
        if(unit==null) { unit = new() {GroupCode=code,GroupName=name,UnitType=0,Description="Đơn vị thôn theo danh sách chủ dự án cung cấp.",CreatedBy="demo-seeder"}; db.Groups.Add(unit); }
        var locality = await db.ApThons.SingleOrDefaultAsync(x=>x.Ma==code || x.Ten==name);
        if(locality==null) {locality = new() {Ma=code,Ten=name}; db.ApThons.Add(locality);}
        locality.GroupId = unit.Id;
        await db.SaveChangesAsync();
        var localityId=locality.Id;
        var prefix=$"DEMO-TA-{v+1:00}-";
        var existing=await db.HoGiaDinhs.Where(x=>x.MaSoHo.StartsWith(prefix)).ToDictionaryAsync(x=>x.MaSoHo);
        var size=selfTest?3:counts[v];
        for(int h=1;h<=size;h++)
        {
            var householdCode=$"{prefix}{h:0000}";
            if(existing.TryGetValue(householdCode,out var prior))
            {
                if(prior.GhiChu!=marker || prior.ApThonId!=localityId) throw new Exception("Demo code collision; existing data unchanged.");
                continue;
            }
            var address=$"Nhà mẫu {h}, {name}, xã Tân An, thành phố Hải Phòng";
            var ho=new HoGiaDinh {MaSoHo=householdCode,ApThon=name,ApThonId=localityId,DiaChi=address,GhiChu=marker};
            var members=3+(h%3);
            for(int m=0;m<members;m++)
            {
                var male=m==0?h%2==0:m==1?h%2!=0:(h+m)%2==0;
                var family=new[]{"Nguyễn","Trần","Lê","Phạm","Hoàng","Vũ","Đặng","Bùi","Đỗ","Hồ","Ngô","Dương","Đinh","Trịnh","Đoàn"}[(h+v+(m==1?3:0))%15];
                var given=male?new[]{"Minh","Hùng","Dũng","Tuấn","Hải","Quang","Long","Nam","Khang","Phúc","Bảo","Sơn"}[(h*7+v+m)%12]:new[]{"Lan","Hương","Mai","Hoa","Linh","Thảo","Trang","Ngọc","Hà","Hạnh","Yến","Anh"}[(h*7+v+m)%12];
                var fullName=$"{family} {(male?new[]{"Văn","Đức","Quốc","Hoàng"}[(h+m)%4]:new[]{"Thị","Thu","Ngọc","Thanh"}[(h+m)%4])} {given}";
                var id=$"DEMO{((v+1)*100000+h*10+m):00000000}";
                var birth=new DateTime(m<2?1970+h%20+(m==1?2:0):2008+(h+m)%10,1+(h+m)%12,1+(h+m)%27);
                var relation=m==0?"Chủ hộ":m==1?(male?"Chồng":"Vợ"):"Con";
                var person=new NhanKhau {HoGiaDinh=ho,HoTen=fullName,CCCD=id,NgaySinh=DateTime.SpecifyKind(birth,DateTimeKind.Utc),GioiTinh=male?GioiTinhEnum.Nam:GioiTinhEnum.Nu,QueQuan="Xã Tân An, thành phố Hải Phòng",ThuongTru=address,QuanHeVoiChuHo=relation,NgheNghiep=m<2?new[]{"Nông dân","Công nhân","Kinh doanh","Nhân viên văn phòng"}[h%4]:"Học sinh",GhiChu=marker};
                db.NhanKhaus.Add(person);
                db.ThanhVienHos.Add(new ThanhVienHo {HoGiaDinh=ho,NhanKhau=person,LaChuHo=m==0,QuanHeVoiChuHo=relation,TuNgay=new DateOnly(2025,7,1),GhiChu=marker});
                if(m==0) {ho.TenChuHo=fullName;ho.CCCDChuHo=id;}
            }
            if(h%100==0) {await db.SaveChangesAsync();db.ChangeTracker.Clear();}
        }
        await db.SaveChangesAsync(); db.ChangeTracker.Clear();
        var households=await db.HoGiaDinhs.CountAsync(x=>x.MaSoHo.StartsWith(prefix));
        var population=await db.NhanKhaus.CountAsync(x=>x.HoGiaDinh!=null && x.HoGiaDinh.MaSoHo.StartsWith(prefix));
        if(households!=size) throw new Exception("Unexpected household count; rolling back.");
        Console.WriteLine($"{name}: {households} households, {population} people.");
    }
    var homes=await db.HoGiaDinhs.CountAsync(x=>x.GhiChu==marker);
    var peopleCount=await db.NhanKhaus.CountAsync(x=>x.GhiChu==marker);
    var memberships=await db.ThanhVienHos.CountAsync(x=>x.GhiChu==marker);
    var heads=await db.ThanhVienHos.CountAsync(x=>x.GhiChu==marker && x.LaChuHo);
    if(heads!=homes || memberships!=peopleCount) throw new Exception("Membership integrity failed.");
    await transaction.CommitAsync();
    Console.WriteLine($"COMMITTED: {homes} demo households; {peopleCount} demo people; {heads} heads; newly added households: {await db.HoGiaDinhs.CountAsync()-before}.");
}

