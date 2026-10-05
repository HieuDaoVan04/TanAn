// "Một sản phẩm của HieuDV"

using System.Threading.Tasks;

namespace Service.Shared.Commons.Services
{
    public class LoggingThaoTacQueue
    {
        public Task EnqueueAsync(object log)
        {
            return Task.CompletedTask;
        }
    }
}
