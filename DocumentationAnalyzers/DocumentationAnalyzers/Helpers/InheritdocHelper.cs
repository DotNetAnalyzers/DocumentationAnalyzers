// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Xml.Linq;
    using Microsoft.CodeAnalysis;

    internal static class InheritdocHelper
    {
        internal static ISymbol GetCandidateSymbol(ISymbol memberSymbol)
        {
            if (memberSymbol is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.MethodKind == MethodKind.Constructor || methodSymbol.MethodKind == MethodKind.StaticConstructor)
                {
                    var baseType = memberSymbol.ContainingType.BaseType;
                    return baseType.Constructors.Where(c => IsSameSignature(methodSymbol, c)).FirstOrDefault();
                }
                else if (!methodSymbol.ExplicitInterfaceImplementations.IsEmpty)
                {
                    // prototype(inheritdoc): do we need 'OrDefault'?
                    return methodSymbol.ExplicitInterfaceImplementations.FirstOrDefault();
                }
                else if (methodSymbol.IsOverride)
                {
                    return methodSymbol.OverriddenMethod;
                }
                else
                {
                    // prototype(inheritdoc): check for implicit interface
                    return null;
                }
            }
            else if (memberSymbol is INamedTypeSymbol typeSymbol)
            {
                if (typeSymbol.TypeKind == TypeKind.Class)
                {
                    // prototype(inheritdoc): when does base class take precedence over interface?
                    return typeSymbol.BaseType;
                }
                else if (typeSymbol.TypeKind == TypeKind.Interface)
                {
                    return typeSymbol.Interfaces.FirstOrDefault();
                }
                else
                {
                    // This includes structs, enums, and delegates as mentioned in the inheritdoc spec
                    return null;
                }
            }

            return null;
        }

        internal static string GetDocumentationCommentXml(ISymbol symbol, CultureInfo preferredCulture, bool expandInheritdoc, bool expandIncludes, CancellationToken cancellationToken)
        {
            var result = symbol.GetDocumentationCommentXml(preferredCulture, expandIncludes, cancellationToken);
            if (expandInheritdoc && !string.IsNullOrEmpty(result))
            {
                var element = XElement.Parse(result);
                var inheritedDocumentation = GetDocumentationCommentXml(GetCandidateSymbol(symbol), preferredCulture, expandInheritdoc: true, expandIncludes: true, cancellationToken);
                if (element.Elements(XmlCommentHelper.InheritdocXmlTag).Any())
                {
                    if (!string.IsNullOrEmpty(inheritedDocumentation))
                    {
                        IEnumerable<XObject> content = element.Attributes();
                        content = content.Concat(XElement.Parse(inheritedDocumentation).Nodes());
                        content = content.Concat(new[] { new XElement(XmlCommentHelper.AutoinheritdocXmlTag) });
                        element.ReplaceAll(content);
                    }
                    else
                    {
                        IEnumerable<XObject> content = element.Attributes();
                        content = content.Concat(new[] { new XElement(XmlCommentHelper.AutoinheritdocXmlTag) });
                        element.ReplaceAll(content);
                    }
                }
                else if (element.Elements(XmlCommentHelper.AutoinheritdocXmlTag).Any())
                {
                    if (!string.IsNullOrEmpty(inheritedDocumentation))
                    {
                        IEnumerable<XObject> content = element.Attributes();
                        content = content.Concat(XElement.Parse(inheritedDocumentation).Nodes());
                        content = content.Concat(new[] { new XElement(XmlCommentHelper.AutoinheritdocXmlTag) });
                        element.ReplaceAll(content);
                    }
                    else
                    {
                        IEnumerable<XObject> content = element.Attributes();
                        content = content.Concat(new[] { new XElement(XmlCommentHelper.AutoinheritdocXmlTag) });
                        element.ReplaceAll(content);
                    }
                }

                result = element.ToString();
            }
            else if (!string.IsNullOrEmpty(result))
            {
                result = XElement.Parse(result).ToString();
            }

            return result;
        }

        private static bool IsSameSignature(IMethodSymbol left, IMethodSymbol right)
        {
            if (left.Parameters.Length != right.Parameters.Length)
            {
                return false;
            }

            if (left.IsStatic != right.IsStatic)
            {
                return false;
            }

            if (!left.ReturnType.Equals(right.ReturnType))
            {
                return false;
            }

            for (int i = 0; i < left.Parameters.Length; i++)
            {
                if (!left.Parameters[i].Type.Equals(right.Parameters[i].Type))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
