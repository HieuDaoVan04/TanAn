using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Service.Shared.Commons.Enums;
using Service.Shared.Commons.Interfaces;

namespace Service.UI.CMS.Blazor.Applications;

/// <summary>Vận chuyển HTTP cho các API quản trị; danh tính lấy từ phiên hiện tại.</summary>
public sealed class ApiServiceTransport(IHttpClientFactory clients, IConfiguration configuration,
    AuthenticationStateProvider authentication, ILogger<ApiServiceTransport> logger)
{
    public async Task<ResultAPI<T>> SendAsync<T>(HttpMethod method, ApiRequestModel request, object? body = null)
    {
        if (request.ApiService != ServicesRegistryEnum.ServiceTanAn)
            return new() { Success = false, Status = StatusCode.BadRequest, Message = "API quản trị thuộc ServiceTanAn." };
        var baseUrl = configuration["ServiceEndpoints:ServiceTanAn"];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri) || baseUri.Scheme is not ("http" or "https"))
            return new() { Success = false, Status = StatusCode.InternalServerError, Message = "Chưa cấu hình ServiceEndpoints:ServiceTanAn." };
        var user = (await authentication.GetAuthenticationStateAsync()).User;
        var sessionId = user.FindFirst(AccountService.SessionClaim)?.Value;
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrEmpty(sessionId))
            return new() { Success = false, Status = StatusCode.Unauthorized, Message = "Phiên đăng nhập không còn hợp lệ." };
        // Endpoint được nối dưới API đã cấu hình; không cho request đổi host hoặc thoát đường dẫn.
        if (!(request.Endpoint.StartsWith("/administration/", StringComparison.Ordinal)
                || request.Endpoint.StartsWith("/system-configuration/", StringComparison.Ordinal)
                || request.Endpoint == "/session-account/password")
            || request.Endpoint.Contains("..") || request.Endpoint.Contains('\\') || request.Endpoint.Contains('%'))
            return new() { Success = false, Status = StatusCode.BadRequest, Message = "Đường dẫn API quản trị không hợp lệ." };
        var url = new Uri(baseUri.AbsoluteUri.TrimEnd('/') + "/" + request.Endpoint.TrimStart('/'));
        using var message = new HttpRequestMessage(method, url);
        message.Headers.Authorization = new AuthenticationHeaderValue("TanAnSession", sessionId);
        if (body != null) message.Content = JsonContent.Create(body);
        try
        {
            using var client = clients.CreateClient("ServiceTanAn");
            using var response = await client.SendAsync(message);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("API Tân An trả về HTTP {StatusCode}: {Method} {Endpoint}",
                    (int)response.StatusCode, method, url.GetLeftPart(UriPartial.Path));
                string? detail = null;
                if (response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    try { detail = (await response.Content.ReadFromJsonAsync<ResultAPI<T>>())?.Message; }
                    catch (JsonException ex)
                    {
                        logger.LogWarning(ex, "Không đọc được JSON lỗi từ API Tân An: {Endpoint}",
                            url.GetLeftPart(UriPartial.Path));
                    }
                }
                return new() { Success = false, Status = (StatusCode)(int)response.StatusCode,
                    Message = detail ?? $"API trả về HTTP {(int)response.StatusCode}." };
            }
            return await response.Content.ReadFromJsonAsync<ResultAPI<T>>()
                ?? new() { Success = false, Status = StatusCode.InternalServerError, Message = "API trả về dữ liệu rỗng." };
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Lỗi kết nối API Tân An: {Method} {Endpoint}",
                method, url.GetLeftPart(UriPartial.Path));
            return new() { Success = false, Status = StatusCode.InternalServerError,
                Message = "Không kết nối được API Tân An. Vui lòng kiểm tra dịch vụ API và địa chỉ cấu hình." };
        }
        catch (TaskCanceledException ex)
        {
            logger.LogError(ex, "Hết thời gian chờ API Tân An hoặc yêu cầu bị hủy: {Method} {Endpoint}",
                method, url.GetLeftPart(UriPartial.Path));
            return new() { Success = false, Status = StatusCode.InternalServerError,
                Message = "Yêu cầu API Tân An đã hết thời gian chờ hoặc bị hủy. Vui lòng thử lại." };
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "API Tân An trả về JSON không hợp lệ: {Method} {Endpoint}",
                method, url.GetLeftPart(UriPartial.Path));
            return new() { Success = false, Status = StatusCode.InternalServerError,
                Message = "API Tân An trả về dữ liệu không đúng định dạng JSON mong đợi." };
        }
    }
}
