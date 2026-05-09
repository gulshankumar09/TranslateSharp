using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TranslateSharp.Abstractions;
using TranslateSharp.Cache;
using TranslateSharp.Core;
using TranslateSharp.Filters;
using TranslateSharp.Providers;

namespace Microsoft.Extensions.DependencyInjection;

public static class TranslateSharpServiceCollectionExtensions
{
    public static IServiceCollection AddTranslateSharp(
        this IServiceCollection services,
        Action<TranslateSharpOptions> configureOptions)
    {
        services.Configure(configureOptions);

        services.AddSingleton<TranslateSharpOptions>(sp =>
        {
            var options = new TranslateSharpOptions();
            configureOptions(options);
            return options;
        });

        services.AddSingleton<IMemoryCache>(sp => new MemoryCache(new MemoryCacheOptions()));

        services.AddSingleton<ITranslationCache>(sp =>
        {
            var options = sp.GetRequiredService<TranslateSharpOptions>();
            IConnectionMultiplexer? redis = null;

            if (!string.IsNullOrEmpty(options.RedisConnection))
            {
                try
                {
                    redis = ConnectionMultiplexer.Connect(options.RedisConnection);
                }
                catch { }
            }

            var memoryCache = sp.GetRequiredService<IMemoryCache>();
            var logger = sp.GetRequiredService<ILogger<CompositeTranslationCache>>();

            return new CompositeTranslationCache(redis, memoryCache, logger, options.CacheTTL);
        });

        services.AddSingleton<ITranslationProvider>(sp =>
        {
            var options = sp.GetRequiredService<TranslateSharpOptions>();
            if (string.IsNullOrEmpty(options.DeepLApiKey))
                throw new InvalidOperationException("DeepLApiKey is required");

            var handler = new SocketsHttpHandler
            {
                Proxy = null,
                UseProxy = false
            };
            var httpClient = new HttpClient(handler);
            var logger = sp.GetRequiredService<ILogger<DeepLProvider>>();
            return new DeepLProvider(httpClient, options.DeepLApiKey, logger);
        });

        services.AddSingleton<TranslationService>();

        services.AddScoped<TranslateResponseAttributeFilter>();

        return services;
    }

    public static IMvcBuilder AddTranslateSharpFilters(this IMvcBuilder builder)
    {
        builder.Services.AddScoped<TranslateResponseAttributeFilter>();
        return builder;
    }
}