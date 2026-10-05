using System.Collections.Generic;

namespace Service.Shared.Commons.Models
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResult<T> Ok(T data, string message = "Thành công")
        {
            return new ApiResult<T> { Success = true, Data = data, Message = message };
        }

        public static ApiResult<T> Fail(string message, List<string>? errors = null)
        {
            return new ApiResult<T> { Success = false, Message = message, Errors = errors };
        }
    }
}

