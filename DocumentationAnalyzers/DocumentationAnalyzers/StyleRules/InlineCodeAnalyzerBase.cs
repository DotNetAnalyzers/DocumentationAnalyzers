// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.StyleRules
{
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This is the base class for diagnostic analyzers which report diagnostics for use of <c>&lt;c&gt;</c> which a
    /// more appropriate inline element is available.
    /// </summary>
    internal abstract class InlineCodeAnalyzerBase : DiagnosticAnalyzer
    {
        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(HandleXmlElementSyntax, SyntaxKind.XmlElement);
        }

        protected abstract void HandleInlineCodeElement(ref SyntaxNodeAnalysisContext context, XmlElementSyntax xmlElement);

        private void HandleXmlElementSyntax(SyntaxNodeAnalysisContext context)
        {
            var xmlElement = (XmlElementSyntax)context.Node;
            var name = xmlElement.StartTag.Name;
            if (name.Prefix != null)
            {
                return;
            }

            if (name.LocalName.ValueText != XmlCommentHelper.CXmlTag)
            {
                return;
            }

            HandleInlineCodeElement(ref context, xmlElement);
        }
    }
}
