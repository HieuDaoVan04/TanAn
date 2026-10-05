// "Một sản phẩm của HieuDV"

using System.Threading.Tasks;
using Service.Shared.Commons.Enums;

namespace Service.Shared.Commons.Interfaces
{
    public enum StatusCode
    {
        OK = 200,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        InternalServerError = 500
    }

    public class ApiRequestModel
    {
        public ServicesRegistryEnum ApiService { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public object? Body { get; set; }
    }

    public class ResultAPI<T>
    {
        public bool Success { get; set; } = true;
        public StatusCode Status { get; set; } = StatusCode.OK;
        public string? Message { get; set; }
        public T? Data { get; set; }
    }

    public class ResultAPI : ResultAPI<object>
    {
    }

    public interface ICallServiceRegistry
    {
        Task<ResultAPI<T>> Get<T>(ApiRequestModel request);
        Task<ResultAPI<T>> Post<T>(ApiRequestModel request);
        Task<ResultAPI<T>> Post<T>(ApiRequestModel request, object body);
        Task<ResultAPI> Post(ApiRequestModel request, object body);
        Task<ResultAPI<T>> Put<T>(ApiRequestModel request, object body);
        Task<ResultAPI> Put(ApiRequestModel request, object body);
        Task<ResultAPI> Delete(ApiRequestModel request);
        Task<ResultAPI<byte[]>> PostForFile(ApiRequestModel request, object body);
        Task<ResultAPI<byte[]>> GetForFile(ApiRequestModel request);
    }
}
