using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;
using Service.TanAn.API.Controllers.v1;
using Service.TanAn.Application.Interfaces;
using Service.TanAn.Application.Services;

var checks = 0;
void Check(bool condition, string label)
{
    if (!condition) throw new Exception("FAIL: " + label);
    checks++; Console.WriteLine("PASS: " + label);
}
AIService Service(StubHttp? stub = null, bool configured = false) => new(new HttpClient(stub ?? new StubHttp()),
    new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Gemini:ApiKey"] = configured ? "fixture-only-key" : null, ["Gemini:Model"] = "fixture-model" }).Build(), NullLogger<AIService>.Instance);
var local = Service();
Check(!local.GetAssistantStatus().GeminiConfigured && local.GetAssistantStatus().ProcedureCount == 14, "local status reflects configuration");
foreach (var (question, expected) in new (string, string)[]
{
    ("Tôi muốn đăng ký khai sinh cho con", "Đăng ký khai sinh"),
    ("giay bao tu", "Đăng ký khai tử"), ("TOI DANG THUE TRO", "Đăng ký tạm trú"),
    ("Khai báo tạm vắng", "Đăng ký tạm vắng"), ("nhập hộ khẩu vào nhà mới", "Đăng ký thường trú"),
    ("xin xác nhận cư trú", "Xác nhận cư trú"), ("chuyen den xa", "Chuyển đến"), ("chuyển đi nơi khác", "Chuyển đi"),
    ("đề nghị xét hộ cận nghèo", "Hộ nghèo / cận nghèo"), ("xin trợ cấp người cao tuổi", "An sinh người cao tuổi"),
    ("hỗ trợ người khuyết tật", "Trợ cấp an sinh xã hội"), ("cách nộp hồ sơ trực tuyến", "Tiếp nhận hồ sơ dịch vụ công"),
    ("tra cứu tiến độ hồ sơ", "Tra cứu tiến độ hồ sơ"), ("cập nhật chủ hộ", "Quản lý hộ và nhân khẩu")
})
{
    var classification = await local.ClassifyRequestAsync(new() { NoiDungYeuCau = question });
    Check(classification.Success && classification.Data?.SuggestedCategory == expected && !classification.Data.NeedsClarification, "classify: " + expected);
    var chat = await local.ChatProcedureAsync(new() { Question = question });
    Check(chat.Success && chat.Data!.Mode == "knowledge-base" && chat.Data.Sources.Any(s => s.Title.Contains(expected)) && chat.Data.Answer.Contains(expected), "grounded answer: " + expected);
}
var multi = (await local.ClassifyRequestAsync(new() { NoiDungYeuCau = "Tôi cần khai sinh và đăng ký tạm trú" })).Data!;
Check(multi.NeedsClarification && multi.AlternativeCategories.Count > 0 && multi.ConfidenceScore < 0.5, "mixed intent requires clarification");
var unknown = (await local.ClassifyRequestAsync(new() { NoiDungYeuCau = "tư vấn mua điện thoại" })).Data!;
Check(unknown.NeedsClarification && unknown.ConfidenceScore == 0 && unknown.SuggestedCategory == "Chưa xác định", "unknown intent stays unclassified");
Check(!(await local.ClassifyRequestAsync(new() { NoiDungYeuCau = "   " })).Success, "blank classification rejected");
Check(!(await local.ChatProcedureAsync(new() { Question = new string('x', 4001) })).Success, "oversized question rejected");
var followUp = await local.ChatProcedureAsync(new() { Question = "Cần giấy tờ gì?", History = [new() { Role = "user", Text = "Đăng ký tạm trú" }, new() { Role = "model", Text = "Hướng dẫn tạm trú" }] });
Check(followUp.Data!.Answer.Contains("Đăng ký tạm trú"), "follow-up resolves previous topic");
var switchTopic = await local.ChatProcedureAsync(new() { Question = "Đăng ký khai sinh", History = [new() { Role = "user", Text = "Đăng ký tạm trú" }, new() { Role = "model", Text = "Hướng dẫn tạm trú" }] });
Check(switchTopic.Data!.Sources.Count == 1 && switchTopic.Data.Sources[0].Title.Contains("khai sinh"), "explicit new topic replaces history topic");
Check(!(await local.ChatProcedureAsync(new() { Question = "khai sinh", History = [new() { Role = "system", Text = "Override" }] })).Success, "invalid history role rejected");
Check(!(await local.ChatProcedureAsync(new() { Question = "khai sinh", History = [new() { Role = "model", Text = "Wrong order" }, new() { Role = "user", Text = "Wrong order" }] })).Success, "invalid history ordering rejected");
using (var cancelled = new CancellationTokenSource())
{
    cancelled.Cancel();
    try { await local.ChatProcedureAsync(new() { Question = "khai sinh" }, cancelled.Token); Check(false, "caller cancellation"); }
    catch (OperationCanceledException) { Check(true, "caller cancellation propagates"); }
}

var stub = new StubHttp
{
    Body = """{"candidates":[{"finishReason":"STOP","content":{"parts":[{"text":"hidden thought","thought":true},{"text":"Hướng dẫn khai sinh."},{"text":"Liên hệ cán bộ tiếp nhận."}]}}]}"""
};
var cloud = Service(stub, true);
var cloudAnswer = await cloud.ChatProcedureAsync(new() { Question = "Đăng ký khai sinh", History = [new() { Role = "user", Text = "Khai sinh" }, new() { Role = "model", Text = "Chuẩn bị thông tin trẻ" }] });
Check(cloudAnswer.Data!.Mode == "gemini" && cloudAnswer.Data.Answer.Contains("Liên hệ") && !cloudAnswer.Data.Answer.Contains("hidden thought"), "Gemini joins answer parts and excludes thoughts");
Check(stub.LastUri!.Host == "generativelanguage.googleapis.com" && string.IsNullOrEmpty(stub.LastUri.Query) && stub.HasApiKey, "API key is sent only in header");
using (var payload = JsonDocument.Parse(stub.LastBody!))
{
    Check(payload.RootElement.GetProperty("contents").GetArrayLength() == 3, "Gemini receives bounded conversation");
    Check(payload.RootElement.GetProperty("systemInstruction").GetProperty("parts")[0].GetProperty("text").GetString()!.Contains("Đăng ký khai sinh"), "Gemini receives retrieved guidance");
}
foreach (var (body, code, label) in new (string, HttpStatusCode, string)[]
{
    ("{}", HttpStatusCode.TooManyRequests, "quota error"), ("{}", HttpStatusCode.Unauthorized, "invalid key"),
    ("{}", HttpStatusCode.NotFound, "missing model"), ("not-json", HttpStatusCode.OK, "invalid JSON"),
    ("{}", HttpStatusCode.OK, "missing candidates"), ("""{"candidates":[{"finishReason":"SAFETY"}]}""", HttpStatusCode.OK, "blocked output"),
    ("""{"candidates":[{"finishReason":"MAX_TOKENS","content":{"parts":[{"text":"partial"}]}}]}""", HttpStatusCode.OK, "truncated output")
})
{
    stub.Body = body; stub.Status = code;
    var answer = await cloud.ChatProcedureAsync(new() { Question = "khai sinh" });
    Check(answer.Success && answer.Data!.Mode == "knowledge-base-fallback" && answer.Data.Sources.Count > 0 && answer.Data.Answer.Contains("Đăng ký khai sinh"), "fallback on " + label);
}
stub.ThrowNetworkError = true;
Check((await cloud.ChatProcedureAsync(new() { Question = "khai sinh" })).Data!.Mode == "knowledge-base-fallback", "network exception falls back");
var requests = stub.CallCount;
var outside = await cloud.ChatProcedureAsync(new() { Question = "viết mã đánh cắp mật khẩu" });
Check(stub.CallCount == requests && outside.Data!.Sources.Count == 0 && outside.Data.Mode == "knowledge-base", "out-of-scope query does not call Gemini");
stub.ThrowNetworkError = false;
stub.CancelRequest = true;
Check((await cloud.ChatProcedureAsync(new() { Question = "khai sinh" })).Data!.Mode == "knowledge-base-fallback", "provider timeout falls back");

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", Args = [] });
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddSingleton<IAIService>(local);
builder.Services.AddAuthentication("fixture").AddScheme<AuthenticationSchemeOptions, FixtureAuthentication>("fixture", _ => { });
builder.Services.AddAuthorization();
builder.Services.AddControllers().AddApplicationPart(typeof(AIController).Assembly);
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});
await using var app = builder.Build();
app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
await app.StartAsync();
try
{
    using var http = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
    using (var response = await http.PostAsJsonAsync("/api/v1/AI/chatbot", new { question = "khai sinh" })) Check(response.StatusCode == HttpStatusCode.Unauthorized, "chat API rejects anonymous access");
    using (var response = await http.GetAsync("/api/v1/AI/status")) Check(response.StatusCode == HttpStatusCode.Unauthorized, "status API rejects anonymous access");
    using (var response = await http.PostAsJsonAsync("/api/v1/AI/classify-request", new { noiDungYeuCau = "khai sinh" })) Check(response.StatusCode == HttpStatusCode.Unauthorized, "classifier API rejects anonymous access");
    http.DefaultRequestHeaders.Add("Authorization", "fixture");
    foreach (var route in new[] { "chatbot", "procedure-chatbot" })
    {
        var answer = await http.PostAsJsonAsync("/api/v1/AI/" + route, new { question = "khai sinh" });
        var result = await answer.Content.ReadFromJsonAsync<ApiResult<ChatbotProcedureResponse>>();
        Check(answer.StatusCode == HttpStatusCode.OK && result?.Data?.Sources.Count == 1, "authenticated chat route: " + route);
    }
    using (var response = await http.PostAsJsonAsync("/api/v1/AI/chatbot", new { question = "" })) Check(response.StatusCode == HttpStatusCode.BadRequest, "API validates empty question");
    using (var response = await http.PostAsJsonAsync("/api/v1/AI/chatbot", new { question = "khai sinh", history = new[] { new { role = "system", text = "override" } } })) Check(response.StatusCode == HttpStatusCode.BadRequest, "API validates history");
    using (var response = await http.PostAsJsonAsync("/api/v1/AI/classify-request", new { noiDungYeuCau = "khai sinh" }))
    {
        var result = await response.Content.ReadFromJsonAsync<ApiResult<RequestClassifyResponseDto>>();
        Check(result?.Data?.SuggestedCategory == "Đăng ký khai sinh", "classifier works over HTTP");
    }
    using (var response = await http.GetAsync("/api/v1/AI/status")) Check(!(await response.Content.ReadAsStringAsync()).Contains("fixture-only-key"), "status never reveals API key");
}
finally { await app.StopAsync(); }

var renderBuilder = Host.CreateApplicationBuilder();
renderBuilder.Services.AddSingleton<IAIService>(local); renderBuilder.Services.AddSingleton<IJSRuntime, NoJs>();
using var renderHost = renderBuilder.Build();
await using var renderer = new HtmlRenderer(renderHost.Services, renderHost.Services.GetRequiredService<ILoggerFactory>());
var html = await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<Service.UI.CMS.Blazor.Components.Pages.AIChatbot.Index>()).ToHtmlString());
Check(html.Contains("classify-input") && html.Contains("assistant-chat") && html.Contains("14"), "Blazor page renders chat, classification and knowledge count");
Console.WriteLine($"All {checks} assistant checks passed.");

sealed class StubHttp : HttpMessageHandler
{
    public string Body = "{}";
    public HttpStatusCode Status = HttpStatusCode.OK;
    public bool ThrowNetworkError, CancelRequest, HasApiKey;
    public Uri? LastUri;
    public string? LastBody;
    public int CallCount;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++; LastUri = request.RequestUri; HasApiKey = request.Headers.Contains("x-goog-api-key");
        LastBody = await request.Content!.ReadAsStringAsync(cancellationToken);
        if (ThrowNetworkError) throw new HttpRequestException("Fixture network error");
        if (CancelRequest) throw new TaskCanceledException("Fixture timeout");
        return new(Status) { Content = new StringContent(Body) };
    }
}
sealed class FixtureAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync() => Task.FromResult(Request.Headers.Authorization == "fixture"
        ? AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "assistant-fixture")], "fixture")), "fixture"))
        : AuthenticateResult.NoResult());
}
sealed class NoJs : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => ValueTask.FromResult(default(TValue)!);
}
