// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Xunit;
    using Verify = DocumentationAnalyzers.Test.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC104UseSeeLangword, DocumentationAnalyzers.StyleRules.DOC104CodeFixProvider>;

    /// <summary>
    /// This class contains unit tests for <see cref="DOC104UseSeeLangword"/>.
    /// </summary>
    public class DOC104UnitTests
    {
        [Theory]
        [InlineData("null")]
        [InlineData("static")]
        [InlineData("virtual")]
        [InlineData("true")]
        [InlineData("false")]
        [InlineData("abstract")]
        [InlineData("sealed")]
        [InlineData("async")]
        [InlineData("await")]
        public async Task TestRecognizedKeywordAsync(string keyword)
        {
            var testCode = $@"
/// <summary>
/// The keyword is [|<c>{keyword}</c>|].
/// </summary>
class TestClass
{{
}}
";
            var fixedCode = $@"
/// <summary>
/// The keyword is <see langword=""{keyword}""/>.
/// </summary>
class TestClass
{{
}}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Theory]
        [InlineData("public")]
        public async Task TestNonKeywordsAsync(string keyword)
        {
            var testCode = $@"
/// <summary>
/// The keyword is <c>{keyword}</c>.
/// </summary>
class TestClass
{{
}}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestNonDiagnosticPotentialCasesAsync()
        {
            // These cases could qualify for this diagnostic, but currently do not.
            var testCode = @"
class TestClass
{
    /// <summary>
    /// The keyword is <c>&#97;wait</c>.
    /// The keyword is <c> true</c>.
    /// The keyword is <c>true </c>.
    /// The keyword is <c> true </c>.
    /// The keyword is <c>true
    /// </c>.
    /// The keyword is <c>
    /// true</c>.
    /// The keyword is <c>
    /// true
    /// </c>.
    /// </summary>
    void Method<a>()
    {
    }
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestNonDiagnosticCasesAsync()
        {
            // These cases *shouldn't* qualify for this diagnostic.
            var testCode = @"
class TestClass
{
    /// <summary>
    /// The keyword is <c>not-keyword</c>.
    /// The keyword is <c>True</c>.
    /// The keyword is <c>true&gt;</c>.
    /// The keyword is <c>&gt;true</c>.
    /// The keyword is <c><em>true</em></c>.
    /// The keyword is <c><em>true</em>true</c>.
    /// </summary>
    void Method<a>()
    {
    }
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestNonCodeAsync()
        {
            var testCode = @"
/// <summary>
/// The keyword is <p:c>true</p:c>.
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
