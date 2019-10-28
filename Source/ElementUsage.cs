//===============================================================================================================
// System  : Extended Doc Comments Completion Provider Package
// File    : ElementUsage.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 10/21/2019
// Note    : Copyright 2019, Eric Woodruff, All rights reserved
//
// This file contains the enumerated type that defines comment element usage types
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

namespace EWSoftware.CompletionProviders
{
    /// <summary>
    /// This enumerated type defines comment element usage types
    /// </summary>
    [Serializable]
    public enum ElementUsage
    {
        /// <summary>
        /// Top-level only
        /// </summary>
        TopLevel = 0,
        /// <summary>
        /// Nested only
        /// </summary>
        Nested = 1,
        /// <summary>
        /// Can be top-level or nested
        /// </summary>
        Both = 2
    }
}
