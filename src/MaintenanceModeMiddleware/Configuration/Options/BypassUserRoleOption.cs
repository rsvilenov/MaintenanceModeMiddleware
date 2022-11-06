﻿using Microsoft.AspNetCore.Http;
using System;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassUserRoleOption : Option<string>, IAllowedRequestMatcher
    {
        public override void LoadFromString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentNullException(nameof(str));
            }

            Value = str;
        }

        public override string GetStringValue()
        {
            return Value;
        }

        bool IAllowedRequestMatcher.IsMatch(HttpContext context)
        {
            return context
                .User
                .IsInRole(Value);
        }
    }
}
