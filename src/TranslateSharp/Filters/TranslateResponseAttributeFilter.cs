using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using TranslateSharp.Core;

namespace TranslateSharp.Filters;

public class TranslateResponseAttributeFilter : IActionFilter
{
    private readonly TranslationService _translationService;
    private readonly ILogger<TranslateResponseAttributeFilter> _logger;

    public TranslateResponseAttributeFilter(TranslationService translationService, ILogger<TranslateResponseAttributeFilter> logger)
    {
        _translationService = translationService;
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var controllerActionDescriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        if (controllerActionDescriptor == null)
            return;

        var hasMethodAttribute = controllerActionDescriptor.MethodInfo.GetCustomAttributes(typeof(TranslateResponseAttribute), true).Length > 0;
        var hasClassAttribute = controllerActionDescriptor.MethodInfo.DeclaringType?.GetCustomAttributes(typeof(TranslateResponseAttribute), true).Length > 0;
        var hasAttribute = hasMethodAttribute || hasClassAttribute;

        _logger.LogInformation("TranslateResponse check - Method: {Method}, HasAttribute: {HasAttr}", controllerActionDescriptor.ActionName, hasAttribute);

        if (!hasAttribute)
            return;

        if (context.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult)
        {
            var value = objectResult.Value;
            if (value != null)
            {
                _logger.LogInformation("Translating object of type: {Type}", value.GetType().Name);
                _translationService.TranslateObjectAsync(value).GetAwaiter().GetResult();
                _logger.LogInformation("Translation completed");
            }
        }
    }
}