// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Interfaces.Extentions;
using Service.Shared.Commons.Model.Commons;
using Service.Shared.Commons.Model.ServiceCustomHttpClient;
using Service.Shared.Commons.Models;
using Service.Shared.Commons.Querys.Grid;
using Service.Shared.Commons.Querys.ModalQuery;
using Service.Shared.Contracts.DTOs;
using Service.Shared.Contracts.Forms;
using Service.UI.CMS.Blazor.Applications.Dtos;

namespace Service.UI.Blazor.Components.Pages.QuanTriHeThong.DonVi
{
    public partial class Edit : ComponentBase
    {
        [CascadingParameter]
        public FluentDialog Dialog { get; set; } = default!;
        [Parameter]
        public EditOrUpdateParametersDto Content { get; set; } = new();
        [Inject]
        private IDialogService DialogService { get; set; } = default!;
        [Inject]
        private ICallServiceRegistry CallService { get; set; } = default!;
        [Inject]
        private IToastService ToastService { get; set; } = default!;
        private bool isSaving = false;
        private GroupForm GroupForm = new GroupForm();
        private string ErrorMessage { get; set; } = string.Empty;
        /// <summary>
        /// Hệ thống cha được chọn. Để trống nghĩa là đang tạo/sửa một hệ thống,
        /// có giá trị nghĩa là một đơn vị trực thuộc hệ thống đó.
        /// </summary>
        private GroupDto? HeThongChaDaChon = null;

        /// <summary>
        /// Quota chỉ áp dụng cho đơn vị nên ô nhập Quota chỉ hiện khi đã chọn hệ thống cha.
        /// </summary>
        private bool LaDonVi => HeThongChaDaChon is not null && HeThongChaDaChon.Id != Guid.Empty;

        protected override async Task OnInitializedAsync()
        {
            if (Content.IsEditMode)
            {
                await GetGroupById();
            }
        }

        private async Task CloseModal()
        {
            await Dialog.CloseAsync(false);
        }

        /// <summary>
        /// Lấy một group theo Id, dùng để hiển thị lại hệ thống cha ở chế độ sửa.
        /// </summary>
        private async Task<GroupDto?> LayGroupTheoIdAsync(Guid id)
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = $"/Group/{id}"
                };

                var result = await CallService.Get<GroupDto>(apiRequest);

                return result.Status == StatusCode.OK ? result.Data as GroupDto : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task GetGroupById()
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = $"/Group/{Content.Id}"
                };
                // Lấy về GroupDto (không phải GroupForm) vì chỉ GroupDto mới mang theo
                // thông tin hệ thống cha, cần cho ô chọn ở chế độ sửa.
                var result = await CallService.Get<GroupDto>(apiRequest);
                if (result.Status == StatusCode.OK)
                {
                    var item = result.Data as GroupDto ?? throw new Exception("Dữ liệu trả về không đúng định dạng GroupDto.");
                    // Có cha nghĩa là đang sửa một đơn vị; không có cha nghĩa là đang sửa một hệ thống.
                    HeThongChaDaChon = item.Parent;

                    // Nếu API không kèm sẵn thông tin cha thì tự lấy theo ParentId,
                    // để ô chọn không bị trống trong khi bản ghi thực sự là đơn vị.
                    if (HeThongChaDaChon is null && item.ParentId is not null && item.ParentId != Guid.Empty)
                    {
                        HeThongChaDaChon = await LayGroupTheoIdAsync(item.ParentId.Value);
                    }

                    GroupForm.Name = item.Name;
                    GroupForm.MaGroup = item.MaGroup;
                    GroupForm.Description = item.Description;
                    GroupForm.ParentId = item.ParentId;
                    GroupForm.Quota = item.Quota;

                }
                else
                {
                    ErrorMessage = ($"Lỗi khi lấy thông tin đơn vị: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi lấy thông tin vai trò: {ex.Message}";
            }
        }

        private async Task HandleSubmit()
        {
            bool result = false;

            if (Content.IsEditMode)
            {
                result = await UpdateGroup(GroupForm);

            }
            else
            {
                result = await CreateGroup(GroupForm);
            }

            if (result)
            {
                await Dialog.CloseAsync();
                await Content.OnRefresh.InvokeAsync();
            }

        }
        private async Task<bool> CreateGroup(GroupForm createRequest)
        {
            isSaving = true;
            ErrorMessage = string.Empty;
            try
            {

                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = "/Group/"
                };
                // Để trống hệ thống cha là tạo hệ thống; có chọn là tạo đơn vị trực thuộc.
                createRequest.ParentId = LaDonVi ? HeThongChaDaChon!.Id : null;
                createRequest.Quota = LaDonVi ? createRequest.Quota : null;

                var result = await CallService.Post<Guid>(apiRequest, createRequest);
                if (result.Status == StatusCode.OK)
                {
                    ToastService.ShowSuccess("Thêm đơn vị thành công.");
                    return true;
                }
                else
                {
                    ErrorMessage = "Thêm đơn vị thất bại: " + result.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Thêm đơn vị thất bại: " + ex.Message;
                return false;
            }
            finally
            {
                isSaving = false;
            }
        }
        private async Task<bool> UpdateGroup(GroupForm updateRequest)
        {
            isSaving = true;
            ErrorMessage = string.Empty;
            try
            {

                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = $"/Group/{Content.Id}"
                };

                // Để trống hệ thống cha là hệ thống; có chọn là đơn vị trực thuộc.
                updateRequest.ParentId = LaDonVi ? HeThongChaDaChon!.Id : null;
                updateRequest.Quota = LaDonVi ? updateRequest.Quota : null;

                var result = await CallService.Put(apiRequest, updateRequest);
                if (result.Status == StatusCode.OK)
                {
                    ToastService.ShowSuccess("Sửa đơn vị thành công.");
                    return true;
                }
                else
                {
                    ErrorMessage = "Sửa đơn vị thất bại: " + result.Message;
                    return false;
                }
            }
            catch (Exception ex)
            {

                ErrorMessage = "Sửa đơn vị thất bại: " + ex.Message;
                return false;
            }
            finally
            {
                isSaving = false;
            }
        }
        private async Task OnSearchHeThongCha(OptionsSearchEventArgs<GroupDto> e)
        {
            string input = e.Text;
            e.Items = await FetchDataHeThongCha(input);
        }
        private async Task<List<GroupDto>> FetchDataHeThongCha(string searchKeyword)
        {
            var output = new List<GroupDto>();

            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {

                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = "/Group/GetPaged"

                };
                var baseQuery = new GroupQuery
                {
                    // Cây chỉ sâu 2 cấp (Đơn vị -> Hệ thống) nên chỉ Đơn vị mới được làm cha.
                    LoaiGroup = EnumLoaiGroup.DonVi,
                    draw = 1,
                    SearchIn = new List<string> { "Name" },
                    Keyword = searchKeyword,
                    gridRequest = new GridRequest
                    {
                        filter = new Filter(),
                        page = 1,
                        pageSize = 20,

                    }
                };
                ResultAPI<DataTableJson<GroupDto>> result = await CallService.Post<DataTableJson<GroupDto>>(apiRequest, baseQuery);

                if (result.Status == StatusCode.OK && result.Data is DataTableJson<GroupDto>)
                {
                    var dataTable = result.Data;
                    output = dataTable.Data.ToList();
                }
            }
            catch (Exception)
            {


            }

            return output;

        }
    }
}
