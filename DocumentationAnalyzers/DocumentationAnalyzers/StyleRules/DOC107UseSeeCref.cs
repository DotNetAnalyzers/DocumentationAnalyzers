// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.StyleRules
{
    using System.Collections.Immutable;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC107UseSeeCref : InlineCodeAnalyzerBase
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC107UseSeeCref"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC107";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC107.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(StyleResources.DOC107Title), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(StyleResources.DOC107MessageFormat), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(StyleResources.DOC107Description), StyleResources.ResourceManager, typeof(StyleResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new(DiagnosticId, Title, MessageFormat, AnalyzerCategory.StyleRules, DiagnosticSeverity.Info, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        protected override void HandleInlineCodeElement(ref SyntaxNodeAnalysisContext context, XmlElementSyntax xmlElement)
        {
            // Currently this rule will only apply if the content is a single XmlTextSyntax containing a single
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
            var name = xmlText.TextTokens[0].ValueText;
            for (var currentSymbol = documentedSymbol; currentSymbol != null; currentSymbol = currentSymbol?.ContainingSymbol)
            {
                switch (currentSymbol.Kind)
                {
                case SymbolKind.NamedType:
                    var namedType = (INamedTypeSymbol)currentSymbol;
                    var matchingMembers = namedType.GetMembers(name);
                    if (matchingMembers.Length != 1)
                    {
                        return;
                    }

                    if (matchingMembers[0].Kind == SymbolKind.Property
                        || matchingMembers[0].Kind == SymbolKind.Field
                        || matchingMembers[0].Kind == SymbolKind.Event)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, xmlElement.GetLocation()));
                    }

                    return;

                case SymbolKind.Namespace:
                case SymbolKind.NetModule:
                    return;

                default:
                    continue;
                }
            }
        }
    }
}
