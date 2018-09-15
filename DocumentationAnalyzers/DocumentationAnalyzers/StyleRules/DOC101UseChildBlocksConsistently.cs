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
    /// Use child blocks consistently.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when a documentation element contains some children which are block-level
    /// elements, but other children which are not.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC101UseChildBlocksConsistently : BlockLevelDocumentationAnalyzerBase
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC101UseChildBlocksConsistently"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC101";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC101.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(StyleResources.DOC101Title), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(StyleResources.DOC101MessageFormat), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(StyleResources.DOC101Description), StyleResources.ResourceManager, typeof(StyleResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.StyleRules, DiagnosticSeverity.Info, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

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
                return element.Content.Any(child => IsBlockLevelNode(child, false));
            }
        }
    }
}
