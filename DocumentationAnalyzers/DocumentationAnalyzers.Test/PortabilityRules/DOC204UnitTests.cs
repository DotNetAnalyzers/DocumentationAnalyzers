// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.PortabilityRules
{
    using System.Threading.Tasks;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.PortabilityRules.DOC204UseInlineElementsCorrectly, DocumentationAnalyzers.PortabilityRules.DOC204CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    public class DOC204UnitTests
    {
        [Fact]
        public async Task TestElementsUsedCorrectlyAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Pass in a <paramref name=""value""/> of type <typeparamref name=""T""/>.
    /// </summary>
    void Method<T>(int value)
    {
    }
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestParamRefUsedAsParamAsync()
        {
            var testCode = @"
class TestClass
{
    /// <summary>
    /// Pass in a value.
    /// </summary>
    /// <$$paramref name=""value"">The value</paramref>
    void Method(int value)
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// Pass in a value.
    /// </summary>
    /// <param name=""value"">The value</param>
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
    /// Pass in a value.
    /// </summary>
    /// <$$typeparamref name=""T"">The type of value</typeparamref>
    void Method<T>()
    {
    }
}
";
            var fixedCode = @"
class TestClass
{
    /// <summary>
    /// Pass in a value.
    /// </summary>
    /// <typeparam name=""T"">The type of value</typeparam>
    void Method<T>()
    {
    }
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
