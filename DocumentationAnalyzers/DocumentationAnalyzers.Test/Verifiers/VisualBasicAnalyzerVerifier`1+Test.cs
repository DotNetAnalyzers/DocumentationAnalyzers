// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Test
{
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Testing.Verifiers;
    using Microsoft.CodeAnalysis.VisualBasic.Testing;

    public static partial class VisualBasicAnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public class Test : VisualBasicAnalyzerTest<TAnalyzer, XUnitVerifier>
        {
            public Test()
            {
            }
        }
    }
}
