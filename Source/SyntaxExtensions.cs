//===============================================================================================================
// System  : Extended Doc Comments Completion Provider Package
// File    : SyntaxExtensions.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/21/2019
// Note    : Copyright 2019, Eric Woodruff, All rights reserved
//
// This file contains various syntax element extension methods
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website:
// https://github.com/EWSoftware/ExtendedDocCommentsProvider.  This notice, the author's name, and all copyright
// notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/09/2013  EFW  Created the code
//===============================================================================================================

using System.Threading;

using Microsoft.CodeAnalysis;

namespace EWSoftware.CompletionProviders
{
    /// <summary>
    /// This class contains various syntax element extension methods
    /// </summary>
    internal static class SyntaxExtensions
    {
        /// <summary>
        /// If the position is inside of a token, return that token.  Otherwise, return the token to the left.
        /// </summary>
        /// <param name="syntaxTree">The syntax tree to search</param>
        /// <param name="position">The position to find</param>
        /// <param name="cancellationToken">A cancellation token used to cancel the operation</param>
        public static SyntaxToken FindTokenOnLeftOfPosition(this SyntaxTree syntaxTree, int position,
          CancellationToken cancellationToken)
        {
            var root = syntaxTree.GetRoot(cancellationToken);
            var token = (position < root.FullSpan.End || !(root is ICompilationUnitSyntax)) ? root.FindToken(position, true) :
                root.GetLastToken(true, true, true, true).GetPreviousToken(false, true, false, false);

            if(position <= token.SpanStart)
            {
                do
                {
                    var skippedToken = FindSkippedTokenBackward(token.LeadingTrivia, position);
                    token = skippedToken.RawKind != 0 ? skippedToken : token.GetPreviousToken(false, true, false, false);

                } while(position <= token.SpanStart && root.FullSpan.Start < token.SpanStart);
            }
            else
                if(token.Span.End < position)
                {
                    var skippedToken = FindSkippedTokenBackward(token.TrailingTrivia, position);
                    token = skippedToken.RawKind != 0 ? skippedToken : token;
                }

            if(token.Span.Length == 0)
                token = token.GetPreviousToken();

            return token;
        }

        /// <summary>
        /// Look inside a trivia list for a skipped token that contains the given position
        /// </summary>
        /// <param name="triviaList">The syntax trivia list to search</param>
        /// <param name="position">The position to find</param>
        private static SyntaxToken FindSkippedTokenBackward(SyntaxTriviaList triviaList, int position)
        {
            foreach(var trivia in triviaList.Reverse())
            {
                if(trivia.HasStructure && trivia.GetStructure() is ISkippedTokensTriviaSyntax skippedTokensTrivia)
                {
                    foreach(var token in skippedTokensTrivia.Tokens)
                    {
                        if(token.Span.Length > 0 && token.SpanStart <= position)
                            return token;
                    }
                }
            }

            return default;
        }

        /// <summary>
        /// This is used to get the ancestor or a syntax node matching a specified type
        /// </summary>
        /// <typeparam name="TNode">The node type to match</typeparam>
        /// <param name="node">The starting node</param>
        /// <returns>The ancestor node of the specified type if found, null if not found</returns>
        public static TNode GetAncestor<TNode>(this SyntaxNode node) where TNode : SyntaxNode
        {
            var current = node.Parent;

            while(current != null)
            {
                if(current is TNode tNode)
                    return tNode;

                var parent = current.Parent;

                if(parent == null && current is IStructuredTriviaSyntax structuredTrivia)
                    parent = structuredTrivia.ParentTrivia.Token.Parent;

                current = parent;
            }

            return null;
        }
    }
}
