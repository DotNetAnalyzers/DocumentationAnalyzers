// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.RefactoringRules
{
    using System.Threading.Tasks;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.RefactoringRules.DOC901ConvertToDocumentationComment, DocumentationAnalyzers.RefactoringRules.DOC901CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    public class DOC901UnitTests
    {
        [Theory]
        [InlineData("class NestedClass { }")]
        [InlineData("TestClass() { }")]
        [InlineData("public static explicit operator bool(TestClass obj) => true;")]
        [InlineData("delegate void NestedDelegate();")]
        [InlineData("~TestClass() { }")]
        [InlineData("enum NestedEnum { }")]
        [InlineData("event System.EventHandler Member { add { } remove { } }")]
        [InlineData("event System.EventHandler Member;")]
        [InlineData("int field;")]
        [InlineData("int this[int value] => 3;")]
        [InlineData("interface NestedInterface { }")]
        [InlineData("int Method() => 3;")]
        [InlineData("public static TestClass operator -(TestClass obj) => default(TestClass);")]
        [InlineData("int Property => 3;")]
        [InlineData("struct NestedStruct { }")]
        public async Task TestMemberSingleLineCommentToDocumentationCommentAsync(string codeElement)
        {
            var testCode = $@"
class TestClass
{{
    [|// This is a comment|]
    {codeElement}
}}
";
            var fixedCode = $@"
class TestClass
{{
    /// <summary>
    /// This is a comment
    /// </summary>
    {codeElement}
}}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMemberMultipleSingleLineCommentToDocumentationCommentAsync()
        {
            var testCode = @"
class TestClass
{
    [|// This is a comment
    // The comment continues|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// This is a comment
    /// The comment continues
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestCommentWithEscapedCharactersAsync()
        {
            var testCode = @"
class TestClass
{
    [|// X&Y <- 'Z' -> ""W""|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// X&amp;Y &lt;- 'Z' -&gt; ""W""
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestConversionIgnoresCommentsSeparatedFromMemberAsync()
        {
            var testCode = @"
class TestClass
{
    // This is not part of the comment

    [|// This is a comment
    // The comment continues|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    // This is not part of the comment

    /// <summary>
    /// This is a comment
    /// The comment continues
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMemberMultipleParagraphSingleLineCommentToDocumentationCommentAsync()
        {
            var testCode = @"
class TestClass
{
    [|// This is a comment
    //
    // The comment continues|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// This is a comment
    /// 
    /// The comment continues
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMemberMultipleParagraphMultiLineCommentToDocumentationCommentAsync()
        {
            var testCode = @"
class TestClass
{
    [|/* This is a comment
     *
     * The comment continues
     */|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// This is a comment
    /// 
    /// The comment continues
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMultiLineComment1Async()
        {
            var testCode = @"
class TestClass
{
    [|/* This is a comment */|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// This is a comment
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMultiLineComment2Async()
        {
            var testCode = @"
class TestClass
{
    [|/*
     * This is a comment */|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// This is a comment
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMultiLineComment3Async()
        {
            var testCode = @"
class TestClass
{
    [|/*
     * This is a comment
     */|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// This is a comment
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMultiLineComment4Async()
        {
            var testCode = @"
class TestClass
{
    [|/* This is a comment
     */|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// This is a comment
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMultiLineComment5Async()
        {
            var testCode = @"
class TestClass
{
    [|/**/|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// 
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMultiLineComment6Async()
        {
            var testCode = @"
class TestClass
{
    [|/* */|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// 
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMixedComment1Async()
        {
            var testCode = @"
class TestClass
{
    // Line comment
    [|/* Block comment */|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    // Line comment
    /// <summary>
    /// Block comment
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMixedComment2Async()
        {
            var testCode = @"
class TestClass
{
    /* Block comment */
    [|// Line comment|]
    int Property => 3;
}
";
            var fixedCode = @"
class TestClass
{
    /* Block comment */
    /// <summary>
    /// Line comment
    /// </summary>
    int Property => 3;
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
