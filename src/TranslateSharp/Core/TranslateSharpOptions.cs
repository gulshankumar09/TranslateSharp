namespace TranslateSharp.Core;

public class TranslateSharpOptions
{
    public string[] DefaultTargetLanguages { get; set; } = [];
    public string DefaultSourceLanguage { get; set; } = "en";
    public bool CacheEnabled { get; set; } = true;
    public TimeSpan CacheTTL { get; set; } = TimeSpan.FromHours(24);
    public string? RedisConnection { get; set; }
    public string? DeepLApiKey { get; set; }
}