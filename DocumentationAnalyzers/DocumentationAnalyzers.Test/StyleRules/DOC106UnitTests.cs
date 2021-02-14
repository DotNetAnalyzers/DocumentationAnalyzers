// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Xunit;
    using Verify = DocumentationAnalyzers.Test.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC106UseTypeparamref, DocumentationAnalyzers.StyleRules.DOC106CodeFixProvider>;

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
        public async Task TestTypeParameterNameEncodedAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Consider a type parameter [|<c>&#97;</c>|].
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
    /// Consider a type parameter <typeparamref name=""&#97;""/>.
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

        [Fact]
        public async Task TestNonDiagnosticPotentialCasesAsync()
        {
            // These cases could qualify for this diagnostic, but currently do not.
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Consider a type parameter <c> a</c>.
    /// Consider a type parameter <c>a </c>.
    /// Consider a type parameter <c> a </c>.
    /// Consider a type parameter <c>a
    /// </c>.
    /// Consider a type parameter <c>
    /// a</c>.
    /// Consider a type parameter <c>
    /// a
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
    /// Consider a type parameter <c>b</c>.
    /// Consider a type parameter <c>A</c>.
    /// Consider a type parameter <c>a&gt;</c>.
    /// Consider a type parameter <c>&gt;a</c>.
    /// Consider a type parameter <c><em>a</em></c>.
    /// Consider a type parameter <c><em>a</em>a</c>.
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
class TestClass
{
    /// <summary>
    /// Consider a type parameter <p:c>a</p:c>.
    /// </summary>
    void Method<a>()
    {
    }
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
