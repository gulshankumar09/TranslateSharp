# TranslateSharp

A .NET library for automatic translation of model fields using attributes, with DeepL provider support and Redis caching.

## Features

- **Attribute-based translation**: Mark properties with `[Translate]` attribute
- **Multiple languages**: Support for multiple target languages per field
- **DeepL integration**: Uses official DeepL.net SDK
- **Caching**: Redis (primary) + In-Memory (fallback) composite cache
- **ASP.NET Core filter**: Automatic translation on API responses

## Installation

```bash
dotnet add package TranslateSharp
```

## Quick Start

### 1. Mark your DTO for translation

```csharp
using TranslateSharp.Attributes;

public class ProductDto
{
    public int Id { get; set; }

    [Translate(TargetLanguages = new[] { "de", "fr" })]
    public string Name { get; set; }

    [Translate(TargetLanguages = new[] { "ar" })]
    public string Description { get; set; }

    [TranslateIgnore]
    public string InternalNotes { get; set; }
}

// Add language-specific properties for translated values
public class ProductDto
{
    public int Id { get; set; }

    [Translate(TargetLanguages = new[] { "de", "fr" })]
    public string Name { get; set; }
    public string Name_de { get; set; }  // German translation
    public string Name_fr { get; set; }  // French translation

    [TranslateIgnore]
    public string InternalNotes { get; set; }
}
```

### 2. Configure services

```csharp
builder.Services.AddTranslateSharp(options =>
{
    options.DefaultTargetLanguages = new[] { "de", "fr" };
    options.CacheEnabled = true;
    options.RedisConnection = "localhost:6379";
    options.DeepLApiKey = Environment.GetEnvironmentVariable("DEEPL_API_KEY");
});
```

### 3. Add translation filter

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<TranslateResponseAttributeFilter>();
});
```

### 4. Use in controller

```csharp
using TranslateSharp.Filters;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id}")]
    [TranslateResponse]
    public ActionResult<ProductDto> GetProduct(int id)
    {
        return Ok(new ProductDto
        {
            Id = id,
            Name = "Sample Product",
            Description = "Product description",
            InternalNotes = "Internal"
        });
    }
}
```

## Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `DefaultTargetLanguages` | `string[]` | `[]` | Default languages for translation |
| `DefaultSourceLanguage` | `string` | `"en"` | Source language |
| `CacheEnabled` | `bool` | `true` | Enable caching |
| `CacheTTL` | `TimeSpan` | `24h` | Cache expiration |
| `RedisConnection` | `string` | `null` | Redis connection string |
| `DeepLApiKey` | `string` | required | DeepL API key |

## Attributes

| Attribute | Description |
|-----------|-------------|
| `[Translate]` | Marks property for translation |
| `[Translate(TargetLanguages = new[] { "de", "fr" })]` | Specify target languages |
| `[TranslateIgnore]` | Exclude property from translation |
| `[TranslateResponse]` | Add to controller/action for auto-translation |

## Output Format

Translated values are stored in `{PropertyName}_{language}` format:
- `Name` → `Name_de`, `Name_fr`, `Name_ar`

## Architecture

```
TranslateSharp/
├── TranslateSharp.Abstractions/   # ITranslationProvider interface
├── TranslateSharp/
│   ├── Attributes/               # [Translate], [TranslateIgnore]
│   ├── Providers/                # DeepL provider
│   ├── Cache/                    # Redis + InMemory cache
│   ├── Core/                     # TranslationService
│   └── Filters/                  # Response filter
└── samples/
    └── TranslateSharp.SampleApi/
```

## Building

```bash
dotnet build
dotnet pack -c Release
```

## License

MIT