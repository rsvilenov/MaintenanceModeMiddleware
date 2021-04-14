using System;
using System.IO;

namespace MaintenanceModeMiddleware.Tests.HelperTypes
{
    public static class SafeTempPath
    {
        public static string Create(string originalPath)
        {
            string randStr = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            if (originalPath == null)
            {
                return Path.Combine(Path.GetTempPath(), randStr);
            }

            if (Path.IsPathRooted(originalPath))
            {
                if (File.Exists(originalPath))
                {
                    string dirName = Path.GetDirectoryName(originalPath);
                    string fileName = Path.GetFileName(originalPath);

                    return Path.Combine(dirName, $"{randStr}{fileName}");
                }

                if (Directory.Exists(originalPath))
                {
                    return Path.Combine(originalPath, randStr);
                }

                throw new InvalidOperationException("The given path leads neither to a file, nor to a directory.");
            }

            return $"{randStr}{originalPath}";
        }
    }
}
