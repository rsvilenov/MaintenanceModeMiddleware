using MaintenanceModeMiddleware.Configuration.Enums;
using System;
using System.IO;

namespace MaintenanceModeMiddleware.Configuration.Data
{
    internal class FileDescriptor
    {
        public FileDescriptor(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new ArgumentNullException("The path is null or empty");
            }

            if (!System.IO.Path.IsPathRooted(fullPath))
            {
                throw new ArgumentException("The path is not a rooted one.");
            }

            Path = fullPath;
        }

        public FileDescriptor(string relativePath, PathBaseDirectory baseDir)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                throw new ArgumentNullException("The path is null or empty");
            }

            if (System.IO.Path.IsPathRooted(relativePath))
            {
                throw new ArgumentException("The path is not a relative one.");
            }

            Path = relativePath;
            BaseDir = baseDir;
        }

        public string Path { get; set; }
        public PathBaseDirectory? BaseDir { get; }
    }
}
