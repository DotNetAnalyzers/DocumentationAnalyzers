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

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DOC205CodeFixProvider))]
    [Shared]
    internal class DOC205CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(DOC205InheritDocumentation.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
            => CustomFixAllProviders.BatchFixer;

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                Debug.Assert(FixableDiagnosticIds.Contains(diagnostic.Id), "Assertion failed: FixableDiagnosticIds.Contains(diagnostic.Id)");

                context.RegisterCodeFix(
                    CodeAction.Create(
                        PortabilityResources.DOC205CodeFix,
                        token => GetTransformedDocumentAsync(context.Document, diagnostic, token),
                        nameof(DOC205CodeFixProvider)),
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

            var newLineText = Environment.NewLine;

            var content = new List<XmlNodeSyntax>();
            if (!string.IsNullOrEmpty(candidateDocumentation))
            {
                var xmlDocumentation = XElement.Parse(candidateDocumentation);
                content.AddRange(xmlDocumentation.Elements().Select(element => XmlSyntaxFactory.Node(newLineText, element)));
                content.Add(XmlSyntaxFactory.NewLine(newLineText));
            }

            var newStartToken = SyntaxFactory.Identifier(oldStartToken.LeadingTrivia, XmlCommentHelper.AutoinheritdocXmlTag, oldStartToken.TrailingTrivia);
            var newXmlNode = xmlNode.ReplaceToken(oldStartToken, newStartToken);

            if (newXmlNode is XmlElementSyntax newXmlElement)
            {
                var oldEndToken = newXmlElement.EndTag.Name.LocalName;
                var newEndToken = SyntaxFactory.Identifier(oldEndToken.LeadingTrivia, XmlCommentHelper.AutoinheritdocXmlTag, oldEndToken.TrailingTrivia);
                newXmlNode = newXmlNode.ReplaceToken(oldEndToken, newEndToken);
            }

            content.Add(newXmlNode);

            return document.WithSyntaxRoot(root.ReplaceNode(xmlNode, content));
        }
    }
}
