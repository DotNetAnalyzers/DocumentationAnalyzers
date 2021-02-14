// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.PortabilityRules
{
    using System.Threading.Tasks;
    using Xunit;
    using Verify = DocumentationAnalyzers.Test.CSharpCodeFixVerifier<DocumentationAnalyzers.PortabilityRules.DOC202UseSectionElementsCorrectly, DocumentationAnalyzers.PortabilityRules.DOC202CodeFixProvider>;

    public class DOC202UnitTests
    {
        [Fact]
        public async Task TestElementsUsedCorrectlyAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Pass in a value.
    /// </summary>
    /// <typeparam name=""T"">The type of value</typeparam>
    /// <param name=""value"">The value</param>
    void Method<T>(int value)
    {
    }
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestParamUsedAsParamRefAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Pass in a <$$param name=""value""/>.
    /// </summary>
    void Method(int value)
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// Pass in a <paramref name=""value""/>.
    /// </summary>
    void Method(int value)
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestTypeParamUsedAsTypeParamRefAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Pass in a <$$typeparam name=""T""/>.
    /// </summary>
    void Method<T>()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// Pass in a <typeparamref name=""T""/>.
    /// </summary>
    void Method<T>()
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
