// "Một sản phẩm của HieuDV"

using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using Service.Shared.Commons.Interfaces;

namespace Service.Shared.Commons.Services
{
    public class MemoryCacheService : ICacheService
    {
        private class CacheEntry
        {
            public string JsonValue { get; set; } = string.Empty;
            public DateTime? ExpirationTime { get; set; }
        }

        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new ConcurrentDictionary<string, CacheEntry>();

        private string FormatKey(RedisTypeKey typeKey, string key) => $"{typeKey}:{key}";

        public Task<T?> GetAsync<T>(RedisTypeKey typeKey, string key)
        {
            var fullKey = FormatKey(typeKey, key);
            if (_cache.TryGetValue(fullKey, out var entry))
            {
                if (entry.ExpirationTime.HasValue && entry.ExpirationTime.Value < DateTime.UtcNow)
                {
                    _cache.TryRemove(fullKey, out _);
                    return Task.FromResult<T?>(default);
                }

                if (string.IsNullOrEmpty(entry.JsonValue))
                    return Task.FromResult<T?>(default);

                var result = JsonSerializer.Deserialize<T>(entry.JsonValue);
                return Task.FromResult(result);
            }

            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(RedisTypeKey typeKey, string key, T value, TimeSpan? expiration = null)
        {
            var fullKey = FormatKey(typeKey, key);
            var entry = new CacheEntry
            {
                JsonValue = JsonSerializer.Serialize(value),
                ExpirationTime = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
            };

            _cache[fullKey] = entry;
            return Task.CompletedTask;
        }

        public Task<bool> KeyExistsAsync(RedisTypeKey typeKey, string key)
        {
            var fullKey = FormatKey(typeKey, key);
            if (_cache.TryGetValue(fullKey, out var entry))
            {
                if (entry.ExpirationTime.HasValue && entry.ExpirationTime.Value < DateTime.UtcNow)
                {
                    _cache.TryRemove(fullKey, out _);
                    return Task.FromResult(false);
                }
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public Task RemoveAsync(RedisTypeKey typeKey, string key)
        {
            var fullKey = FormatKey(typeKey, key);
            _cache.TryRemove(fullKey, out _);
            return Task.CompletedTask;
        }
    }
}
