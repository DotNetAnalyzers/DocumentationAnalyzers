// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.PortabilityRules
{
    using System.Threading.Tasks;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.PortabilityRules.DOC203UseBlockElementsCorrectly, DocumentationAnalyzers.PortabilityRules.DOC203CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    public class DOC203UnitTests
    {
        [Fact]
        public async Task TestCodeUsedAsExampleAsync()
        {
            var testCode = @"
class TestClass
{
    /// <example>
    /// <code>
    /// this is some code...
    /// </code>
    /// </example>
    void Method()
    {
    }
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestCodeUsedAsInlineWithinParagraphAsync()
        {
            var testCode = @"
class TestClass
{
    /// <remarks>
    /// <para>Executes some <$$code>code</code>.</para>
    /// </remarks>
    void Method()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    /// <remarks>
    /// <para>Executes some <c>code</c>.</para>
    /// </remarks>
    void Method()
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestCodeUsedAsInlineAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Executes some <$$code>code</code>.
    /// </summary>
    void Method()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// Executes some <c>code</c>.
    /// </summary>
    void Method()
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
