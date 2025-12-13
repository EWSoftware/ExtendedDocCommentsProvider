---
uid: b146cb94-e594-4462-9262-d5cdba76c331
alt-uid: KnownIssues
title: Known Issues
keywords: known issues
---
The following are some known issues.

- Completion providers are additive.  Each one returns a set of items to add to the completion list based on the
  current editor context.  The providers are unaware of other providers and the items added by them.  They cannot
  remove items from other providers or make decisions about what to add based on other items that may appear.

  As such, you will see completion items from the built-in XML comments completion provider that may not be
  appropriate.  For example, you will see the standard set of inline elements inside of the `revisionHistory`
  element.  The same issue can be seen with standard elements such as `list`.  This provider does allow the
  definition of valid parent elements so it will not present the custom child elements in inappropriate contexts.
- Only C# is supported right now.  A provider for Visual Basic is planned for a later release.
- The current set of extended elements, attributes, and values is currently fixed and cannot be
  changed.  A future release will allow the items to be customized and additional user-defined elements to be
  added.

## See Also
**Other Resources**  
[](@141160ac-ba6a-4f6d-a802-1110aadea932)  
