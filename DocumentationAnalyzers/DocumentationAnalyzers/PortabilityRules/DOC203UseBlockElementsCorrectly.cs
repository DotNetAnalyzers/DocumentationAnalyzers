// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.PortabilityRules
{
    using System.Collections.Immutable;
    using System.Diagnostics;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC203UseBlockElementsCorrectly : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DOC203UseBlockElementsCorrectly"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DOC203";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC203.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(PortabilityResources.DOC203Title), PortabilityResources.ResourceManager, typeof(PortabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(PortabilityResources.DOC203MessageFormat), PortabilityResources.ResourceManager, typeof(PortabilityResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(PortabilityResources.DOC203Description), PortabilityResources.ResourceManager, typeof(PortabilityResources));

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

            switch (name.LocalName.ValueText)
            {
            case XmlCommentHelper.CodeXmlTag:
                break;

            default:
                return;
            }

            if (!RequiresSectionContent(xmlNodeSyntax.Parent) && !RequiresInlineContent(xmlNodeSyntax.Parent))
            {
                // The parent may or may not allow block content here. For now, assume that the element is treated as a
                // proper block if the start element is the first on the line, and the end element is the last on its
                // line.
                if (xmlNodeSyntax is XmlElementSyntax xmlElement)
                {
                    if (IsFirstOnLine(xmlElement.StartTag) && IsLastOnLine(xmlElement.EndTag))
                    {
                        return;
                    }
                }
                else
                {
                    if (IsFirstOnLine(xmlNodeSyntax) && IsLastOnLine(xmlNodeSyntax))
                    {
                        return;
                    }
                }
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, name.LocalName.GetLocation()));
        }

        private static bool IsFirstOnLine(XmlNodeSyntax xmlNode)
        {
            // Need to examine the preceding sibling
            SyntaxList<XmlNodeSyntax> parentContent;
            if (xmlNode.Parent is DocumentationCommentTriviaSyntax documentationCommentTrivia)
            {
                parentContent = documentationCommentTrivia.Content;
            }
            else if (xmlNode.Parent is XmlElementSyntax xmlElement)
            {
                parentContent = xmlElement.Content;
            }
            else
            {
                return false;
            }

            for (int i = parentContent.IndexOf(xmlNode) - 1; i >= 0; i--)
            {
                if (!(parentContent[i] is XmlTextSyntax xmlText))
                {
                    return false;
                }

                for (int j = xmlText.TextTokens.Count - 1; j >= 0; j--)
                {
                    if (xmlText.TextTokens[j].IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                    {
                        return true;
                    }
                    else if (!string.IsNullOrWhiteSpace(xmlText.TextTokens[j].ValueText))
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private static bool IsFirstOnLine(XmlElementStartTagSyntax xmlElementStartTag)
        {
            // Need to examine the preceding sibling of the parent
            return xmlElementStartTag.Parent is XmlElementSyntax xmlElement
                && IsFirstOnLine(xmlElement);
        }

        private static bool IsFirstOnLine(XmlElementEndTagSyntax xmlElementEndTag)
        {
            // Need to examine the last content text
            SyntaxList<XmlNodeSyntax> parentContent;
            if (xmlElementEndTag.Parent is XmlElementSyntax xmlElement)
            {
                parentContent = xmlElement.Content;
            }
            else
            {
                return false;
            }

            for (int i = parentContent.Count - 1; i >= 0; i--)
            {
                if (!(parentContent[i] is XmlTextSyntax xmlText))
                {
                    return false;
                }

                for (int j = xmlText.TextTokens.Count - 1; j >= 0; j--)
                {
                    if (xmlText.TextTokens[j].IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                    {
                        return true;
                    }
                    else if (!string.IsNullOrWhiteSpace(xmlText.TextTokens[j].ValueText))
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private static bool IsLastOnLine(XmlNodeSyntax xmlNode)
        {
            // Need to examine the following sibling
            SyntaxList<XmlNodeSyntax> parentContent;
            if (xmlNode.Parent is DocumentationCommentTriviaSyntax documentationCommentTrivia)
            {
                parentContent = documentationCommentTrivia.Content;
            }
            else if (xmlNode.Parent is XmlElementSyntax xmlElement)
            {
                parentContent = xmlElement.Content;
            }
            else
            {
                return false;
            }

            for (int i = parentContent.IndexOf(xmlNode) + 1; i < parentContent.Count; i++)
            {
                if (!(parentContent[i] is XmlTextSyntax xmlText))
                {
                    return false;
                }

                for (int j = 0; j < xmlText.TextTokens.Count; j++)
                {
                    if (xmlText.TextTokens[j].IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                    {
                        return true;
                    }
                    else if (!string.IsNullOrWhiteSpace(xmlText.TextTokens[j].ValueText))
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private static bool IsLastOnLine(XmlElementStartTagSyntax xmlElementStartTag)
        {
            // Need to examine the first content text
            SyntaxList<XmlNodeSyntax> parentContent;
            if (xmlElementStartTag.Parent is XmlElementSyntax xmlElement)
            {
                parentContent = xmlElement.Content;
            }
            else
            {
                return false;
            }

            for (int i = 0; i < parentContent.Count; i++)
            {
                if (!(parentContent[i] is XmlTextSyntax xmlText))
                {
                    return false;
                }

                for (int j = 0; j < xmlText.TextTokens.Count; j++)
                {
                    if (xmlText.TextTokens[j].IsKind(SyntaxKind.XmlTextLiteralNewLineToken))
                    {
                        return true;
                    }
                    else if (!string.IsNullOrWhiteSpace(xmlText.TextTokens[j].ValueText))
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        private static bool IsLastOnLine(XmlElementEndTagSyntax xmlElementEndTag)
        {
            // Need to examine the following sibling of the parent
            return xmlElementEndTag.Parent is XmlElementSyntax xmlElement
                && IsLastOnLine(xmlElement);
        }

        private static bool RequiresSectionContent(SyntaxNode parent)
        {
            return parent.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                || parent.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
        }

        private static bool RequiresInlineContent(SyntaxNode parent)
        {
            Debug.Assert(!RequiresSectionContent(parent), "Assertion failed: !RequiresSectionContent(parent)");

            if (!(parent is XmlNodeSyntax xmlNode))
            {
                // Unrecognized parent element kind => unknown content kind
                return false;
            }

            var name = xmlNode.GetName();
            if (name is null || name.Prefix != null)
            {
                return false;
            }

            switch (name.LocalName.ValueText)
            {
            case XmlCommentHelper.ParaXmlTag:
                return true;

            default:
                return false;
            }
        }
    }
}
