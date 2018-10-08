// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.PortabilityRules
{
    using System.Threading.Tasks;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.PortabilityRules.DOC209UseSeeHrefCorrectly, Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    public class DOC209UnitTests
    {
        [Fact]
        public async Task TestAbsoluteUriAsync()
        {
            var testCode = $@"
/// <summary>
/// <see href=""https://github.com""/>
/// <see href=""https://github.com""></see>
/// </summary>
/// <seealso href=""https://github.com""/>
/// <seealso href=""https://github.com""></see>
class TestClass
{{
}}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestRelativeUriAsync()
        {
            var testCode = $@"
/// <summary>
/// <see href=""docs/index.md""/>
/// <see href=""docs/index.md""></see>
/// </summary>
/// <seealso href=""docs/index.md""/>
/// <seealso href=""docs/index.md""></see>
class TestClass
{{
}}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestEscapesAllowedAsync()
        {
            var testCode = @"
/// <summary>
/// <see href=""https://gith&#117;b.com""/>
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
/// <see [|href|]=""https://""/>
/// <see [|href|]=""https://""></see>
/// </summary>
/// <seealso [|href|]=""https://""/>
/// <seealso [|href|]=""https://""></see>
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
/// <see Href=""https://""/>
/// <see x:href=""https://""/>
/// <see name=""https://""/>
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
/// <p:see href=""https://""/>
/// </summary>
/// <p:seealso href=""https://""/>
class TestClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
