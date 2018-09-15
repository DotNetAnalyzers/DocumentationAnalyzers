// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Helpers
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    internal static class SyntaxNodeExtensionsEx
    {
        public static bool IsSymbolDeclaration(this SyntaxNode node)
        {
            switch (node.Kind())
            {
            case SyntaxKind.ClassDeclaration:
            case SyntaxKind.ConstructorDeclaration:
            case SyntaxKind.ConversionOperatorDeclaration:
            case SyntaxKind.DelegateDeclaration:
            case SyntaxKind.DestructorDeclaration:
            case SyntaxKind.EnumDeclaration:
            case SyntaxKind.EnumMemberDeclaration:
            case SyntaxKind.EventDeclaration:
            case SyntaxKind.EventFieldDeclaration:
            case SyntaxKind.FieldDeclaration:
            case SyntaxKind.IndexerDeclaration:
            case SyntaxKind.InterfaceDeclaration:
            case SyntaxKind.MethodDeclaration:
            case SyntaxKind.NamespaceDeclaration:
            case SyntaxKind.OperatorDeclaration:
            case SyntaxKind.PropertyDeclaration:
            case SyntaxKind.StructDeclaration:
                return true;

            default:
                return false;
            }
        }
    }
}
