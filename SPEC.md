# TranslateSharp - .NET Translation Package Specification

## 1. Project Overview

- **Project Name**: TranslateSharp
- **Package Version**: 1.0.0 (targeting .NET 8/10)
- **Type**: NuGet Library Package
- **Core Functionality**: Attribute-based automatic translation of model fields with multi-provider support (DeepL) and Redis caching

## 2. Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    TranslateSharp                           │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │  Attributes │  │   Providers │  │      Cache          │ │
│  │ - Translate │  │ - DeepL     │  │ - Redis (primary)   │ │
│  │ - TranslateIgnore │  │ - (extensible)│  │ - InMemory (fallback)│ │
│  └─────────────┘  └─────────────┘  └─────────────────────┘ │
│         │                │                    │            │
│         └────────────────┼────────────────────┘            │
│                          ▼                                  │
│              ┌───────────────────────┐                      │
│              │   TranslationService  │                      │
│              │   (Core Engine)       │                      │
│              └───────────────────────┘                      │
│                          ▼                                  │
│              ┌───────────────────────┐                      │
│              │  ResponseInterceptor │                      │
│              │  (Auto-translate on   │                      │
│              │   response)           │                      │
│              └───────────────────────┘                      │
└─────────────────────────────────────────────────────────────┘
```

## 3. Component Details

### 3.1 Attributes

| Attribute | Target | Description |
|-----------|--------|-------------|
| `TranslateAttribute` | Property/Field | Marks field for translation. Options: target languages, source language, key prefix |
| `TranslateIgnoreAttribute` | Property/Field | Excludes field from translation |
| `TranslateContextAttribute` | Class | Marks entire class for translation scanning |

### 3.2 Providers

| Provider | Interface | Features |
|----------|-----------|----------|
| DeepL | `ITranslationProvider` | API key auth, formality options, context preservation |
| Extensible | `ITranslationProvider` | Interface for custom providers |

### 3.3 Cache Strategy

- **Primary**: Redis (StackExchange.Redis)
- **Fallback**: In-Memory (Microsoft.Extensions.Caching.Memory)
- **Key Format**: `ts:lang:{source}:{target}:{key}:{hash}`
- **TTL**: Configurable (default 24 hours)

### 3.4 Response Interceptor

- ASP.NET Core Filter for automatic translation on JSON serialization

## 4. API Usage

```csharp
// Mark fields for translation
public class ProductDto
{
    public int Id { get; set; }

    [Translate(TargetLanguages = new[] { "de", "fr" })]
    public string Name { get; set; }

    [Translate(TargetLanguages = new[] { "de" })]
    public string Description { get; set; }

    [TranslateIgnore]
    public string InternalNotes { get; set; }
}

// Configure services
services.AddTranslateSharp(options =>
{
    options.DefaultTargetLanguages = new[] { "de", "fr", "es" };
    options.CacheEnabled = true;
    options.RedisConnection = "localhost:6379";
    options.DeepLApiKey = "your-api-key";
});

// Use in controllers
[TranslateResponse]
public IActionResult GetProduct(int id) => Ok(product);
```

## 5. Configuration Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `DefaultTargetLanguages` | `string[]` | `[]` | Default languages for translation |
| `DefaultSourceLanguage` | `string` | `"en"` | Source language |
| `CacheEnabled` | `bool` | `true` | Enable caching |
| `CacheTTL` | `TimeSpan` | `24h` | Cache expiration |
| `RedisConnection` | `string` | `null` | Redis connection string |
| `DeepLApiKey` | `string` | `null` | DeepL API key |
| `Provider` | `TranslationProvider` | `DeepL` | Translation provider |

## 6. Project Structure

```
TranslateSharp/
├── src/
│   ├── TranslateSharp/
│   │   ├── Attributes/
│   │   ├── Providers/
│   │   ├── Cache/
│   │   ├── Core/
│   │   ├── Filters/
│   │   └── TranslateSharp.csproj
│   └── TranslateSharp.Abstractions/
│       └── ITranslationProvider.cs
├── samples/
│   └── TranslateSharp.SampleApi/
└── tests/
    └── TranslateSharp.Tests/
```

## 7. NuGet Publishing

- **Package ID**: TranslateSharp
- **Target Framework**: net8.0, net10.0
- **Dependencies**:
  - Microsoft.Extensions.DependencyInjection.Abstractions
  - Microsoft.Extensions.Caching.StackExchangeRedis
  - Microsoft.Extensions.Caching.Memory
  - System.Text.Json

## 8. Acceptance Criteria

1. Fields marked with `[Translate]` are automatically translated on response
2. Redis caching works with fallback to in-memory
3. DeepL provider integration functional
4. ASP.NET Core filter for automatic translation
5. Package installs via NuGet
6. Multiple target languages supported per field
7. Nested object translation support
8. Configuration via options pattern