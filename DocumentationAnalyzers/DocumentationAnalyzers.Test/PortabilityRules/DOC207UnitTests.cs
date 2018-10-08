// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.PortabilityRules
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.PortabilityRules.DOC207UseSeeLangwordCorrectly, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    public class DOC207UnitTests
    {
        public static IEnumerable<object[]> Keywords
        {
            get
            {
                foreach (var keywordKind in SyntaxFacts.GetKeywordKinds())
                {
                    yield return new[] { SyntaxFacts.GetText(keywordKind) };
                }
            }
        }

        [Theory]
        [MemberData(nameof(Keywords))]
        public async Task TestRecognizedKeywordsAsync(string keyword)
        {
            var testCode = $@"
/// <summary>
/// <see langword=""{keyword}""/>
/// <see langword=""{keyword}""></see>
/// </summary>
class TestClass
{{
}}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestSpacesNotAllowedAsync()
        {
            var testCode = @"
/// <summary>
/// <see langword=""null""/>
/// <see [|langword|]="" null""/>
/// <see [|langword|]=""null ""/>
/// <see [|langword|]="" null ""/>
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestEscapesAllowedAsync()
        {
            var testCode = @"
/// <summary>
/// <see langword=""n&#117;ll""/>
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestEmptyAndFullElementsValidatedAsync()
        {
            var testCode = @"
/// <summary>
/// <see [|langword|]=""not a keyword""/>
/// <see [|langword|]=""not a keyword""></see>
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestOtherAttributesIgnoredAsync()
        {
            var testCode = @"
/// <summary>
/// <see Langword=""not a keyword""/>
/// <see x:langword=""not a keyword""/>
/// <see name=""not a keyword""/>
/// <see cref=""System.String""/>
/// </summary>
class TestClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestOtherElementsIgnoredAsync()
        {
            var testCode = @"
/// <summary>
/// <p:see langword=""not a keyword""/>
/// </summary>
/// 
class TestClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
