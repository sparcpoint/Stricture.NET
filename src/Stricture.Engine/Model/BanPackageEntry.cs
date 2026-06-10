namespace Stricture
{
    /// <summary>A declared <c>BanPackage</c>.</summary>
    internal sealed class BanPackageEntry
    {
        public BanPackageEntry(string assemblyName, string? message)
        {
            AssemblyName = assemblyName;
            Message = message;
        }

        public string AssemblyName { get; }

        public string? Message { get; }
    }
}
