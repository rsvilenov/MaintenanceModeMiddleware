namespace MaintenanceModeMiddleware.Configuration.Builders
{
    public class CustomActionOptionsBuilder<TBuilder> : 
        StatusCodeOptionsBuilder<TBuilder>
        where TBuilder : CustomActionOptionsBuilder<TBuilder>
    {
        internal CustomActionOptionsBuilder()
        { }
    }

    public class CustomActionOptionsBuilder : CustomActionOptionsBuilder<CustomActionOptionsBuilder>
    {
        internal CustomActionOptionsBuilder()
        { }
    }
}
