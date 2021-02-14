// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Xunit;
    using Verify = DocumentationAnalyzers.Test.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC108AvoidEmptyParagraphs, DocumentationAnalyzers.StyleRules.DOC108CodeFixProvider>;

    /// <summary>
    /// This class contains unit tests for <see cref="DOC108AvoidEmptyParagraphs"/>.
    /// </summary>
    public class DOC108UnitTests
    {
        [Fact]
        public async Task TestEmptyParagraphElementSeparatesParagraphsAsync()
        {
            var testCode = @"
/// <summary>
/// Summary 1
/// $$<para/>
/// Summary 2
/// </summary>
class TestClass
{
}
";
            var fixedCode = @"
/// <summary>
/// Summary 1
/// 
/// Summary 2
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestEmptySeeElementSeparatesParagraphsAsync()
        {
            var testCode = @"
/// <summary>
/// Summary 1
/// <see cref=""TestClass""/>
/// Summary 2
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestPrefixedParagraphElementSeparatesParagraphsAsync()
        {
            var testCode = @"
/// <summary>
/// Summary 1
/// <html:p/>
/// Summary 2
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestEmptyElementWithMissingNameAsync()
        {
            var testCode = @"
/// <summary>
/// <
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestEmptyHtmlParagraphElementSeparatesParagraphsAsync()
        {
            var testCode = @"
/// <summary>
/// Summary 1
/// $$<p/>
/// Summary 2
/// </summary>
class TestClass
{
}
";
            var fixedCode = @"
/// <summary>
/// Summary 1
/// 
/// Summary 2
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestEmptyParagraphBeforeFullParagraphAsync()
        {
            var testCode = @"
/// <summary>
/// Summary 1
/// $$<para/>
/// <para>Summary 2</para>
/// </summary>
class TestClass
{
}
";
            var fixedCode = @"
/// <summary>
/// Summary 1
/// 
/// <para>Summary 2</para>
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestEmptyHtmlParagraphBeforeFullHtmlParagraphAsync()
        {
            var testCode = @"
/// <summary>
/// Summary 1
/// $$<p/>
/// <p>Summary 2</p>
/// </summary>
class TestClass
{
}
";
            var fixedCode = @"
/// <summary>
/// Summary 1
/// 
/// <p>Summary 2</p>
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
