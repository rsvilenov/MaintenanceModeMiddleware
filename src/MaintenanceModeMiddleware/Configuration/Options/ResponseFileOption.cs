using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseFileOption : Option<FileDescriptor>
    {
        private const char PARTS_SEPARATOR = ';';

        public override void FromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"{nameof(str)} is in incorrect format.");
            }

            if (!Enum.TryParse(parts[0], out PathBaseDirectory baseDir))
            {
                throw new ArgumentException($"Unknown base directory type {parts[0]}");
            }

            Value = new FileDescriptor(parts[1], baseDir);
        }

        public override string ToString()
        {
            return $"{Value.BaseDir}{PARTS_SEPARATOR}{Value.FilePath}";
        }
    }
}
