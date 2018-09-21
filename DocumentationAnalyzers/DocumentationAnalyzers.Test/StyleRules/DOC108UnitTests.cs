// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC108AvoidEmptyParagraphs, DocumentationAnalyzers.StyleRules.DOC108CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

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
