# TranslateSharp - .NET Translation Package

## Project Overview

- **Type**: NuGet library package for .NET 9
- **Purpose**: Attribute-based automatic translation of model fields with multi-provider support (DeepL) and Redis caching
- **Goal**: Published to NuGet Package Manager

## Architecture

```
TranslateSharp/
├── src/
│   ├── TranslateSharp.Abstractions/  # Interfaces for extensibility
│   └── TranslateSharp/               # Main library
│       ├── Attributes/               # [Translate], [TranslateIgnore]
│       ├── Providers/                # DeepL provider
│       ├── Cache/                    # Redis + InMemory fallback
│       ├── Core/                     # TranslationService
│       └── Filters/                  # TranslateResponse filter
└── samples/
    └── TranslateSharp.SampleApi/    # Sample API
```

## Current Status

- ✅ Solution and projects created
- ✅ TranslateSharp.Abstractions (ITranslationProvider interface)
- ✅ TranslateAttribute and TranslateIgnoreAttribute
- ✅ DeepL Provider implementation
- ✅ Cache (Redis + InMemory composite)
- ✅ TranslationService (core engine)
- ✅ Response filter and attribute
- ✅ Sample API project
- ✅ Build succeeds, sample runs

## Key Files

- `src/TranslateSharp.Abstractions/ITranslationProvider.cs` - Provider interface
- `src/TranslateSharp/Attributes/TranslateAttribute.cs` - Translation attribute
- `src/TranslateSharp/Providers/DeepLProvider.cs` - DeepL implementation
- `src/TranslateSharp/Core/TranslationService.cs` - Main service
- `src/TranslateSharp/TranslateSharpServiceCollectionExtensions.cs` - DI registration

## Commands

- Build: `dotnet build`
- Pack: `dotnet pack -c Release`
- Publish: `dotnet nuget push` to nuget.org

## Notes

- Target: net9.0
- Has NuGet warnings about version bounds - needs refinement for production publish
- Translation output: sets `{PropertyName}_{language}` (e.g., `Name_de`, `Name_fr`)