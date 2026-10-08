using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Services.Core;
using Service.TanAn.Infrastructure.Persistence;

namespace Service.UI.CMS.Blazor.Applications;

public sealed class MenuTreeService(IServiceScopeFactory scopeFactory)
{
    public async Task<List<ModuleTreeDto>> GetPublishedAsync()
    {
        // Context riêng mỗi lần tải, tránh dùng chung DbContext giữa các event của circuit.
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TanAnDbContext>();
        var modules = await db.Modules.AsNoTracking()
            .Where(x => x.ModerationStatus == ModerationStatus.Approved && (x.PhanHeId == null || x.PhanHe!.HoatDong)).ToListAsync();
        return MenuTreeBuilder.Build(modules);
    }
}
