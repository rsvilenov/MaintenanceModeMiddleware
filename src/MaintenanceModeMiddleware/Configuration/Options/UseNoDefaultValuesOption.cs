namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class UseNoDefaultValuesOption : Option<bool>
    {
        public override void FromString(string str)
        {
            Value = bool.Parse(str);
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
