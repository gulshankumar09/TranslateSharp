using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TranslateSharp.Abstractions;
using TranslateSharp.Attributes;
using TranslateSharp.Cache;

namespace TranslateSharp.Core;

public class TranslationService
{
    private readonly ITranslationProvider _provider;
    private readonly ITranslationCache _cache;
    private readonly TranslateSharpOptions _options;
    private readonly ILogger<TranslationService> _logger;

    public TranslationService(
        ITranslationProvider provider,
        ITranslationCache cache,
        TranslateSharpOptions options,
        ILogger<TranslationService> logger)
    {
        _provider = provider;
        _cache = cache;
        _options = options;
        _logger = logger;
    }

    public async Task TranslateObjectAsync(object obj, CancellationToken cancellationToken = default)
    {
        if (obj == null) return;

        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            if (!prop.CanRead || !prop.CanWrite) continue;

            var translateAttr = prop.GetCustomAttribute<TranslateAttribute>();
            var ignoreAttr = prop.GetCustomAttribute<TranslateIgnoreAttribute>();

            if (ignoreAttr != null || translateAttr == null) continue;

            var value = prop.GetValue(obj);
            if (value is not string strValue || string.IsNullOrEmpty(strValue)) continue;

            var targetLanguages = translateAttr.TargetLanguages.Length > 0
                ? translateAttr.TargetLanguages
                : _options.DefaultTargetLanguages;

            var sourceLang = translateAttr.SourceLanguage ?? _options.DefaultSourceLanguage;

            foreach (var targetLang in targetLanguages)
            {
                var translated = await TranslateTextAsync(strValue, sourceLang, targetLang, cancellationToken);
                SetNestedTranslation(obj, prop.Name, targetLang, translated);
            }
        }
    }

    private void SetNestedTranslation(object obj, string propertyName, string language, string translatedValue)
    {
        var nestedProp = obj.GetType().GetProperty($"{propertyName}_{language}", BindingFlags.Public | BindingFlags.Instance)
            ?? obj.GetType().GetProperty(propertyName + language, BindingFlags.Public | BindingFlags.Instance);

        if (nestedProp != null && nestedProp.CanWrite)
        {
            nestedProp.SetValue(obj, translatedValue);
        }
    }

    public async Task<string> TranslateTextAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var cacheKey = _cache.GenerateKey(sourceLanguage, targetLanguage, text);

        if (_options.CacheEnabled)
        {
            var cached = await _cache.GetAsync(cacheKey, cancellationToken);
            if (cached != null)
            {
                _logger.LogDebug("Cache hit for key: {Key}", cacheKey);
                return cached;
            }
        }

        var result = await _provider.TranslateAsync(text, sourceLanguage, targetLanguage, null, cancellationToken);

        if (result.Success && _options.CacheEnabled)
        {
            await _cache.SetAsync(cacheKey, result.TranslatedText, _options.CacheTTL, cancellationToken);
        }

        return result.TranslatedText;
    }
}