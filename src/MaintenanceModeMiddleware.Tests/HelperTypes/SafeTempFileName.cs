using System.IO;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    public static class SafeTempFileName
    {
        public static string Create(string originalFileName)
        {
            string randPrefix = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            return $"{randPrefix}{originalFileName}";
        }
    }
}
