using Stricture;

// ARCH1030: IServiceCollection extension methods must live in a single partial
// ServiceCollectionExtensions class in the Microsoft.Extensions.DependencyInjection namespace, so
// every feature can contribute registrations to the same discoverable public surface.
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>Dependency-injection registration helpers for CleanSample.</summary>
    [PublicApi]
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>Registers CleanSample services in the container.</summary>
        /// <param name="services">The service collection to add to.</param>
        /// <returns>The same <paramref name="services"/> instance, for chaining.</returns>
        public static IServiceCollection AddCleanSample(this IServiceCollection services) => services;
    }
}
