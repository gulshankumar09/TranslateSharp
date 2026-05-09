namespace TranslateSharp.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class TranslateAttribute : Attribute
{
    public string[] TargetLanguages { get; set; } = [];
    public string SourceLanguage { get; set; } = "en";
    public string? KeyPrefix { get; set; }
}