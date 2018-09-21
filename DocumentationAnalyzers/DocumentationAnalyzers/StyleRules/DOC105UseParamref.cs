// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.StyleRules
{
    using System;
    using System.Collections.Immutable;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC105UseParamref : InlineCodeAnalyzerBase
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC105UseParamref"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC105";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC105.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(StyleResources.DOC105Title), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(StyleResources.DOC105MessageFormat), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(StyleResources.DOC105Description), StyleResources.ResourceManager, typeof(StyleResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.StyleRules, DiagnosticSeverity.Info, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        protected override void HandleInlineCodeElement(ref SyntaxNodeAnalysisContext context, XmlElementSyntax xmlElement)
        {
            // This rule will only apply if the content is a single XmlTextSyntax containing a single
            // XmlTextLiteralToken token
            if (xmlElement.Content.Count != 1)
            {
                return;
            }

            if (!(xmlElement.Content[0] is XmlTextSyntax xmlText))
            {
                return;
            }

            if (xmlText.TextTokens.Count != 1)
            {
                return;
            }

            var semanticModel = context.SemanticModel;
            var documentedSymbol = semanticModel.GetDeclaredSymbol(xmlElement.FirstAncestorOrSelf<SyntaxNode>(SyntaxNodeExtensionsEx.IsSymbolDeclaration), context.CancellationToken);
            if (!documentedSymbol.HasAnyParameter(xmlText.TextTokens[0].ValueText, StringComparer.Ordinal))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, xmlElement.GetLocation()));
        }
    }
}
