// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Helpers
{
    using System;
    using System.Linq;
    using Microsoft.CodeAnalysis;

    internal static class SymbolExtensions
    {
        /// <summary>
        /// Determines if a symbol has a parameter with a given name.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="comparer">The comparer to use for symbol names.</param>
        /// <returns><see langword="true"/> if a parameter with the name <paramref name="name"/> is accessible in the
        /// context of the specified <paramref name="symbol"/>; otherwise, <see langword="false"/>.</returns>
        public static bool HasAnyParameter(this ISymbol symbol, string name, StringComparer comparer)
        {
            if (symbol.Kind == SymbolKind.Method)
            {
                var methodSymbol = (IMethodSymbol)symbol;
                return methodSymbol.Parameters.Any(parameter => comparer.Equals(parameter.Name, name));
            }

            return false;
        }

        /// <summary>
        /// Determines if a symbol has a type parameter with a given name.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="name">The name of the type parameter.</param>
        /// <param name="comparer">The comparer to use for symbol names.</param>
        /// <returns><see langword="true"/> if a type parameter with the name <paramref name="name"/> is accessible in
        /// the context of the specified <paramref name="symbol"/>; otherwise, <see langword="false"/>.</returns>
        public static bool HasAnyTypeParameter(this ISymbol symbol, string name, StringComparer comparer)
        {
            for (var currentSymbol = symbol; currentSymbol != null; currentSymbol = currentSymbol.ContainingSymbol)
            {
                switch (currentSymbol.Kind)
                {
                case SymbolKind.NamedType:
                    if (((INamedTypeSymbol)symbol).TypeParameters.Any(parameter => comparer.Equals(parameter.Name, name)))
                    {
                        return true;
                    }

                    break;

                case SymbolKind.Method:
                    if (((IMethodSymbol)symbol).TypeParameters.Any(parameter => comparer.Equals(parameter.Name, name)))
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }
    }
}
