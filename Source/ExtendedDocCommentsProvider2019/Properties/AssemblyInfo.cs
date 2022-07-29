//===============================================================================================================
// System  : Extended Doc Comments Completion Provider Package
// File    : AssemblyInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 07/29/2021
// Note    : Copyright 2019-2022, Eric Woodruff, All rights reserved
//
// Extended documentation comments completion provider package attributes
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website:
// https://github.com/EWSoftware/ExtendedDocCommentsProvider.  This notice, the author's name, and all copyright
// notices must remain intact in all applications, documentation, and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/22/2019  EFW  Created the code
//===============================================================================================================

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General assembly information
[assembly: AssemblyProduct("Extended Doc Comments Completion Provider Package")]
[assembly: AssemblyTitle("Extended Doc Comments Completion Provider Package")]
[assembly: AssemblyDescription("An editor extension that augments the standard C# XML documentation comments " +
    "elements with custom elements, attributes, and attribute values supported by the Sandcastle Help File " +
    "Builder and other documentation tools")]
[assembly: AssemblyCompany("Eric Woodruff")]
[assembly: AssemblyCopyright("Copyright \xA9 2019-2022, Eric Woodruff, All Rights Reserved")]
[assembly: AssemblyCulture("")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// This assembly is not CLS compliant
[assembly: CLSCompliant(false)]

// Not visible to COM
[assembly: ComVisible(false)]

// Resources contained within the assembly are English
[assembly: NeutralResourcesLanguage("en")]

// Version numbers.  All version numbers for an assembly consists of the following four values:
//
//      Year of release
//      Month of release
//      Day of release
//      Revision (typically zero unless multiple releases are made on the same day)
//

// Common assembly strong name version - Typically not change unless necessary but doesn't apply to this project
// as it is self-contained and is not referenced by anything else.  Keep it in sync with the versions below.
[assembly: AssemblyVersion("2022.7.29.0")]

// Common assembly file version
//
// This is used to set the assembly file version.  This will change with each new release.  MSIs only support a
// Major value between 0 and 255 so we drop the century from the year on this one.
[assembly: AssemblyFileVersion("22.7.29.0")]

// Common product version
//
// This may contain additional text to indicate Alpha or Beta states.  The version number will always match the
// file version above but includes the century on the year.
[assembly: AssemblyInformationalVersion("2022.7.29.0")]
