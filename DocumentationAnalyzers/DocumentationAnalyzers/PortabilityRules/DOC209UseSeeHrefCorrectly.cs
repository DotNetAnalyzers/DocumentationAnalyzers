// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.PortabilityRules
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NoCodeFix("Cannot convert an invalid URI to one intended by the author.")]
    internal class DOC209UseSeeHrefCorrectly : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC209UseSeeHrefCorrectly"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC209";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC209.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(PortabilityResources.DOC209Title), PortabilityResources.ResourceManager, typeof(PortabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(PortabilityResources.DOC209MessageFormat), PortabilityResources.ResourceManager, typeof(PortabilityResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(PortabilityResources.DOC209Description), PortabilityResources.ResourceManager, typeof(PortabilityResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new(DiagnosticId, Title, MessageFormat, AnalyzerCategory.PortabilityRules, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

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
            var xmlNodeSyntax = (XmlNodeSyntax)context.Node;
            var name = xmlNodeSyntax.GetName();
            if (name is null || name.Prefix != null)
            {
                return;
            }

            if (name.LocalName.ValueText != XmlCommentHelper.SeeXmlTag
                && name.LocalName.ValueText != XmlCommentHelper.SeeAlsoXmlTag)
            {
                return;
            }

            SyntaxList<XmlAttributeSyntax> attributes;
            if (xmlNodeSyntax is XmlEmptyElementSyntax xmlEmptyElement)
            {
                attributes = xmlEmptyElement.Attributes;
            }
            else
            {
                attributes = ((XmlElementSyntax)xmlNodeSyntax).StartTag.Attributes;
            }

            foreach (var attribute in attributes)
            {
                if (attribute.Name is null || attribute.Name.Prefix != null)
                {
                    continue;
                }

                if (attribute.Name.LocalName.ValueText != XmlCommentHelper.HrefArgumentName)
                {
                    continue;
                }

                var text = ((XmlTextAttributeSyntax)attribute).TextTokens;
                string valueText;
                if (text.Count == 1)
                {
                    valueText = text[0].ValueText;
                }
                else
                {
                    valueText = string.Join(string.Empty, text.Select(textToken => textToken.ValueText));
                }

                if (Uri.TryCreate(valueText, System.UriKind.RelativeOrAbsolute, out _))
                {
                    continue;
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, attribute.Name.LocalName.GetLocation()));
            }
        }
    }
}
