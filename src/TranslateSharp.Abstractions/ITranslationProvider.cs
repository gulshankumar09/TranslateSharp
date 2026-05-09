namespace TranslateSharp.Abstractions;

public interface ITranslationProvider
{
    Task<TranslationResult> TranslateAsync(string text, string sourceLanguage, string targetLanguage, TranslationOptions? options = null, CancellationToken cancellationToken = default);
}

public record TranslationResult(string TranslatedText, string DetectedSourceLanguage, bool Success, string? ErrorMessage = null);

public class TranslationOptions
{
    public string? Formality { get; set; }
    public string? Context { get; set; }
    public bool PreserveFormatting { get; set; } = true;
}