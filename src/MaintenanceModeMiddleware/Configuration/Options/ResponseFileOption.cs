﻿using MaintenanceModeMiddleware.Configuration.Data;
using MaintenanceModeMiddleware.Configuration.Enums;
using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class ResponseFileOption : Option<FileDescriptor>
    {
        internal const char PARTS_SEPARATOR = ';';

        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split(PARTS_SEPARATOR);
            if (parts.Length != 2)
            {
                throw new FormatException($"{nameof(str)} is in incorrect format.");
            }

            if (!Enum.TryParse(parts[0], out PathBaseDirectory baseDir))
            {
                throw new ArgumentException($"Unknown base directory type {parts[0]}");
            }

            Value = new FileDescriptor(parts[1], baseDir);
        }

        public override string GetStringValue()
        {
            return $"{Value.BaseDir}{PARTS_SEPARATOR}{Value.FilePath}";
        }
    }
}