// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test.PortabilityRules
{
    using System.Threading.Tasks;
    using Xunit;
    using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpCodeFixVerifier<DocumentationAnalyzers.PortabilityRules.DOC205InheritDocumentation, DocumentationAnalyzers.PortabilityRules.DOC205CodeFixProvider, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

    public class DOC205UnitTests
    {
        [Fact]
        public async Task TestConvertToAutoinheritdoc1Async()
        {
            var testCode = @"
/// [|<inheritdoc/>|]
class TestClass : BaseClass
{
}

class BaseClass
{
}
";
            var fixedCode = @"
/// <autoinheritdoc/>
class TestClass : BaseClass
{
}

class BaseClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestConvertToAutoinheritdoc2Async()
        {
            var testCode = @"
/// [|<inheritdoc></inheritdoc>|]
class TestClass : BaseClass
{
}

class BaseClass
{
}
";
            var fixedCode = @"
/// <autoinheritdoc></autoinheritdoc>
class TestClass : BaseClass
{
}

class BaseClass
{
}
";

            await Verify.VerifyCodeFixAsync(testCode, fixedCode);
        }

        [Fact]
        public async Task TestInheritSummaryAsync()
        {
            var testCode = @"
/// [|<inheritdoc/>|]
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
