// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.PortabilityRules
{
    using System.Collections.Immutable;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC201ItemShouldHaveDescription : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC201ItemShouldHaveDescription"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC201";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC201.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(PortabilityResources.DOC201Title), PortabilityResources.ResourceManager, typeof(PortabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(PortabilityResources.DOC201MessageFormat), PortabilityResources.ResourceManager, typeof(PortabilityResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(PortabilityResources.DOC201Description), PortabilityResources.ResourceManager, typeof(PortabilityResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.PortabilityRules, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

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
            case XmlCommentHelper.ItemXmlTag:
                break;

            default:
                return;
            }

            // check for a <term> or <description> child element
            foreach (var node in xmlElementSyntax.Content)
            {
                var childName = node.GetName();
                if (childName is null || childName.Prefix != null)
                {
                    continue;
                }

                switch (childName.LocalName.ValueText)
                {
                case XmlCommentHelper.TermXmlTag:
                case XmlCommentHelper.DescriptionXmlTag:
                    // Avoid analyzing <item> that already has <term> and/or <description>
                    return;

                default:
                    break;
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, name.LocalName.GetLocation()));
        }
    }
}
