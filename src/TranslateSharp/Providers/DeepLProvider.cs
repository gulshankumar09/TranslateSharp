using DeepL;
using Microsoft.Extensions.Logging;
using TranslateSharp.Abstractions;

namespace TranslateSharp.Providers;

public class DeepLProvider : ITranslationProvider
{
    private readonly DeepLClient _client;
    private readonly ILogger _logger;

    public DeepLProvider(HttpClient httpClient, string apiKey, ILogger<DeepLProvider> logger)
    {
        _client = new DeepLClient(apiKey);
        _logger = logger;
        _logger.LogInformation("DeepLProvider initialized with key prefix: {KeyPrefix}", apiKey.Substring(0, 8));
    }

    public async Task<TranslationResult> TranslateAsync(
        string text,
        string sourceLanguage,
        string targetLanguage,
        TranslationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Translating '{Text}' from {Source} to {Target}", text, sourceLanguage, targetLanguage);

            var result = await _client.TranslateTextAsync(
                text,
                sourceLanguage,
                targetLanguage,
                cancellationToken: cancellationToken
            );

            var translatedText = result.Text;

            _logger.LogInformation("Translation result: {Text}", translatedText);
            return new TranslationResult(translatedText, sourceLanguage, true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Translation failed for text: {Text}, Error: {Error}", text, ex.Message);
            return new TranslationResult(string.Empty, sourceLanguage, false, ex.Message);
        }
    }
}