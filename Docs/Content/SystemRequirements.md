---
uid: 3BBB6221-417C-4F5C-B89A-563A8052CE35
alt-uid: SystemRequirements
title: System Requirements and Building the Code
keywords: system requirements
---
The master branch is for Visual Studio 2019 and later and is the default branch for all current and future
development.  In order to build the projects you will need Visual Studio 2019 or later.

## Folder Layout
*.\Source* - This folder contains the source code for all of the projects.

*.\Docs* - The [Sandcastle Help File Builder](https://GitHub.com/EWSoftware/SHFB) project used to manage the
extended documentation comments provider project's documentation.

*.\Deployment* - This folder contains the deployment resources (the installer and all related files) generated
by building the projects.

## Building and Debugging the Projects
To build the projects, open the solution file (*\*.sln*) for your version of Visual Studio (currently 2019 or
2022) found in the *Source\\* folder and build it.  You can also run the *MasterBuild.bat* script from a command
prompt to build the projects.

In order to debug the projects:

- Set the *ExtendedDocCommentsProvider* project as the default project and then open its project properties.
- Go to the **Debug** category.
- For the **Start Action** option, set it to **Start external program**, click the "..." button after the text
  box, navigate to the installation folder for your version of Visual Studio and select the *devenv.exe* file in
  that folder (i.e. *C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe*).
- For the **Command line arguments** option, enter the value `/rootsuffix Exp`.
- When you run the project, it should start a new instance of Visual Studio using the experimental instance
  settings.

## See Also
**Other Resources**  
[](@141160ac-ba6a-4f6d-a802-1110aadea932)  
[License Agreement](https://github.com/EWSoftware/ExtendedDocCommentsProvider/blob/master/LICENSE)  
