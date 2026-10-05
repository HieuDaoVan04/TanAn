from pathlib import Path
base=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Shared')
(base/'VillageNotifications.razor').write_text('''@using Service.TanAn.Domain.Entities
<h3>Thông báo theo thôn</h3>
@if (!string.IsNullOrEmpty(error)) { <p role="alert">@error</p> }
<FluentButton Disabled="busy" OnClick="Load">Làm mới</FluentButton>
@if (user.Role == "Admin")
{
    <div class="notice-form">
        <label>Thôn nhận thông báo<select @bind="villageId"><option value="@Guid.Empty">— Chọn thôn —</option>@foreach(var village in villages) { <option value="@village.Id">@village.Ten</option> }</select></label>
        <label>Tiêu đề<input @bind="title" maxlength="200" /></label>
        <label>Nội dung<textarea @bind="body" maxlength="2000"></textarea></label>
        <FluentButton Disabled="busy" Appearance="Appearance.Accent" OnClick="Send">Gửi tới cán bộ phụ trách thôn</FluentButton>
    </div>
}
<p>@unread thông báo chưa đọc</p>
@foreach(var notice in notices)
{
    <article class="notice @(notice.DaDocLuc == null ? "unread" : "")">
        <strong>@notice.TieuDe</strong><p>@notice.NoiDung</p>
        <small>@notice.Thon.Ten · @notice.NgayTao.ToLocalTime().ToString("dd/MM/yyyy HH:mm")</small>
        @if(notice.DaDocLuc == null) { <FluentButton Disabled="busy" OnClick="@(() => Read(notice.Id))">Đánh dấu đã đọc</FluentButton> }
    </article>
}
@if(notices.Count == 0) { <p>Chưa có thông báo trong các thôn được giao.</p> }
''',encoding='utf-8')
(base/'VillageNotifications.razor.cs').write_text('''using Microsoft.AspNetCore.Components;
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
    [Inject] public IServiceScopeFactory Scopes { get; set; } = default!;
    [Inject] public IUserService Users { get; set; } = default!;
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
            db.AuditLogs.Add(new AuditLog {Username=user.UserName,Action="Gửi thông báo thôn",EntityName="ApThon",EntityId=villageId.ToString()});
            await db.SaveChangesAsync();title="";body="";error=$"Đã gửi tới {recipients.Count} tài khoản phụ trách.";
        }catch(Exception ex) {error=ex.Message;}
        finally {busy=false;}
    }
}
''',encoding='utf-8')
(base/'VillageNotifications.razor.css').write_text('.notice-form{display:grid;gap:12px;margin:16px 0}.notice-form label{display:grid;gap:6px}.notice{border-bottom:1px solid var(--neutral-stroke-rest);padding:14px 8px}.notice.unread{border-left:3px solid var(--accent-fill-rest)}.notice p{white-space:pre-wrap}',encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Layout/Component/NotificationCenterPanel.razor');s=p.read_text(encoding='utf-8-sig');s+='\n<Service.UI.CMS.Blazor.Components.Shared.VillageNotifications />\n';p.write_text(s,encoding='utf-8')
p=Path('src/Service.UI/Service.UI.CMS.Blazor/Components/Layout/Component/NotificationCenter.razor.cs');s=p.read_text(encoding='utf-8-sig').replace('Title = $"Notifications"','Title = "Thông báo"');p.write_text(s,encoding='utf-8')
