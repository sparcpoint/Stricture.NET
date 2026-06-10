namespace CleanSample.Platform.Logging.Internal
{
    // Classifies to no category, so it lands in the Platform structure's "Internal" fallback folder.
    internal sealed class LogScope
    {
        public string Name { get; set; } = string.Empty;
    }
}
