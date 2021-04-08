namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class BypassAllAuthenticatedUsersOption : Option<bool>
    {
        public override void LoadFromString(string str)
        {
            Value = bool.Parse(str);
        }

        public override string GetStringValue()
        {
            return Value.ToString();
        }
    }
}
