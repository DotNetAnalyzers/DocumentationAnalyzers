// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC103UseUnicodeCharacters, DocumentationAnalyzers.StyleRules.DOC103CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    public class DOC103UnitTests
    {
        [Fact]
        public async Task TestApostropheReplacementAsync()
        {
            var testCode = @"
/// <summary>
/// Don[|&apos;|]t use <![CDATA[&apos;]]> this <element attr=""&apos;"" attr2='&apos;'/>.
/// </summary>
class TestClass
{
}
";
            var fixedCode = @"
/// <summary>
/// Don't use <![CDATA[&apos;]]> this <element attr=""&apos;"" attr2='&apos;'/>.
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestApostropheReplacementByNumberAsync()
        {
            var testCode = @"
/// <summary>
/// Don[|&#39;|]t use <![CDATA[&#39;]]> this <element attr=""&#39;"" attr2='&#39;'/>.
/// </summary>
class TestClass
{
}
";
            var fixedCode = @"
/// <summary>
/// Don't use <![CDATA[&#39;]]> this <element attr=""&#39;"" attr2='&#39;'/>.
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestQuoteReplacementAsync()
        {
            var testCode = @"
/// <summary>
/// Don[|&quot;|]t use <![CDATA[&quot;]]> this <element attr=""&quot;"" attr2='&quot;'/>.
/// </summary>
class TestClass
{
}
";
            var fixedCode = @"
/// <summary>
/// Don""t use <![CDATA[&quot;]]> this <element attr=""&quot;"" attr2='&quot;'/>.
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestHtmlEntityReplacementAsync()
        {
            var testCode = @"
/// <summary>
/// From A&rarr;B.
/// </summary>
class TestClass
{
}
";
            var fixedCode = @"
/// <summary>
/// From A→B.
/// </summary>
class TestClass
{
}
";

            await new CSharpCodeFixTest<DOC103UseUnicodeCharacters, DOC103CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                ExpectedDiagnostics = { DiagnosticResult.CompilerWarning("CS1570").WithSpan(3, 11, 3, 11).WithMessage("XML comment has badly formed XML -- 'Reference to undefined entity 'rarr'.'") },
                FixedCode = fixedCode,
                CompilerDiagnostics = CompilerDiagnostics.Warnings,
            }.RunAsync();
        }
    }
}
