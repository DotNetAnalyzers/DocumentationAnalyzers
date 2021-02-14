// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Xunit;
    using Verify = DocumentationAnalyzers.Test.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC105UseParamref, DocumentationAnalyzers.StyleRules.DOC105CodeFixProvider>;

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
        public async Task TestParameterNameEncodedAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Consider a parameter [|<c>&#97;</c>|].
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
    /// Consider a parameter <paramref name=""&#97;""/>.
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

        [Fact]
        public async Task TestNonDiagnosticPotentialCasesAsync()
        {
            // These cases could qualify for this diagnostic, but currently do not.
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Consider a parameter <c> a</c>.
    /// Consider a parameter <c>a </c>.
    /// Consider a parameter <c> a </c>.
    /// Consider a parameter <c>a
    /// </c>.
    /// Consider a parameter <c>
    /// a</c>.
    /// Consider a parameter <c>
    /// a
    /// </c>.
    /// </summary>
    void Method(int a)
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
    /// Consider a parameter <c>b</c>.
    /// Consider a parameter <c>A</c>.
    /// Consider a parameter <c>a&gt;</c>.
    /// Consider a parameter <c>&gt;a</c>.
    /// Consider a parameter <c><em>a</em></c>.
    /// Consider a parameter <c><em>a</em>a</c>.
    /// </summary>
    void Method(int a)
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
class TestClass
{
    /// <summary>
    /// Consider a parameter <p:c>a</p:c>.
    /// </summary>
    void Method(int a)
    {
    }
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
