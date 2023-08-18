using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResourceFilters {
    public class FeautureDisabledResourceFilter : IAsyncResourceFilter {
        private readonly ILogger<FeautureDisabledResourceFilter> _logger;
        private readonly bool _isDisabled;
        public FeautureDisabledResourceFilter(ILogger<FeautureDisabledResourceFilter> logger, bool isDisabled = true) {
            _logger = logger;
            _isDisabled = isDisabled;
        }
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next) {
            //TO DO : before logic
            _logger.LogInformation("{FilterName}.{MethodName} - before", nameof(FeautureDisabledResourceFilter), nameof(OnResourceExecutionAsync));
            if (_isDisabled) {
                //context.Result = new NotFoundResult();
                context.Result = new StatusCodeResult(501); // 501 - Not Implemented
            } else {
                await next();
            }
            //TO DO : after logic
            _logger.LogInformation("{FilterName}.{MethodName} - after", nameof(FeautureDisabledResourceFilter), nameof(OnResourceExecutionAsync));

        }
    }
}
