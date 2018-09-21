// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.StyleRules
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DOC107CodeFixProvider))]
    [Shared]
    internal class DOC107CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(DOC107UseSeeCref.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
            => CustomFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                Debug.Assert(FixableDiagnosticIds.Contains(diagnostic.Id), "Assertion failed: FixableDiagnosticIds.Contains(diagnostic.Id)");

                context.RegisterCodeFix(
                    CodeAction.Create(
                        StyleResources.DOC107CodeFix,
                        token => GetTransformedDocumentAsync(context.Document, diagnostic, token),
                        nameof(DOC107CodeFixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var xmlElement = (XmlElementSyntax)root.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);

            var newXmlElement = XmlSyntaxFactory.EmptyElement(XmlCommentHelper.SeeXmlTag)
                .AddAttributes(XmlSyntaxFactory.TextAttribute(
                    "cref",
                    SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.XmlTextLiteralToken, xmlElement.Content.ToFullString(), xmlElement.Content.ToFullString(), SyntaxTriviaList.Empty)))
                .WithTriviaFrom(xmlElement);

            return document.WithSyntaxRoot(root.ReplaceNode(xmlElement, newXmlElement));
        }
    }
}
