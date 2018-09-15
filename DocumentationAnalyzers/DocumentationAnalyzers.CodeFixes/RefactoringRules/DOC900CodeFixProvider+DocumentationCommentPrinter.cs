// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace DocumentationAnalyzers.RefactoringRules
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using CommonMark;
    using CommonMark.Syntax;
    using DocumentationAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;

    internal partial class DOC900CodeFixProvider
    {
        internal static class DocumentationCommentPrinter
        {
            private const string HexCharacters = "0123456789ABCDEF";

            private static readonly char[] EscapeHtmlCharacters = new[] { '&', '<', '>', '"' };

            private static readonly char[] EscapeHtmlLessThan = "&lt;".ToCharArray();
            private static readonly char[] EscapeHtmlGreaterThan = "&gt;".ToCharArray();
            private static readonly char[] EscapeHtmlAmpersand = "&amp;".ToCharArray();
            private static readonly char[] EscapeHtmlQuote = "&quot;".ToCharArray();

            private static readonly string[] HeaderOpenerTags = new[] { "<h1>", "<h2>", "<h3>", "<h4>", "<h5>", "<h6>" };
            private static readonly string[] HeaderCloserTags = new[] { "</h1>", "</h2>", "</h3>", "</h4>", "</h5>", "</h6>" };

            private static readonly bool[] UrlSafeCharacters = new[]
            {
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,
                false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false,
                false, true,  false, true,  true,  true,  false, false, true,  true,  true,  true,  true,  true,  true,  true,
                true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false, true,  false, true,
                true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,
                true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false, false, false, false, true,
                false, true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,
                true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  true,  false, false, false, false, false,
            };

            /// <summary>
            /// Convert a block list to HTML.  Returns 0 on success, and sets result.
            /// </summary>
            /// <remarks>Orig: blocks_to_html.</remarks>
            public static void BlocksToHtml(System.IO.TextWriter writer, Block block, CommonMarkSettings settings, ISymbol documentedSymbol)
            {
                var wrapper = new DocumentationCommentTextWriter(writer);
                BlocksToHtmlInner(wrapper, block, settings, documentedSymbol);
            }

            /// <summary>
            /// Escapes special URL characters.
            /// </summary>
            /// <remarks>Orig: escape_html(inp, preserve_entities).</remarks>
            private static void EscapeUrl(string input, DocumentationCommentTextWriter target)
            {
                if (input == null)
                {
                    return;
                }

                char c;
                int lastPos = 0;
                int len = input.Length;
                char[] buffer;

                if (target.Buffer.Length < len)
                {
                    buffer = target.Buffer = input.ToCharArray();
                }
                else
                {
                    buffer = target.Buffer;
                    input.CopyTo(0, buffer, 0, len);
                }

                // since both \r and \n are not url-safe characters and will be encoded, all calls are
                // made to WriteConstant.
                for (var pos = 0; pos < len; pos++)
                {
                    c = buffer[pos];

                    if (c == '&')
                    {
                        target.WriteConstant(buffer, lastPos, pos - lastPos);
                        lastPos = pos + 1;
                        target.WriteConstant(EscapeHtmlAmpersand);
                    }
                    else if (c < 128 && !UrlSafeCharacters[c])
                    {
                        target.WriteConstant(buffer, lastPos, pos - lastPos);
                        lastPos = pos + 1;

                        target.WriteConstant(new[] { '%', HexCharacters[c / 16], HexCharacters[c % 16] });
                    }
                    else if (c > 127)
                    {
                        target.WriteConstant(buffer, lastPos, pos - lastPos);
                        lastPos = pos + 1;

                        byte[] bytes;
                        if (c >= '\ud800' && c <= '\udfff' && len != lastPos)
                        {
                            // this char is the first of UTF-32 character pair
                            bytes = Encoding.UTF8.GetBytes(new[] { c, buffer[lastPos] });
                            lastPos = ++pos + 1;
                        }
                        else
                        {
                            bytes = Encoding.UTF8.GetBytes(new[] { c });
                        }

                        for (var i = 0; i < bytes.Length; i++)
                        {
                            target.WriteConstant(new[] { '%', HexCharacters[bytes[i] / 16], HexCharacters[bytes[i] % 16] });
                        }
                    }
                }

                target.WriteConstant(buffer, lastPos, len - lastPos);
            }

            /// <summary>
            /// Escapes special HTML characters.
            /// </summary>
            /// <remarks>Orig: escape_html(inp, preserve_entities).</remarks>
            private static void EscapeHtml(string input, DocumentationCommentTextWriter target)
            {
                if (input.Length == 0)
                {
                    return;
                }

                int pos;
                int lastPos = 0;
                char[] buffer;

                if (target.Buffer.Length < input.Length)
                {
                    buffer = target.Buffer = new char[input.Length];
                }
                else
                {
                    buffer = target.Buffer;
                }

                input.CopyTo(0, buffer, 0, input.Length);

                while ((pos = input.IndexOfAny(EscapeHtmlCharacters, lastPos, input.Length - lastPos + 0)) != -1)
                {
                    target.Write(buffer, lastPos - 0, pos - lastPos);
                    lastPos = pos + 1;

                    switch (input[pos])
                    {
                    case '<':
                        target.WriteConstant(EscapeHtmlLessThan);
                        break;
                    case '>':
                        target.WriteConstant(EscapeHtmlGreaterThan);
                        break;
                    case '&':
                        target.WriteConstant(EscapeHtmlAmpersand);
                        break;
                    case '"':
                        target.WriteConstant(EscapeHtmlQuote);
                        break;
                    }
                }

                target.Write(buffer, lastPos - 0, input.Length - lastPos + 0);
            }

            /// <summary>
            /// Escapes special HTML characters.
            /// </summary>
            /// <remarks>Orig: escape_html(inp, preserve_entities).</remarks>
            private static void EscapeHtml(StringContent inp, DocumentationCommentTextWriter target)
            {
                int pos;
                int lastPos;
                char[] buffer = target.Buffer;

                var part = inp.ToString(new StringBuilder());

                if (buffer.Length < part.Length)
                {
                    buffer = target.Buffer = new char[part.Length];
                }

                part.CopyTo(0, buffer, 0, part.Length);

                lastPos = pos = 0;
                while ((pos = part.IndexOfAny(EscapeHtmlCharacters, lastPos, part.Length - lastPos + 0)) != -1)
                {
                    target.Write(buffer, lastPos - 0, pos - lastPos);
                    lastPos = pos + 1;

                    switch (part[pos])
                    {
                    case '<':
                        target.WriteConstant(EscapeHtmlLessThan);
                        break;
                    case '>':
                        target.WriteConstant(EscapeHtmlGreaterThan);
                        break;
                    case '&':
                        target.WriteConstant(EscapeHtmlAmpersand);
                        break;
                    case '"':
                        target.WriteConstant(EscapeHtmlQuote);
                        break;
                    }
                }

                target.Write(buffer, lastPos - 0, part.Length - lastPos + 0);
            }

            private static void BlocksToHtmlInner(DocumentationCommentTextWriter writer, Block block, CommonMarkSettings settings, ISymbol documentedSymbol)
            {
                var stack = new Stack<BlockStackEntry>();
                var inlineStack = new Stack<InlineStackEntry>();
                bool visitChildren;
                string stackLiteral = null;
                bool stackTight = false;
                bool tight = false;
                int x;

                while (block != null)
                {
                    visitChildren = false;

                    switch (block.Tag)
                    {
                    case BlockTag.Document:
                        stackLiteral = null;
                        stackTight = false;
                        visitChildren = true;
                        break;

                    case BlockTag.Paragraph:
                        if (tight)
                        {
                            InlinesToHtml(writer, block.InlineContent, settings, documentedSymbol, inlineStack);
                        }
                        else
                        {
                            writer.EnsureLine();
                            writer.WriteConstant("<para>");
                            InlinesToHtml(writer, block.InlineContent, settings, documentedSymbol, inlineStack);
                            writer.WriteLineConstant("</para>");
                        }

                        break;

                    case BlockTag.BlockQuote:
                        writer.EnsureLine();
                        writer.WriteLineConstant("<note>");

                        stackLiteral = "</note>";
                        stackTight = false;
                        visitChildren = true;
                        break;

                    case BlockTag.ListItem:
                        writer.EnsureLine();
                        writer.WriteConstant("<item><description>");

                        stackLiteral = "</description></item>";
                        stackTight = tight;
                        visitChildren = true;
                        break;

                    case BlockTag.List:
                        // make sure a list starts at the beginning of the line:
                        writer.EnsureLine();
                        var data = block.ListData;
                        writer.WriteConstant(data.ListType == ListType.Bullet ? "<list type=\"bullet\"" : "<list type=\"number\"");
                        if (data.Start != 1)
                        {
                            writer.WriteConstant(" start=\"");
                            writer.WriteConstant(data.Start.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            writer.Write('\"');
                        }

                        writer.WriteLineConstant(">");

                        stackLiteral = "</list>";
                        stackTight = data.IsTight;
                        visitChildren = true;
                        break;

                    case BlockTag.AtxHeading:
                    case BlockTag.SetextHeading:
                        writer.EnsureLine();

                        x = block.Heading.Level;
                        writer.WriteConstant(x > 0 && x < 7 ? HeaderOpenerTags[x - 1] : "<h" + x.ToString(CultureInfo.InvariantCulture) + ">");
                        InlinesToHtml(writer, block.InlineContent, settings, documentedSymbol, inlineStack);
                        writer.WriteLineConstant(x > 0 && x < 7 ? HeaderCloserTags[x - 1] : "</h" + x.ToString(CultureInfo.InvariantCulture) + ">");
                        break;

                    case BlockTag.IndentedCode:
                        writer.EnsureLine();
                        writer.WriteConstant("<code>");
                        EscapeHtml(block.StringContent, writer);
                        writer.WriteLineConstant("</code>");
                        break;

                    case BlockTag.FencedCode:
                        writer.EnsureLine();
                        writer.WriteConstant("<code");
                        var info = block.FencedCodeData.Info;
                        if (info != null && info.Length > 0)
                        {
                            x = info.IndexOf(' ');
                            if (x == -1)
                            {
                                x = info.Length;
                            }

                            writer.WriteConstant(" language=\"");
                            EscapeHtml(info.Substring(0, x), writer);
                            writer.Write('\"');
                        }

                        writer.Write('>');
                        writer.WriteLine();
                        EscapeHtml(block.StringContent, writer);
                        writer.WriteLineConstant("</code>");
                        break;

                    case BlockTag.HtmlBlock:
                        writer.Write(block.StringContent.ToString(new StringBuilder()));
                        break;

                    case BlockTag.ThematicBreak:
                        writer.WriteLineConstant("<hr />");
                        break;

                    case BlockTag.ReferenceDefinition:
                        break;

                    default:
                        throw new CommonMarkException("Block type " + block.Tag + " is not supported.", block);
                    }

                    if (visitChildren)
                    {
                        stack.Push(new BlockStackEntry(stackLiteral, block.NextSibling, tight));

                        tight = stackTight;
                        block = block.FirstChild;
                    }
                    else if (block.NextSibling != null)
                    {
                        block = block.NextSibling;
                    }
                    else
                    {
                        block = null;
                    }

                    while (block == null && stack.Count > 0)
                    {
                        var entry = stack.Pop();

                        writer.WriteLineConstant(entry.Literal);
                        tight = entry.IsTight;
                        block = entry.Target;
                    }
                }
            }

            /// <summary>
            /// Writes the inline list to the given writer as plain text (without any HTML tags).
            /// </summary>
            /// <seealso href="https://github.com/jgm/CommonMark/issues/145"/>
            private static void InlinesToPlainText(DocumentationCommentTextWriter writer, Inline inline, Stack<InlineStackEntry> stack)
            {
                bool withinLink = false;
                bool stackWithinLink = false;
                bool visitChildren;
                string stackLiteral = null;
                var origStackCount = stack.Count;

                while (inline != null)
                {
                    visitChildren = false;

                    switch (inline.Tag)
                    {
                    case InlineTag.String:
                    case InlineTag.Code:
                    case InlineTag.RawHtml:
                        EscapeHtml(inline.LiteralContent, writer);
                        break;

                    case InlineTag.LineBreak:
                    case InlineTag.SoftBreak:
                        writer.WriteLine();
                        break;

                    case InlineTag.Link:
                        if (withinLink)
                        {
                            writer.Write('[');
                            stackLiteral = "]";
                            visitChildren = true;
                            stackWithinLink = withinLink;
                        }
                        else
                        {
                            visitChildren = true;
                            stackWithinLink = true;
                            stackLiteral = string.Empty;
                        }

                        break;

                    case InlineTag.Image:
                        visitChildren = true;
                        stackWithinLink = true;
                        stackLiteral = string.Empty;
                        break;

                    case InlineTag.Strong:
                    case InlineTag.Emphasis:
                        stackLiteral = string.Empty;
                        stackWithinLink = withinLink;
                        visitChildren = true;
                        break;

                    default:
                        throw new CommonMarkException("Inline type " + inline.Tag + " is not supported.", inline);
                    }

                    if (visitChildren)
                    {
                        stack.Push(new InlineStackEntry(stackLiteral, inline.NextSibling, withinLink));

                        withinLink = stackWithinLink;
                        inline = inline.FirstChild;
                    }
                    else if (inline.NextSibling != null)
                    {
                        inline = inline.NextSibling;
                    }
                    else
                    {
                        inline = null;
                    }

                    while (inline == null && stack.Count > origStackCount)
                    {
                        var entry = stack.Pop();
                        writer.WriteConstant(entry.Literal);
                        inline = entry.Target;
                        withinLink = entry.IsWithinLink;
                    }
                }
            }

            /// <summary>
            /// Writes the inline list to the given writer as HTML code.
            /// </summary>
            private static void InlinesToHtml(DocumentationCommentTextWriter writer, Inline inline, CommonMarkSettings settings, ISymbol documentedSymbol, Stack<InlineStackEntry> stack)
            {
                var uriResolver = settings.UriResolver;
                bool withinLink = false;
                bool stackWithinLink = false;
                bool visitChildren;
                string stackLiteral = null;

                while (inline != null)
                {
                    visitChildren = false;

                    switch (inline.Tag)
                    {
                    case InlineTag.String:
                        EscapeHtml(inline.LiteralContent, writer);
                        break;

                    case InlineTag.LineBreak:
                        writer.WriteLineConstant("<br />");
                        break;

                    case InlineTag.SoftBreak:
                        if (settings.RenderSoftLineBreaksAsLineBreaks)
                        {
                            writer.WriteLineConstant("<br />");
                        }
                        else
                        {
                            writer.WriteLine();
                        }

                        break;

                    case InlineTag.Code:
                        if (documentedSymbol.HasAnyParameter(inline.LiteralContent, StringComparer.Ordinal))
                        {
                            writer.WriteConstant("<paramref name=\"");
                            EscapeHtml(inline.LiteralContent, writer);
                            writer.WriteConstant("\"/>");
                        }
                        else if (documentedSymbol.HasAnyTypeParameter(inline.LiteralContent, StringComparer.Ordinal))
                        {
                            writer.WriteConstant("<typeparamref name=\"");
                            EscapeHtml(inline.LiteralContent, writer);
                            writer.WriteConstant("\"/>");
                        }
                        else
                        {
                            writer.WriteConstant("<c>");
                            EscapeHtml(inline.LiteralContent, writer);
                            writer.WriteConstant("</c>");
                        }

                        break;

                    case InlineTag.RawHtml:
                        writer.Write(inline.LiteralContent);
                        break;

                    case InlineTag.Link:
                        if (withinLink)
                        {
                            writer.Write('[');
                            stackLiteral = "]";
                            stackWithinLink = withinLink;
                            visitChildren = true;
                        }
                        else
                        {
                            writer.WriteConstant("<see href=\"");
                            if (uriResolver != null)
                            {
                                EscapeUrl(uriResolver(inline.TargetUrl), writer);
                            }
                            else
                            {
                                EscapeUrl(inline.TargetUrl, writer);
                            }

                            writer.Write('\"');
                            if (inline.LiteralContent.Length > 0)
                            {
                                writer.WriteConstant(" title=\"");
                                EscapeHtml(inline.LiteralContent, writer);
                                writer.Write('\"');
                            }

                            writer.Write('>');

                            visitChildren = true;
                            stackWithinLink = true;
                            stackLiteral = "</see>";
                        }

                        break;

                    case InlineTag.Image:
                        writer.WriteConstant("<img src=\"");
                        if (uriResolver != null)
                        {
                            EscapeUrl(uriResolver(inline.TargetUrl), writer);
                        }
                        else
                        {
                            EscapeUrl(inline.TargetUrl, writer);
                        }

                        writer.WriteConstant("\" alt=\"");
                        InlinesToPlainText(writer, inline.FirstChild, stack);
                        writer.Write('\"');
                        if (inline.LiteralContent.Length > 0)
                        {
                            writer.WriteConstant(" title=\"");
                            EscapeHtml(inline.LiteralContent, writer);
                            writer.Write('\"');
                        }

                        writer.WriteConstant(" />");
                        break;

                    case InlineTag.Strong:
                        writer.WriteConstant("<strong>");
                        stackLiteral = "</strong>";
                        stackWithinLink = withinLink;
                        visitChildren = true;
                        break;

                    case InlineTag.Emphasis:
                        writer.WriteConstant("<em>");
                        stackLiteral = "</em>";
                        visitChildren = true;
                        stackWithinLink = withinLink;
                        break;

                    case InlineTag.Strikethrough:
                        writer.WriteConstant("<del>");
                        stackLiteral = "</del>";
                        visitChildren = true;
                        stackWithinLink = withinLink;
                        break;

                    default:
                        throw new CommonMarkException("Inline type " + inline.Tag + " is not supported.", inline);
                    }

                    if (visitChildren)
                    {
                        stack.Push(new InlineStackEntry(stackLiteral, inline.NextSibling, withinLink));

                        withinLink = stackWithinLink;
                        inline = inline.FirstChild;
                    }
                    else if (inline.NextSibling != null)
                    {
                        inline = inline.NextSibling;
                    }
                    else
                    {
                        inline = null;
                    }

                    while (inline == null && stack.Count > 0)
                    {
                        var entry = stack.Pop();
                        writer.WriteConstant(entry.Literal);
                        inline = entry.Target;
                        withinLink = entry.IsWithinLink;
                    }
                }
            }

            private readonly struct BlockStackEntry
            {
                public readonly string Literal;
                public readonly Block Target;
                public readonly bool IsTight;

                public BlockStackEntry(string literal, Block target, bool isTight)
                {
                    Literal = literal;
                    Target = target;
                    IsTight = isTight;
                }
            }

            private readonly struct InlineStackEntry
            {
                public readonly string Literal;
                public readonly Inline Target;
                public readonly bool IsWithinLink;

                public InlineStackEntry(string literal, Inline target, bool isWithinLink)
                {
                    Literal = literal;
                    Target = target;
                    IsWithinLink = isWithinLink;
                }
            }
        }
    }
}
