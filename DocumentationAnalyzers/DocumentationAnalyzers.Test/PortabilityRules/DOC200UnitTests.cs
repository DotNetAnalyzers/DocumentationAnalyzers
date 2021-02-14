// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.PortabilityRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.PortabilityRules;
    using Xunit;
    using Verify = DocumentationAnalyzers.Test.CSharpCodeFixVerifier<DocumentationAnalyzers.PortabilityRules.DOC200UseXmlDocumentationSyntax, DocumentationAnalyzers.PortabilityRules.DOC200CodeFixProvider>;

    /// <summary>
    /// This class contains unit tests for <see cref="DOC200UseXmlDocumentationSyntax"/>.
    /// </summary>
    public class DOC200UnitTests
    {
        [Fact]
        public async Task TestHtmlParagraphAsync()
        {
            var testCode = @"
/// <remarks>
/// <[|p|]>This is a paragraph.</p>
/// </remarks>
class TestClass { }
";
            var fixedCode = @"
/// <remarks>
/// <para>This is a paragraph.</para>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestHtmlParagraphWithAttributeAsync()
        {
            var testCode = @"
/// <remarks>
/// <[|p|] attr=""value"">This is a paragraph.</p>
/// </remarks>
class TestClass { }
";
            var fixedCode = @"
/// <remarks>
/// <para attr=""value"">This is a paragraph.</para>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestPrefixAsync()
        {
            var testCode = @"
/// <remarks>
/// <not:p>This is a paragraph.</not:p>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestHtmlCodeAsync()
        {
            var testCode = @"
/// <remarks>
/// <para>This is <[|tt|]>code</tt>.</para>
/// </remarks>
class TestClass { }
";
            var fixedCode = @"
/// <remarks>
/// <para>This is <c>code</c>.</para>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestHtmlCodeBlockAsync()
        {
            var testCode = @"
/// <remarks>
/// <para>This is a code block:</para>
/// <[|pre|]>
/// code goes here
/// more code here
/// </pre>
/// </remarks>
class TestClass { }
";
            var fixedCode = @"
/// <remarks>
/// <para>This is a code block:</para>
/// <code>
/// code goes here
/// more code here
/// </code>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestHtmlOrderedListAsync()
        {
            var testCode = @"
/// <remarks>
/// <para>This is an ordered list:</para>
/// <[|ol|]>
/// <li>Item 1</li>
/// <li>Item 2</li>
/// </ol>
/// </remarks>
class TestClass { }
";
            var fixedCode = @"
/// <remarks>
/// <para>This is an ordered list:</para>
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
        public async Task TestHtmlUnorderedListAsync()
        {
            var testCode = @"
/// <remarks>
/// <para>This is an ordered list:</para>
/// <[|ul|]>
/// <li>Item 1</li>
/// <li>Item 2</li>
/// </ul>
/// </remarks>
class TestClass { }
";
            var fixedCode = @"
/// <remarks>
/// <para>This is an ordered list:</para>
/// <list type=""bullet"">
/// <item><description>Item 1</description></item>
/// <item><description>Item 2</description></item>
/// </list>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestHtmlUnorderedListMultilineItemAsync()
        {
            var testCode = @"
/// <remarks>
/// <para>This is an ordered list:</para>
/// <[|ul|]>
/// <li>
///     Item 1
/// </li>
/// </ul>
/// </remarks>
class TestClass { }
";
            var fixedCode = @"
/// <remarks>
/// <para>This is an ordered list:</para>
/// <list type=""bullet"">
/// <item><description>
///     Item 1
/// </description></item>
/// </list>
/// </remarks>
class TestClass { }
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
