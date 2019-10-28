//===============================================================================================================
// System  : Extended Doc Comments Completion Provider Package
// File    : ElementAttribute.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/21/2019
// Note    : Copyright 2019, Eric Woodruff, All rights reserved
//
// This file contains the class used to define XML documentation comments element attributes and their values
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website:
// https://github.com/EWSoftware/ExtendedDocCommentsProvider.  This notice, the author's name, and all copyright
// notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/18/2013  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace EWSoftware.CompletionProviders
{
    /// <summary>
    /// This is used to define element attributes and their values
    /// </summary>
    public class ElementAttribute
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// The attribute name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// An optional description to show when the completion item is selected in the completion popup list
        /// </summary>
        /// <value>If not set, no description will be shown</value>
        public string Description { get; set; }

        public IEnumerable<string> Values { get; }

        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor for an attribute with no provided attribute values
        /// </summary>
        /// <overloads>There are two overloads for the constructor</overloads>
        public ElementAttribute() : this(null)
        {
        }

        /// <summary>
        /// Constructor for an attribute with provided attribute values
        /// </summary>
        /// <param name="values">An enumerable list of values or null if there are none</param>
        public ElementAttribute(IEnumerable<string> values)
        {
            this.Values = values ?? Enumerable.Empty<string>();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to compare element attributes by name
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if equal by attribute name, false if not</returns>
        public override bool Equals(object obj)
        {
            return (obj is ElementAttribute attr && attr.Name.Equals(this.Name, StringComparison.Ordinal));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
        #endregion
    }
}
