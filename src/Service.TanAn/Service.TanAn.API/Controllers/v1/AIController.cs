using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Service.TanAn.Application.Interfaces;
using Service.Shared.Commons.Models;
using Service.Shared.Contracts.DTOs;

namespace Service.TanAn.API.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AIController : ControllerBase
    {
        private readonly IAIService _aiService;

        public AIController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpGet("duplicates")]
        public async Task<ActionResult<ApiResult<List<DuplicateRecordResultDto>>>> DetectDuplicates()
        {
            var res = await _aiService.DetectDuplicatesAsync();
            return Ok(res);
        }

        [HttpGet("anomalies")]
        public async Task<ActionResult<ApiResult<List<AnomalyRecordResultDto>>>> DetectAnomalies()
        {
            var res = await _aiService.DetectAnomaliesAsync();
            return Ok(res);
        }

        [HttpPost("classify-request")]
        [Authorize]
        public async Task<ActionResult<ApiResult<RequestClassifyResponseDto>>> ClassifyRequest([FromBody] RequestClassifyForm form)
        {
            var res = await _aiService.ClassifyRequestAsync(form);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpPost("procedure-chatbot")]
        [HttpPost("chatbot")]
        [Authorize]
        public async Task<ActionResult<ApiResult<ChatbotProcedureResponse>>> ProcedureChatbot([FromBody] ChatbotProcedureRequest request, System.Threading.CancellationToken cancellationToken)
        {
            var res = await _aiService.ChatProcedureAsync(request, cancellationToken);
            return res.Success ? Ok(res) : BadRequest(res);
        }

        [HttpGet("status")]
        [Authorize]
        public ActionResult<ApiResult<AssistantStatusDto>> Status() => Ok(ApiResult<AssistantStatusDto>.Ok(_aiService.GetAssistantStatus()));
    }
}


