using Microsoft.AspNetCore.Http;

namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassAllAuthenticatedUsersOption : Option<bool>, IAllowedRequestMatcher
    {
        public override void LoadFromString(string str)
        {
            Value = bool.Parse(str);
        }

        public override string GetStringValue()
        {
            return Value.ToString();
        }

        bool IAllowedRequestMatcher.IsMatch(HttpContext context)
        {
            return context
                .User
                .Identity
                .IsAuthenticated;
        }
    }
}
