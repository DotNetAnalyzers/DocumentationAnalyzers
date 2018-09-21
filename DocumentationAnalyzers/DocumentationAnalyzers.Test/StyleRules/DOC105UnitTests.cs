// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC105UseParamref, DocumentationAnalyzers.StyleRules.DOC105CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    /// <summary>
    /// This class contains unit tests for <see cref="DOC105UseParamref"/>.
    /// </summary>
    public class DOC105UnitTests
    {
        [Fact]
        public async Task TestParameterNameAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Consider a parameter [|<c>a</c>|].
    /// </summary>
    void Method(int a)
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// Consider a parameter <paramref name=""a""/>.
    /// </summary>
    void Method(int a)
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestParameterNameMatchesKeywordAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Consider parameter [|<c>true</c>|].
    /// </summary>
    void Method(int @true)
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// Consider parameter <paramref name=""true""/>.
    /// </summary>
    void Method(int @true)
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
