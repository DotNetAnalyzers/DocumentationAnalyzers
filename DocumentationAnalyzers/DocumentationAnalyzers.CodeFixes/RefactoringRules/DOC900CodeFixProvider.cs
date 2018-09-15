// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.RefactoringRules
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using CommonMark;
    using CommonMark.Syntax;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using StringReader = System.IO.StringReader;
    using StringWriter = System.IO.StringWriter;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DOC900CodeFixProvider))]
    [Shared]
    internal partial class DOC900CodeFixProvider : CodeFixProvider
    {
        private static readonly SyntaxAnnotation UnnecessaryParagraphAnnotation =
            new SyntaxAnnotation("Documentation:UnnecessaryParagraph");

        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(DOC900RenderAsMarkdown.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
        {
            // this is unlikely to work as expected
            return null;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (!string.Equals(diagnostic.Id, DOC900RenderAsMarkdown.DiagnosticId, StringComparison.Ordinal))
                {
                    continue;
                }

                var documentRoot = await context.Document.GetSyntaxRootAsync(context.CancellationToken);
                SyntaxNode syntax = documentRoot.FindNode(diagnostic.Location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);
                if (syntax == null)
                {
                    continue;
                }

                DocumentationCommentTriviaSyntax documentationCommentTriviaSyntax = syntax.FirstAncestorOrSelf<DocumentationCommentTriviaSyntax>();
                if (documentationCommentTriviaSyntax == null)
                {
                    continue;
                }

                string description = "Render documentation as Markdown";
                context.RegisterCodeFix(
                    CodeAction.Create(
                        description,
                        cancellationToken => CreateChangedDocumentAsync(context, documentationCommentTriviaSyntax, cancellationToken),
                        nameof(DOC900CodeFixProvider)),
                    diagnostic);
            }
        }

        private static bool IsUnnecessaryParaElement(XmlElementSyntax elementSyntax)
        {
            if (elementSyntax == null)
            {
                return false;
            }

            if (HasAttributes(elementSyntax))
            {
                return false;
            }

            if (!IsParaElement(elementSyntax))
            {
                return false;
            }

            if (HasLooseContent(elementSyntax.Content))
            {
                return false;
            }

            return true;
        }

        private static bool HasLooseContent(SyntaxList<XmlNodeSyntax> content)
        {
            foreach (XmlNodeSyntax node in content)
            {
                if (node is XmlTextSyntax textSyntax)
                {
                    if (textSyntax.TextTokens.Any(token => !string.IsNullOrWhiteSpace(token.ValueText)))
                    {
                        return true;
                    }
                }

                if (node is XmlCDataSectionSyntax)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAttributes(XmlElementSyntax syntax)
        {
            return syntax?.StartTag?.Attributes.Count > 0;
        }

        private static bool IsParaElement(XmlElementSyntax syntax)
        {
            return string.Equals(XmlCommentHelper.ParaXmlTag, syntax?.StartTag?.Name?.ToString(), StringComparison.Ordinal);
        }

        private async Task<Document> CreateChangedDocumentAsync(CodeFixContext context, DocumentationCommentTriviaSyntax documentationCommentTriviaSyntax, CancellationToken cancellationToken)
        {
            StringBuilder leadingTriviaBuilder = new StringBuilder();
            SyntaxToken parentToken = documentationCommentTriviaSyntax.ParentTrivia.Token;
            int documentationCommentIndex = parentToken.LeadingTrivia.IndexOf(documentationCommentTriviaSyntax.ParentTrivia);
            for (int i = 0; i < documentationCommentIndex; i++)
            {
                SyntaxTrivia trivia = parentToken.LeadingTrivia[i];
                switch (trivia.Kind())
                {
                case SyntaxKind.EndOfLineTrivia:
                    leadingTriviaBuilder.Clear();
                    break;

                case SyntaxKind.WhitespaceTrivia:
                    leadingTriviaBuilder.Append(trivia.ToFullString());
                    break;

                default:
                    break;
                }
            }

            leadingTriviaBuilder.Append(documentationCommentTriviaSyntax.GetLeadingTrivia().ToFullString());

            // this is the trivia that should appear at the beginning of each line of the comment.
            SyntaxTrivia leadingTrivia = SyntaxFactory.DocumentationCommentExterior(leadingTriviaBuilder.ToString());

            string newLineText = context.Document.Project.Solution.Workspace.Options.GetOption(FormattingOptions.NewLine, LanguageNames.CSharp);
            DocumentationCommentTriviaSyntax contentsOnly = RemoveExteriorTrivia(documentationCommentTriviaSyntax);
            contentsOnly = contentsOnly.ReplaceNodes(contentsOnly.ChildNodes(), (originalNode, rewrittenNode) => RenderBlockElementAsMarkdown(originalNode, rewrittenNode, newLineText));
            string renderedContent = contentsOnly.Content.ToFullString();
            string[] lines = renderedContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            SyntaxList<XmlNodeSyntax> newContent = XmlSyntaxFactory.List();
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (i == lines.Length - 1)
                    {
                        break;
                    }

                    line = string.Empty;
                }

                if (newContent.Count > 0)
                {
                    newContent = newContent.Add(XmlSyntaxFactory.NewLine(newLineText).WithTrailingTrivia(SyntaxFactory.DocumentationCommentExterior("///")));
                }

                newContent = newContent.Add(XmlSyntaxFactory.Text(line.TrimEnd(), xmlEscape: false));
            }

            contentsOnly = contentsOnly.WithContent(newContent);
            contentsOnly =
                contentsOnly
                .ReplaceExteriorTrivia(leadingTrivia)
                .WithLeadingTrivia(SyntaxFactory.DocumentationCommentExterior("///"))
                .WithTrailingTrivia(SyntaxFactory.EndOfLine(Environment.NewLine));

            string fullContent = contentsOnly.ToFullString();
            SyntaxTriviaList parsedTrivia = SyntaxFactory.ParseLeadingTrivia(fullContent);
            SyntaxTrivia documentationTrivia = parsedTrivia.FirstOrDefault(i => i.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia));
            contentsOnly = documentationTrivia.GetStructure() as DocumentationCommentTriviaSyntax;
            if (contentsOnly == null)
            {
                return context.Document;
            }

            // Remove unnecessary nested paragraph elements
            contentsOnly = contentsOnly.ReplaceNodes(contentsOnly.DescendantNodes().OfType<XmlElementSyntax>(), MarkUnnecessaryParagraphs);
            contentsOnly = contentsOnly.ReplaceNodes(contentsOnly.DescendantNodes().OfType<XmlElementSyntax>(), RemoveUnnecessaryParagraphs);

            SyntaxNode root = await context.Document.GetSyntaxRootAsync(cancellationToken);
            SyntaxNode newRoot = root.ReplaceNode(documentationCommentTriviaSyntax, contentsOnly);
            if (documentationCommentTriviaSyntax.IsEquivalentTo(contentsOnly))
            {
                return context.Document;
            }

            if (documentationCommentTriviaSyntax.ToFullString().Equals(contentsOnly.ToFullString(), StringComparison.Ordinal))
            {
                return context.Document;
            }

            return context.Document.WithSyntaxRoot(newRoot);
        }

        private SyntaxNode MarkUnnecessaryParagraphs(SyntaxNode originalNode, SyntaxNode rewrittenNode)
        {
            XmlElementSyntax elementSyntax = rewrittenNode as XmlElementSyntax;
            if (IsUnnecessaryParaElement(elementSyntax))
            {
                return elementSyntax.WithAdditionalAnnotations(UnnecessaryParagraphAnnotation);
            }

            if (string.Equals(XmlCommentHelper.SummaryXmlTag, elementSyntax?.StartTag?.Name?.ToString(), StringComparison.Ordinal))
            {
                SyntaxList<XmlNodeSyntax> trimmedContent = elementSyntax.Content.WithoutFirstAndLastNewlines();
                if (trimmedContent.Count == 1
                    && IsParaElement(trimmedContent[0] as XmlElementSyntax)
                    && !HasAttributes(trimmedContent[0] as XmlElementSyntax))
                {
                    XmlNodeSyntax paraToRemove = elementSyntax.Content.GetFirstXmlElement(XmlCommentHelper.ParaXmlTag);
                    return elementSyntax.ReplaceNode(paraToRemove, paraToRemove.WithAdditionalAnnotations(UnnecessaryParagraphAnnotation));
                }
            }

            return rewrittenNode;
        }

        private SyntaxNode RemoveUnnecessaryParagraphs(XmlElementSyntax originalNode, XmlElementSyntax rewrittenNode)
        {
            bool hasUnnecessary = false;
            SyntaxList<XmlNodeSyntax> content = rewrittenNode.Content;
            for (int i = 0; i < content.Count; i++)
            {
                if (content[i].HasAnnotation(UnnecessaryParagraphAnnotation))
                {
                    hasUnnecessary = true;
                    XmlElementSyntax unnecessaryElement = (XmlElementSyntax)content[i];
                    content = content.ReplaceRange(unnecessaryElement, unnecessaryElement.Content);
                    i += unnecessaryElement.Content.Count;
                }
            }

            if (!hasUnnecessary)
            {
                return rewrittenNode;
            }

            return rewrittenNode.WithContent(content);
        }

        private SyntaxNode RenderBlockElementAsMarkdown(SyntaxNode originalNode, SyntaxNode rewrittenNode, string newLineText)
        {
            if (!(rewrittenNode is XmlElementSyntax elementSyntax))
            {
                return rewrittenNode;
            }

            switch (elementSyntax.StartTag?.Name?.ToString())
            {
            case XmlCommentHelper.SummaryXmlTag:
            case XmlCommentHelper.RemarksXmlTag:
            case XmlCommentHelper.ReturnsXmlTag:
            case XmlCommentHelper.ValueXmlTag:
                break;

            default:
                return rewrittenNode;
            }

            string rendered = RenderAsMarkdown(elementSyntax.Content.ToString()).Trim();
            return elementSyntax.WithContent(
                XmlSyntaxFactory.List(
                    XmlSyntaxFactory.NewLine(newLineText).WithoutTrailingTrivia(),
                    XmlSyntaxFactory.Text(" " + rendered.Replace("\n", "\n "), xmlEscape: false),
                    XmlSyntaxFactory.NewLine(newLineText).WithoutTrailingTrivia(),
                    XmlSyntaxFactory.Text(" ")));
        }

        private string RenderAsMarkdown(string text)
        {
            Block document;
            using (var reader = new StringReader(text))
            {
                document = CommonMarkConverter.ProcessStage1(reader, CommonMarkSettings.Default);
                CommonMarkConverter.ProcessStage2(document, CommonMarkSettings.Default);
            }

            StringBuilder builder = new StringBuilder();
            using (var writer = new StringWriter(builder))
            {
                DocumentationCommentPrinter.BlocksToHtml(writer, document, CommonMarkSettings.Default);
            }

            return builder.ToString();
        }

        private DocumentationCommentTriviaSyntax RemoveExteriorTrivia(DocumentationCommentTriviaSyntax documentationComment)
        {
            return documentationComment.ReplaceTrivia(
                documentationComment.DescendantTrivia(descendIntoTrivia: true).Where(i => i.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia)),
                (originalTrivia, rewrittenTrivia) => default);
        }
    }
}
