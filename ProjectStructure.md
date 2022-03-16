# Overview of the project structure


## Aras.Method.Libs project

This project contains core classes to perform actions with method's code, partial classes and load project's configuration.


### Aras.Method.Libs.Aras.Package namespace

The namespace provides `PackageInfo` class that encapsulates logic for building the correct package structure.


### Aras.Method.Libs.Code namespace

This namespace contains set of classes that help to generate and operate with CSharp code of the Methods.

There are two main classes here:
- `CSharpCodeItemProvider` provides infromation about allowed code elements that can be processed by the plugin (classes, methods, enums, etc.).
- `CSharpCodeProvider` provides following functionality:
    - new source code generation.
    - code elements extraction within partial class or to a separate file.
    - add and remove special attributes (`PartialPathAttribute`, `ExternalPathAttribute`) for the code elements.


### Aras.Method.Libs.Templates.TemplateLoader class

The class provides possibility to load method templates from the `method-config.xml` file and use them for the code generation process.


### Aras.Method.Libs.Configurations.ProjectConfigurations namespace

The namespace contains class structure of the configuration model of a VSMP project. `Aras.Method.Libs.Configurations.ProjectConfigurations.ProjectConfiguraiton` is a root class of the configuration. Here is an information about last connections, last user search and data about currently loaded methods in the project.


## Aras.VS.MethodPlugin project

The `Aras.VS.MethodPlugin` is the main project of the solution. Here is an entry poit of the plugin: `Aras.VS.MethodPlugin.ArasMainMenuCmdPackage` class where all components and commands are created and initialized.


### Aras.VS.MethodPlugin.ArasInnovator namespace

Provides classes to load data about available methods from Aras Innovator server.


### Aras.VS.MethodPlugin.Authentication namespace

In this namespace you can find classes that are responsible for user authentication. Note, that each method project contains its own `IOM.dll` file (for specific Innovator version). The IOM classes are loaded dynamically by `Aras.VS.MethodPlugin.IOMWrapper`.


### Aras.VS.MethodPlugin.Commands namespace

The plugin provides set of the commands that are accessible from main menu, toolbars or context menu of the code editor. All commands are derived from the `Aras.VS.MethodPlugin.Commands.CmdBase` class. Another abstract class `Aras.VS.MethodPlugin.Commands.AuthenticationCommandBase` represents a base for the commands that require connection to the Aras Innovator.

Implemented commands:
- `ConnectionInfoCmd` shows 'Connection Info' dialog.
- `CreateCodeItemCmd` creates and places a new file with a new code item (interface, class, etc.). More information [More information](https://github.com/arasplm/ArasVSMethodPlugin/wiki/Create-Code-Item).
- `CreateMethodCmd` creates new method. [More information](https://github.com/arasplm/ArasVSMethodPlugin/wiki/Create-Method).
- `DebugMethodCmd` starts debug session for the method. [More information](https://github.com/arasplm/ArasVSMethodPlugin/wiki/Debug-Method).
- `ImportOpenInVSActionCmd` imports 'Open in VS' action to the Aras Innovator. [More information](https://github.com/arasplm/ArasVSMethodPlugin/wiki/Import-%27Open-in-Visual-Studio%27-Action).
- `MoveToCmd` moves a code element to the partial class or external file. [More information](https://github.com/arasplm/ArasVSMethodPlugin/wiki/Move-to...).
- `OpenFromArasCmd` opens method from Aras Innovator. [More information](https://github.com/arasplm/ArasVSMethodPlugin/wiki/Open-Method-from-Aras).
- `OpenFromPackageCmd` opens method from AML Import package. [More information](https://github.com/arasplm/ArasVSMethodPlugin/wiki/Open-Method-from-AML-Package).
- `SaveToArasCmd` saves a method to the Aras Innovator.
- `SaveToPackageCmd` saves a method to the AML Import package.
- `UpdateMethodCmd` updates existing method in a VS project with the latest version from the Aras Innovator.


### Aras.VS.MethodPlugin.Configurations.GlobalConfiguration class

Provides access to the global configuration file (stored in `My Documents` folder).


### Aras.VS.MethodPlugin.Dialogs namespace

This namespace contain all UI components of the plugin (Views, View-Models, Controls and Styles).


### Aras.VS.MethodPlugin.ItemSearch namespace

Contains implementaion of the Aras Innovator search functionality for the Search Dialog window.


### Aras.VS.MethodPlugin.OpenMethodInVS namespace

Provides classes that support 'Open in VS' action functionality. A method can be opened in an existing or new project. Visual Studio uses custom protocol `openinvs`.


### Aras.VS.MethodPlugin.PackageManagement.PackageManager class

The `PackageManager` is responsible for getting and updating information about the methods' package elements in 'Package Definitions'.


### ProjectTemplates folder

This folder contains zipped project templates for each supported Aras Innovator version.


### Aras.VS.MethodPlugin.SolutionManagement namespace

The namespace provides following classes to control a solution structure:
- `ProjectManager` provides possibility to operate with project items (files, documents, etc.), execute commands and control text editor operations.
- `EventListener` controls subscription to the events related to the Visual Studio (project item removed, renamed, selected).


### source.extension.vsixmanifest file

Contains detailed information about plugin's assets and installation.


### ArasMainMenuCmdPackage.vsct file

Defines where buttons or menu items for commands should be placed.


## MethodLauncher project

`MethodLauncher` project contains special console application that executes compiled Method localy. It is used to start execution process and attach to it Visual Studio to enable debugging session. The applicaiton crate connection to the Aras Innovator, loads a library with the Server Method and calls entry method with appropriate parameters.

## MethodLauncherNetCore project

`MethodLauncherNetCore` is the same application as `MethodLauncher`, but it is updated to support .NET Core for Innovator of 14+ versions.