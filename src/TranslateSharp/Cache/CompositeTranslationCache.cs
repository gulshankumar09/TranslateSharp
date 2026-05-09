using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace TranslateSharp.Cache;

public class CompositeTranslationCache : ITranslationCache
{
    private readonly ITranslationCache? _redisCache;
    private readonly InMemoryTranslationCache _memoryCache;
    private readonly ILogger<CompositeTranslationCache> _logger;

    public CompositeTranslationCache(
        IConnectionMultiplexer? redis,
        IMemoryCache memoryCache,
        ILogger<CompositeTranslationCache> logger,
        TimeSpan defaultTtl)
    {
        var redisLogger = logger;
        _redisCache = redis != null ? new RedisTranslationCache(redis, redisLogger, defaultTtl) : null;
        _memoryCache = new InMemoryTranslationCache(memoryCache, defaultTtl);
        _logger = logger;
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_redisCache != null)
        {
            var result = await _redisCache.GetAsync(key, cancellationToken);
            if (result != null)
                return result;
        }

        return await _memoryCache.GetAsync(key, cancellationToken);
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        await _memoryCache.SetAsync(key, value, expiration, cancellationToken);

        if (_redisCache != null)
        {
            try
            {
                await _redisCache.SetAsync(key, value, expiration, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to write to Redis cache, continuing with memory cache only: {Error}", ex.Message);
            }
        }
    }

    public string GenerateKey(string sourceLanguage, string targetLanguage, string text)
    {
        var hash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text)))
            .Replace("/", "_").Replace("+", "-")[..16];
        return $"ts:lang:{sourceLanguage}:{targetLanguage}:{hash}";
    }
}

public class RedisTranslationCache : ITranslationCache
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger _logger;
    private readonly TimeSpan _defaultTtl;

    public RedisTranslationCache(IConnectionMultiplexer redis, ILogger logger, TimeSpan defaultTtl)
    {
        _redis = redis;
        _logger = logger;
        _defaultTtl = defaultTtl;
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        return await db.StringGetAsync(key);
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(key, value, expiration ?? _defaultTtl);
    }

    public string GenerateKey(string sourceLanguage, string targetLanguage, string text)
    {
        var hash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text)))
            .Replace("/", "_").Replace("+", "-")[..16];
        return $"ts:lang:{sourceLanguage}:{targetLanguage}:{hash}";
    }
}

public class InMemoryTranslationCache : ITranslationCache
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultTtl;

    public InMemoryTranslationCache(IMemoryCache cache, TimeSpan defaultTtl)
    {
        _cache = cache;
        _defaultTtl = defaultTtl;
    }

    public Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.Get<string>(key));
    }

    public Task SetAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        _cache.Set(key, value, expiration ?? _defaultTtl);
        return Task.CompletedTask;
    }

    public string GenerateKey(string sourceLanguage, string targetLanguage, string text)
    {
        var hash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(text)))
            .Replace("/", "_").Replace("+", "-")[..16];
        return $"ts:lang:{sourceLanguage}:{targetLanguage}:{hash}";
    }
}