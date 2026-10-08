using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Service.Shared.Commons.Extensions;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Interfaces.Extentions;
using Service.Shared.Commons.Model.Commons;
using Service.Shared.Contracts.DTOs.File;
using Service.UI.CMS.Blazor.Applications;

namespace Service.UI.CMS.Blazor.Components.Layout.Component.Attachments
{
    public interface IFileActionService
    {
        /// <summary>
        /// Mở 1 file ở 1 tab mới hoặc dialog trong hệ thống. Hệ thống sẽ tự động lấy link preview hoặc edit phù hợp với loại file và quyền của người dùng
        /// </summary>
        public Task OpenViewAsync(FileDinhKemDto file, bool OpenNewTab, CancellationToken ct = default);

        /// <summary>
        /// Mở link tải xuống ở 1 tab mới. Hệ thống sẽ tự động lấy link download phù hợp với loại file và quyền của người dùng
        /// </summary>
        public Task DownloadAsync(FileDinhKemDto file, CancellationToken ct = default);

        /// <summary>
        /// Mở link tải xuống ở 1 tab mới. Hệ thống sẽ tự động lấy link download phù hợp với loại file và quyền của người dùng
        /// </summary>
        public Task<string> GetLinkDownloadAsync(FileDinhKemDto file, CancellationToken ct = default);
    }

    public class FileActionService : IFileActionService
    {
        private readonly IPreviewTokenService _previewTokenService;
        private readonly IJSRuntime _jsRuntime;
        private readonly IDialogService _dialog;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl = "";

        public FileActionService(
            IConfiguration configuration,
            IUserService userService,
            IJSRuntime jsRuntime,
            IDialogService dialogService,
            IPreviewTokenService previewTokenService,
            ICallServiceRegistry callService,
            HttpClient http)
        {
            _jsRuntime = jsRuntime;
            _dialog = dialogService;
            _previewTokenService = previewTokenService;
            _configuration = configuration;
            _baseUrl = _configuration["ServicesRegistry:ServiceAIM"] ?? "";
        }

        public async Task OpenViewAsync(FileDinhKemDto file, bool OpenNewTab, CancellationToken ct = default)
        {
            //string? url = await GetPreviewUrlInternalAsync(file, isEdit: false, ct);

            //if (string.IsNullOrWhiteSpace(url))
            //{
            //    throw new InvalidInputException("Không thể xem trước file này!");
            //}

            //if (OpenNewTab)
            //{
            //    await OpenLinkNewTab(url);
            //    return;
            //}
            //await ShowDialogFileAsync(url, file.FileName, file);
            await Task.CompletedTask;
        }

        public async Task DownloadAsync(FileDinhKemDto file, CancellationToken ct = default)
        {
            (string? token, DateTime _) = _previewTokenService.CreateToken(file.Bucket,
                file.PathServer, AppGuids.Beautiful,
                TimeSpan.FromSeconds(40));

            string url = $"{_baseUrl}/api/v1/SFile/Download/{token}";

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidInputException("Không thể tải file này!");
            }

            await OpenLinkNewTab(url);
        }

        public async Task<string> GetLinkDownloadAsync(FileDinhKemDto file, CancellationToken ct = default)
        {
            (string? token, DateTime _) = _previewTokenService.CreateToken(file.Bucket,
                file.PathServer, AppGuids.Beautiful,
                TimeSpan.FromSeconds(40));

            string url = $"{_baseUrl}/api/v1/Attachments/Download/{token}";

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidInputException("Không thể tải file này!");
            }

            return url;
        }

        private async Task ShowDialogFileAsync(string url, string fileName, FileDinhKemDto? file = null)
        {
            ViewOrEditParametersDto parameters = new()
            {
                Id = Guid.Empty,
                IsEditMode = false,
                Parameter = url,
                FileName = fileName,
                DataObject = file
            };

            _ = await _dialog.ShowDialogAsync<IframeFile>(
                parameters,
                new DialogParameters
                {
                    PreventDismissOnOverlayClick = true,
                    PreventScroll = true,
                    Modal = true,
                    Width = "75vw",
                });
        }

        private async Task OpenLinkNewTab(string? url)
        {
            if (!string.IsNullOrWhiteSpace(url))
            {
                await _jsRuntime.InvokeVoidAsync("open", url, "_blank");
            }
        }
    }
}

namespace Service.UI.Blazor.Components.Layout.Component.Attachments
{
    public interface IFileActionService : Service.UI.CMS.Blazor.Components.Layout.Component.Attachments.IFileActionService { }
    public class FileActionService : Service.UI.CMS.Blazor.Components.Layout.Component.Attachments.FileActionService
    {
        public FileActionService(
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            Service.UI.CMS.Blazor.Applications.IUserService userService,
            IJSRuntime jsRuntime,
            IDialogService dialogService,
            IPreviewTokenService previewTokenService,
            ICallServiceRegistry callService,
            HttpClient http)
            : base(configuration, userService, jsRuntime, dialogService, previewTokenService, callService, http) { }
    }
}
