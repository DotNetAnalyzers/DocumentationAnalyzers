// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.StyleRules
{
    using System.Collections.Immutable;
    using System.Linq;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Use child blocks consistently across elements of the same kind.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when a documentation contains sibling elements of the same kind, where
    /// some siblings use block-level content, but others do not.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC102UseChildBlocksConsistentlyAcrossElementsOfTheSameKind : BlockLevelDocumentationAnalyzerBase
    {
        /// <summary>
        /// The ID for diagnostics produced by the
        /// <see cref="DOC102UseChildBlocksConsistentlyAcrossElementsOfTheSameKind"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC102";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC102.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(StyleResources.DOC102Title), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(StyleResources.DOC102MessageFormat), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(StyleResources.DOC102Description), StyleResources.ResourceManager, typeof(StyleResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new(DiagnosticId, Title, MessageFormat, AnalyzerCategory.StyleRules, DiagnosticSeverity.Info, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnosticsValue =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return SupportedDiagnosticsValue;
            }
        }

        /// <inheritdoc/>
        protected override bool ElementRequiresBlockContent(XmlElementSyntax element, SemanticModel semanticModel)
        {
            var name = element.StartTag?.Name;
            if (name == null || name.LocalName.IsMissingOrDefault())
            {
                // unrecognized
                return false;
            }

            if (name.Prefix != null)
            {
                // not a standard element
                return false;
            }

            switch (name.LocalName.ValueText)
            {
            case XmlCommentHelper.RemarksXmlTag:
            case XmlCommentHelper.NoteXmlTag:
                if (semanticModel.Compilation.Options.SpecificDiagnosticOptions.GetValueOrDefault(DOC100PlaceTextInParagraphs.DiagnosticId, ReportDiagnostic.Default) != ReportDiagnostic.Suppress)
                {
                    // these elements are covered by SA1653, when enabled
                    return false;
                }

                // otherwise this diagnostic will still apply
                goto default;

            default:
                if (semanticModel.Compilation.Options.SpecificDiagnosticOptions.GetValueOrDefault(DOC101UseChildBlocksConsistently.DiagnosticId, ReportDiagnostic.Default) != ReportDiagnostic.Suppress)
                {
                    if (element.Content.Any(child => IsBlockLevelNode(child, false)))
                    {
                        // these elements are covered by SA1654, when enabled
                        return false;
                    }
                }

                break;
            }

            SyntaxList<XmlNodeSyntax> parentContent;
            if (element.Parent is XmlElementSyntax parentElement)
            {
                parentContent = parentElement.Content;
            }
            else
            {
                if (element.Parent is DocumentationCommentTriviaSyntax parentSyntax)
                {
                    parentContent = parentSyntax.Content;
                }
                else
                {
                    return false;
                }
            }

            foreach (XmlElementSyntax sibling in parentContent.GetXmlElements(name.LocalName.ValueText).OfType<XmlElementSyntax>())
            {
                if (sibling == element)
                {
                    continue;
                }

                if (sibling.Content.Any(child => IsBlockLevelNode(child, false)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
