// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Service.Shared.Commons.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.Shared.Commons.Model.SQL;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Domain.Entities;

namespace Service.UI.CMS.Blazor.Applications
{
    public class ModuleTypeState
    {
        public string Current { get; private set; } = "default";
        public event Action? Changed;

        public void SetState(string newState)
        {
            if (Current != newState)
            {
                Current = newState;
                Changed?.Invoke();
            }
        }
    }

    public class CallServiceRegistry : ICallServiceRegistry
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IExcelExportService _excelService;
        private readonly ApiServiceTransport _transport;

        public CallServiceRegistry(IServiceProvider serviceProvider, IExcelExportService excelService, ApiServiceTransport transport)
        {
            _serviceProvider = serviceProvider;
            _excelService = excelService;
            _transport = transport;
        }

        public Task<ResultAPI<T>> Get<T>(ApiRequestModel request)
        {
            if (IsAdministrationEndpoint(request.Endpoint))
                return _transport.SendAsync<T>(HttpMethod.Get, request, null);
            var endpoint = request.Endpoint ?? string.Empty;
            if (endpoint.Contains("/Group/get-tree", StringComparison.OrdinalIgnoreCase))
            {
                string? search = null;
                var qIdx = endpoint.IndexOf("searchTerm=");
                if (qIdx >= 0)
                {
                    search = Uri.UnescapeDataString(endpoint.Substring(qIdx + 11));
                }
                var tree = MockGroupService.GetTree(search);
                if (tree is T typedTree)
                {
                    return Task.FromResult(new ResultAPI<T> { Success = true, Status = StatusCode.OK, Data = typedTree });
                }
            }

            if (endpoint.StartsWith("/Group/", StringComparison.OrdinalIgnoreCase) && typeof(T) == typeof(GroupDto))
            {
                var idStr = endpoint.Substring(7).Split('?')[0];
                if (Guid.TryParse(idStr, out var gId))
                {
                    var g = MockGroupService.GetById(gId);
                    if (g is T typedG)
                    {
                        return Task.FromResult(new ResultAPI<T> { Success = true, Status = StatusCode.OK, Data = typedG });
                    }
                }
            }

            return Task.FromResult(new ResultAPI<T>
            {
                Success = true,
                Status = StatusCode.OK,
                Data = default
            });
        }

        public Task<ResultAPI<T>> Post<T>(ApiRequestModel request)
        {
            if (IsAdministrationEndpoint(request.Endpoint))
                return _transport.SendAsync<T>(HttpMethod.Post, request, request.Body);
            var endpoint = request.Endpoint ?? string.Empty;
            if (endpoint.Contains("/Group/GetPaged", StringComparison.OrdinalIgnoreCase))
            {
                var query = request.Body as GroupQuery;
                var paged = MockGroupService.GetPaged(query);
                if (paged is T typedPaged)
                {
                    return Task.FromResult(new ResultAPI<T> { Success = true, Status = StatusCode.OK, Data = typedPaged });
                }
            }

            return Task.FromResult(new ResultAPI<T>
            {
                Success = true,
                Status = StatusCode.OK,
                Data = default
            });
        }

        public Task<ResultAPI<T>> Post<T>(ApiRequestModel request, object body)
        {
            request.Body = body;
            return Post<T>(request);
        }

        public async Task<ResultAPI> Post(ApiRequestModel request, object body)
        {
            var res = await Post<object>(request, body);
            return new ResultAPI
            {
                Success = res.Success,
                Status = res.Status,
                Message = res.Message,
                Data = res.Data
            };
        }

        public Task<ResultAPI<T>> Put<T>(ApiRequestModel request, object body)
        {
            if (IsAdministrationEndpoint(request.Endpoint))
                return _transport.SendAsync<T>(HttpMethod.Put, request, body);
            var endpoint = request.Endpoint ?? string.Empty;
            if (endpoint.Contains("/Approve", StringComparison.OrdinalIgnoreCase))
            {
                var parts = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && Guid.TryParse(parts[1], out var id))
                {
                    MockGroupService.SetModerationStatus(id, ModerationStatus.Approved);
                }
            }
            else if (endpoint.Contains("/Reject", StringComparison.OrdinalIgnoreCase))
            {
                var parts = endpoint.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2 && Guid.TryParse(parts[1], out var id))
                {
                    MockGroupService.SetModerationStatus(id, ModerationStatus.Rejected);
                }
            }

            return Task.FromResult(new ResultAPI<T>
            {
                Success = true,
                Status = StatusCode.OK,
                Data = default
            });
        }

        public async Task<ResultAPI> Put(ApiRequestModel request, object body)
        {
            var res = await Put<object>(request, body);
            return new ResultAPI
            {
                Success = res.Success,
                Status = res.Status,
                Message = res.Message,
                Data = res.Data
            };
        }

        public Task<ResultAPI> Delete(ApiRequestModel request)
        {
            if (IsAdministrationEndpoint(request.Endpoint))
                return DeleteApiAsync(request);
            var endpoint = request.Endpoint ?? string.Empty;
            if (endpoint.StartsWith("/Group/", StringComparison.OrdinalIgnoreCase))
            {
                var idStr = endpoint.Substring(7).Split('?')[0];
                if (Guid.TryParse(idStr, out var gId))
                {
                    MockGroupService.Delete(gId);
                }
            }

            return Task.FromResult(new ResultAPI
            {
                Success = true,
                Status = StatusCode.OK
            });
        }

        public Task<ResultAPI<byte[]>> GetForFile(ApiRequestModel request)
        {
            return PostForFile(request, new object());
        }

        private static bool IsAdministrationEndpoint(string? endpoint) => endpoint != null
            && (endpoint.StartsWith("/administration/", StringComparison.Ordinal)
                || endpoint.StartsWith("/system-configuration/", StringComparison.Ordinal)
                || endpoint == "/session-account/password");

        public async Task<ResultAPI<byte[]>> PostForFile(ApiRequestModel request, object body)
        {
            try
            {
                var endpoint = request.Endpoint ?? string.Empty;

                if (endpoint.Contains("HoGiaDinh", StringComparison.OrdinalIgnoreCase) ||
                    endpoint.Contains("HoKhau", StringComparison.OrdinalIgnoreCase))
                {
                    var popService = _serviceProvider.GetRequiredService<IPopulationService>();
                    string? keyword = GetStringProp(body, "Keyword", "SearchKeyword", "Search");
                    string? apThon = GetStringProp(body, "ApThon", "SelectedApThon");
                    var res = await popService.GetHoGiaDinhsAsync(keyword, apThon, 1, 100000);
                    var list = res.Data?.Items ?? new List<HoGiaDinhDto>();

                    var columns = new List<ExcelColumn<HoGiaDinhDto>>
                    {
                        new("Mã Sổ Hộ", x => x.MaSoHo),
                        new("Tên Chủ Hộ", x => x.TenChuHo),
                        new("Số CCCD", x => x.CCCDChuHo),
                        new("Thôn / Ấp", x => x.ApThon),
                        new("Địa Chỉ Cư Trú", x => x.DiaChi),
                        new("Số Thành Viên", x => x.SoThanhVien),
                        new("Ghi Chú", x => x.GhiChu)
                    };

                    var bytes = _excelService.ExportToExcel("Sổ Hộ Khẩu", list, columns);
                    return new ResultAPI<byte[]> { Success = true, Status = StatusCode.OK, Data = bytes };
                }

                if (endpoint.Contains("BienDong", StringComparison.OrdinalIgnoreCase))
                {
                    var popService = _serviceProvider.GetRequiredService<IPopulationService>();
                    string? keyword = GetStringProp(body, "Keyword", "SearchKeyword", "Search");
                    var changeType = GetIntProp(body, "LoaiBienDong");
                    var fromDate = GetDateTimeProp(body, "TuNgay");
                    var toDate = GetDateTimeProp(body, "DenNgay");
                    Guid? villageId = Guid.TryParse(GetStringProp(body, "ApThonId"), out var parsedVillageId) ? parsedVillageId : null;
                    var res = await popService.GetBienDongsAsync(keyword, 1, int.MaxValue, changeType, fromDate, toDate, villageId);
                    if (!res.Success) return new ResultAPI<byte[]> { Success = false, Status = StatusCode.BadRequest, Message = res.Message };
                    var list = res.Data?.Items ?? new List<BienDongDto>();

                    var columns = new List<ExcelColumn<BienDongDto>>
                    {
                        new("Loại Biến Động", b => b.LoaiBienDong.ToString()),
                        new("Họ Tên Nhân Khẩu", b => b.HoTenNhanKhau),
                        new("Số CCCD", b => b.CCCDNhanKhau),
                        new("Ngày Phát Sinh", b => b.NgayPhatSinh.ToString("dd/MM/yyyy")),
                        new("Nơi Đến / Đi", b => b.NoiDenOrDi),
                        new("Lý Do", b => b.LyDo),
                        new("Cán Bộ Ghi Nhận", b => b.CanBoGhiNhan)
                    };

                    var bytes = _excelService.ExportToExcel("Biến Động Dân Cư", list, columns);
                    return new ResultAPI<byte[]> { Success = true, Status = StatusCode.OK, Data = bytes };
                }

                if (endpoint.Contains("AnSinh", StringComparison.OrdinalIgnoreCase) ||
                    endpoint.Contains("Welfare", StringComparison.OrdinalIgnoreCase))
                {
                    var welfareService = _serviceProvider.GetRequiredService<IWelfareService>();
                    string? keyword = GetStringProp(body, "Keyword", "SearchKeyword", "Search");
                    int? category = GetIntProp(body, "LoaiDoiTuong", "Category", "Nhom");
                    var res = await welfareService.GetDoiTuongAnSinhsAsync(keyword, category, 1, 100000);
                    var list = res.Data?.Items ?? new List<DoiTuongAnSinhDto>();

                    var columns = new List<ExcelColumn<DoiTuongAnSinhDto>>
                    {
                        new("Họ Tên Đối Tượng", x => x.HoTen),
                        new("Số CCCD", x => x.CCCD),
                        new("Thôn / Ấp", x => x.ApThon),
                        new("Phân Loại An Sinh", x => x.LoaiDoiTuong.ToString()),
                        new("Trợ Cấp / Tháng (VNĐ)", x => x.MucTroCapHangThang),
                        new("Ngày Bắt Đầu Hưởng", x => x.NgayBatDauHuong.ToString("dd/MM/yyyy")),
                        new("Trạng Thái", x => x.TrangThaiHoatDong ? "Đang hưởng" : "Tạm dừng"),
                        new("Ghi Chú", x => x.GhiChu)
                    };

                    var bytes = _excelService.ExportToExcel("An Sinh Xã Hội", list, columns);
                    return new ResultAPI<byte[]> { Success = true, Status = StatusCode.OK, Data = bytes };
                }

                if (endpoint.Contains("DichVu", StringComparison.OrdinalIgnoreCase) ||
                    endpoint.Contains("CitizenRequest", StringComparison.OrdinalIgnoreCase))
                {
                    var reqService = _serviceProvider.GetRequiredService<ICitizenRequestService>();
                    string? keyword = GetStringProp(body, "Keyword", "SearchKeyword", "Search");
                    int? status = GetIntProp(body, "TrangThai", "TrangThaiHoatDong");
                    var res = await reqService.GetYeuCausAsync(keyword, status, 1, 100000);
                    var list = res.Data?.Items ?? new List<YeuCauDto>();

                    var columns = new List<ExcelColumn<YeuCauDto>>
                    {
                        new("Mã Hồ Sơ", r => r.MaYeuCau),
                        new("Người Yêu Cầu", r => r.HoTenNguoiYeuCau),
                        new("Số CCCD", r => r.CCCDNguoiYeuCau),
                        new("Số Điện Thoại", r => r.SoDienThoai),
                        new("Loại Thủ Tục", r => r.LoaiYeuCau),
                        new("Nội Dung", r => r.NoiDung),
                        new("Trạng Thái", r => r.TrangThai.ToString()),
                        new("Cán Bộ Xử Lý", r => r.CanBoXuLy),
                        new("Ngày Gửi", r => r.NgayGui.ToString("dd/MM/yyyy HH:mm:ss"))
                    };

                    var bytes = _excelService.ExportToExcel("Dịch Vụ Công", list, columns);
                    return new ResultAPI<byte[]> { Success = true, Status = StatusCode.OK, Data = bytes };
                }

                if (endpoint.Contains("AuditLog", StringComparison.OrdinalIgnoreCase))
                {
                    var auditService = _serviceProvider.GetRequiredService<IAuditLogService>();
                    string? keyword = GetStringProp(body, "Keyword", "SearchKeyword", "Search");
                    var res = await auditService.GetAuditLogsAsync(keyword, 1, 100000);
                    var list = res.Data?.Items ?? new List<AuditLog>();

                    var columns = new List<ExcelColumn<AuditLog>>
                    {
                        new("Thời Gian", x => x.Timestamp.ToString("dd/MM/yyyy HH:mm:ss")),
                        new("Cán Bộ Thao Tác", x => x.Username),
                        new("Hành Động", x => x.Action),
                        new("Đối Tượng", x => x.EntityName),
                        new("Mã Định Danh", x => x.EntityId),
                        new("Giá Trị Cũ", x => x.OldValues),
                        new("Giá Trị Mới / Mô Tả", x => x.NewValues),
                        new("Địa Chỉ IP", x => x.IpAddress)
                    };

                    var bytes = _excelService.ExportToExcel("Nhật Ký Hệ Thống", list, columns);
                    return new ResultAPI<byte[]> { Success = true, Status = StatusCode.OK, Data = bytes };
                }

                if (endpoint.Contains("QuanLyPhien", StringComparison.OrdinalIgnoreCase) ||
                    endpoint.Contains("Session", StringComparison.OrdinalIgnoreCase))
                {
                    var sessionService = _serviceProvider.GetRequiredService<SessionAdministrationService>();
                    string? search = GetStringProp(body, "Keyword", "Search", "SearchKeyword");
                    var sessions = await sessionService.ListAsync();
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        sessions = sessions.Where(x => x.Username.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    var columns = new List<ExcelColumn<LoginSession>>
                    {
                        new("Tài Khoản", x => x.Username),
                        new("Đăng Nhập Lúc", x => x.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss")),
                        new("Hết Hạn Lúc", x => x.ExpiresAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss")),
                        new("Địa Chỉ IP", x => x.IpAddress),
                        new("Trình Duyệt", x => x.UserAgent)
                    };

                    var bytes = _excelService.ExportToExcel("Phiên Đăng Nhập", sessions, columns);
                    return new ResultAPI<byte[]> { Success = true, Status = StatusCode.OK, Data = bytes };
                }

                return new ResultAPI<byte[]>
                {
                    Success = false,
                    Status = StatusCode.NotFound,
                    Message = $"Endpoint '{request.Endpoint}' không được hỗ trợ xuất Excel."
                };
            }
            catch (Exception ex)
            {
                return new ResultAPI<byte[]>
                {
                    Success = false,
                    Status = StatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        private async Task<ResultAPI> DeleteApiAsync(ApiRequestModel request)
        {
            var result = await _transport.SendAsync<object>(HttpMethod.Delete, request);
            return new ResultAPI { Success = result.Success, Status = result.Status, Message = result.Message, Data = result.Data };
        }
        private static string? GetStringProp(object? obj, params string[] propNames)
        {
            if (obj == null) return null;
            var type = obj.GetType();
            foreach (var name in propNames)
            {
                var prop = type.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    var val = prop.GetValue(obj)?.ToString();
                    if (!string.IsNullOrWhiteSpace(val)) return val;
                }
            }
            return null;
        }

        private static DateTime? GetDateTimeProp(object? obj, string propName)
        {
            var prop = obj?.GetType().GetProperty(propName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
            return prop?.GetValue(obj) is DateTime date ? date : null;
        }

        private static int? GetIntProp(object? obj, params string[] propNames)
        {
            if (obj == null) return null;
            var type = obj.GetType();
            foreach (var name in propNames)
            {
                var prop = type.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase);
                if (prop != null)
                {
                    var val = prop.GetValue(obj);
                    if (val is int i) return i;
                    if (val != null && int.TryParse(val.ToString(), out int parsed)) return parsed;
                }
            }
            return null;
        }
    }
}
