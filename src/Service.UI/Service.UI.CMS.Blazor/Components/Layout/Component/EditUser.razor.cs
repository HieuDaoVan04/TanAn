// "Một sản phẩm của HieuDV"
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.UI.CMS.Blazor.Applications.Dtos;
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Layout.Component;

public partial class EditUser : ComponentBase
{
    [Inject] protected ICallServiceRegistry CallService { get; set; } = default!;
    [Inject] protected IToastService ToastService { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [CascadingParameter] public FluentDialog Dialog { get; set; } = default!;
    [CascadingParameter] public CurrentUserDto CurrentUser { get; set; } = new();
    [Parameter] public EditOrUpdateParametersDto Content { get; set; } = new();
    protected UsersForm UserForm { get; set; } = new();
    protected string CurrentPassword { get; set; } = "";
    protected string ConfirmPassword { get; set; } = "";
    protected PasswordPolicyDto Policy { get; set; } = new();
    protected bool IsLoading { get; set; }
    protected bool IsSaving { get; set; }
    protected bool PolicyLoaded { get; set; }
    protected EditContext? editContext;
    private ValidationMessageStore? messageStore;

    protected override async Task OnInitializedAsync()
    {
        IsLoading = true;
        UserForm = new() { Id = CurrentUser.UserId, UserName = CurrentUser.UserName, FullName = CurrentUser.FullName };
        editContext = new EditContext(UserForm);
        messageStore = new ValidationMessageStore(editContext);
        try
        {
            Policy = (await CallService.Get<PasswordPolicyDto>(new ApiRequestModel
            { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/system-configuration/password-policy" })).RequireData();
            PolicyLoaded = true;
        }
        catch (Exception ex) { ToastService.ShowError($"Không tải được chính sách mật khẩu: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    protected async Task HandleValidSubmit()
    {
        if (IsLoading || IsSaving || !PolicyLoaded) return;
        messageStore?.Clear();
        if (string.IsNullOrEmpty(CurrentPassword)) messageStore?.Add(() => CurrentPassword, "Hãy nhập mật khẩu hiện tại.");
        try { Policy.Validate(UserForm.PasswordMoi); }
        catch (ArgumentException ex) { messageStore?.Add(() => UserForm.PasswordMoi!, ex.Message); }
        if (string.IsNullOrEmpty(ConfirmPassword) || UserForm.PasswordMoi != ConfirmPassword)
            messageStore?.Add(() => ConfirmPassword, "Mật khẩu xác nhận không khớp.");
        editContext?.NotifyValidationStateChanged();
        if (editContext?.GetValidationMessages().Any() == true) return;
        IsSaving = true;
        try
        {
            (await CallService.Put(new ApiRequestModel
            { ApiService = ServicesRegistryEnum.ServiceTanAn, Endpoint = "/session-account/password" },
                new ChangeOwnPasswordForm { CurrentPassword = CurrentPassword, NewPassword = UserForm.PasswordMoi ?? "" })).EnsureSuccess();
            ToastService.ShowSuccess("Đổi mật khẩu thành công. Hãy đăng nhập lại.");
            CurrentPassword = ""; ConfirmPassword = ""; UserForm.PasswordMoi = "";
            await Dialog.CloseAsync();
            Navigation.NavigateTo("/account/login", forceLoad: true);
        }
        catch (Exception ex) { ToastService.ShowError($"Lỗi khi đổi mật khẩu: {ex.Message}"); }
        finally { IsSaving = false; }
    }
    protected Task CancelAsync() => Dialog.CancelAsync();
    protected bool HasMinLength(string? password) => password?.Length is int length && length >= Policy.MinLength && length <= 128;
    protected bool HasUppercase(string? password) => password?.Any(char.IsUpper) == true;
    protected bool HasLowercase(string? password) => password?.Any(char.IsLower) == true;
    protected bool HasNumber(string? password) => password?.Any(char.IsDigit) == true;
    protected bool HasSpecialCharacter(string? password) => password?.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)) == true;
    protected string PasswordRuleClass(bool valid) => valid ? "password-rule active" : "password-rule";
}
