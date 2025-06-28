using Microsoft.Extensions.Caching.Memory;

namespace SmartHomeAutomation.API.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
        Task RemoveAsync(string key);
        Task RemoveByPatternAsync(string pattern);
    }

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;
        private readonly HashSet<string> _cacheKeys = new();
        private readonly object _lock = new();

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var value = _memoryCache.Get<T>(key);
                _logger.LogDebug("Cache {Action} for key: {Key}", value != null ? "HIT" : "MISS", key);
                return Task.FromResult(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
                return Task.FromResult<T?>(default);
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var options = new MemoryCacheEntryOptions();
                
                if (expiration.HasValue)
                {
                    options.SetAbsoluteExpiration(expiration.Value);
                }
                else
                {
                    options.SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // Default 30 minutes
                }

                options.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    lock (_lock)
                    {
                        _cacheKeys.Remove(key.ToString()!);
                    }
                    _logger.LogDebug("Cache entry evicted: {Key}, Reason: {Reason}", key, reason);
                });

                _memoryCache.Set(key, value, options);
                
                lock (_lock)
                {
                    _cacheKeys.Add(key);
                }

                _logger.LogDebug("Cache SET for key: {Key}", key);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemoveAsync(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                lock (_lock)
                {
                    _cacheKeys.Remove(key);
                }
                _logger.LogDebug("Cache REMOVE for key: {Key}", key);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemoveByPatternAsync(string pattern)
        {
            try
            {
                List<string> keysToRemove;
                lock (_lock)
                {
                    keysToRemove = _cacheKeys.Where(k => k.Contains(pattern)).ToList();
                }

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    lock (_lock)
                    {
                        _cacheKeys.Remove(key);
                    }
                }

                _logger.LogDebug("Cache REMOVE BY PATTERN: {Pattern}, Removed {Count} keys", pattern, keysToRemove.Count);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache values by pattern: {Pattern}", pattern);
                return Task.CompletedTask;
            }
        }
    }
} 