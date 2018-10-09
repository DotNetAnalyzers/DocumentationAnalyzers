// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.PortabilityRules
{
    using System.Collections.Immutable;
    using System.Globalization;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC206SynchronizeDocumentation : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC206SynchronizeDocumentation"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC206";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC206.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(PortabilityResources.DOC206Title), PortabilityResources.ResourceManager, typeof(PortabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(PortabilityResources.DOC206MessageFormat), PortabilityResources.ResourceManager, typeof(PortabilityResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(PortabilityResources.DOC206Description), PortabilityResources.ResourceManager, typeof(PortabilityResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.PortabilityRules, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(HandleXmlNodeSyntax, SyntaxKind.XmlElement, SyntaxKind.XmlEmptyElement);
        }

        private static void HandleXmlNodeSyntax(SyntaxNodeAnalysisContext context)
        {
            var xmlNode = (XmlNodeSyntax)context.Node;
            var name = xmlNode.GetName();
            if (name.Prefix != null)
            {
                return;
            }

            if (name.LocalName.ValueText != XmlCommentHelper.AutoinheritdocXmlTag)
            {
                return;
            }

            var documentedSymbol = context.SemanticModel.GetDeclaredSymbol(xmlNode.FirstAncestorOrSelf<SyntaxNode>(SyntaxNodeExtensionsEx.IsSymbolDeclaration), context.CancellationToken);
            var currentDocumentation = InheritdocHelper.GetDocumentationCommentXml(documentedSymbol, CultureInfo.CurrentCulture, expandInheritdoc: false, expandIncludes: false, context.CancellationToken);
            var expectedDocumentation = InheritdocHelper.GetDocumentationCommentXml(documentedSymbol, CultureInfo.CurrentCulture, expandInheritdoc: true, expandIncludes: false, context.CancellationToken);
            if (currentDocumentation == expectedDocumentation)
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
        }
    }
}
