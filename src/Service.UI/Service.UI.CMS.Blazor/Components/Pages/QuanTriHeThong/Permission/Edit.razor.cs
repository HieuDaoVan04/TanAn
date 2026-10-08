// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;

namespace Service.UI.CMS.Blazor.Components.Pages.QuanTriHeThong.Permission
{
    public partial class Edit : ComponentBase, IDialogContentComponent<EditOrUpdateParametersDto>
    {
        [CascadingParameter]
        public FluentDialog Dialog { get; set; } = default!;

        [Parameter]
        public EditOrUpdateParametersDto Content { get; set; } = new();

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject]
        private IToastService ToastService { get; set; } = default!;

        [Inject]
        private ICallServiceRegistry CallService { get; set; } = default!;

        private string ErrorMessage { get; set; } = string.Empty;
        private bool IsLoading { get; set; } = true;
        private bool IsSaving { get; set; } = false;
        private PermissionForm PermissionForm { get; set; } = new();

        protected override async Task OnAfterRenderAsync(bool firstLoad)
        {
            if (firstLoad)
            {
                try
                {
                    if (Content.IsEditMode && Content.Id != Guid.Empty)
                    {
                        PermissionForm = await FetchDataById(Content.Id);
                    }
                    else
                    {
                        PermissionForm = new();
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Lỗi khi tải dữ liệu: {ex.Message}";
                }
                finally
                {
                    IsLoading = false;
                }
                StateHasChanged();
            }
        }

        private async Task<PermissionForm> FetchDataById(Guid Id)
        {
            ApiRequestModel apiRequest = new ApiRequestModel()
            {
                ApiService = ServicesRegistryEnum.ServicePortal,
                Endpoint = $"/Permission/{Id}"
            };

            ResultAPI<PermissionForm> result = await CallService.Get<PermissionForm>(apiRequest);
            if (result.Status != StatusCode.OK || result.Data is null)
            {
                await ShowErrorMessage("Có lỗi khi gọi api lấy dữ liệu");
                return new();
            }
            return result.Data;
        }

        private async Task Save()
        {
            IsSaving = true;
            try
            {
                if (Content.Id == Guid.Empty)
                {
                    ApiRequestModel apiRequest = new ApiRequestModel()
                    {
                        ApiService = ServicesRegistryEnum.ServicePortal,
                        Endpoint = "/Permission"
                    };
                    ResultAPI<string> result = await CallService.Post<string>(apiRequest, PermissionForm);

                    if (result.Status != StatusCode.OK)
                    {
                        ToastService.ShowWarning($"Lỗi: {result.Message ?? "Đã xảy ra lỗi khi thực hiện thao tác"}");
                    }
                    else
                    {
                        ToastService.ShowSuccess("Thêm mới thành công!");
                        await Dialog.CloseAsync();
                        await Content.OnRefresh.InvokeAsync();
                    }
                }
                else
                {
                    ApiRequestModel apiRequest = new ApiRequestModel()
                    {
                        ApiService = ServicesRegistryEnum.ServicePortal,
                        Endpoint = $"/Permission/{Content.Id}"
                    };
                    ResultAPI<object> result = await CallService.Put<object>(apiRequest, PermissionForm);

                    if (result.Status != StatusCode.OK)
                    {
                        ToastService.ShowWarning("Cập nhật thất bại: " + result.Message);
                    }
                    else
                    {
                        ToastService.ShowSuccess("Cập nhật thành công!");
                        await Dialog.CloseAsync();
                        await Content.OnRefresh.InvokeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowErrorMessage(ex.Message);
            }
            finally
            {
                IsSaving = false;
            }
        }

        private async Task ShowErrorMessage(string? content)
        {
            await DialogService.ShowMessageBoxAsync(new DialogParameters<MessageBoxContent>
            {
                Content = new()
                {
                    Title = "Lỗi",
                    MarkupMessage = new MarkupString($"Có lỗi: {content}"),
                    Icon = new Icons.Regular.Size24.ErrorCircle(),
                    IconColor = Color.Error,
                },
                PrimaryAction = "OK",
                PrimaryActionEnabled = true,
            });
        }

        private async Task CancelAsync()
        {
            await Dialog.CancelAsync();
        }
    }
}
