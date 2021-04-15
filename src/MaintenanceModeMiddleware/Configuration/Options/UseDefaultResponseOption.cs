using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Text;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class UseDefaultResponseOption : Option<bool>, IResponseHolder
    {
        private const int DEFAULT_503_RETRY_INTERVAL = 5300;

        public override void LoadFromString(string str)
        {
            Value = bool.Parse(str);
        }

        public override string GetStringValue()
        {
            return Value.ToString();
        }

        public MaintenanceResponse GetResponse(IWebHostEnvironment webHostEnv)
        {
            using (Stream resStream = GetType()
                    .Assembly
                    .GetManifestResourceStream($"{nameof(MaintenanceModeMiddleware)}.Resources.DefaultResponse.html"))
            {
                using var resSr = new StreamReader(resStream, Encoding.UTF8);
                return new MaintenanceResponse
                {
                    ContentBytes = resSr.CurrentEncoding.GetBytes(resSr.ReadToEnd()),
                    ContentEncoding = resSr.CurrentEncoding,
                    ContentType = ContentType.Html,
                    Code503RetryInterval = DEFAULT_503_RETRY_INTERVAL
                };
            }
        }
    }
}
