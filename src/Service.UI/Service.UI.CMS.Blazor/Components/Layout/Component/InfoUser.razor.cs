// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Model.SQL;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;

namespace Service.UI.CMS.Blazor.Components.Layout.Component
{
    public partial class InfoUser : ComponentBase
    {
        [Inject] protected ICallServiceRegistry CallServiceRegistry { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] private ICallServiceRegistry CallService { get; set; } = default!;

        [CascadingParameter]
        public FluentDialog Dialog { get; set; } = default!;

        [CascadingParameter]
        protected CurrentUserDto CurrentUser { set; get; } = new();

        [Parameter]
        public Guid Content { get; set; }
        public string? ErrorMessage { get; set; } = string.Empty;
        public UserDto Item { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                Item = await GetInforUser(Content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
            }
        }

        public async Task<UserDto> GetInforUser(Guid? Id)
        {
            try
            {
                ApiRequestModel apiRequest = new ApiRequestModel()
                {
                    ApiService = ServicesRegistryEnum.ServicePortal,
                    Endpoint = $"/Users/{Id}"
                };

                var result = await CallService.Get<UserDto>(apiRequest);

                if (result.Status == StatusCode.OK && result.Data != null)
                {
                    return result.Data;
                }
                else
                {
                    return new UserDto
                    {
                        Id = CurrentUser.UserId,
                        UserName = CurrentUser.UserName,
                        FullName = CurrentUser.FullName,
                        Email = CurrentUser.Email,
                        PhoneNumber = "0912345678",
                        ModerationStatus = ModerationStatus.Approved
                    };
                }
            }
            catch (Exception)
            {
                return new UserDto
                {
                    Id = CurrentUser.UserId,
                    UserName = CurrentUser.UserName,
                    FullName = CurrentUser.FullName,
                    Email = CurrentUser.Email,
                    PhoneNumber = "0912345678",
                    ModerationStatus = ModerationStatus.Approved
                };
            }
        }

        protected async Task OpenChangePasswordModal()
        {
            try
            {
                var userId = Content;

                await Dialog.CloseAsync();

                await Task.Yield();

                var parameters = new EditOrUpdateParametersDto
                {
                    Id = userId,
                    IsEditMode = true,
                    IsChangePasswordMode = true,
                    OnRefresh = EventCallback.Factory.Create(this, RefreshUserAsync)
                };

                await DialogService.ShowDialogAsync<EditUser>(
                    parameters,
                    new DialogParameters
                    {
                        Title = "Đổi mật khẩu",
                        PreventDismissOnOverlayClick = true,
                        PreventScroll = true,
                        Modal = true,
                        PrimaryAction = null,
                        SecondaryAction = null
                    });
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync(
                    $"Lỗi khi mở modal đổi mật khẩu: {ex.Message}");
            }
        }

        private async Task RefreshUserAsync()
        {
            try
            {
                Item = await GetInforUser(Content);
                StateHasChanged();
            }
            catch (Exception ex)
            {
                await DialogService.ShowErrorAsync($"Lỗi khi tải lại thông tin người dùng: {ex.Message}");
            }
        }

        public async Task CancelAsync()
        {
            await Dialog.CancelAsync();
        }
    }
}
