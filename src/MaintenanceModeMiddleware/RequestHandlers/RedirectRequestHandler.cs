using MaintenanceModeMiddleware.Configuration;
using MaintenanceModeMiddleware.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.RequestHandlers
{
    internal class RedirectRequestHandler : IRequestHandler
    {
        private readonly IRedirectInitializer _redirectInitializer;
        public RedirectRequestHandler(IMaintenanceOptionsService optionsService)
        {
            _redirectInitializer = optionsService
                .GetOptions()
                .GetSingleOrDefault<IRedirectInitializer>();
        }

        public Task Postprocess(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public Task<PreprocessResult> Preprocess(HttpContext context)
        {
            context
                .Response
                .Redirect(_redirectInitializer
                    .RedirectLocation);

            return Task.FromResult(new PreprocessResult { CallNext = false });
        }

        public bool ShouldApply(HttpContext context)
        {
            return _redirectInitializer != null;
        }
    }
}
