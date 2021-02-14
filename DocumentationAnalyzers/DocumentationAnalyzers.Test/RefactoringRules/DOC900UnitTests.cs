// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.RefactoringRules
{
    using System;
    using System.Threading.Tasks;
    using DocumentationAnalyzers.RefactoringRules;
    using Microsoft.CodeAnalysis.Testing;
    using Xunit;
    using Verify = DocumentationAnalyzers.Test.CSharpCodeFixVerifier<DocumentationAnalyzers.RefactoringRules.DOC900RenderAsMarkdown, DocumentationAnalyzers.RefactoringRules.DOC900CodeFixProvider>;

    /// <summary>
    /// This class contains unit tests for <see cref="DOC900RenderAsMarkdown"/>.
    /// </summary>
    public class DOC900UnitTests
    {
        [Fact]
        public async Task TestMultiParagraphSummaryAsync()
        {
            var testCode = @"
///$$ <summary>
/// Para 1
///
/// Para 2
/// </summary>
class TestClass { }
";
            var fixedCode = @"
///$$ <summary>
/// <para>Para 1</para>
/// <para>Para 2</para>
/// </summary>
class TestClass { }
";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestSingleParagraphSummaryAsync()
        {
            var testCode = @"
///$$ <summary>
/// Para 1
/// </summary>
class TestClass { }
";
            var fixedCode = testCode;

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestSingleParagraphRemarksAsync()
        {
            var testCode = @"
///$$ <remarks>
/// Para 1
/// </remarks>
class TestClass { }
";
            var fixedCode = @"
///$$ <remarks>
/// <para>Para 1</para>
/// </remarks>
class TestClass { }
";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestParamElementNotRenderedAsync()
        {
            var testCode = @"
class TestClass {
    ///$$ <param name=""param"">Provide a value for `param`.</param>
    void Method(int param) { }
}
";
            var fixedCode = testCode;

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestBulletedListAsync()
        {
            var testCode = @"
///$$ <summary>
/// Para 1
///
/// * Item 1
/// * Item 2
/// * Item 3
/// </summary>
class TestClass { }
";
            var fixedCode = @"
///$$ <summary>
/// <para>Para 1</para>
/// <list type=""bullet"">
/// <item><description>Item 1</description></item>
/// <item><description>Item 2</description></item>
/// <item><description>Item 3</description></item>
/// </list>
/// </summary>
class TestClass { }
";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestNumberedListAsync()
        {
            var testCode = @"
///$$ <summary>
/// Para 1
///
/// 1. Item 1
/// 1. Item 2
/// 1. Item 3
/// </summary>
class TestClass { }
";
            var fixedCode = @"
///$$ <summary>
/// <para>Para 1</para>
/// <list type=""number"">
/// <item><description>Item 1</description></item>
/// <item><description>Item 2</description></item>
/// <item><description>Item 3</description></item>
/// </list>
/// </summary>
class TestClass { }
";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestExampleCodeAsync()
        {
            var testCode = @"
///$$ <summary>
/// Summary text
/// </summary>
/// <example>
/// The following example shows constructing a new `TestClass` instance:
///
/// ```csharp
/// var x = new TestClass();
/// ```
/// </example>
class TestClass { }
";
            var fixedCode = @"
///$$ <summary>
/// Summary text
/// </summary>
/// <example>
/// <para>The following example shows constructing a new <c>TestClass</c> instance:</para>
/// <code language=""csharp"">
/// var x = new TestClass();
/// </code>
/// </example>
class TestClass { }
";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestParameterReferenceAsync()
        {
            var testCode = @"
class TestClass {
    ///$$ <summary>
    /// Provide a value for `param`.
    /// </summary>
    void Method(int param) { }
}
";
            var fixedCode = @"
class TestClass {
    ///$$ <summary>
    /// Provide a value for <paramref name=""param""/>.
    /// </summary>
    void Method(int param) { }
}
";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestLocalTypeParameterReferenceAsync()
        {
            var testCode = @"
///$$ <summary>
/// Provide a value for `T`.
/// </summary>
class TestClass<T> {
    ///$$ <summary>
    /// Provide a value for `T2`.
    /// </summary>
    void Method<T2>() { }
}
";
            var fixedCode = @"
///$$ <summary>
/// Provide a value for <typeparamref name=""T""/>.
/// </summary>
class TestClass<T> {
    ///$$ <summary>
    /// Provide a value for <typeparamref name=""T2""/>.
    /// </summary>
    void Method<T2>() { }
}
";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        [WorkItem(42, "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/issues/42")]
        public async Task TestPropertyDocumentationWithCodeBlockAsync()
        {
            var testCode = @"
class TestClass {
    ///$$ <summary>
    /// Provide a `value`.
    /// </summary>
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass {
    ///$$ <summary>
    /// Provide a <c>value</c>.
    /// </summary>
    int Property => 3;
}
";

            await VerifyCodeFixAsync(testCode, fixedCode);
        }

        private static async Task VerifyCodeFixAsync(string testCode, string fixedCode)
        {
            int iterations;
            if (testCode == fixedCode)
            {
                iterations = 1;
            }
            else
            {
                // One iteration per documentation comment fully renders the documentation. An addition iteration offers
                // a code fix to render documentation, but no changes are made by the fix so the iterations stop.
                iterations = testCode.Split(new[] { "$$" }, StringSplitOptions.None).Length;
            }

            await new Verify.Test
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                FixedState = { MarkupHandling = MarkupMode.Allow },
                BatchFixedState = { MarkupHandling = MarkupMode.Allow },
                NumberOfIncrementalIterations = iterations,
            }.RunAsync();
        }
    }
}
