namespace TranslateSharp.Cache;

public interface ITranslationCache
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, string value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    string GenerateKey(string sourceLanguage, string targetLanguage, string text);
}