// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test
{
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Microsoft.CodeAnalysis.VisualBasic.Testing;

    public static partial class VisualBasicCodeRefactoringVerifier<TCodeRefactoring>
        where TCodeRefactoring : CodeRefactoringProvider, new()
    {
        public class Test : VisualBasicCodeRefactoringTest<TCodeRefactoring, XUnitVerifier>
        {
        }
    }
}
