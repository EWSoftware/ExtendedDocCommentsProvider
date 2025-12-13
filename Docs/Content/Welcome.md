---
uid: 141160ac-ba6a-4f6d-a802-1110aadea932
alt-uid: Welcome
title: Welcome
keywords: Welcome
---
<!-- Ignore Spelling: cpp, fs, javascript, vbnet, html, xml, xsl, xaml, sql, py, pshell href threadsafety seealso todo -->

Welcome to the **Extended XML Documentation Comments Completion Provider** project.

This project is a Visual Studio editor extension that augments the standard C# XML documentation comments
elements with custom elements, attributes, and attribute values supported by the Sandcastle Help File Builder and
other documentation tools.  The following elements, attributes, and attribute values are provided:

| Elements                 | Attributes  | Attribute Values |
| ---                      | ---         | --- |
| AttachedEventComments    |             |   |
| AttachedPropertyComments |             |   |
| code                     | language    | cs, cpp, c, fs, javascript, vb, vbnet, html, xml, xsl, xaml, sql, py, pshell, bat, none |
|                          | title       |   |
|                          | source      |   |
|                          | region      |   |
|                          | removeRegionMarkers | true, false |
|                          | tabSize     |   |
|                          | numberLines | true, false |
|                          | outlining   | true, false |
|                          | keepSeeTags | true, false |
| conceptualLink           |             |   |
| event                    |             |   |
| list                     | type        | definition |
|                          | start       |   |
| note                     | type        | note, tip, implement, caller, inherit, caution, warning, important, security, cs, cpp, vb, todo |
|                          | title       |   |
| overloads                |             |   |
| preliminary              |             |   |
| revisionHistory          | visible     | true, false |
| revision                 | date        |   |
|                          | version     |   |
|                          | author      |   |
|                          | visible     | true, false |
| see/seealso              | qualifyHint | true, false |
|                          | autoUpgrade | true, false |
|                          | href        |   |
|                          | alt         |   |
|                          | target      | _blank, _self, _parent, _top |
| threadsafety             | static      | true, false |
|                          | instance    | true, false |
| token                    |             |   |

For information on the above elements and all other XML comments elements, see the
[Sandcastle XML Comments Guide](https://EWSoftware.github.io/XMLCommentsGuide/index.html "Sandcastle XML Comments Guide").

## Making a Donation
If you would like to support this project, you can make a donation of any amount you like by clicking on the
PayPal donation button below.  Another option is to click the **Sponsor** button in the page footer to sponsor
the project on GitHub with either a one-time or monthly amount.

[![Make donations with PayPal - It's fast, free and secure!](@PayPal)](https://www.paypal.com/donate/?hosted_button_id=QK8P2B8B5WBJ2 "Make a donation")

Thanks to those of you that have made a donation. It is much appreciated!

## See Also
**Other Resources**  
[](@102EF998-5094-482A-B708-9DA64C04C859)  
[](@caa20db4-521f-4c4c-8399-aea8a81ae6a7)  
[License Agreement](https://github.com/EWSoftware/ExtendedDocCommentsProvider/blob/master/LICENSE)  
