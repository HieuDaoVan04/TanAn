// "Một sản phẩm của HieuDV"

using System;
using Service.Shared.Commons.Models;

namespace Service.Shared.Contracts.DTOs
{
    public class LogHeThongDto
    {
        public int STT { get; set; }
        public string Id { get; set; } = string.Empty;
        public string? Index { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime CreatedDate { get => Created; set => Created = value; }
        public string? LogLevel { get; set; } = "Info";
        public string? Message { get; set; }
        public string? Action { get; set; }
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceVersion { get; set; }
        public string? Logger { get; set; }
        public string? TraceId { get; set; }
        public string? SpanId { get; set; }
        public string? RequestId { get; set; }
        public string? ConnectionId { get; set; }
        public string? ServerIp { get; set; }
        public string? Hostname { get; set; }
        public string? UserName { get; set; }
        public string? UserDomain { get; set; }
        public string? UrlPath { get; set; }
        public string? OsVersion { get; set; }
        public string? OsPlatform { get; set; }
        public string? AgentType { get; set; }
        public string? AgentVersion { get; set; }
        public string? ProcessName { get; set; }
        public int? ProcessId { get; set; }
        public int? ThreadId { get; set; }
        public string? ThreadName { get; set; }
        public int? EventSeverity { get; set; }
        public string? EventTimezone { get; set; }
        public string? ClientIp { get; set; }
        public string? ClientIP { get => ClientIp; set => ClientIp = value; }
        public string? ErrorMessage { get; set; }
        public string? Stacktrace { get; set; }
    }

    public class LogHeThongQuery : BaseQuery
    {
        public string? MucDo { get; set; }
        public string? ServiceName { get; set; }
        public DateTime? TimKiemTuNgay { get; set; }
        public DateTime? TimKiemDenNgay { get; set; }
        public DateTime? SearchTuNgay { get => TimKiemTuNgay; set => TimKiemTuNgay = value; }
        public DateTime? SearchDenNgay { get => TimKiemDenNgay; set => TimKiemDenNgay = value; }
        public string? LogLevel { get => MucDo; set => MucDo = value; }
        public string? UserName { get; set; }
    }
}
