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

        [Fact]
        public async Task TestUnknownEntityNotReplacedAsync()
        {
            var testCode = @"
/// <summary>
/// Unknown entity &myEntity;.
/// </summary>
class TestClass
{
}
";
            var fixedCode = testCode;

            await new CSharpCodeFixTest<DOC103UseUnicodeCharacters, DOC103CodeFixProvider, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { testCode },
                    ExpectedDiagnostics = { DiagnosticResult.CompilerWarning("CS1570").WithSpan(3, 20, 3, 20).WithMessage("XML comment has badly formed XML -- 'Reference to undefined entity 'myEntity'.'") },
                },
                FixedState =
                {
                    Sources = { fixedCode },
                    InheritanceMode = StateInheritanceMode.AutoInheritAll,
                },
                CompilerDiagnostics = CompilerDiagnostics.Warnings,
            }.RunAsync();
        }

        [Fact]
        public async Task TestHtmlEntityReplacementInInvalidXmlAsync()
        {
            var testCode = @"
/// <summary>
/// From A&rarr;B.
/// <p>
/// An unterminated second paragraph...
/// </summary>
class TestClass
{
}
";
            var fixedCode = @"
/// <summary>
/// From A→B.
/// <p>
/// An unterminated second paragraph...
/// </summary>
class TestClass
{
}
";

            await new CSharpCodeFixTest<DOC103UseUnicodeCharacters, DOC103CodeFixProvider, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { testCode },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("CS1570").WithSpan(3, 11, 3, 11).WithMessage("XML comment has badly formed XML -- 'Reference to undefined entity 'rarr'.'"),
                        DiagnosticResult.CompilerWarning("CS1570").WithSpan(6, 7, 6, 14).WithMessage("XML comment has badly formed XML -- 'End tag 'summary' does not match the start tag 'p'.'"),
                        DiagnosticResult.CompilerWarning("CS1570").WithSpan(7, 1, 7, 1).WithMessage("XML comment has badly formed XML -- 'Expected an end tag for element 'summary'.'"),
                    },
                },
                FixedState =
                {
                    Sources = { fixedCode },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("CS1570").WithSpan(6, 7, 6, 14).WithMessage("XML comment has badly formed XML -- 'End tag 'summary' does not match the start tag 'p'.'"),
                        DiagnosticResult.CompilerWarning("CS1570").WithSpan(7, 1, 7, 1).WithMessage("XML comment has badly formed XML -- 'Expected an end tag for element 'summary'.'"),
                    },
                },
                CompilerDiagnostics = CompilerDiagnostics.Warnings,
            }.RunAsync();
        }

        [Fact]
        public async Task TestNoCodeFixForRequiredEntityAsync()
        {
            var testCode = @"
/// <summary>
/// Processing for <c>&lt;code&gt;</c> elements.
/// </summary>
class TestClass
{
}
";
            var fixedCode = testCode;

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestNoCodeFixForInvalidXmlAsync()
        {
            var testCode = @"
/// <summary>
/// From A to B.
/// <p>
/// An unterminated second paragraph...
/// </summary>
class TestClass
{
}
";
            var fixedCode = testCode;

            await new CSharpCodeFixTest<DOC103UseUnicodeCharacters, DOC103CodeFixProvider, XUnitVerifier>
            {
                TestState =
                {
                    Sources = { testCode },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("CS1570").WithSpan(6, 7, 6, 14).WithMessage("XML comment has badly formed XML -- 'End tag 'summary' does not match the start tag 'p'.'"),
                        DiagnosticResult.CompilerWarning("CS1570").WithSpan(7, 1, 7, 1).WithMessage("XML comment has badly formed XML -- 'Expected an end tag for element 'summary'.'"),
                    },
                },
                FixedState =
                {
                    Sources = { fixedCode },
                    InheritanceMode = StateInheritanceMode.AutoInheritAll,
                },
                CompilerDiagnostics = CompilerDiagnostics.Warnings,
            }.RunAsync();
        }
    }
}
