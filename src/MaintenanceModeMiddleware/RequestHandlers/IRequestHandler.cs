using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.RequestHandlers
{
    internal interface IRequestHandler
    {
        bool ShouldApply(HttpContext context);
        Task<PreprocessResult> Preprocess(HttpContext context);
        Task Postprocess(HttpContext context);
    }
}
