using MaintenanceModeMiddleware.Configuration.Data;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MaintenanceModeMiddleware.Configuration
{
    internal interface IRequestHandler : IOption
    {
        Task<PreprocessResult> Preprocess(HttpContext context);
        Task Postprocess(HttpContext context);
    }
}
