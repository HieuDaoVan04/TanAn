using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Enums;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Model.SQL;
using Service.UI.CMS.Blazor.Applications;
namespace Service.UI.CMS.Blazor.Components.Shared;
public partial class VillageNotifications
{
    [Parameter] public bool AllowSend { get; set; } = true;
    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] public IUserService Users { get; set; } = default!;
    [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; } = default!;
    private CurrentUserDto user = new();
    private List<ThongBaoThon> notices = new();
    private List<ApThon> villages = new();
    private string title="",body="",error="";
    private Guid villageId;
    private bool busy;
    private int unread;
    protected override Task OnInitializedAsync() => Load();
    private async Task Load()
    {
        busy=true;error="";
        try {
            user=await Users.GetCurrentUserAsync();
            if(!user.IsAuthenticated) {notices.Clear();return;}
            using var scope=Scopes.CreateScope(); var db=scope.ServiceProvider.GetRequiredService<ITanAnDbContext>();
            notices=await db.ThongBaoThons.AsNoTracking().Include(x=>x.Thon).OrderByDescending(x=>x.NgayTao).Take(100).ToListAsync();
            unread=await db.ThongBaoThons.CountAsync(x=>x.DaDocLuc==null);
            if(user.Role=="Admin") villages=await db.ApThons.AsNoTracking().Where(x=>x.DangHoatDong).OrderBy(x=>x.Ten).ToListAsync();
        }catch {error="Không tải được thông báo. Vui lòng thử lại.";}
        finally {busy=false;}
    }
    private async Task Read(Guid id)
    {
        try {using var scope=Scopes.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ITanAnDbContext>();
            var notice=await db.ThongBaoThons.SingleOrDefaultAsync(x=>x.Id==id);
            if(notice!=null) {notice.DaDocLuc=DateTime.UtcNow;await db.SaveChangesAsync();}
            await Load();
        }catch {error="Không đánh dấu được thông báo.";}
    }
    private async Task Send()
    {
        busy=true;error="";
        try {
            user=await Users.GetCurrentUserAsync();
            if(!user.IsAuthenticated || user.Role!="Admin") throw new UnauthorizedAccessException("Chỉ quản trị xã được gửi thông báo chung.");
            if(villageId==Guid.Empty || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body)) throw new ArgumentException("Hãy chọn thôn và nhập tiêu đề, nội dung.");
            using var scope=Scopes.CreateScope();var db=scope.ServiceProvider.GetRequiredService<ITanAnDbContext>();
            var recipients=await db.PhuTrachThons.Where(x=>x.ApThonId==villageId && x.User.Role==RoleEnum.CanBoThon && x.User.ModerationStatus==ModerationStatus.Approved).Select(x=>x.UserId).ToListAsync();
            if(recipients.Count==0) throw new ArgumentException("Thôn chưa có tài khoản phụ trách đang hoạt động.");
            db.ThongBaoThons.AddRange(recipients.Select(id=>new ThongBaoThon{NguoiNhanId=id,ApThonId=villageId,TieuDe=title.Trim(),NoiDung=body.Trim(),NguoiTaoId=user.UserId}));
            db.AuditLogs.Add(new AuditLog
            {
                Username=user.UserName,
                Action="Gửi thông báo thôn",
                EntityName="ApThon",
                EntityId=villageId.ToString(),
                NewValues=JsonSerializer.Serialize(new { TieuDe=title.Trim(), SoNguoiNhan=recipients.Count }),
                IpAddress=HttpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString()
            });
            await db.SaveChangesAsync();title="";body="";error=$"Đã gửi tới {recipients.Count} tài khoản phụ trách.";
        }catch(Exception ex) {error=ex.Message;}
        finally {busy=false;}
    }
}
