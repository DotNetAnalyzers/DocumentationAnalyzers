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
    /// Use XML documentation syntax.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC103UseXmlDocumentationSyntax : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC103UseXmlDocumentationSyntax"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC103";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC103.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(StyleResources.DOC103Title), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(StyleResources.DOC103MessageFormat), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(StyleResources.DOC103Description), StyleResources.ResourceManager, typeof(StyleResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.StyleRules, DiagnosticSeverity.Info, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

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
