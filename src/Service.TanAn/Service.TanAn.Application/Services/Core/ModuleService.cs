// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.Application.Interfaces.Core;
using Service.TanAn.Domain.Entities;
using Service.TanAn.Domain.Interfaces;

namespace Service.TanAn.Application.Services.Core
{
    public class ModuleService : IModuleService
    {
        private readonly IUnitOfWorkQuanTriHeThong _unitOfWork;

        public ModuleService(IUnitOfWorkQuanTriHeThong unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> AddAsync(ModuleForm request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Dữ liệu không hợp lệ");
            if (string.IsNullOrEmpty(request.TenModule))
                throw new ArgumentException("Tên module không thể trống! Vui lòng kiểm tra lại.", nameof(request.TenModule));

            var existing = await _unitOfWork.ModuleRepository.FindAllAsync(x =>
                x.TenModule == request.TenModule &&
                x.PhanLoai == request.PhanLoai &&
                x.ModuleChaId == request.ModuleChaId
            );
            if (existing.Any())
                throw new InvalidOperationException("Tên module đã tồn tại trong phân loại này và cùng module cha!");

            var module = new Module
            {
                TenModule = request.TenModule,
                Icon = request.Icon,
                LienKet = request.LienKet,
                Expands = request.Expands,
                ViTri = request.ViTri,
                PhanLoai = request.PhanLoai,
                ModuleChaId = request.ModuleChaId,
                ModerationStatus = ModerationStatus.Approved,
            };

            var itemSaved = await _unitOfWork.ModuleRepository.AddAsync(module);
            await _unitOfWork.CompleteAsync();

            return itemSaved.Id;
        }

        public async Task<bool> ApproveAsync(Guid id)
        {
            var module = await _unitOfWork.ModuleRepository.GetByIdAsync(id);
            if (module == null)
                return false;

            module.ModerationStatus = ModerationStatus.Approved;
            _unitOfWork.ModuleRepository.Update(module);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var module = await _unitOfWork.ModuleRepository.GetByIdAsync(id);
            if (module == null)
                return false;

            if (module.ModerationStatus == ModerationStatus.Approved)
                throw new InvalidOperationException("Module đã được duyệt, không thể xóa.");

            _unitOfWork.ModuleRepository.Delete(module);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<Module> GetByIdAsync(Guid id)
        {
            var module = await _unitOfWork.ModuleRepository.GetByIdAsync(id);
            if (module == null)
                throw new InvalidOperationException("Module không tồn tại!");
            if (module.ModuleChaId is not null)
            {
                module.ModuleCha = await _unitOfWork.ModuleRepository.GetByIdAsync((Guid)module.ModuleChaId);
            }

            return module;
        }

        public DataTableJson GetPaged(BaseQuery query)
        {
            int totalRecords = 0;
            var dataQuery = _unitOfWork.ModuleRepository.FilterData(
                q => q.Where(x =>
                    !query.isgetBylisID || query.lstIDGet.Any(id => id == x.Id)
                ),
                query.gridRequest,
                ref totalRecords
            )
            .Select(m => new ModuleDto
            {
                Id = m.Id,
                TenModule = m.TenModule,
                PhanLoai = m.PhanLoai,
                Icon = m.Icon,
                LienKet = m.LienKet ?? "Empty",
                Expands = m.Expands,
                ViTri = m.ViTri,
                ModuleChaId = m.ModuleChaId,
                ModerationStatus = m.ModerationStatus,
            });

            List<ModuleDto> data = dataQuery.ToList();
            DataTableJson dataTableJson = new DataTableJson(data.ConvertAll(x => (object)x), query.draw, totalRecords);
            dataTableJson.querytext = dataQuery.ToString();
            return dataTableJson;
        }

        public async Task<bool> RejectAsync(Guid id)
        {
            var module = await _unitOfWork.ModuleRepository.GetByIdAsync(id);
            if (module == null)
                return false;

            module.ModerationStatus = ModerationStatus.Pending;
            _unitOfWork.ModuleRepository.Update(module);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<bool> UpdateAsync(Guid id, ModuleForm request)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID không hợp lệ", nameof(id));

            if (request == null)
                throw new ArgumentNullException(nameof(request), "Dữ liệu không hợp lệ");

            if (string.IsNullOrWhiteSpace(request.TenModule))
                throw new ArgumentException("Tên module không thể để trống.", nameof(request.TenModule));

            var module = await _unitOfWork.ModuleRepository.GetByIdAsync(id);
            if (module == null)
                throw new InvalidOperationException("Module không tồn tại.");

            if (module.ModerationStatus == ModerationStatus.Approved)
                throw new InvalidOperationException("Module đã được duyệt, không thể chỉnh sửa.");

            var isNameExist = await _unitOfWork.ModuleRepository
            .FindAllAsync(x =>
                x.TenModule == request.TenModule &&
                x.PhanLoai == request.PhanLoai &&
                x.ModuleChaId == request.ModuleChaId &&
                x.Id != id
            );

            if (isNameExist.Any())
                throw new InvalidOperationException("Tên module đã tồn tại trong phân loại này và cùng module cha!");

            module.TenModule = request.TenModule;
            module.Icon = request.Icon;
            module.LienKet = request.LienKet;
            module.ViTri = request.ViTri;
            module.PhanLoai = request.PhanLoai;
            module.ModuleChaId = request.ModuleChaId;
            module.Expands = request.Expands;

            _unitOfWork.ModuleRepository.Update(module);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public List<ModuleTreeDto> GetFilteredTree(int phanLoai, string searchTerm)
        {
            if (!Enum.IsDefined(typeof(EnumModuleType), phanLoai))
            {
                return new List<ModuleTreeDto>();
            }

            var listModule = _unitOfWork.ModuleRepository
                            .FindAll(m => m.PhanLoai == (EnumModuleType)phanLoai)
                            .ToList();

            var filteredChildren = BuildFilteredModuleHierarchy(listModule, null, searchTerm.ToLowerInvariant());

            return filteredChildren;
        }

        public List<ModuleTreeDto> GetFilteredTreePhanQuyen(int phanLoai, string searchTerm, Guid roleId)
        {
            if (!Enum.IsDefined(typeof(EnumModuleType), phanLoai))
            {
                return new List<ModuleTreeDto>();
            }

            var listModule = _unitOfWork.ModuleRepository
                            .FindAll(m => phanLoai == (int)EnumModuleType.KhongXacDinh || m.PhanLoai == (EnumModuleType)phanLoai)
                            .ToList();
            var roleModules = _unitOfWork.RoleModulesRepository
                      .FindAll(rm => rm.RoleId == roleId)
                      .Select(rm => rm.ModuleId)
                      .ToHashSet();

            var filteredChildren = BuildFilteredModulePhanQuyenHierarchy(listModule, null, searchTerm.ToLowerInvariant(), roleModules);

            return filteredChildren;
        }

        private List<ModuleTreeDto> BuildFilteredModuleHierarchy(List<Module> modules, Guid? parentId, string searchTerm)
        {
            var result = new List<ModuleTreeDto>();
            var moduleTemp = modules.Where(m => m.ModuleChaId == parentId).ToList();

            foreach (var module in moduleTemp)
            {
                bool matchesSearch = string.IsNullOrEmpty(searchTerm) ||
                    module.TenModule?.ToLowerInvariant().Contains(searchTerm) == true;

                var children = BuildFilteredModuleHierarchy(modules, module.Id, searchTerm);

                if (matchesSearch || children.Any())
                {
                    result.Add(new ModuleTreeDto
                    {
                        Id = module.Id,
                        PhanLoai = module.PhanLoai,
                        TenModule = module.TenModule ?? string.Empty,
                        Icon = module.Icon,
                        LienKet = module.LienKet,
                        Expands = module.Expands,
                        ViTri = module.ViTri,
                        ModuleChaId = module.ModuleChaId,
                        ModerationStatus = module.ModerationStatus,
                        Children = children
                    });
                }
            }

            return result.OrderBy(m => m.ViTri).ToList();
        }

        private List<ModuleTreeDto> BuildFilteredModulePhanQuyenHierarchy(List<Module> modules, Guid? parentId, string searchTerm, HashSet<Guid> roleModules)
        {
            return modules
                .Where(m => m.ModuleChaId == parentId)
                .Where(m => string.IsNullOrEmpty(searchTerm) || m.TenModule.ToLower().Contains(searchTerm))
                .Select(m => new ModuleTreeDto
                {
                    Id = m.Id,
                    ModuleChaId = m.ModuleChaId,
                    TenModule = m.TenModule,
                    Icon = m.Icon,
                    LienKet = m.LienKet,
                    Expands = m.Expands,
                    ViTri = m.ViTri,
                    PhanLoai = m.PhanLoai,
                    Name = m.TenModule,
                    Checked = roleModules.Contains(m.Id),
                    Children = BuildFilteredModulePhanQuyenHierarchy(modules, m.Id, searchTerm, roleModules)
                }).OrderBy(m => m.ViTri).ToList();
        }

        public List<Module> GetAll()
        {
            var listModule = _unitOfWork.ModuleRepository.GetTableNoTracking();
            return listModule.ToList();
        }

        public List<ModuleTreeDto> GetTreePublishingForMCS(EnumModuleType phanLoai)
        {
            var listModule = _unitOfWork.ModuleRepository
                           .FindAll(m => m.PhanLoai == phanLoai && m.ModerationStatus == ModerationStatus.Approved)
                           .ToList();

            var filteredChildren = BuildFilteredModuleHierarchy(listModule, null, "");

            return filteredChildren;
        }
    }
}
