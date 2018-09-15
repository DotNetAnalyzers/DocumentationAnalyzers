// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Testing;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC102UseChildBlocksConsistentlyAcrossElementsOfTheSameKind, DocumentationAnalyzers.StyleRules.BlockLevelDocumentationCodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    /// <summary>
    /// This class contains unit tests for <see cref="DOC102UseChildBlocksConsistentlyAcrossElementsOfTheSameKind"/>.
    /// </summary>
    /// <remarks>
    /// <para>This set of tests includes tests with the same inputs as <see cref="DOC100UnitTests"/> and
    /// <see cref="DOC101UseChildBlocksConsistently"/> to ensure
    /// <see cref="DOC102UseChildBlocksConsistentlyAcrossElementsOfTheSameKind"/> is not also reported in those
    /// cases.</para>
    /// </remarks>
    public class DOC102UnitTests
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

            // reported as DOC101
            await Verify.VerifyAnalyzerAsync(testCode);
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

            // reported as DOC101
            await Verify.VerifyAnalyzerAsync(testCode);
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

            // reported as DOC101
            await Verify.VerifyAnalyzerAsync(testCode);
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

            // reported as DOC100 and DOC101
            await Verify.VerifyAnalyzerAsync(testCode);
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

        [Fact]
        public async Task TestExceptionElementsAsync()
        {
            var testCode = @"
using System;
public class ClassName
{
    /// <exception cref=""ArgumentNullException"">If <paramref name=""x""/> is <see langword=""null""/>.</exception>
    /// <exception cref=""ArgumentException"">
    /// If <paramref name=""x""/> is empty.
    /// <para>-or-</para>
    /// <para>If <paramref name=""y""/> is empty.</para>
    /// </exception>
    public void MethodName(string x, string y)
    {
    }
}";

            var fixedCode = @"
using System;
public class ClassName
{
    /// <exception cref=""ArgumentNullException""><para>If <paramref name=""x""/> is <see langword=""null""/>.</para></exception>
    /// <exception cref=""ArgumentException"">
    /// If <paramref name=""x""/> is empty.
    /// <para>-or-</para>
    /// <para>If <paramref name=""y""/> is empty.</para>
    /// </exception>
    public void MethodName(string x, string y)
    {
    }
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(5, 49);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }

        [Fact]
        public async Task TestListItemsAsync()
        {
            var testCode = @"
/// <remarks>
/// <list type=""bullet"">
/// <item>Item 1.</item>
/// <item><para>Item 2.</para></item>
/// </list>
/// </remarks>
public class ClassName
{
}";

            var fixedCode = @"
/// <remarks>
/// <list type=""bullet"">
/// <item><para>Item 1.</para></item>
/// <item><para>Item 2.</para></item>
/// </list>
/// </remarks>
public class ClassName
{
}";

            DiagnosticResult expected = Verify.Diagnostic().WithLocation(4, 11);
            await Verify.VerifyCodeFixAsync(testCode, expected, fixedCode);
        }
    }
}
