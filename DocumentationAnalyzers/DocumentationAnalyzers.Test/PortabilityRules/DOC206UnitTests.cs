// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.PortabilityRules
{
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.PortabilityRules.DOC206SynchronizeDocumentation, DocumentationAnalyzers.PortabilityRules.DOC206CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    public class DOC206UnitTests
    {
        [Fact]
        public async Task TestInheritSummaryAsync()
        {
            var testCode = @"
/// <summary>
/// Summary text.
/// </summary>
/// <autoinheritdoc/>
class TestClass : BaseClass
{
}

/// <summary>
/// Summary text.
/// </summary>
class BaseClass
{
}
";

            await Verify.VerifyAnalyzerAsync(testCode);
        }

        [Fact]
        public async Task TestIncorrectSummaryAsync()
        {
            var testCode = @"
/// <summary>
/// Incorrect summary text.
/// </summary>
/// [|<autoinheritdoc/>|]
class TestClass : BaseClass
{
}

/// <summary>
/// Summary text.
/// </summary>
class BaseClass
{
}
";
            var fixedCode = @"
/// <summary>
/// Summary text.
/// </summary>
/// <autoinheritdoc/>
class TestClass : BaseClass
{
}

/// <summary>
/// Summary text.
/// </summary>
class BaseClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestMissingSummaryAsync()
        {
            var testCode = @"
/// [|<autoinheritdoc/>|]
class TestClass : BaseClass
{
}

/// <summary>
/// Summary text.
/// </summary>
class BaseClass
{
}
";
            var fixedCode = @"
/// <summary>
/// Summary text.
/// </summary>
/// <autoinheritdoc/>
class TestClass : BaseClass
{
}

/// <summary>
/// Summary text.
/// </summary>
class BaseClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }
    }
}
