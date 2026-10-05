using Service.Shared.Contracts.DTOs;
using Service.TanAn.Domain.Entities;

namespace Service.TanAn.Application.Services.Core;

public static class MenuTreeBuilder
{
    public static List<ModuleTreeDto> Build(IEnumerable<Module> source)
    {
        var modules = source.ToList();
        var byId = modules.ToDictionary(x => x.Id);
        // Không cho quan hệ cha-con vòng làm treo render; con có cha không công bố sẽ bị ẩn.
        foreach (var module in modules)
        {
            var seen = new HashSet<Guid> { module.Id };
            var parentId = module.ModuleChaId;
            while (parentId is Guid id && byId.TryGetValue(id, out var parent))
            {
                if (!seen.Add(id)) throw new InvalidOperationException("Cây menu có quan hệ cha-con vòng.");
                parentId = parent.ModuleChaId;
            }
        }
        List<ModuleTreeDto> Children(Guid? parentId) => modules
            .Where(x => x.ModuleChaId == parentId).OrderBy(x => x.ViTri).ThenBy(x => x.TenModule)
            .Select(x => new ModuleTreeDto
            {
                Id = x.Id, ModuleChaId = x.ModuleChaId, TenModule = x.TenModule,
                LienKet = x.LienKet, Icon = x.Icon, Expands = x.Expands, ViTri = x.ViTri,
                PhanLoai = x.PhanLoai, ModerationStatus = x.ModerationStatus, Children = Children(x.Id)
            }).ToList();
        return Children(null);
    }
}
