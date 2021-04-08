namespace MaintenanceModeMiddleware.Configuration.Options
{
    internal class Code503RetryIntervalOption : Option<int>
    {
        public override void LoadFromString(string str)
        {
            Value = int.Parse(str);
        }

        public override string GetStringValue()
        {
            return Value.ToString();
        }
    }
}
