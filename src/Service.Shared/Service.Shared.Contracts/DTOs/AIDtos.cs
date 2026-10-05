using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Service.Shared.Contracts.DTOs
{
    public class DuplicateRecordResultDto
    {
        public Guid NhanKhauId1 { get; set; }
        public string HoTen1 { get; set; } = string.Empty;
        public string CCCD1 { get; set; } = string.Empty;
        public Guid NhanKhauId2 { get; set; }
        public string HoTen2 { get; set; } = string.Empty;
        public string CCCD2 { get; set; } = string.Empty;
        public double SimilarityPercentage { get; set; }
        public string MatchReason { get; set; } = string.Empty;
    }

    public class AnomalyRecordResultDto
    {
        public Guid NhanKhauId { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
        public string AnomalyType { get; set; } = string.Empty; // e.g. TuoiKhongHopLe, DaKhaiTuVanGiaoDich, TrungSoHoKhau
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "High"; // Low, Medium, High
    }

    public class RequestClassifyForm
    {
        [Required, StringLength(4000)]
        public string NoiDungYeuCau { get; set; } = string.Empty;
    }

    public class RequestClassifyResponseDto
    {
        public string SuggestedCategory { get; set; } = string.Empty; // Khai sinh, Tạm trú, An sinh, Xác nhận cư trú
        public double ConfidenceScore { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public bool NeedsClarification { get; set; }
        public List<string> AlternativeCategories { get; set; } = new();
        public string Method { get; set; } = "keyword-rules";
    }

    public class ChatbotProcedureRequest
    {
        [Required, StringLength(4000)]
        public string Question { get; set; } = string.Empty;
        [MaxLength(12)]
        public List<AssistantConversationTurn> History { get; set; } = new();
    }

    public class AssistantConversationTurn
    {
        [Required, RegularExpression("^(user|model)$")]
        public string Role { get; set; } = "user";
        [Required, StringLength(4000)]
        public string Text { get; set; } = string.Empty;
    }

    public class ChatbotProcedureResponse
    {
        public string Answer { get; set; } = string.Empty;
        public List<string>? RelatedProcedures { get; set; }
        public string Mode { get; set; } = "knowledge-base";
        public string Notice { get; set; } = string.Empty;
        public List<AssistantSourceDto> Sources { get; set; } = new();
    }

    public class AssistantSourceDto
    {
        public string Title { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string UpdatedOn { get; set; } = string.Empty;
    }

    public class AssistantStatusDto
    {
        public bool GeminiConfigured { get; set; }
        public string Model { get; set; } = string.Empty;
        public int ProcedureCount { get; set; }
        public string KnowledgeVersion { get; set; } = string.Empty;
    }
}

