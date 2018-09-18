// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers
{
    /// <summary>
    /// Class defining the analyzer category constants.
    /// </summary>
    internal static class AnalyzerCategory
    {
        /// <summary>
        /// Category definition for style rules.
        /// </summary>
        public const string StyleRules = nameof(DocumentationAnalyzers) + "." + nameof(StyleRules);

        /// <summary>
        /// Category definition for portability rules.
        /// </summary>
        public const string PortabilityRules = nameof(DocumentationAnalyzers) + "." + nameof(PortabilityRules);

        /// <summary>
        /// Category definition for refactorings.
        /// </summary>
        public const string Refactorings = nameof(DocumentationAnalyzers) + "." + nameof(Refactorings);
    }
}
