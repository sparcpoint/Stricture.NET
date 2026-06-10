namespace Stricture
{
    /// <summary>A declared <c>BanNamespace</c>.</summary>
    internal sealed class BanNamespaceEntry
    {
        public BanNamespaceEntry(string ns, string? message)
        {
            Namespace = ns;
            Message = message;
        }

        public string Namespace { get; }

        public string? Message { get; }
    }
}
