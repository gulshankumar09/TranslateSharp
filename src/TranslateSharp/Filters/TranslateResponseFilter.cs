using Microsoft.AspNetCore.Mvc.Filters;
using TranslateSharp.Core;

namespace TranslateSharp.Filters;

public class TranslateResponseFilter : IAsyncResultFilter
{
    private readonly TranslationService _translationService;

    public TranslateResponseFilter(TranslationService translationService)
    {
        _translationService = translationService;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        if (context.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult)
        {
            var value = objectResult.Value;
            if (value != null)
            {
                await _translationService.TranslateObjectAsync(value);
            }
        }
    }
}