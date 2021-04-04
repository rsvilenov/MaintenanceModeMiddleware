using System;
using System.IO;

namespace MaintenanceModeMiddleware.Configuration
{
    internal class FileDescriptor
    {
        internal FileDescriptor(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new ArgumentNullException("The path is null or empty");
            }

            if (!Path.IsPathRooted(fullPath))
            {
                throw new ArgumentException("The path is not a rooted one.");
            }

            FilePath = fullPath;
        }

        internal FileDescriptor(string relativePath, PathBaseDirectory baseDir)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException("The path is null or empty");
            }

            if (Path.IsPathRooted(relativePath))
            {
                throw new ArgumentException("The path is not a relative one.");
            }

            FilePath = relativePath;
            BaseDir = baseDir;
        }

        internal string FilePath { get; set; }
        internal PathBaseDirectory? BaseDir { get; }
    }
}
