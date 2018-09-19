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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DOC204CodeFixProvider))]
    [Shared]
    internal class DOC204CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(DOC204UseInlineElementsCorrectly.DiagnosticId);

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
                        PortabilityResources.DOC204CodeFix,
                        token => GetTransformedDocumentAsync(context.Document, diagnostic, token),
                        nameof(DOC204CodeFixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxToken token = root.FindToken(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);

            var xmlNode = token.Parent.FirstAncestorOrSelf<XmlNodeSyntax>();
            var oldStartToken = xmlNode.GetName().LocalName;

            string newIdentifier;
            switch (oldStartToken.ValueText)
            {
            case XmlCommentHelper.ParamRefXmlTag:
                newIdentifier = XmlCommentHelper.ParamXmlTag;
                break;

            case XmlCommentHelper.TypeParamRefXmlTag:
                newIdentifier = XmlCommentHelper.TypeParamXmlTag;
                break;

            default:
                // Not handled
                return document;
            }

            var newStartToken = SyntaxFactory.Identifier(oldStartToken.LeadingTrivia, newIdentifier, oldStartToken.TrailingTrivia);
            var newXmlNode = xmlNode.ReplaceToken(oldStartToken, newStartToken);

            if (newXmlNode is XmlElementSyntax newXmlElement)
            {
                var oldEndToken = newXmlElement.EndTag.Name.LocalName;
                var newEndToken = SyntaxFactory.Identifier(oldEndToken.LeadingTrivia, newIdentifier, oldEndToken.TrailingTrivia);
                newXmlNode = newXmlNode.ReplaceToken(oldEndToken, newEndToken);
            }

            return document.WithSyntaxRoot(root.ReplaceNode(xmlNode, newXmlNode));
        }
    }
}
