using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Stricture.Tests.Infrastructure
{
    /// <summary>
    /// A pre-wired <see cref="CSharpAnalyzerTest{TAnalyzer, TVerifier}"/> for the root analyzer:
    /// targets a .NET reference set and references the consumer's attribute library.
    /// </summary>
    internal sealed class StrictureAnalyzerTest : CSharpAnalyzerTest<StrictureAnalyzer, DefaultVerifier>
    {
        public StrictureAnalyzerTest()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
            TestState.AdditionalReferences.Add(
                MetadataReference.CreateFromFile(typeof(FolderStructureAttribute).Assembly.Location));
        }
    }
}
