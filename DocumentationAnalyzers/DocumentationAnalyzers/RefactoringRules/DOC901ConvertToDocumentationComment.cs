// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.RefactoringRules
{
    using System.Collections.Immutable;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    /* This should be converted */
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC901ConvertToDocumentationComment : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DOC901";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC901.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(RefactoringResources.DOC901Title), RefactoringResources.ResourceManager, typeof(RefactoringResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(RefactoringResources.DOC901MessageFormat), RefactoringResources.ResourceManager, typeof(RefactoringResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(RefactoringResources.DOC901Description), RefactoringResources.ResourceManager, typeof(RefactoringResources));

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, AnalyzerCategory.Refactorings, DiagnosticSeverity.Hidden, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.ConstructorDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.ConversionOperatorDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.DelegateDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.DestructorDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.EnumDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.EnumMemberDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.EventDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.EventFieldDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.FieldDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.IndexerDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.InterfaceDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.MethodDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.NamespaceDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.OperatorDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(HandleDocumentedNode, SyntaxKind.StructDeclaration);
        }

        private void HandleDocumentedNode(SyntaxNodeAnalysisContext context)
        {
            DocumentationCommentTriviaSyntax documentationComment = context.Node.GetDocumentationCommentTriviaSyntax();
            if (documentationComment != null)
            {
                // The element already has a documentation comment.
                return;
            }

            SyntaxTrivia? firstComment = null;
            SyntaxTrivia? lastComment = null;
            bool isAtEndOfLine = false;
            var leadingTrivia = context.Node.GetLeadingTrivia();
            for (int i = leadingTrivia.Count - 1; i >= 0; i--)
            {
                switch (leadingTrivia[i].Kind())
                {
                case SyntaxKind.WhitespaceTrivia:
                    if (isAtEndOfLine)
                    {
                        // Multiple newlines
                        break;
                    }

                    // Ignore indentation
                    continue;

                case SyntaxKind.EndOfLineTrivia:
                    if (isAtEndOfLine)
                    {
                        // Multiple newlines
                        break;
                    }

                    isAtEndOfLine = true;
                    continue;

                case SyntaxKind.SingleLineCommentTrivia:
                    firstComment = leadingTrivia[i];
                    lastComment = lastComment ?? firstComment;
                    isAtEndOfLine = false;
                    continue;

                case SyntaxKind.MultiLineCommentTrivia:
                    if (lastComment != null)
                    {
                        // Have a multiline comment preceding a single line comment. Only consider the latter for this
                        // refactoring.
                        break;
                    }

                    firstComment = leadingTrivia[i];
                    lastComment = firstComment;
                    break;
                }

                // Reaching here means we don't want to continue the loop
                break;
            }

            if (firstComment is null)
            {
                return;
            }

            var location = Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(firstComment.Value.SpanStart, lastComment.Value.Span.End));
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
        }
    }
}
