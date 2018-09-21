// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Microsoft.CodeAnalysis.Testing;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC100PlaceTextInParagraphs, DocumentationAnalyzers.StyleRules.BlockLevelDocumentationCodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    /// <summary>
    /// This class contains unit tests for <see cref="DOC100PlaceTextInParagraphs"/>.
    /// </summary>
    public class DOC100UnitTests
    {
        [Fact]
        public async Task TestNoDocumentationAsync()
        {
            var testCode = @"
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestSummaryDocumentationAsync()
        {
            var testCode = @"
/// <summary>
/// Summary.
/// </summary>
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestSimpleParagraphBlockAsync()
        {
            var testCode = @"
/// <remarks>
/// <para>Remarks.</para>
/// </remarks>
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestTwoSimpleParagraphBlockAsync()
        {
            var testCode = @"
/// <remarks>
/// <para>Remarks.</para>
/// <para>Remarks.</para>
/// </remarks>
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestMultipleBlockElementsAsync()
        {
            var testCode = @"
/// <remarks>
/// <!-- Supported XML documentation comment block-level elements -->
/// <code>Remarks.</code>
/// <list><item>Item</item></list>
/// <note><para>Remarks.</para></note>
/// <para>Remarks.</para>
/// <!-- Supported SHFB elements which may be block-level elements -->
/// <inheritdoc/>
/// <token>SomeTokenName</token>
/// <include />
/// <quote>Quote</quote>
/// <!-- Supported HTML block-level elements -->
/// <div>Remarks.</div>
/// <p>Remarks.</p>
/// </remarks>
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestSimpleInlineBlockAsync()
        {
            var testCode = @"
/// <remarks>Remarks.</remarks>
public class ClassName
{
}";

            var fixedCode = @"
/// <remarks><para>Remarks.</para></remarks>
public class ClassName
{
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(2, 14);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestMultiLineInlineBlockAsync()
        {
            var testCode = @"
/// <remarks>
/// Remarks.
/// </remarks>
public class ClassName
{
}";

            var fixedCode = @"
/// <remarks>
/// <para>Remarks.</para>
/// </remarks>
public class ClassName
{
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(3, 5);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestMultiLineParagraphInlineBlockAsync()
        {
            var testCode = @"
/// <remarks>
/// Remarks.
/// Line 2.
/// </remarks>
public class ClassName
{
}";

            var fixedCode = @"
/// <remarks>
/// <para>Remarks.
/// Line 2.</para>
/// </remarks>
public class ClassName
{
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(3, 5);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestFirstParagraphInlineBlockAsync()
        {
            var testCode = @"
/// <remarks>
/// Remarks.
/// <para>Paragraph 2.</para>
/// </remarks>
public class ClassName
{
}";

            var fixedCode = @"
/// <remarks>
/// <para>Remarks.</para>
/// <para>Paragraph 2.</para>
/// </remarks>
public class ClassName
{
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(3, 5);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestInlineParagraphAndCodeAsync()
        {
            var testCode = @"
/// <remarks>
/// Remarks.
/// <code>Code.</code>
/// </remarks>
public class ClassName
{
}";

            var fixedCode = @"
/// <remarks>
/// <para>Remarks.</para>
/// <code>Code.</code>
/// </remarks>
public class ClassName
{
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(3, 5);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestCodeAndInlineParagraphAsync()
        {
            var testCode = @"
/// <remarks>
/// <code>Code.</code>
/// Remarks.
/// </remarks>
public class ClassName
{
}";

            var fixedCode = @"
/// <remarks>
/// <code>Code.</code>
/// <para>Remarks.</para>
/// </remarks>
public class ClassName
{
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(4, 5);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestThreeInlineParagraphsWithOtherElementsAsync()
        {
            var testCode = @"
/// <remarks>
/// Leading remarks.
/// <code>Code.</code>
/// <para>Remarks.</para>
/// <note>Note.</note>
/// Closing remarks.
/// </remarks>
public class ClassName
{
}";

            var fixedCode = @"
/// <remarks>
/// <para>Leading remarks.</para>
/// <code>Code.</code>
/// <para>Remarks.</para>
/// <note><para>Note.</para></note>
/// <para>Closing remarks.</para>
/// </remarks>
public class ClassName
{
}";

            DiagnosticResult[] expected =
            {
                Verify.Diagnostic().WithLocation(3, 5),
                Verify.Diagnostic().WithLocation(6, 11),
                Verify.Diagnostic().WithLocation(7, 5),
            };
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestSeeIsAnInlineElementAsync()
        {
            var testCode = @"
/// <remarks>
/// Leading remarks.
/// <see cref=""ClassName""/>
/// <para>Remarks.</para>
/// <note>Note.</note>
/// Closing remarks.
/// </remarks>
public class ClassName
{
}";

            var fixedCode = @"
/// <remarks>
/// <para>Leading remarks.
/// <see cref=""ClassName""/></para>
/// <para>Remarks.</para>
/// <note><para>Note.</para></note>
/// <para>Closing remarks.</para>
/// </remarks>
public class ClassName
{
}";

            DiagnosticResult[] expected =
            {
                Verify.Diagnostic().WithLocation(3, 5),
                Verify.Diagnostic().WithLocation(6, 11),
                Verify.Diagnostic().WithLocation(7, 5),
            };
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }
    }
}
