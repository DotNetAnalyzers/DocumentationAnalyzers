// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.PortabilityRules
{
    using System.Threading.Tasks;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.PortabilityRules.DOC201ItemShouldHaveDescription, DocumentationAnalyzers.PortabilityRules.DOC201CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    public class DOC201UnitTests
    {
        [Fact]
        public async Task TestListItemsWithoutDescriptionAsync()
        {
            var testCode = @"
/// <remarks>
/// <list type=""number"">
/// <[|item|]>Item 1</item>
/// <[|item|]>Item 2</item>
/// </list>
/// </remarks>
class TestClass { }
";
            var fixedCode = @"
/// <remarks>
/// <list type=""number"">
/// <item><description>Item 1</description></item>
/// <item><description>Item 2</description></item>
/// </list>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMultilineListItemsWithoutDescriptionAsync()
        {
            var testCode = @"
/// <remarks>
/// <list type=""number"">
/// <[|item|]>
///     Item 1
/// </item>
/// <[|item|]>
///     Item 2
/// </item>
/// </list>
/// </remarks>
class TestClass { }
";
            var fixedCode = @"
/// <remarks>
/// <list type=""number"">
/// <item><description>
///     Item 1
/// </description></item>
/// <item><description>
///     Item 2
/// </description></item>
/// </list>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestListItemsWithEmptyDescriptionAsync()
        {
            var testCode = @"
/// <remarks>
/// <list type=""number"">
/// <item><description></description></item>
/// <item><description/></item>
/// </list>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestListItemsWithTermAsync()
        {
            var testCode = @"
/// <remarks>
/// <list type=""number"">
/// <item><term>Item 1</term></item>
/// <item><term>Item 2</term><description>Description</description></item>
/// </list>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
