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

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DOC900RenderAsMarkdown : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DOC900";
        private const string HelpLink = "https://github.com/DotNetAnalyzers/DocumentationAnalyzers/blob/master/docs/DOC900.md";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(RefactoringResources.DOC900Title), RefactoringResources.ResourceManager, typeof(RefactoringResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(RefactoringResources.DOC900MessageFormat), RefactoringResources.ResourceManager, typeof(RefactoringResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(RefactoringResources.DOC900Description), RefactoringResources.ResourceManager, typeof(RefactoringResources));

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
            if (documentationComment == null)
            {
                return;
            }

            // only report the diagnostic for elements which have documentation comments
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, documentationComment.GetLocation()));
        }
    }
}
