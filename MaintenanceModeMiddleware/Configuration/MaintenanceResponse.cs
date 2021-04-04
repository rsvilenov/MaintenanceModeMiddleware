using System.Text;

namespace MaintenanceModeMiddleware.Configuration
{
    public class MaintenanceResponse
    {
        public string ContentType { get; set; } = "text/plain";
        public Encoding ContentEncoding { get; set; } = Encoding.UTF8;
        public byte[] ContentBytes { get; set; } = Encoding.UTF8.GetBytes("The site is down for maintenance.");
    }
}
