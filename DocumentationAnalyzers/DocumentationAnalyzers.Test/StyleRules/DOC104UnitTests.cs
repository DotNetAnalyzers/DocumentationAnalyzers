// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC104UseSeeLangword, DocumentationAnalyzers.StyleRules.DOC104CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

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
    }
}
