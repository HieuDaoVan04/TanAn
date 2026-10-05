// "Một sản phẩm của HieuDV"

using System;
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
    public partial class View : ComponentBase
    {
        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private ICallServiceRegistry CallService { get; set; } = default!;

        [Inject]
        private IToastService ToastService { get; set; } = default!;

        [CascadingParameter]
        public FluentDialog Dialog { get; set; } = default!;

        [Parameter]
        public EditOrUpdateParametersDto Content { get; set; } = new EditOrUpdateParametersDto();

        private GroupsDto Item { get; set; } = new();
        private bool IsLoading { get; set; } = true;

        protected override async Task OnAfterRenderAsync(bool firtsLoad)
        {
            if (firtsLoad)
            {
                try
                {
                    if (Content.Id != Guid.Empty)
                        Item = await FetchDataById(Content.Id);
                    else
                        Item = new();
                }
                catch (Exception ex)
                {
                    ToastService.ShowWarning($"Lỗi khi tải dữ liệu chi tiết: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
                StateHasChanged();
            }
        }

        private async Task<GroupsDto> FetchDataById(Guid Id)
        {
            ApiRequestModel apiRequest = new ApiRequestModel()
            {
                ApiService = ServicesRegistryEnum.ServicePortal,
                Endpoint = $"/Groups/{Id}"
            };

            ResultAPI<GroupsDto> result = await CallService.Get<GroupsDto>(apiRequest);
            if (result.Status != StatusCode.OK || result.Data is null)
            {
                ToastService.ShowWarning("Có lỗi khi gọi api lấy dữ liệu");
                return new();
            }
            return result.Data;
        }

        private async Task CancelAsync()
        {
            await Dialog.CancelAsync();
        }
    }
}
