// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.StyleRules
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Avoid empty paragraphs.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC108AvoidEmptyParagraphs : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC108AvoidEmptyParagraphs"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC108";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC108.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(StyleResources.DOC108Title), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(StyleResources.DOC108MessageFormat), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(StyleResources.DOC108Description), StyleResources.ResourceManager, typeof(StyleResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new(DiagnosticId, Title, MessageFormat, AnalyzerCategory.PortabilityRules, DiagnosticSeverity.Info, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(HandleXmlEmptyElementSyntax, SyntaxKind.XmlEmptyElement);
        }

        private static void HandleXmlEmptyElementSyntax(SyntaxNodeAnalysisContext context)
        {
            var xmlEmptyElement = (XmlEmptyElementSyntax)context.Node;
            var name = xmlEmptyElement.Name;
            if (name.Prefix != null)
            {
                return;
            }

            switch (name.LocalName.ValueText)
            {
            case "para":
            case "p":
                break;

            default:
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, xmlEmptyElement.GetLocation()));
        }
    }
}
