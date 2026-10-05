using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Interfaces.Extentions;
using Service.Shared.Commons.Model.ServiceCustomHttpClient;
using Service.Shared.Contracts.DTOs;
using Service.Shared.Contracts.DTOs.File;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;

namespace Service.UI.CMS.Blazor.Components.Layout.Component.Attachments
{
    public partial class EditAttachment
    {
        [Inject]
        private IJSRuntime JS { get; set; } = default!;

        [Inject]
        private IToastService ToastService { get; set; } = default!;

        [Inject]
        private ICallServiceRegistry CallService { get; set; } = default!;

        [Inject]
        private IDialogService DialogService { get; set; } = default!;

        [Inject] 
        private IFileActionService _fileActionService { get; set; } = default!;

        /// <summary>
        /// Dữ liệu hiện tại từ component cha.
        /// Không được mutate trực tiếp trong component con.
        /// </summary>
        [Parameter]
        public IReadOnlyList<FileDinhKemDto>? Value { get; set; } = Array.Empty<FileDinhKemDto>();

        /// <summary>
        /// Emit ra danh sách mới sau khi thay đổi.
        /// </summary>
        [Parameter]
        public EventCallback<List<FileDinhKemDto>> ValueChanged { get; set; }

        /// <summary>
        /// Callback sau khi danh sách thay đổi.
        /// Trả về full state mới.
        /// </summary>
        [Parameter]
        public EventCallback<IReadOnlyList<FileDinhKemDto>> OnChanged { get; set; }

        /// <summary>
        /// Callback khi upload thành công file mới.
        /// </summary>
        [Parameter]
        public EventCallback<IReadOnlyList<FileDinhKemDto>> OnFilesAdded { get; set; }

        /// <summary>
        /// Callback khi xóa 1 file.
        /// </summary>
        [Parameter]
        public EventCallback<FileDinhKemDto> OnFileRemoved { get; set; }

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public bool AllowMultiple { get; set; } = true;

        [Parameter]
        public int MaxFile { get; set; } = 10;

        /// <summary>
        /// MB
        /// </summary>
        [Parameter]
        public long MaxFileSize { get; set; } = 1;

        /// <summary>
        /// Ví dụ: ".png,.jpg,.docx,.pdf"
        /// </summary>
        [Parameter]
        public string FilterFile { get; set; } = string.Empty;

        [Parameter]
        public Guid UserId { get; set; }

        private FluentInputFile? myFileUploader = default!;
        private long MaxFileSizeByte { get; set; }

        /// <summary>
        /// True : đang upload file >> khóa mọi button lại
        /// </summary>
        bool LoadingComponent { get; set; } = false;

        protected override async Task OnInitializedAsync()
        {
            MaxFileSizeByte = MaxFileSize * 1024 * 1024;
            await base.OnInitializedAsync();
        }

        /// <summary>
        /// Upload file lên hệ thống và emit state mới ra ngoài.
        /// </summary>
        private async Task OnCompleted(IEnumerable<FluentInputFileEventArgs> files)
        {
            try
            {
                LoadingComponent = true;

                List<FluentInputFileEventArgs> fileList = files?.ToList() ?? [];
                if (fileList.Count == 0)
                {
                    return;
                }

                HashSet<string> allowedExtensions = await GetUploadConfigAsync();

                foreach (FluentInputFileEventArgs? file in fileList)
                {
                    if (!string.IsNullOrWhiteSpace(file.ErrorMessage))
                    {
                        ToastService.ShowError(
                            file.ErrorMessage == "The maximum size allowed is reached"
                                ? $"File '{file.Name}' ({file.Size / 1024 / 1024}MB) vượt quá giới hạn ({MaxFileSizeByte / 1024 / 1024}MB)."
                                : $"{file.Name}: {file.ErrorMessage}"
                        );
                        return;
                    }

                    string ext = Path.GetExtension(file.Name)?.TrimStart('.') ?? string.Empty;
                    if (!allowedExtensions.Contains(ext))
                    {
                        ToastService.ShowError(
                            $"File '{file.Name}' không đúng định dạng. " +
                            $"Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}"
                        );
                        return;
                    }
                }

                ApiRequestModel request = new()
                {
                    ApiService = ServicesRegistryEnum.ServiceAIM,
                    Endpoint = "/UploadHandler/UploadFile",
                };

                // Tạo task riêng cho từng file
                List<Task<List<UploadedFileResponeDto>?>> uploadTasks = fileList.Select(file =>
                {
                    if (file.LocalFile is null || !file.LocalFile.Exists)
                    {
                        ToastService.ShowError($"File '{file.Name}' không hợp lệ!");
                        return Task.FromResult<List<UploadedFileResponeDto>?>(null);
                    }

                    // Mỗi file có formData riêng — KHÔNG share chung
                    MultipartFormDataContent formData = [];

                    var stream = file.LocalFile.OpenRead();
                    var fileContent = new StreamContent(stream);

                    fileContent.Headers.Add("Content-Type", "application/octet-stream");
                    formData.Add(fileContent, "files", file.Name);

                    return CallService.Post<List<UploadedFileResponeDto>>(request, formData)
                        .ContinueWith(t =>
                        {
                            stream.Dispose();
                            formData.Dispose(); // dispose sau khi request xong
                            return t.Result.Data;
                        });
                }).ToList();

                // Chờ tất cả hoàn thành song song
                List<UploadedFileResponeDto>?[] results = await Task.WhenAll(uploadTasks);

                // Kiểm tra kết quả
                List<FileDinhKemDto> addedFiles = [];
                foreach (List<UploadedFileResponeDto>? result in results)
                {
                    if (result != null)
                    {
                        addedFiles.AddRange(result.Select(x => new FileDinhKemDto(x)));
                    }
                }

                List<FileDinhKemDto> nextValue = BuildNextValueAfterAdd(addedFiles);
                await EmitValueChangedAsync(nextValue);

                if (OnFilesAdded.HasDelegate)
                {
                    await OnFilesAdded.InvokeAsync(addedFiles);
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Upload file thất bại. {ex.Message}");
            }
            finally
            {
                LoadingComponent = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        #region Nơi chứa logic xử lý action của file (view/edit/download/delete/sign)

        #region Action file

        private async Task HandleDownloadAsync(FileDinhKemDto file)
        {
            try
            {
                if (file == null || string.IsNullOrWhiteSpace(file.PathServer))
                {
                    ToastService.ShowWarning("Không tìm thấy thông tin file.");
                    return;
                }
                await _fileActionService.DownloadAsync(file);
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Không tải được file. {ex.Message}");
            }
        }

        private async Task HandleDeleteAsync(FileDinhKemDto item)
        {
            try
            {
                if (Disabled || item == null)
                    return;

                LoadingComponent = true;

                var nextValue = (Value ?? Array.Empty<FileDinhKemDto>()).ToList();

                var exists = nextValue.Any(x => IsSameFile(x, item));
                if (!exists)
                    return;

                nextValue.RemoveAll(x => IsSameFile(x, item));

                await EmitValueChangedAsync(nextValue);

                if (OnFileRemoved.HasDelegate)
                {
                    await OnFileRemoved.InvokeAsync(item);
                }
            }
            catch (Exception ex)
            {
                ToastService.ShowError($"Xóa file thất bại. {ex.Message}");
            }
            finally
            {
                LoadingComponent = false;
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task HandleViewAsync(FileDinhKemDto file)
        {
            await Task.CompletedTask;
        }

        #endregion
        #endregion

        private Task RemoveFile(FileDinhKemDto item) => HandleDeleteAsync(item);

        private IDialogReference? _dialog;
        private async Task OpenSplashScanFileAsync()
        {
            DialogParameters<SplashScreenContent> parameters = new()
            {
                Content = new()
                {
                    DisplayTime = 0,
                    Title = "Đang kết nối tới máy Scan",
                    LoadingText = "Đang tải...",
                    Message = (MarkupString)"Hãy đảm bảo đã <strong>cài đặt tool</strong> kết nối với máy <i>scan</i>!",
                },
                PreventDismissOnOverlayClick = true,
                Modal = true,
                Width = "640px",
                Height = "480px",
            };
            _dialog = await DialogService.ShowSplashScreenAsync(parameters);

            var splashScreen = (SplashScreenContent)_dialog.Instance.Content;

            await Task.Delay(2000);
            splashScreen.UpdateLabels(loadingText: "Đang thử kết nối lại...");
            await Task.Delay(2000);
            splashScreen.UpdateLabels(loadingText: "Kết nối thất bại...");
            await Task.Delay(2000);

            await _dialog.CloseAsync();
        }

        /// <summary>
        /// Tạo danh sách mới sau khi thêm file.
        /// Không sửa trực tiếp vào Value hiện tại.
        /// </summary>
        private List<FileDinhKemDto> BuildNextValueAfterAdd(List<FileDinhKemDto> addedFiles)
        {
            var nextValue = (Value ?? Array.Empty<FileDinhKemDto>()).ToList();

            if (!AllowMultiple)
            {
                // Chế độ 1 file: thay thế toàn bộ file hiện tại
                nextValue = new List<FileDinhKemDto>();
            }

            nextValue.AddRange(addedFiles);
            return nextValue;
        }

        /// <summary>
        /// Emit state mới ra ngoài theo đúng thứ tự:
        /// ValueChanged -> OnChanged
        /// </summary>
        private async Task EmitValueChangedAsync(List<FileDinhKemDto> nextValue)
        {
            await ValueChanged.InvokeAsync(nextValue);

            if (OnChanged.HasDelegate)
            {
                await OnChanged.InvokeAsync(nextValue);
            }
        }

        /// <summary>
        /// So sánh theo khóa nghiệp vụ thay vì reference object.
        /// Ưu tiên PathServer, fallback theo tên file.
        /// </summary> 
        private static bool IsSameFile(FileDinhKemDto x, FileDinhKemDto y)
        {
            if (!string.IsNullOrWhiteSpace(x.PathServer) && !string.IsNullOrWhiteSpace(y.PathServer))
            {
                return string.Equals(x.PathServer, y.PathServer, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(x.FileName, y.FileName, StringComparison.OrdinalIgnoreCase);
        }

        private Icon GetFileIcon(string? fileName)
        {
            var ext = Path.GetExtension(fileName ?? string.Empty).ToLowerInvariant();

            return ext switch
            {
                ".pdf" => new Icons.Regular.Size20.DocumentPdf(),
                ".doc" or ".docx" => new Icons.Regular.Size20.DocumentText(),
                ".xls" or ".xlsx" => new Icons.Regular.Size20.DocumentTable(),
                ".ppt" or ".pptx" => new Icons.Regular.Size20.Document(),
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => new Icons.Regular.Size20.Image(),
                ".zip" or ".rar" or ".7z" => new Icons.Regular.Size20.FolderZip(),
                ".txt" => new Icons.Regular.Size20.DocumentText(),
                ".csv" => new Icons.Regular.Size20.DocumentTable(),
                _ => new Icons.Regular.Size20.Document()
            };
        }

        private string GetFileExtension(string? fileName)
        {
            return Path.GetExtension(fileName ?? string.Empty)
                .TrimStart('.')
                .ToUpperInvariant();
        }

        private async Task<HashSet<string>> GetUploadConfigAsync()
        {
            HashSet<string> allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
            {
                "pdf", "jpg", "jpeg", "png", "doc", "docx",
                "xls", "xlsx", "zip", "rar", "7z", "csv", "txt"
            };
            return await Task.FromResult(allowedExtensions);
        }
    }
}
