// "Một sản phẩm của HieuDV"

using System;
using System.Threading.Tasks;

namespace Service.Shared.Commons.Interfaces
{
    public enum RedisTypeKey
    {
        Session,
        Cache,
        Security,
        User
    }

    public interface ICacheService
    {
        Task<T?> GetAsync<T>(RedisTypeKey typeKey, string key);
        Task SetAsync<T>(RedisTypeKey typeKey, string key, T value, TimeSpan? expiration = null);
        Task<bool> KeyExistsAsync(RedisTypeKey typeKey, string key);
        Task RemoveAsync(RedisTypeKey typeKey, string key);
    }
}
