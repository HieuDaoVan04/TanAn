// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;
using Service.UI.CMS.Blazor.Components.Layout.Component;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.Group
{
    public partial class ModalThemNguoIDungVaoPhongBan : ComponentBase
    {
        [CascadingParameter]
        public FluentDialog Dialog { get; set; } = default!;

        [Parameter]
        public EditOrUpdateParametersGroupDto Content { get; set; } = new();

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private IToastService ToastService { get; set; } = default!;

        [Inject]
        private ICallServiceRegistry CallService { get; set; } = default!;

        public string? SearchKeyWord { get; set; }
        private string ErrorMessage { get; set; } = string.Empty;
        protected PaginationState pagination = new PaginationState { ItemsPerPage = 10 };
        protected FluentDataGrid<UserDto> Grid { get; set; } = default!;

        private async Task CancelAsync()
        {
            await Dialog.CancelAsync();
            await Content.OnRefresh.InvokeAsync();
        }

        private async Task OpenDetailsModal(Guid userId)
        {
            try
            {
                var parameters = new EditOrUpdateParametersDto
                {
                    Id = userId,
                };

                await DialogService.ShowDialogAsync<InfoUser>(parameters, new DialogParameters
                {
                    Title = "Thông tin chi tiết người dùng",
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    Width = "800px"
                });
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi mở modal chi tiết: {ex.Message}");
            }
        }

        private async Task RefreshData(int value)
        {
            try
            {
                pagination.ItemsPerPage = value;
                await pagination.SetCurrentPageIndexAsync(0);
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách: {ex.Message}");
            }
        }

        private async Task GanNguoiDung(Guid Id)
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/Users/gannguoidungvaogroup/{Id}/{Content?.Object?.Id}"
                };

                ResultAPI<object> result = await CallService.Put<object>(apiRequest, new object());
                if (result.Status == StatusCode.OK)
                {
                    await Grid.RefreshDataAsync();
                    ToastService.ShowSuccess("Thêm người dùng thành công!");
                }
                else
                {
                    throw new Exception(result.Message ?? "Lỗi khi thêm người dùng.");
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi thêm người dùng: {ex.Message}");
            }
        }

        private async ValueTask<GridItemsProviderResult<UserDto>> LoadDatas(GridItemsProviderRequest<UserDto> request)
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = "/Users/getpagedbygroupid"
                };
                var baseQuery = new UserQuery
                {
                    draw = 1,
                    SearchIn = new List<string> { "UserName", "FullName" },
                    Keyword = SearchKeyWord,
                    gridRequest = new GridRequest
                    {
                        filter = new Filter(),
                        page = request.StartIndex / pagination.ItemsPerPage + 1,
                        pageSize = pagination.ItemsPerPage,
                    }
                };
                if (Content.Object is not null)
                {
                    baseQuery.GroupId = Content.Object.Id;
                    baseQuery.IsLayThuocDonVi = false;
                }
                ResultAPI<DataTableJson<UserDto>> result = await CallService.Post<DataTableJson<UserDto>>(apiRequest, baseQuery);

                if (result.Status == StatusCode.OK && result.Data is DataTableJson<UserDto>)
                {
                    var dataTable = result.Data;
                    var indexedItems = dataTable.Data.Select((item, idx) =>
                    {
                        item.Index = request.StartIndex + idx + 1;
                        return item;
                    }).ToList();

                    var totalRecords = dataTable.RecordsTotal;
                    return GridItemsProviderResult.From(indexedItems, totalRecords);
                }
                return GridItemsProviderResult.From(new List<UserDto>(), 0);
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Lỗi khi tải danh sách vai trò: {ex.Message}");
                return GridItemsProviderResult.From(new List<UserDto>(), 0);
            }
        }

        private async Task HandleSearchChanged(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await Grid.RefreshDataAsync();
            }
        }
    }
}
