using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Service.TanAn.API.Controllers.v1.Core;

public sealed class SystemParameterExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var status = context.Exception switch
        {
            UnauthorizedAccessException => 403,
            KeyNotFoundException => 404,
            ArgumentException or InvalidOperationException => 400,
            _ => 0
        };
        if (status == 0) return;
        context.Result = new ObjectResult(new { Message = context.Exception.Message }) { StatusCode = status };
        context.ExceptionHandled = true;
    }
}
