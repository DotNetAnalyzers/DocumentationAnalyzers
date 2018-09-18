// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.PortabilityRules
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Use XML documentation syntax.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC200UseXmlDocumentationSyntax : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC200UseXmlDocumentationSyntax"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC200";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC200.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(PortabilityResources.DOC200Title), PortabilityResources.ResourceManager, typeof(PortabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(PortabilityResources.DOC200MessageFormat), PortabilityResources.ResourceManager, typeof(PortabilityResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(PortabilityResources.DOC200Description), PortabilityResources.ResourceManager, typeof(PortabilityResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.PortabilityRules, DiagnosticSeverity.Info, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(HandleXmlElementSyntax, SyntaxKind.XmlElement);
        }

        private static void HandleXmlElementSyntax(SyntaxNodeAnalysisContext context)
        {
            var xmlElementSyntax = (XmlElementSyntax)context.Node;
            var name = xmlElementSyntax.StartTag?.Name;
            if (name is null || name.Prefix != null)
            {
                return;
            }

            switch (name.LocalName.ValueText)
            {
            case "p":
            case "pre":
            case "tt":
            case "ol":
            case "ul":
                break;

            default:
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, name.LocalName.GetLocation()));
        }
    }
}
