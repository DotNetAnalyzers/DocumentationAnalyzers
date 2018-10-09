// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.PortabilityRules
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DOC206CodeFixProvider))]
    [Shared]
    internal class DOC206CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(DOC206SynchronizeDocumentation.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
            => CustomFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                Debug.Assert(FixableDiagnosticIds.Contains(diagnostic.Id), "Assertion failed: FixableDiagnosticIds.Contains(diagnostic.Id)");

                context.RegisterCodeFix(
                    CodeAction.Create(
                        PortabilityResources.DOC206CodeFix,
                        token => GetTransformedDocumentAsync(context.Document, diagnostic, token),
                        nameof(DOC206CodeFixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var xmlNode = (XmlNodeSyntax)root.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
            var oldStartToken = xmlNode.GetName().LocalName;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var documentedSymbol = semanticModel.GetDeclaredSymbol(xmlNode.FirstAncestorOrSelf<SyntaxNode>(SyntaxNodeExtensionsEx.IsSymbolDeclaration), cancellationToken);
            var candidateSymbol = InheritdocHelper.GetCandidateSymbol(documentedSymbol);
            var candidateDocumentation = candidateSymbol.GetDocumentationCommentXml(expandIncludes: false, cancellationToken: cancellationToken);

            var xmlDocumentation = XElement.Parse(candidateDocumentation);
            var newLineText = Environment.NewLine;

            var content = new List<XmlNodeSyntax>();
            content.AddRange(xmlDocumentation.Elements().Select(element => XmlSyntaxFactory.Node(newLineText, element)));
            content.Add(XmlSyntaxFactory.NewLine(newLineText));
            content.Add(xmlNode);

            return document.WithSyntaxRoot(root.ReplaceNode(
                xmlNode.FirstAncestorOrSelf<DocumentationCommentTriviaSyntax>(),
                XmlSyntaxFactory.DocumentationComment(newLineText, content.ToArray())));
        }
    }
}
