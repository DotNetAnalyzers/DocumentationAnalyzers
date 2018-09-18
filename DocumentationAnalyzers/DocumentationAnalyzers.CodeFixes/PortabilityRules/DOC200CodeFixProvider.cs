// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.PortabilityRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DOC200CodeFixProvider))]
    [Shared]
    internal class DOC200CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(DOC200UseXmlDocumentationSyntax.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
            => CustomFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (!FixableDiagnosticIds.Contains(diagnostic.Id))
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        PortabilityResources.DOC200CodeFix,
                        token => GetTransformedDocumentAsync(context.Document, diagnostic, token),
                        nameof(DOC200CodeFixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxToken token = root.FindToken(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);

            var xmlElement = token.Parent.FirstAncestorOrSelf<XmlElementSyntax>();
            var oldStartToken = xmlElement.StartTag.Name.LocalName;

            string newIdentifier;
            switch (oldStartToken.ValueText)
            {
            case "p":
                newIdentifier = XmlCommentHelper.ParaXmlTag;
                break;

            case "tt":
                newIdentifier = XmlCommentHelper.CXmlTag;
                break;

            case "pre":
                newIdentifier = XmlCommentHelper.CodeXmlTag;
                break;

            case "ul":
            case "ol":
                newIdentifier = XmlCommentHelper.ListXmlTag;
                break;

            default:
                // Not handled
                return document;
            }

            var newStartToken = SyntaxFactory.Identifier(oldStartToken.LeadingTrivia, newIdentifier, oldStartToken.TrailingTrivia);
            var newXmlElement = xmlElement.ReplaceToken(oldStartToken, newStartToken);

            var oldEndToken = newXmlElement.EndTag.Name.LocalName;
            var newEndToken = SyntaxFactory.Identifier(oldEndToken.LeadingTrivia, newIdentifier, oldEndToken.TrailingTrivia);
            newXmlElement = newXmlElement.ReplaceToken(oldEndToken, newEndToken);

            if (newIdentifier == XmlCommentHelper.ListXmlTag)
            {
                // Add an attribute for the list kind
                string listType = oldStartToken.ValueText == "ol" ? "number" : "bullet";
                newXmlElement = newXmlElement.WithStartTag(newXmlElement.StartTag.AddAttributes(XmlSyntaxFactory.TextAttribute(XmlCommentHelper.TypeAttributeName, listType)));

                // Replace each <li>...</li> element with <item><description>...</description></item>
                for (int i = 0; i < newXmlElement.Content.Count; i++)
                {
                    if (newXmlElement.Content[i] is XmlElementSyntax childXmlElement
                        && childXmlElement.StartTag?.Name?.LocalName.ValueText == "li"
                        && childXmlElement.StartTag.Name.Prefix == null)
                    {
                        oldStartToken = childXmlElement.StartTag.Name.LocalName;
                        newStartToken = SyntaxFactory.Identifier(oldStartToken.LeadingTrivia, XmlCommentHelper.ItemXmlTag, oldStartToken.TrailingTrivia);
                        var newChildXmlElement = childXmlElement.ReplaceToken(oldStartToken, newStartToken);

                        oldEndToken = newChildXmlElement.EndTag.Name.LocalName;
                        newEndToken = SyntaxFactory.Identifier(oldEndToken.LeadingTrivia, XmlCommentHelper.ItemXmlTag, oldEndToken.TrailingTrivia);
                        newChildXmlElement = newChildXmlElement.ReplaceToken(oldEndToken, newEndToken);

                        newChildXmlElement = newChildXmlElement.WithContent(XmlSyntaxFactory.List(XmlSyntaxFactory.Element(XmlCommentHelper.DescriptionXmlTag, newChildXmlElement.Content)));

                        newXmlElement = newXmlElement.ReplaceNode(childXmlElement, newChildXmlElement);
                    }
                }
            }

            return document.WithSyntaxRoot(root.ReplaceNode(xmlElement, newXmlElement));
        }
    }
}
