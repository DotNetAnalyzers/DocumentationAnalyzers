// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.RefactoringRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.RefactoringRules;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Xunit;

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

            await new CSharpCodeFixTest<DOC900RenderAsMarkdown, DOC900CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                FixedState = { MarkupHandling = MarkupMode.Allow },
                BatchFixedState = { MarkupHandling = MarkupMode.Allow },

                // The first iteration fully renders the documentation. The second iteration offers a code fix to render
                // documentation, but no changes are made by the fix so the iterations stop.
                NumberOfIncrementalIterations = 2,
            }.RunAsync();
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
/// <item>Item 1</item>
/// <item>Item 2</item>
/// <item>Item 3</item>
/// </list>
/// </summary>
class TestClass { }
";

            await new CSharpCodeFixTest<DOC900RenderAsMarkdown, DOC900CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                FixedState = { MarkupHandling = MarkupMode.Allow },
                BatchFixedState = { MarkupHandling = MarkupMode.Allow },

                // The first iteration fully renders the documentation. The second iteration offers a code fix to render
                // documentation, but no changes are made by the fix so the iterations stop.
                NumberOfIncrementalIterations = 2,
            }.RunAsync();
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
/// <item>Item 1</item>
/// <item>Item 2</item>
/// <item>Item 3</item>
/// </list>
/// </summary>
class TestClass { }
";

            await new CSharpCodeFixTest<DOC900RenderAsMarkdown, DOC900CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                FixedState = { MarkupHandling = MarkupMode.Allow },
                BatchFixedState = { MarkupHandling = MarkupMode.Allow },

                // The first iteration fully renders the documentation. The second iteration offers a code fix to render
                // documentation, but no changes are made by the fix so the iterations stop.
                NumberOfIncrementalIterations = 2,
            }.RunAsync();
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

            await new CSharpCodeFixTest<DOC900RenderAsMarkdown, DOC900CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                FixedState = { MarkupHandling = MarkupMode.Allow },
                BatchFixedState = { MarkupHandling = MarkupMode.Allow },

                // The first iteration fully renders the documentation. The second iteration offers a code fix to render
                // documentation, but no changes are made by the fix so the iterations stop.
                NumberOfIncrementalIterations = 2,
            }.RunAsync();
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
    /// Provide a value for <c>param</c>.
    /// </summary>
    void Method(int param) { }
}
";

            await new CSharpCodeFixTest<DOC900RenderAsMarkdown, DOC900CodeFixProvider, XUnitVerifier>
            {
                TestCode = testCode,
                FixedCode = fixedCode,
                FixedState = { MarkupHandling = MarkupMode.Allow },
                BatchFixedState = { MarkupHandling = MarkupMode.Allow },

                // The first iteration fully renders the documentation. The second iteration offers a code fix to render
                // documentation, but no changes are made by the fix so the iterations stop.
                NumberOfIncrementalIterations = 2,
            }.RunAsync();
        }
    }
}
