namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class UseDefaultResponseOption : Option<bool>
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
