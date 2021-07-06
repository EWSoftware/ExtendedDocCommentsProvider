//===============================================================================================================
// System  : Extended Doc Comments Completion Provider Package
// File    : CSharpXmlDocCommentsCompletionProvider.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/05/2021
// Note    : Copyright 2019-2021, Eric Woodruff, All rights reserved
//
// This file contains the extended documentation comments completion provider for C#
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website:
// https://github.com/EWSoftware/ExtendedDocCommentsProvider.  This notice, the author's name, and all copyright
// notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/09/2019  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: lexer attr cref

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;

namespace EWSoftware.CompletionProviders
{
    /// <summary>
    /// This completion provider augments the standard XML documentation comments elements with custom elements,
    /// attributes, and attribute values supported by the Sandcastle Help File Builder and other documentation
    /// tools.
    /// </summary>
    /// <remarks>The core logic used to determine which completion items to return based on the current context
    /// is based largely on the Roslyn XML comments completion providers.</remarks>
    [ExportCompletionProvider(nameof(CSharpXmlDocCommentsCompletionProvider), LanguageNames.CSharp)]
    internal sealed class CSharpXmlDocCommentsCompletionProvider : CompletionProvider
    {
        #region CompletionProvider abstract method implementation
        //=====================================================================

        /// <inheritdoc />
        public override async Task<CompletionChange> GetChangeAsync(Document document, CompletionItem item,
          char? commitChar, CancellationToken cancellationToken)
        {
            var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            if(!item.Properties.TryGetValue(nameof(CommentsElement.TextBeforeCaret), out string replacementText))
                replacementText = item.DisplayText;

            var itemSpan = item.Span;
            var replacementSpan = TextSpan.FromBounds(text[itemSpan.Start - 1] == '<' && replacementText.Length > 0 &&
                replacementText[0] == '<' ? itemSpan.Start - 1 : itemSpan.Start, itemSpan.End);

            int newPosition = replacementSpan.Start + replacementText.Length;

            // Include the commit character?
            if(commitChar != null && !Char.IsWhiteSpace(commitChar.Value) &&
              commitChar.Value != replacementText[replacementText.Length - 1])
            {
                replacementText += commitChar.Value;
                newPosition++;
            }

            if(item.Properties.TryGetValue(nameof(CommentsElement.TextAfterCaret), out string afterCaretText))
                replacementText += afterCaretText;

            return CompletionChange.Create(new TextChange(replacementSpan, replacementText), newPosition, true);
        }

        /// <inheritdoc />
        public override Task<CompletionDescription> GetDescriptionAsync(
          Document document, CompletionItem item, CancellationToken cancellationToken)
        {
            if(!item.Properties.TryGetValue(nameof(CommentsElement.Description), out string description))
                return Task.FromResult(CompletionDescription.Empty);

            return Task.FromResult(CompletionDescription.Create(ImmutableArray.Create(new TaggedText[] {
                new TaggedText(TextTags.Text, description) })));
        }

        /// <inheritdoc />
        public override async Task ProvideCompletionsAsync(CompletionContext context)
        {
            var items = await GetItemsAsync(context.Document, context.Position, context.Trigger,
                context.CancellationToken).ConfigureAwait(false);

            if(items != null)
                context.AddItems(items);
        }
        
        /// <inheritdoc />
        public override bool ShouldTriggerCompletion(SourceText text, int position, CompletionTrigger trigger,
          OptionSet options)
        {
            if(trigger.Kind != CompletionTriggerKind.Insertion || position < 1)
                return false;

            // Trigger it on an open bracket, quote, or at the start of a word
            char ch = text[position - 1];

            if(ch == '<' || ch == '"' || (ch == ' ' && (position == text.Length ||
              !SyntaxFacts.IsIdentifierStartCharacter(text[position]))))
            {
                return true;
            }

            position--;

            // We only want to trigger if we're the first character in an identifier.  If there's a character
            // before or after us, then we don't want to trigger.
            if(!SyntaxFacts.IsIdentifierStartCharacter(text[position]) ||
              (position > 0 && SyntaxFacts.IsIdentifierPartCharacter(text[position - 1])) ||
              (position < text.Length - 1 && SyntaxFacts.IsIdentifierPartCharacter(text[position + 1])))
            {
                return false;
            }

            // Try to honor the "Show completion list after a character is typed" option
            return TriggerOnTypingLetters(options);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to get the <c>TriggerOnTypingLetters</c> option from the option set
        /// </summary>
        /// <param name="options">The option set from which to get the option</param>
        /// <returns>The per-language options are passed to <see cref="ShouldTriggerCompletion"/> but Roslyn
        /// implements a majority of that stuff internally so it's inaccessible to us publicly.  As such, we
        /// resort to Reflection to get at it.  If there's a better way to do this, I can't find it.</returns>
        private static bool TriggerOnTypingLetters(OptionSet options)
        {
            try
            {
                FieldInfo fi = options.GetType().GetField("_values", BindingFlags.NonPublic |
                    BindingFlags.Instance);

                if(fi != null)
                {
                    if(fi.GetValue(options) is ImmutableDictionary<OptionKey, object> optionValues)
                    {
                        var key = optionValues.Keys.FirstOrDefault(k => k.Language == LanguageNames.CSharp &&
                            k.Option.Name == nameof(TriggerOnTypingLetters) && k.Option.Type == typeof(bool));

                        if(key != null)
                            return (bool)optionValues[key];
                    }
                }
            }
            catch(Exception ex)
            {
                // Ignore exceptions, we'll just use the default
                System.Diagnostics.Debug.WriteLine(ex);

                if(System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }

            // If we couldn't get it, use the default value which assumes it is enabled
            return true;
        }

        /// <summary>
        /// This is used to determine which completion items should appear, if any
        /// </summary>
        /// <param name="document">The document for which to provide completions</param>
        /// <param name="position">The position of the caret within the document</param>
        /// <param name="trigger">The action that triggered the completion to start</param>
        /// <param name="cancellationToken">A cancellation token used to cancel the operation</param>
        /// <returns>An enumerable set of completion items if any are determined to be relevant</returns>
        private static async Task<IEnumerable<CompletionItem>> GetItemsAsync(Document document, int position,
          CompletionTrigger trigger, CancellationToken cancellationToken)
        {
            try
            {
                var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
                var token = tree.FindTokenOnLeftOfPosition(position, cancellationToken);
                var parentTrivia = token.Parent?.FirstAncestorOrSelf<DocumentationCommentTriviaSyntax>();

                if(parentTrivia == null || parentTrivia.ParentTrivia.Token.Kind() == SyntaxKind.None)
                    return null;

                // Offer attribute names based on the element
                if(IsAttributeNameContext(token, position, out string elementName, out var existingAttributes))
                {
                    if(CommentsElement.CustomElements.TryGetValue(elementName, out CommentsElement ce) &&
                      ce.Attributes.Count != 0)
                    {
                        return ce.Attributes.Where(attr => !existingAttributes.Contains(attr.Name)).Select(
                            attr => CommentsElement.AttributeToCompletionItem(attr));
                    }

                    return null;
                }

                // Only attribute names should be triggered by a space.  Nothing past this point should be.
                if(trigger.Kind == CompletionTriggerKind.Insertion && trigger.Character == ' ')
                    return null;

                // Offer attribute values based on the element and attribute names
                if(IsAttributeValueContext(token, out elementName, out string attributeName))
                {
                    if(CommentsElement.CustomElements.TryGetValue(elementName, out CommentsElement ce))
                    {
                        var attr = ce.Attributes.FirstOrDefault(a => a.Name == attributeName);

                        if(attr != null)
                            return attr.Values.Select(v => CommentsElement.AttributeValueToCompletionItem(v));
                    }

                    return null;
                }

                // Everything below this point should only be offered if triggered by an opening bracket
                if(trigger.Kind == CompletionTriggerKind.Insertion && trigger.Character != '<')
                    return null;

                var elementNames = new HashSet<string>();

                if(token.Parent.IsKind(SyntaxKind.XmlEmptyElement) || token.Parent.IsKind(SyntaxKind.XmlText) ||
                  (token.Parent.IsKind(SyntaxKind.XmlElementEndTag) && token.IsKind(SyntaxKind.GreaterThanToken)) ||
                  (token.Parent.IsKind(SyntaxKind.XmlName) && token.Parent.Parent.IsKind(SyntaxKind.XmlEmptyElement)))
                {
                    // Add child elements with no defined parentage
                    if(token.Parent.Parent.IsKind(SyntaxKind.XmlElement) ||
                      token.Parent.Parent.Parent.IsKind(SyntaxKind.XmlElement))
                    {
                        elementNames.UnionWith(CommentsElement.CustomElements.Values.Where(
                            el => el.ElementUsage != ElementUsage.TopLevel && !el.ChildOf.Any()).Select(
                            el => el.DisplayText));
                    }

                    // Add child elements based on the parent element's name
                    if(token.Parent.Parent is XmlElementSyntax xmlElement)
                    {
                        elementNames.UnionWith(CommentsElement.CustomElements.Values.Where(el => el.ChildOf.Contains(
                            xmlElement.StartTag.Name.LocalName.ValueText)).Select(el => el.DisplayText));
                    }

                    if(token.Parent.Parent.IsKind(SyntaxKind.XmlEmptyElement) &&
                      token.Parent.Parent.Parent is XmlElementSyntax nestedXmlElement)
                    {
                        elementNames.UnionWith(CommentsElement.CustomElements.Values.Where(el => el.ChildOf.Contains(
                            nestedXmlElement.StartTag.Name.LocalName.ValueText)).Select(el => el.DisplayText));
                    }

                    // Add top-level elements
                    if(token.Parent.Parent is DocumentationCommentTriviaSyntax ||
                      (token.Parent.Parent.IsKind(SyntaxKind.XmlEmptyElement) &&
                      token.Parent.Parent.Parent is DocumentationCommentTriviaSyntax))
                    {
                        var existingTopLevelElements = new HashSet<string>(parentTrivia.Content.Select(
                            n => GetElementNameAndAttributes(n).Name).Where(el => el != null));

                        // Top-level single-use elements
                        elementNames.UnionWith(CommentsElement.CustomElements.Values.Where(
                            el => el.ElementUsage == ElementUsage.TopLevel && el.IsSingleUse &&
                            !existingTopLevelElements.Contains(el.Name)).Select(el => el.DisplayText));

                        // Top-level repeatable elements
                        elementNames.UnionWith(CommentsElement.CustomElements.Values.Where(
                            el => el.ElementUsage == ElementUsage.TopLevel && !el.IsSingleUse).Select(el => el.DisplayText));
                    }
                }

                // This case only appears to happen if using the shortcut key to invoke completion and the caret
                // is at the end of a start tag: <elem>|.  In such cases, add child elements with no defined
                // parentage and child elements based on the start tag's name.
                if(token.Parent is XmlElementStartTagSyntax startTag && token == startTag.GreaterThanToken)
                {
                    elementNames.UnionWith(CommentsElement.CustomElements.Values.Where(
                        el => el.ElementUsage != ElementUsage.TopLevel && !el.ChildOf.Any()).Select(
                        el => el.DisplayText));

                    elementNames.UnionWith(CommentsElement.CustomElements.Values.Where(el => el.ChildOf.Contains(
                        startTag.Name.LocalName.ValueText)).Select(el => el.DisplayText));
                }

                // Add use anywhere elements
                elementNames.UnionWith(CommentsElement.CustomElements.Values.Where(
                    el => el.ElementUsage == ElementUsage.Both).Select(el => el.DisplayText));

                if(elementNames.Any())
                    return elementNames.Select(el => CommentsElement.CustomElements[el].ToCompletionItem());
            }
            catch(Exception ex)
            {
                // Ignore any exceptions.  We just won't offer any completions.
                if(!(ex is OperationCanceledException))
                    System.Diagnostics.Debug.WriteLine(ex);

                if(System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }

            return null;
        }

        /// <summary>
        /// Get the element name and attributes from the current syntax node
        /// </summary>
        /// <param name="node">The syntax node to use</param>
        /// <returns>A tuple containing the element name and the attributes</returns>
        private static (string Name, IEnumerable<XmlAttributeSyntax> Attributes) GetElementNameAndAttributes(
          SyntaxNode node)
        {
            switch(node)
            {
                // Self-closing or incomplete element: <elem attr="|" /> or <elem attr="|"
                case XmlEmptyElementSyntax emptyElement:
                    return (emptyElement.Name.LocalName.ValueText, emptyElement.Attributes);

                // Parent node of a non-empty element: <elem></elem>
                case XmlElementSyntax nonEmptyElement:
                    return GetElementNameAndAttributes(nonEmptyElement.StartTag);

                // Start tag: <elem attr="|">
                case XmlElementStartTagSyntax startTag:
                    return (startTag.Name.LocalName.ValueText, startTag.Attributes);

                default:
                    return (null, null);
            }
        }

        /// <summary>
        /// This is used to determine whether or not the current caret location is in an attribute name context
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <param name="position">The position of the caret.</param>
        /// <param name="elementName">On return, this will contain the element name if it is an attribute name
        /// context, null if not.</param>
        /// <param name="attributeNames">On return, this will contain an enumerable list of the current
        /// attribute names for the element if it is an attribute name context, null if not.</param>
        /// <returns>True if it is within the attribute name context, false if not.</returns>
        private static bool IsAttributeNameContext(SyntaxToken token, int position, out string elementName,
          out IEnumerable<string> attributeNames)
        {
            elementName = null;

            // Unlike VB, the C# lexer has a preference for leading trivia.  In cases such as "<elem    |",
            // the trailing whitespace will not be attached as trivia to any node.  Instead it will be
            // treated as an independent XmlTextLiteralToken, so skip backwards by one token.
            if(token.IsKind(SyntaxKind.XmlTextLiteralToken) && String.IsNullOrWhiteSpace(token.Text))
                token = token.GetPreviousToken();

            // Handle the "<elem|" case by going back one token
            if(token.Span.IntersectsWith(position) && (token.RawKind == (int)SyntaxKind.IdentifierToken ||
              SyntaxFacts.IsReservedKeyword((SyntaxKind)token.RawKind) ||
              SyntaxFacts.IsContextualKeyword((SyntaxKind)token.RawKind) ||
              SyntaxFacts.IsPreprocessorKeyword((SyntaxKind)token.RawKind)))
            {
                token = token.GetPreviousToken(false, true);
            }

            IEnumerable<XmlAttributeSyntax> attributes = null;

            if(token.IsKind(SyntaxKind.IdentifierToken) && token.Parent.IsKind(SyntaxKind.XmlName))
            {
                // "<elem |" or "<elem attr|"
                (elementName, attributes) = GetElementNameAndAttributes(token.Parent.Parent);
            }
            else
                if(token.Parent.IsKind(SyntaxKind.XmlCrefAttribute) || token.Parent.IsKind(SyntaxKind.XmlNameAttribute) ||
                  token.Parent.IsKind(SyntaxKind.XmlTextAttribute))
                {
                    // In the following, "attr1" may be a general attribute or the special cref or name attribute:
                    // <elem attr1="" |
                    // <elem attr1="" |attr2	
                    // <elem attr1="" attr2|
                    var attributeSyntax = (XmlAttributeSyntax)token.Parent;

                    if(token == attributeSyntax.EndQuoteToken)
                        (elementName, attributes) = GetElementNameAndAttributes(attributeSyntax.Parent);
                }

            if(attributes != null)
                attributeNames = new HashSet<string>(attributes.Select(attr => attr.Name.LocalName.ValueText));
            else
                attributeNames = null;

            return elementName != null;
        }

        /// <summary>
        /// This is used to determine whether or not the current caret location is in an attribute value context
        /// </summary>
        /// <param name="token">The token to check</param>
        /// <param name="elementName">On return, this will contain the element name if it is an attribute value
        /// context, null if not.</param>
        /// <param name="attributeName">On return, this will contain the attribute name if it is an attribute
        /// value context, null if not.</param>
        /// <returns>True if it is within the attribute value context, false if not.</returns>
        private static bool IsAttributeValueContext(SyntaxToken token, out string elementName, out string attributeName)
        {
            XmlAttributeSyntax attributeSyntax = null;

            if(token.IsKind(SyntaxKind.XmlTextLiteralToken) && token.Parent.IsKind(SyntaxKind.XmlTextAttribute))
            {
                // General attribute: attr="value|
                attributeSyntax = (XmlTextAttributeSyntax)token.Parent;
            }
            else
                if(token.Parent.IsKind(SyntaxKind.XmlNameAttribute) || token.Parent.IsKind(SyntaxKind.XmlTextAttribute))
                {
                    // Return the parent attribute if there is no value yet: attr="|
                    attributeSyntax = (XmlAttributeSyntax)token.Parent;

                    if(token != attributeSyntax.StartQuoteToken)
                        attributeSyntax = null;
                }

            if(attributeSyntax != null)
            {
                attributeName = attributeSyntax.Name.LocalName.ValueText;

                var emptyElement = attributeSyntax.GetAncestor<XmlEmptyElementSyntax>();

                if(emptyElement != null)
                {
                    // Self-closing or incomplete element: <elem attr="|" /> or <elem attr="|"
                    elementName = emptyElement.Name.LocalName.Text;
                    return true;
                }

                var startTag = token.Parent?.FirstAncestorOrSelf<XmlElementStartTagSyntax>();

                if(startTag != null)
                {
                    // Start tag: <elem attr="|">
                    elementName = startTag.Name.LocalName.Text;
                    return true;
                }
            }

            attributeName = elementName = null;
            return false;
        }
        #endregion
    }
}
