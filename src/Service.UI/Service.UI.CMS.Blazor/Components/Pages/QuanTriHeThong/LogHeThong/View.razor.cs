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

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.LogHeThong
{
    public partial class View : ComponentBase, IDialogContentComponent<EditOrUpdateParametersDto>
    {
        [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
        [Parameter] public EditOrUpdateParametersDto Content { get; set; } = new EditOrUpdateParametersDto();

        [Inject] private IDialogService DialogService { get; set; } = default!;
        [Inject] private ICallServiceRegistry CallService { get; set; } = default!;
        [Inject] private IToastService ToastService { get; set; } = default!;

        private LogHeThongDto Item { get; set; } = new();
        private bool IsLoading { get; set; } = true;

        protected override async Task OnAfterRenderAsync(bool firstLoad)
        {
            if (firstLoad)
            {
                try
                {
                    if (string.IsNullOrEmpty(Content.Parameter))
                        Item = new();
                    else
                        Item = await FetchDataById(Content.Parameter);
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

        private async Task<LogHeThongDto> FetchDataById(string Id)
        {
            ApiRequestModel apiRequest = new ApiRequestModel()
            {
                ApiService = ServicesRegistryEnum.ServicePortal,
                Endpoint = $"/LogsHeThong/{Id}/GetByIdLogHeThong"
            };

            ResultAPI<LogHeThongDto> result = await CallService.Get<LogHeThongDto>(apiRequest);
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

        private static string Display(string? value) =>
            string.IsNullOrWhiteSpace(value) ? "—" : value;
    }
}
