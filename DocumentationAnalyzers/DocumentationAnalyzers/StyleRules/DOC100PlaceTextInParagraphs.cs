// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.StyleRules
{
    using System.Collections.Immutable;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// Place text in paragraphs.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when a &lt;remarks&gt; or &lt;note&gt; documentation element contains
    /// content which is not wrapped in a block-level element.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC100PlaceTextInParagraphs : BlockLevelDocumentationAnalyzerBase
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC100PlaceTextInParagraphs"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC100";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC100.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(StyleResources.DOC100Title), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(StyleResources.DOC100MessageFormat), StyleResources.ResourceManager, typeof(StyleResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(StyleResources.DOC100Description), StyleResources.ResourceManager, typeof(StyleResources));

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
            if (name == null || name.LocalName.IsMissing || name.LocalName == default)
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
                return true;

            default:
                return false;
            }
        }
    }
}
