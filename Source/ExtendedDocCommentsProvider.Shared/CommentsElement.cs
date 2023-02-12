//===============================================================================================================
// System  : Extended Doc Comments Completion Provider Package
// File    : CommentsElement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/10/2023
// Note    : Copyright 2019-2023, Eric Woodruff, All rights reserved
//
// This file contains the class used to define an XML documentation comments element
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website:
// https://github.com/EWSoftware/ExtendedDocCommentsProvider.  This notice, the author's name, and all copyright
// notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/18/2019  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: cpp fs javascript vbnet html xml xsl xaml sql py pshell cref threadsafety href seealso todo

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis.Completion;

namespace EWSoftware.CompletionProviders
{
    /// <summary>
    /// This is used to define an XML documentation comments element
    /// </summary>
    public class CommentsElement
    {
        #region Private data members
        //=====================================================================

        private static readonly CompletionItemRules defaultRules = CompletionItemRules.Create(
            ImmutableArray.Create(CharacterSetModificationRule.Create(CharacterSetModificationKind.Add, '!', '-', '[')),
            ImmutableArray.Create(CharacterSetModificationRule.Create(CharacterSetModificationKind.Add, '>', '\t')),
            EnterKeyRule.Never);
        private static readonly CharacterSetModificationRule withoutQuoteRule =
            CharacterSetModificationRule.Create(CharacterSetModificationKind.Remove, '"');
        private static readonly CharacterSetModificationRule withoutSpaceRule =
            CharacterSetModificationRule.Create(CharacterSetModificationKind.Remove, ' ');

        private readonly HashSet<ElementAttribute> attributes;
        private readonly HashSet<string> childOf;

        private string displayText;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This static read-only property returns a dictionary containing the current set of custom element
        /// definitions.
        /// </summary>
        public static Dictionary<string, CommentsElement> CustomElements { get; } = CreateDefaultCommentElements();

        /// <summary>
        /// The element name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Optional display text to show in the completion popup
        /// </summary>
        /// <value>If not set, the <see cref="Name"/> is used.  This allows alternate text to be shown in place
        /// or the element name or to allow multiple instances of a particular element with different before and
        /// after text for example.</value>
        public string DisplayText
        {
            get => displayText ?? this.Name;
            set
            {
                if(String.IsNullOrWhiteSpace(value))
                    displayText = null;
                else
                    displayText = value.Trim();
            }
        }

        /// <summary>
        /// An optional description to show when the completion item is selected in the completion popup list
        /// </summary>
        /// <value>If not set, no description will be shown</value>
        public string Description { get; set; }

        /// <summary>
        /// Additional text to include after the element name and before the caret if the element is inserted
        /// </summary>
        /// <value>This is typically a default attribute name, the equals sign, and the opening quote mark</value>
        public string TextBeforeCaret { get; set; }

        /// <summary>
        /// Additional text to include after the caret if the element is inserted
        /// </summary>
        /// <value>When <see cref="TextBeforeCaret"/> is specified, this is typically a closing quote mark</value>
        public string TextAfterCaret { get; set; }

        /// <summary>
        /// This indicates how the element is used
        /// </summary>
        /// <remarks>If <see cref="ElementUsage.TopLevel"/>, this element cannot be nested within other elements.
        /// If <see cref="ElementUsage.Nested"/> or <see cref="ElementUsage.Both"/>, it can.  If
        /// <see cref="ChildOf"/> is empty, it can appear within any element.  If specific parent elements are
        /// specified, it can only appear within them.</remarks>
        public ElementUsage ElementUsage { get; set; }

        /// <summary>
        /// This indicates whether or not the top-level element is single-use or can appear multiple times
        /// </summary>
        public bool IsSingleUse { get; set; }

        /// <summary>
        /// This is used to indicate whether or not the element is inserted as a self-closing element or if it
        /// is left open.
        /// </summary>
        public bool IsSelfClosing { get; set; }

        /// <summary>
        /// This returns a collection of attributes supported by the element, if any
        /// </summary>
        public IReadOnlyCollection<ElementAttribute> Attributes => attributes;

        /// <summary>
        /// This returns a collection of parent element names of which this element can be a child
        /// </summary>
        /// <value>If empty and not a top-level element, this element can appear in any other element.  If not
        /// empty, it can only appear within the specified elements.</value>
        public IReadOnlyCollection<string> ChildOf => childOf;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public CommentsElement()
        {
            attributes = new HashSet<ElementAttribute>();
            childOf = new HashSet<string>();
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Add supported attribute information to the element
        /// </summary>
        /// <param name="elementNames">An enumerable list of attribute information to add</param>
        /// <returns>A reference to the comment element instance</returns>
        public CommentsElement AddAttributes(IEnumerable<ElementAttribute> elementNames)
        {
            attributes.UnionWith(elementNames);
            return this;
        }

        /// <summary>
        /// Add parent element information to the element
        /// </summary>
        /// <param name="parentElements">An enumerable list of parent element names</param>
        /// <returns>A reference to the comment element instance</returns>
        public CommentsElement AddChildOfElements(IEnumerable<string> parentElements)
        {
            childOf.UnionWith(parentElements);
            return this;
        }

        /// <summary>
        /// Convert the element to a <see cref="CompletionItem"/>
        /// </summary>
        /// <returns>The element information as a <see cref="CompletionItem"/></returns>
        public CompletionItem ToCompletionItem()
        {
            var rules = defaultRules;

            if(this.DisplayText.IndexOfAny(new[] { '"', ' ' }) != -1)
            {
                var commitRules = defaultRules.CommitCharacterRules;

                if(this.DisplayText.IndexOf('"') != -1)
                    commitRules = commitRules.Add(withoutQuoteRule);

                if(this.DisplayText.IndexOf(' ') != -1)
                    commitRules = commitRules.Add(withoutSpaceRule);

                rules = defaultRules.WithCommitCharacterRules(commitRules);
            }

            string beforeCaret = "<" + this.Name, afterCaret = this.TextAfterCaret;

            if(!String.IsNullOrWhiteSpace(this.TextBeforeCaret))
                beforeCaret += " " + this.TextBeforeCaret;

            if(this.IsSelfClosing)
                afterCaret += "/>";

            var properties = new Dictionary<string, string> { { nameof(TextBeforeCaret), beforeCaret } };

            if(!String.IsNullOrWhiteSpace(afterCaret))
                properties.Add(nameof(TextAfterCaret), afterCaret);

            if(!String.IsNullOrWhiteSpace(this.Description))
                properties.Add(nameof(Description), this.Description);

            return CompletionItem.Create(this.DisplayText, null, null, ImmutableDictionary.CreateRange(properties),
                ImmutableArray.Create("Keyword"), rules);
        }

        /// <summary>
        /// Convert an attribute to a <see cref="CompletionItem"/>
        /// </summary>
        /// <param name="attribute">The attribute to use</param>
        /// <returns>The attribute as a <see cref="CompletionItem"/></returns>
        public static CompletionItem AttributeToCompletionItem(ElementAttribute attribute)
        {
            if(attribute == null)
                throw new ArgumentNullException(nameof(attribute));

            var properties = new Dictionary<string, string>
            {
                { nameof(TextBeforeCaret), attribute.Name + "=\"" },
                { nameof(TextAfterCaret), "\"" }
            };

            if(!String.IsNullOrWhiteSpace(attribute.Description))
                properties.Add(nameof(Description), attribute.Description);

            return CompletionItem.Create(attribute.Name, null, null, ImmutableDictionary.CreateRange(properties),
                ImmutableArray.Create("Keyword"), defaultRules);
        }

        /// <summary>
        /// Convert an attribute value to a <see cref="CompletionItem"/>
        /// </summary>
        /// <param name="value">The attribute value to use</param>
        /// <returns>The attribute value as a <see cref="CompletionItem"/></returns>
        public static CompletionItem AttributeValueToCompletionItem(string value)
        {
            return CompletionItem.Create(value, null, null, ImmutableDictionary<string, string>.Empty,
                ImmutableArray.Create("Keyword"), defaultRules);
        }

        /// <summary>
        /// Create the default comment element dictionary
        /// </summary>
        /// <returns>A dictionary containing the default comment element information</returns>
        private static Dictionary<string, CommentsElement> CreateDefaultCommentElements()
        {
            return new List<CommentsElement>
            {
                new CommentsElement
                {
                    Name = "AttachedEventComments",
                    ElementUsage = ElementUsage.TopLevel,
                    Description = "Define the content that should appear on the auto-generated attached event " +
                        "member topic for a given WPF routed event member."
                },
                new CommentsElement
                {
                    Name = "AttachedPropertyComments",
                    ElementUsage = ElementUsage.TopLevel,
                    Description = "Define the content that should appear on the auto-generated attached " +
                        "property member topic for a given WPF dependency property member."
                },
                new CommentsElement
                {
                    Name = "code",
                    ElementUsage = ElementUsage.Nested,
                    TextBeforeCaret="language=\"",
                    TextAfterCaret="\"",
                    Description = "Format a multi-line section of text as source code.",
                }.AddAttributes(new[] {
                    new ElementAttribute(new[] { "cs", "cpp", "c", "fs", "javascript", "vb", "vbnet",
                        "html", "xml", "xsl", "xaml", "sql", "py", "pshell", "bat", "none" }) {
                        Name = "language", Description = "Specify the code language." },
                    new ElementAttribute { Name = "title", Description = "An optional title or a space to " +
                        "suppress the title." },
                    new ElementAttribute { Name = "source", Description = "Specify a source code file from " +
                        "which this element's content will be imported." },
                    new ElementAttribute { Name = "region", Description = "Limit the imported code to a " +
                        "specific named region within it." },
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "removeRegionMarkers", Description = "Indicate whether or not region markers " +
                            "within the imported code file or region are removed." },
                    new ElementAttribute { Name = "tabSize", Description = "Override the default tab size " +
                        "setting for the language which is defined by the code colorizer." },
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "numberLines", Description = "Override the default line numbering setting in " +
                            "the code colorizer." },
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "outlining", Description = "Override the default outlining setting in the " +
                            "code colorizer." },
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "keepSeeTags", Description = "Override the default setting in the code " +
                            "colorizer that determines whether or not <see> elements in the code are " +
                            "rendered as clickable links or are rendered as literal text."}
                }),
                new CommentsElement
                {
                    Name = "conceptualLink",
                    ElementUsage = ElementUsage.Both,
                    TextBeforeCaret = "target=\"",
                    TextAfterCaret = "\"",
                    IsSelfClosing = true,
                    Description = "Create a link to a MAML topic within the See Also section of a topic or an " +
                        "inline link to a MAML topic within one of the other XML comments elements."
                },
                new CommentsElement
                {
                    Name = "event",
                    ElementUsage = ElementUsage.TopLevel,
                    TextBeforeCaret = "cref=\"",
                    TextAfterCaret = "\"",
                    Description = "List events that can be raised by a type's member."
                },
                new CommentsElement
                {
                    Name = "list",
                    ElementUsage = ElementUsage.Nested,
                    TextBeforeCaret = "type=\"",
                    TextAfterCaret = "\"",
                    Description = "Specify content that should be displayed as a list or a table."
                }.AddAttributes(new[] {
                    new ElementAttribute(new[] { "definition" }) {
                        Name="type", Description = "The list type" },
                    new ElementAttribute { Name = "start", Description = "The starting number for numbered lists." }
                }),
                new CommentsElement
                {
                    Name = "note",
                    ElementUsage = ElementUsage.Nested,
                    TextBeforeCaret = "type=\"",
                    TextAfterCaret = "\"",
                    Description = "Create a note within a topic to draw attention to some important information."
                }.AddAttributes(new [] {
                    new ElementAttribute(new[] { "note", "tip", "implement", "caller", "inherit", "caution",
                        "warning", "important", "security", "cs", "cpp", "vb", "todo" }) {
                        Name = "type", Description = "Specifies the note type." },
                    new ElementAttribute { Name = "title", Description = "An optional title override" }
                }),
                new CommentsElement
                {
                    Name = "overloads",
                    ElementUsage = ElementUsage.TopLevel,
                    Description = "Define the content that should appear on the auto-generated overloads topic " +
                        "for a given set of member overloads."
                },
                new CommentsElement
                {
                    Name = "preliminary",
                    ElementUsage = ElementUsage.TopLevel,
                    IsSelfClosing = true,
                    Description = "Indicate that a particular type or member is preliminary and is subject to change."
                },
                new CommentsElement
                {
                    Name = "revisionHistory",
                    ElementUsage = ElementUsage.TopLevel,
                    TextBeforeCaret = "visible=\"",
                    TextAfterCaret = "true\"",
                    Description = "Display revision history for a type or its members."
                }.AddAttributes(new[] {
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "visible",
                        Description = "Indicate whether or not the revision history is included in the help topic." },
                }),
                new CommentsElement
                {
                    Name = "revision",
                    ElementUsage = ElementUsage.Nested,
                    TextBeforeCaret = "date=\"",
                    TextAfterCaret = "\" version=\"\"",
                    Description = "Describe a revision to the type or member."
                }.AddAttributes(new[] {
                    new ElementAttribute{ Name = "date", Description = "The revision date." },
                    new ElementAttribute{ Name = "version", Description = "The version in which the revision was made." },
                    new ElementAttribute{ Name = "author", Description = "The name of the person that made the revision." },
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "visible",
                        Description = "Indicate whether or not the revision is included in the help topic." },
                }).AddChildOfElements(
                    new[] { "revisionHistory" }),
                new CommentsElement
                {
                    Name = "see",
                    ElementUsage = ElementUsage.Nested,
                    TextBeforeCaret = "cref=\"",
                    TextAfterCaret = "\"",
                    IsSelfClosing = true,
                    Description = "Create an inline link to another API topic or an external website."
                }.AddAttributes(new[] {
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "qualifyHint",
                        Description = "Indicate whether or not the type or member name should be qualified." },
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "autoUpgrade",
                        Description = "Indicate whether or not the link should go to the method overloads topic." },
                    new ElementAttribute { Name = "href", Description = "Specify a URL to which the link should go." },
                    new ElementAttribute { Name = "alt", Description = "Specify alternate text for a URL link." },
                    new ElementAttribute(new[] { "_blank", "_self", "_parent", "_top" }) {
                        Name = "target", Description = "Specify where a URL link will be opened." },
                }),
                new CommentsElement
                {
                    Name = "seealso",
                    ElementUsage = ElementUsage.Nested,
                    TextBeforeCaret = "cref=\"",
                    TextAfterCaret = "\"",
                    IsSelfClosing = true,
                    Description = "Create an link to another API topic or an external website in the See Also " +
                        "section of a help topic."
                }.AddAttributes(new[] {
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "qualifyHint",
                        Description = "Indicate whether or not the type or member name should be qualified." },
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "autoUpgrade",
                        Description = "Indicate whether or not the link should go to the method overloads topic." },
                    new ElementAttribute { Name = "href", Description = "Specify a URL to which the link should go." },
                    new ElementAttribute { Name = "alt", Description = "Specify alternate text for a URL link." },
                    new ElementAttribute(new[] { "_blank", "_self", "_parent", "_top" }) {
                        Name = "target", Description = "Specify where a URL link will be opened." },
                }),
                new CommentsElement
                {
                    Name = "threadsafety",
                    ElementUsage = ElementUsage.TopLevel,
                    TextBeforeCaret = "static=\"",
                    TextAfterCaret = "true\" instance=\"false\"",
                    IsSelfClosing = true,
                    Description = "Indicate whether or not a class or structure's static and instance members " +
                        "are safe for use in multi-threaded scenarios."
                }.AddAttributes(new[] {
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "static", Description = "The thread safety of static members." },
                    new ElementAttribute(new[] { "true", "false" }) {
                        Name = "instance", Description = "The thread safety of instance member." }
                }),
                new CommentsElement
                {
                    Name = "token",
                    ElementUsage = ElementUsage.Both,
                    Description = "Insert a replaceable tag within a topic."
                }
            }.ToDictionary(k => k.DisplayText, v => v);
        }
        #endregion
    }
}
