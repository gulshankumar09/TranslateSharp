using TranslateSharp;
using TranslateSharp.Attributes;
using TranslateSharp.Core;
using TranslateSharp.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<TranslateResponseAttributeFilter>();
});

builder.Services.AddTranslateSharp(options =>
{
    options.DefaultTargetLanguages = new[] { "ar" };
    options.CacheEnabled = true;
    options.DeepLApiKey = Environment.GetEnvironmentVariable("DEEPL_API_KEY") ?? "ad62375b-0701-4997-9515-246516275057";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// DTO with language-specific properties for translations
public class ProductDto
{
    public int Id { get; set; }

    [Translate(TargetLanguages = new[] { "ar", "fr" })]
    public string Name { get; set; } = string.Empty;
    public string Name_ar { get; set; } = string.Empty;
    public string Name_fr { get; set; } = string.Empty;

    [Translate(TargetLanguages = new[] { "fr" })]
    public string Description { get; set; } = string.Empty;
    public string Description_fr { get; set; } = string.Empty;

    [TranslateIgnore]
    public string InternalNotes { get; set; } = string.Empty;
}