// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.Group
{
    public partial class ModalPhanChuyenTrang : ComponentBase
    {
        [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
        [Parameter] public EditOrUpdateModuleParametersDto Content { get; set; } = new();
        [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
        [Inject] private IToastService ToastService { get; set; } = default!;
        private List<ChuyenTrangDto> DanhSachChuyenTrang = new();
        private UserDto? ThongTinNguoiDung;
        public bool IsSaving { get; set; }
        public bool IsLoading { get; set; }
        private bool IsAllSelected => DanhSachChuyenTrang.Any() && DanhSachChuyenTrang.All(x => x.DaGan);

        protected override async Task OnInitializedAsync()
        {
            await LoadThongTinNguoiDung();
            await LoadDanhSachChuyenTrang();
        }

        #region Data Loading
        private async Task LoadThongTinNguoiDung()
        {
            try
            {
                var apiRequest = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/Users/{Content.ParameterGuid}"
                };

                var result = await CallService.Get<UserDto>(apiRequest);
                if (result.Status == StatusCode.OK)
                {
                    ThongTinNguoiDung = result.Data;
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải thông tin người dùng: {ex.Message}");
            }
        }

        private async Task LoadDanhSachChuyenTrang()
        {
            try
            {
                var apiRequest = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/ChuyenTrang/GetAllForPhanQuyen"
                };

                var query = new ChuyenTrangQuery
                {
                    UserId = Content.ParameterGuid,
                    gridRequest = new GridRequest
                    {
                        page = 1,
                        pageSize = int.MaxValue
                    }
                };

                var result = await CallService.Post<DataTableJson<ChuyenTrangDto>>(apiRequest, query);
                if (result.Status == StatusCode.OK && result.Data != null)
                {
                    DanhSachChuyenTrang = result.Data.Data?.ToList() ?? new List<ChuyenTrangDto>();
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách chuyên trang: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        private void OnChuyenTrangCheckedChanged(ChuyenTrangDto chuyenTrang, bool isChecked)
        {
            chuyenTrang.DaGan = isChecked;
            StateHasChanged();
        }

        private void ToggleSelectAll()
        {
            bool newValue = !IsAllSelected;

            foreach (var item in DanhSachChuyenTrang)
            {
                item.DaGan = newValue;
            }

            StateHasChanged();
        }

        #endregion

        #region Submit

        private async Task LuuPhanChuyenTrang()
        {
            try
            {
                IsSaving = true;

                var selectedChuyenTrang = DanhSachChuyenTrang.Where(x => x.DaGan).ToList();

                if (!selectedChuyenTrang.Any())
                {
                    ToastService.ShowWarning("Vui lòng chọn ít nhất một chuyên trang.");
                    return;
                }

                var phanChuyenTrangDto = new PhanChuyenTrangDto
                {
                    UserId = Content.ParameterGuid,
                    ChuyenTrangIds = selectedChuyenTrang.Select(x => x.Id).ToList()
                };

                var apiRequest = new ApiRequestModel
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/PhanQuyen/LuuPhanChuyenTrang"
                };

                var response = await CallService.Post<object>(apiRequest, phanChuyenTrangDto);

                if (response.Status == StatusCode.OK)
                {
                    ToastService.ShowSuccess($"Đã phân {selectedChuyenTrang.Count} chuyên trang cho người dùng thành công");

                    if (Content.OnRefresh.HasDelegate)
                    {
                        await Content.OnRefresh.InvokeAsync();
                    }

                    await Dialog.CloseAsync();
                }
                else
                {
                    ToastService.ShowError("Phân chuyên trang thất bại");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi phân chuyên trang: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }

        #endregion

        private async Task HideDialog()
        {
            await Dialog.CloseAsync();
        }
    }
}
