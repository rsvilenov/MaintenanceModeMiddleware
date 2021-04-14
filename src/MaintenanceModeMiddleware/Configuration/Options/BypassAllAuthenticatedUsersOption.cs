using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassAllAuthenticatedUsersOption : Option<bool>, IContextMatcher
    {
        public override void LoadFromString(string str)
        {
            Value = bool.Parse(str);
        }

        public override string GetStringValue()
        {
            return Value.ToString();
        }

        public bool IsMatch(HttpContext context)
        {
            return context
                .User
                .Identity
                .IsAuthenticated;
        }
    }
}
