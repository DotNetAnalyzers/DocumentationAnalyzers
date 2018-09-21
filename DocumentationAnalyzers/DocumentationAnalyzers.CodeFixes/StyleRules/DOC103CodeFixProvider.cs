// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.StyleRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DOC103CodeFixProvider))]
    [Shared]
    internal class DOC103CodeFixProvider : CodeFixProvider
    {
        private const string CS1570 = nameof(CS1570);

        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(DOC103UseUnicodeCharacters.DiagnosticId, CS1570);

        public override FixAllProvider GetFixAllProvider()
            => CustomFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                Debug.Assert(FixableDiagnosticIds.Contains(diagnostic.Id), "Assertion failed: FixableDiagnosticIds.Contains(diagnostic.Id)");

                SyntaxToken token = root.FindToken(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);
                if (!token.IsKind(SyntaxKind.XmlEntityLiteralToken))
                {
                    // Could be an unrelated CS1570 error.
                    return;
                }

                string newText = token.ValueText;
                if (newText == token.Text)
                {
                    // The entity is not recognized. Try decoding as an HTML entity.
                    newText = WebUtility.HtmlDecode(token.Text);
                }

                if (newText == token.Text)
                {
                    // Unknown entity
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        StyleResources.DOC103CodeFix,
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, newText, cancellationToken),
                        nameof(DOC103CodeFixProvider)),
                    diagnostic);
            }
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, string newText, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxToken token = root.FindToken(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);

            var newToken = SyntaxFactory.Token(token.LeadingTrivia, SyntaxKind.XmlTextLiteralToken, newText, newText, token.TrailingTrivia);

            return document.WithSyntaxRoot(root.ReplaceToken(token, newToken));
        }
    }
}
