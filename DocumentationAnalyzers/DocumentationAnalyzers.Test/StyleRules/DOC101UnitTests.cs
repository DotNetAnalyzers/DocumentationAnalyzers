// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Microsoft.CodeAnalysis.Testing;
    using Xunit;
    using Verify = DocumentationAnalyzers.Test.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC101UseChildBlocksConsistently, DocumentationAnalyzers.StyleRules.BlockLevelDocumentationCodeFixProvider>;

    /// <summary>
    /// This class contains unit tests for <see cref="DOC101UseChildBlocksConsistently"/>.
    /// </summary>
    public class DOC101UnitTests
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
/// <summary>
/// <para>Summary.</para>
/// </summary>
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestTwoSimpleParagraphBlockAsync()
        {
            var testCode = @"
/// <summary>
/// <para>Summary.</para>
/// <para>Summary.</para>
/// </summary>
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestMultipleBlockElementsAsync()
        {
            var testCode = @"
/// <summary>
/// <!-- Supported XML documentation comment block-level elements -->
/// <code>Summary.</code>
/// <list><item>Item</item></list>
/// <note><para>Summary.</para></note>
/// <para>Summary.</para>
/// <!-- Supported SHFB elements which may be block-level elements -->
/// <inheritdoc/>
/// <token>SomeTokenName</token>
/// <include />
/// <!-- Supported HTML block-level elements -->
/// <div>Summary.</div>
/// <p>Summary.</p>
/// </summary>
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestSimpleInlineBlockAsync()
        {
            var testCode = @"
/// <summary>Summary.</summary>
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestMultiLineParagraphInlineBlockAsync()
        {
            var testCode = @"
/// <summary>
/// Remarks.
/// Line 2.
/// </summary>
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestFirstParagraphInlineBlockAsync()
        {
            var testCode = @"
/// <summary>
/// Summary.
/// <para>Paragraph 2.</para>
/// </summary>
public class ClassName
{
}";

            var fixedCode = @"
/// <summary>
/// <para>Summary.</para>
/// <para>Paragraph 2.</para>
/// </summary>
public class ClassName
{
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(3, 5);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestFirstParagraphInlineBlockInRemarksAsync()
        {
            var testCode = @"
/// <remarks>
/// Remarks.
/// <para>Paragraph 2.</para>
/// </remarks>
public class ClassName
{
}";

            // reported as DOC100
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestInlineParagraphAndCodeAsync()
        {
            var testCode = @"
/// <summary>
/// Summary.
/// <code>Code.</code>
/// </summary>
public class ClassName
{
}";

            var fixedCode = @"
/// <summary>
/// <para>Summary.</para>
/// <code>Code.</code>
/// </summary>
public class ClassName
{
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(3, 5);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestInlineParagraphAndCodeInRemarksAsync()
        {
            var testCode = @"
/// <remarks>
/// Remarks.
/// <code>Code.</code>
/// </remarks>
public class ClassName
{
}";

            // reported as DOC100
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestCodeAndInlineParagraphAsync()
        {
            var testCode = @"
/// <summary>
/// <code>Code.</code>
/// Summary.
/// </summary>
public class ClassName
{
}";

            var fixedCode = @"
/// <summary>
/// <code>Code.</code>
/// <para>Summary.</para>
/// </summary>
public class ClassName
{
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(4, 5);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestCodeAndInlineParagraphInRemarksAsync()
        {
            var testCode = @"
/// <remarks>
/// <code>Code.</code>
/// Remarks.
/// </remarks>
public class ClassName
{
}";

            // reported as DOC100
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestThreeInlineParagraphsWithOtherElementsAsync()
        {
            var testCode = @"
/// <summary>
/// Leading summary.
/// <code>Code.</code>
/// <para>Summary.</para>
/// <note>Note.</note>
/// Closing summary.
/// </summary>
public class ClassName
{
}";

            var fixedCode = @"
/// <summary>
/// <para>Leading summary.</para>
/// <code>Code.</code>
/// <para>Summary.</para>
/// <note>Note.</note>
/// <para>Closing summary.</para>
/// </summary>
public class ClassName
{
}";

            // the <note> element is also covered by SA1653, even when it appears inside the <summary> element.
            DiagnosticResult[] expected =
            {
                Verify.Diagnostic().WithLocation(3, 5),
                Verify.Diagnostic().WithLocation(7, 5),
            };
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestThreeInlineParagraphsWithOtherElementsInRemarksAsync()
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

            // reported as DOC100
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestSeeIsAnInlineElementAsync()
        {
            var testCode = @"
/// <summary>
/// Leading summary.
/// <see cref=""ClassName""/>.
/// </summary>
public class ClassName
{
}";

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
