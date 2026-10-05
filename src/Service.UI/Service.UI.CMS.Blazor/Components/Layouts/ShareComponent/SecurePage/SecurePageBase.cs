// "Một sản phẩm từ phòng sharepoint. SIMAX-CôngVM"

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Service.Shared.Commons.Model.Commons;
using Service.Shared.Commons.Models;

namespace Services.Components.Layouts.ShareComponent.SecurePage
{
    /// <summary>
    /// Base class cho các page cần kiểm soát phân quyền theo menu + permission.
    /// <para>Cơ chế kiểm tra theo 2 tầng (ưu tiên theo thứ tự):</para>
    /// <list type="number">
    ///   <item><b>Tầng 1 - Menu (bắt buộc):</b> User có được cấp menu này không? Nếu không → redirect <c>/no-access</c>.</item>
    ///   <item><b>Tầng 2 - Permission (optional):</b> Nếu page khai báo <see cref="RequiredPermissions"/> thì check tiếp. Thiếu quyền → redirect <c>/no-access</c>.</item>
    /// </list>
    /// <para><b>Fast path:</b> Nếu user là <c>IsSuperUser</c> thì bypass toàn bộ.</para>
    /// </summary>
    public abstract class SecurePageBase : ComponentBase
    {
        /// <summary>
        /// Thông tin user hiện tại, được cascade từ layout cha.
        /// </summary>
        [CascadingParameter]
        protected CurrentUserDto CurrentUser { get; set; } = default!;

        protected override Task OnParametersSetAsync()
        {
            if (CurrentUser != null)
            {
                ArgumentNullException.ThrowIfNull(CurrentUser);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Cache HashSet menu của user (scoped per-circuit) để check O(1).
        /// </summary>
        [Inject] protected NavigationManager Nav { get; set; } = default!;
        [Inject] protected IMenuAccessCache MenuCache { get; set; } = default!;

        /// <summary>
        /// ID của menu gắn với page này — <b>BẮT BUỘC</b> override.
        /// </summary>
        protected abstract Guid MenuId { get; }

        /// <summary>
        /// Danh sách permission code page này yêu cầu (optional).
        /// </summary>
        protected virtual string[] RequiredPermissions => Array.Empty<string>();

        /// <summary>
        /// Cờ báo đã pass toàn bộ check quyền.
        /// </summary>
        protected bool IsAuthorized { get; private set; }

        /// <summary>
        /// Pipeline check quyền chạy theo thứ tự: user context → SuperUser → menu → permission.
        /// </summary>
        protected override Task OnInitializedAsync()
        {
            if (CurrentUser is null)
            {
                var returnUrl = Uri.EscapeDataString(Nav.ToBaseRelativePath(Nav.Uri));
                Nav.NavigateTo($"/account/login?returnUrl={returnUrl}", replace: true);
                return Task.CompletedTask;
            }

            // Fast path: SuperUser bypass toàn bộ
            if (CurrentUser.IsSuperUser)
            {
                IsAuthorized = true;
                return OnAuthorizedAsync();
            }

            // Tầng 1 (ưu tiên): check menu — O(1) nhờ HashSet cache
            if (!MenuCache.HasMenu(CurrentUser, MenuId))
            {
                Redirect(); // Cái này giữ nguyên để báo no-access nếu ĐÃ đăng nhập nhưng KHÔNG có quyền
                return Task.CompletedTask;
            }

            IsAuthorized = true;
            return OnAuthorizedAsync();
        }

        private void Redirect() => Nav.NavigateTo("/no-access", replace: true);

        /// <summary>
        /// Hook chạy SAU KHI đã pass toàn bộ check quyền.
        /// </summary>
        protected virtual Task OnAuthorizedAsync() => Task.CompletedTask;
    }
}

namespace Service.UI.CMS.Blazor.Components.Layouts.ShareComponent.SecurePage
{
    public abstract class SecurePageBase : Services.Components.Layouts.ShareComponent.SecurePage.SecurePageBase
    {
    }
}
