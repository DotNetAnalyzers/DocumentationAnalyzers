// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.StyleRules
{
    using System.Threading.Tasks;
    using DocumentationAnalyzers.StyleRules;
    using Xunit;
    using Verify = DocumentationAnalyzers.Test.CSharpCodeFixVerifier<DocumentationAnalyzers.StyleRules.DOC107UseSeeCref, DocumentationAnalyzers.StyleRules.DOC107CodeFixProvider>;

    /// <summary>
    /// This class contains unit tests for <see cref="DOC107UseSeeCref"/>.
    /// </summary>
    public class DOC107UnitTests
    {
        [Fact]
        public async Task TestPropertyNameAsync()
        {
            var testCode = @"
class TestClass
{
    int a => 3;

    /// <summary>
    /// Consider a property [|<c>a</c>|].
    /// </summary>
    void Method()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    int a => 3;

    /// <summary>
    /// Consider a property <see cref=""a""/>.
    /// </summary>
    void Method()
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestPropertyNameEncodedAsync()
        {
            var testCode = @"
class TestClass
{
    int a => 3;

    /// <summary>
    /// Consider a property [|<c>&#97;</c>|].
    /// </summary>
    void Method()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    int a => 3;

    /// <summary>
    /// Consider a property <see cref=""&#97;""/>.
    /// </summary>
    void Method()
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestFieldNameAsync()
        {
            var testCode = @"
class TestClass
{
    int a = 3;

    /// <summary>
    /// Consider a member [|<c>a</c>|].
    /// </summary>
    void Method()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    int a = 3;

    /// <summary>
    /// Consider a member <see cref=""a""/>.
    /// </summary>
    void Method()
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestEventNameAsync()
        {
            var testCode = @"
class TestClass
{
    event System.EventHandler a;

    /// <summary>
    /// Consider a member [|<c>a</c>|].
    /// </summary>
    void Method()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    event System.EventHandler a;

    /// <summary>
    /// Consider a member <see cref=""a""/>.
    /// </summary>
    void Method()
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMethodNameAsync()
        {
            var testCode = @"
class TestClass
{
    int a() => 3;

    /// <summary>
    /// Consider a member <c>a</c>.
    /// </summary>
    void Method()
    {
    }
}
";

            // Methods are not currently checked
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestContainingNamespaceNameAsync()
        {
            var testCode = @"
namespace a
{
    class TestClass
    {
        /// <summary>
        /// Consider a containing namespace <c>a</c>.
        /// </summary>
        void Method()
        {
        }
    }
}
";

            // Containing namespaces are not currently checked
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestContainingTypeNameAsync()
        {
            var testCode = @"
class a
{
    class TestClass
    {
        /// <summary>
        /// Consider a containing type <c>a</c>.
        /// </summary>
        void Method()
        {
        }
    }
}
";

            // Containing types are not currently checked
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestNestedTypeNameAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Consider a nested type <c>a</c>.
    /// </summary>
    void Method()
    {
    }

    class a { }
}
";

            // Nested types are not currently checked
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestSiblingTypeNameAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Consider a type <c>a</c>.
    /// </summary>
    void Method()
    {
    }
}

class a { }
";

            // Types are not currently checked
            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestPropertyNameMatchesKeywordAsync()
        {
            var testCode = @"
class TestClass
{
    int @true => 3;

    /// <summary>
    /// Consider property [|<c>true</c>|].
    /// </summary>
    void Method()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    int @true => 3;

    /// <summary>
    /// Consider property <see cref=""true""/>.
    /// </summary>
    void Method()
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
    int a => 3;

    /// <summary>
    /// Consider a property <c> a</c>.
    /// Consider a property <c>a </c>.
    /// Consider a property <c> a </c>.
    /// Consider a property <c>a
    /// </c>.
    /// Consider a property <c>
    /// a</c>.
    /// Consider a property <c>
    /// a
    /// </c>.
    /// </summary>
    void Method()
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
    int a => 3;

    /// <summary>
    /// Consider a property <c>b</c>.
    /// Consider a property <c>A</c>.
    /// Consider a property <c>a&gt;</c>.
    /// Consider a property <c>&gt;a</c>.
    /// Consider a property <c><em>a</em></c>.
    /// Consider a property <c><em>a</em>a</c>.
    /// </summary>
    void Method()
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
    int a => 3;

    /// <summary>
    /// Consider a property <p:c>a</p:c>.
    /// </summary>
    void Method()
    {
    }
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }
    }
}
