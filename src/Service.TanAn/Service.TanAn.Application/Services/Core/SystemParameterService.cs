// "Một sản phẩm của HieuDV"
using Microsoft.EntityFrameworkCore;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Application.Interfaces.Core;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces;

namespace Service.TanAn.Application.Services.Core;

/// <summary>Giữ hợp đồng API cũ; mọi thay đổi dùng cùng nghiệp vụ cấu hình mới.</summary>
public sealed class SystemParameterService(SystemConfigurationService configuration, ITanAnDbContext db,
    IUnitOfWorkQuanTriHeThong unitOfWork, IRequestContext context) : ISystemParameterService
{
    public Task<Guid> CreateAsync(SystemParameterForm form)
    {
        form.Id = Guid.Empty;
        return configuration.SaveParameterAsync(form, context.CurrentUser);
    }
    public Task<bool> DeleteAsync(Guid id) => configuration.DeleteAsync(id, context.CurrentUser);
    public async Task<bool> UpdateAsync(Guid id, SystemParameterForm form)
    {
        if (!await db.SystemParameters.AnyAsync(x => x.Id == id)) return false;
        form.Id = id;
        await configuration.SaveParameterAsync(form, context.CurrentUser);
        return true;
    }
    public async Task<SystemParameterDto> GetByIdAsync(Guid id) => ToDto(
        await db.SystemParameters.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id)
        ?? throw new KeyNotFoundException("Tham số không tồn tại."));
    public async Task<SystemParameterDto> GetByCodeAsync(string code)
    {
        code = SystemParameterCatalog.NormalizeCode(code);
        var rows = await db.SystemParameters.AsNoTracking().ToListAsync();
        return ToDto(rows.SingleOrDefault(x => x.Code.Trim().Equals(code, StringComparison.OrdinalIgnoreCase))
            ?? throw new KeyNotFoundException("Tham số không tồn tại."));
    }
    public async Task<DataTableJson> GetPaged(BaseQuery query)
    {
        var (raw, total) = await unitOfWork.SystemParameterRepository.GetPagedDtoAsync(query);
        var items = (List<SystemParameterDto>)raw;
        return new DataTableJson(items.Cast<object>().ToList(), query.draw, total, items.Count);
    }
    public async Task<bool> SyncSystemParameterFromEnum()
    {
        await configuration.SyncDefinitionsAsync(context.CurrentUser);
        return true;
    }
    public Task<bool> ChangeModerationStatusAsync(Guid id, ModerationStatus status)
        => configuration.ChangeStatusAsync(id, status, context.CurrentUser);
    public async Task<bool> UpdateValueByCodeAsync(string code, SystemParameterForm form)
    {
        var current = await GetByCodeAsync(code);
        form.Id = current.Id; form.Code = current.Code; form.Description = current.Description;
        await configuration.SaveParameterAsync(form, context.CurrentUser);
        return true;
    }
    private static SystemParameterDto ToDto(SystemParameter x) => new()
    {
        Id = x.Id, Code = x.Code, Value = x.Value, Description = x.Description, IsSync = x.IsSync,
        Created = x.Created, LastModified = x.LastModified ?? x.Created, ModerationStatus = x.ModerationStatus
    };
}
