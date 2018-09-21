// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC106UseTypeparamref, DocumentationAnalyzers.StyleRules.DOC106CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    /// <summary>
    /// This class contains unit tests for <see cref="DOC106UseTypeparamref"/>.
    /// </summary>
    public class DOC106UnitTests
    {
        [Fact]
        public async Task TestTypeParameterNameAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Consider a type parameter [|<c>a</c>|].
    /// </summary>
    void Method<a>()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// Consider a type parameter <typeparamref name=""a""/>.
    /// </summary>
    void Method<a>()
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestTypeParameterNameMatchesKeywordAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Consider type parameter [|<c>true</c>|].
    /// </summary>
    void Method<@true>()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// Consider type parameter <typeparamref name=""true""/>.
    /// </summary>
    void Method<@true>()
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
