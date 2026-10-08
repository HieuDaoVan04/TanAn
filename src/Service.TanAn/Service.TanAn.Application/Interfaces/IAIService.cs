using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.Application.Interfaces
{
    public interface IAIService
    {
        Task<ApiResult<List<DuplicateRecordResultDto>>> DetectDuplicatesAsync();
        Task<ApiResult<List<AnomalyRecordResultDto>>> DetectAnomaliesAsync();
        Task<ApiResult<RequestClassifyResponseDto>> ClassifyRequestAsync(RequestClassifyForm form);
        AssistantStatusDto GetAssistantStatus();
        Task<ApiResult<ChatbotProcedureResponse>> ChatProcedureAsync(ChatbotProcedureRequest request, System.Threading.CancellationToken cancellationToken = default);
    }
}


