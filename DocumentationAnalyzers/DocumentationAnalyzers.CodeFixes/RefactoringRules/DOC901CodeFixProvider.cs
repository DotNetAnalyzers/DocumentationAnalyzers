// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.RefactoringRules
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DOC901CodeFixProvider))]
    [Shared]
    internal partial class DOC901CodeFixProvider : CodeFixProvider
    {
        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(DOC901ConvertToDocumentationComment.DiagnosticId);

        public override FixAllProvider GetFixAllProvider()
        {
            // this is unlikely to work as expected
            return null;
        }

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                Debug.Assert(FixableDiagnosticIds.Contains(diagnostic.Id), "Assertion failed: FixableDiagnosticIds.Contains(diagnostic.Id)");

                context.RegisterCodeFix(
                    CodeAction.Create(
                        RefactoringResources.DOC901CodeFix,
                        token => GetTransformedDocumentAsync(context.Document, diagnostic, token),
                        nameof(DOC901CodeFixProvider)),
                    diagnostic);
            }

            return SpecializedTasks.CompletedTask;
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var firstComment = root.FindTrivia(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);
            var parentToken = firstComment.Token;

            var lines = new List<string>();
            for (int i = 0; i < parentToken.LeadingTrivia.Count; i++)
            {
                if (!parentToken.LeadingTrivia[i].IsKind(SyntaxKind.SingleLineCommentTrivia)
                    && !parentToken.LeadingTrivia[i].IsKind(SyntaxKind.MultiLineCommentTrivia))
                {
                    continue;
                }

                if (!diagnostic.Location.SourceSpan.Contains(parentToken.LeadingTrivia[i].Span))
                {
                    continue;
                }

                if (parentToken.LeadingTrivia[i].IsKind(SyntaxKind.SingleLineCommentTrivia))
                {
                    lines.Add(parentToken.LeadingTrivia[i].ToString().Substring(2));
                }
                else
                {
                    var commentText = parentToken.LeadingTrivia[i].ToString();
                    var normalizedText = commentText.Substring(1, commentText.Length - 3)
                        .Replace("\r\n", "\n").Replace('\r', '\n');
                    foreach (var line in normalizedText.Split('\n'))
                    {
                        if (Regex.IsMatch(line, "^\\s*\\*"))
                        {
                            lines.Add(line.Substring(line.IndexOf('*') + 1));
                        }
                        else
                        {
                            lines.Add(line);
                        }
                    }

                    lines[lines.Count - 1] = lines[lines.Count - 1].TrimEnd();
                }
            }

            int firstContentLine = lines.FindIndex(line => !string.IsNullOrWhiteSpace(line));
            if (firstContentLine >= 0)
            {
                lines.RemoveRange(0, firstContentLine);
                int lastContentLine = lines.FindLastIndex(line => !string.IsNullOrWhiteSpace(line));
                lines.RemoveRange(lastContentLine + 1, lines.Count - lastContentLine - 1);
            }

            if (lines.All(line => line.Length == 0 || line.StartsWith(" ")))
            {
                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Length == 0)
                    {
                        continue;
                    }

                    lines[i] = lines[i].Substring(1);
                }
            }

            var nodes = new List<XmlNodeSyntax>(lines.Select(line => XmlSyntaxFactory.Text(line)));
            for (int i = nodes.Count - 1; i > 0; i--)
            {
                nodes.Insert(i, XmlSyntaxFactory.NewLine(Environment.NewLine));
            }

            var summary = XmlSyntaxFactory.SummaryElement(Environment.NewLine, nodes.ToArray());

            var leadingTrivia = SyntaxFactory.TriviaList(parentToken.LeadingTrivia.TakeWhile(trivia => !trivia.Equals(firstComment)));
            var newParentToken = parentToken.WithLeadingTrivia(leadingTrivia.Add(SyntaxFactory.Trivia(XmlSyntaxFactory.DocumentationComment(Environment.NewLine, summary))));

            var newRoot = root.ReplaceToken(parentToken, newParentToken);
            return document.WithSyntaxRoot(root.ReplaceToken(parentToken, newParentToken));
        }
    }
}
