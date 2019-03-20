//------------------------------------------------------------------------------
// <copyright file="ProjectManager.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Extensions;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.CodeAnalysis.Text;

namespace Aras.VS.MethodPlugin.SolutionManagement
{
	public class ProjectManager : IProjectManager
	{
		private const string arasLibsFolderName = "DefaultCodeTemplates";
		private const string defaultCodeTemplatesFolderName = "DefaultCodeTemplates";
		private const string serverMethodsFolderName = "ServerMethods";
		private const string projectConfigFileName = "projectConfig.xml";
		private const string methodConfigFileName = "method-config.xml";
		private const string globalSuppressionsFileName = "GlobalSuppressions.cs";

		private VisualStudioWorkspace visualStudioWorkspace;

		private readonly IServiceProvider serviceProvider;
		private readonly IDialogFactory dialogFactory;
		private readonly IIOWrapper iOWrapper;
		private readonly IVsPackageWrapper vsPackageWrapper;

		public ProjectManager(IServiceProvider serviceProvider, IDialogFactory dialogFactory, IIOWrapper iOWrapper, IVsPackageWrapper vsPackageWrapper)
		{
			if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (iOWrapper == null) throw new ArgumentNullException(nameof(iOWrapper));
			if (vsPackageWrapper == null) throw new ArgumentNullException(nameof(vsPackageWrapper));

			this.serviceProvider = serviceProvider;
			this.dialogFactory = dialogFactory;
			this.iOWrapper = iOWrapper;
			this.vsPackageWrapper = vsPackageWrapper;
		}

		public string ProjectConfigPath
		{
			get
			{
				var project = GetFirstSelectedProject();
				ProjectItem projectConfigItem = project.ProjectItems.Item(projectConfigFileName);
				return projectConfigItem.FileNames[0];
			}
		}

		public string MethodConfigPath
		{
			get
			{
				var project = GetFirstSelectedProject();
				var methodConfigFile = project.ProjectItems.Item(methodConfigFileName);
				return methodConfigFile.FileNames[0];
			}
		}

		public string DefaultCodeTemplatesPath
		{
			get
			{
				var project = GetFirstSelectedProject();
				ProjectItem defaultmethodConfigFile = project.ProjectItems.Item(defaultCodeTemplatesFolderName);
				return defaultmethodConfigFile.FileNames[0];
			}
		}

		public string GlobalSuppressionsPath
		{
			get
			{
				var project = GetFirstSelectedProject();
				ProjectItem defaultmethodConfigFile = project.ProjectItems.Item(globalSuppressionsFileName);
				return defaultmethodConfigFile.FileNames[0];
			}
		}

		public string MethodPath
		{
			get
			{
				string selectedFilePath = this.GetSelectedFiles().FirstOrDefault();
				return GetMethodPath(selectedFilePath);
			}
		}

		public string MethodName
		{
			get
			{
				string methodPath = this.MethodPath;
				string methodName = Path.GetFileNameWithoutExtension(methodPath);
				return methodName;
			}
		}

		public Microsoft.CodeAnalysis.Document ActiveDocument
		{
			get
			{
				IWpfTextView textView = GetTextView();
				SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
				return caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();
			}
		}

		public Microsoft.CodeAnalysis.SyntaxNode ActiveSyntaxNode
		{
			get
			{
				IWpfTextView textView = GetTextView();
				SnapshotPoint caretPosition = textView.Caret.Position.BufferPosition;
				Microsoft.CodeAnalysis.Document document = caretPosition.Snapshot.GetOpenDocumentInCurrentContextWithChanges();

				var activeSyntaxNode = document.GetSyntaxRootAsync().Result.FindToken(caretPosition).Parent;
				if (activeSyntaxNode is BlockSyntax)
				{
					activeSyntaxNode = activeSyntaxNode.Parent;
				}

				return activeSyntaxNode;
			}
		}

		public string ActiveDocumentMethodFullPath
		{
			get
			{
				var dte = (DTE2)serviceProvider.GetService(typeof(DTE));
				string ActiveDocumentlFilePath = dte.ActiveDocument.FullName;
				return GetMethodPath(ActiveDocumentlFilePath);
			}
		}

		public string ActiveDocumentMethodName
		{
			get
			{
				string activeDocumentMethodPath = this.ActiveDocumentMethodFullPath;
				return Path.GetFileNameWithoutExtension(activeDocumentMethodPath);
			}
		}

		public string ActiveDocumentMethodFolderPath
		{
			get
			{
				return Path.GetDirectoryName(ActiveDocumentMethodFullPath);
			}
		}

		public string SelectedFilePath
		{
			get
			{
				string selectedFilePath = this.GetSelectedFiles().FirstOrDefault();
				return selectedFilePath;
			}
		}

		public string SelectedFileName
		{
			get
			{
				string selectedFilePath = this.SelectedFilePath;
				string selectedFileName = Path.GetFileNameWithoutExtension(selectedFilePath);
				return selectedFileName;
			}
		}

		public string SelectedFolderPath
		{
			get
			{
				string selectedFilePath = this.GetSelectedFiles().FirstOrDefault();
				string selectedFolder = Path.GetDirectoryName(selectedFilePath);
				return selectedFolder;
			}
		}

		public Project SelectedProject
		{
			get
			{
				var dte = (DTE2)serviceProvider.GetService(typeof(DTE));
				var solutionProjects = dte.ActiveSolutionProjects as Array;
				if (solutionProjects.Length == 0)
				{
					return null;
				}
				return solutionProjects.GetValue(0) as Project;
			}
		}

		public IVsUIShell UIShell
		{
			get
			{
				return (IVsUIShell)serviceProvider.GetService(typeof(SVsUIShell));
			}
		}

		public ProjectItem ServerMethodsFolderItem
		{
			get
			{
				if (!SelectedProject.ProjectItems.Exists(serverMethodsFolderName))
				{
					SelectedProject.ProjectItems.AddFolder(serverMethodsFolderName);
				}

				return SelectedProject.ProjectItems.Item(serverMethodsFolderName);
			}
		}

		public ProjectItems ServerMethodFolderItems
		{
			get
			{
				return this.ServerMethodsFolderItem.ProjectItems;
			}
		}

		public string ServerMethodFolderPath
		{
			get
			{
				var project = GetFirstSelectedProject();
				ProjectItem defaultmethodConfigFile = this.ServerMethodsFolderItem;
				return defaultmethodConfigFile.FileNames[0];
			}
		}

		public OleMenuCommandService CommandService
		{
			get
			{
				return serviceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			}
		}

		public bool IsArasProject
		{
			get
			{
				Project selectedProject = this.SelectedProject;
				if (selectedProject == null)
				{
					return false;
				}
				if (selectedProject.ProjectItems.Exists(arasLibsFolderName) &&
					selectedProject.ProjectItems.Exists(defaultCodeTemplatesFolderName) &&
					selectedProject.ProjectItems.Exists(serverMethodsFolderName) &&
					selectedProject.ProjectItems.Exists(methodConfigFileName) &&
					selectedProject.ProjectItems.Exists(projectConfigFileName))
				{
					return true;
				}

				return false;
			}
		}

		public bool SolutionHasProject
		{
			get
			{
				var dte = (DTE2)serviceProvider.GetService(typeof(DTE));
				var countProject = dte.Solution.Projects.Count;
				if (countProject > 0)
				{
					return true;
				}

				return false;
			}
		}

		public bool IsCommandForMethod(Guid commandId)
		{
			var listOfMethodNotDependentOfSelection = new List<Guid> {
					CommandIds.CreateMethod,
					CommandIds.OpenFromAras,
					CommandIds.OpenFromPackage,
					CommandIds.ConnectionInfo,
					CommandIds.RefreshConfig,
				};
			if (listOfMethodNotDependentOfSelection.Contains(commandId))
			{
				return true;
			}
			if (string.IsNullOrEmpty(MethodName))
			{
				return false;
			}
			return true;
		}

		public VisualStudioWorkspace VisualStudioWorkspace
		{
			get
			{
				if (this.visualStudioWorkspace == null)
				{
					var componentModel = (IComponentModel)this.vsPackageWrapper.GetGlobalService(typeof(SComponentModel));
					this.visualStudioWorkspace = componentModel.GetService<VisualStudioWorkspace>();
				}

				return this.visualStudioWorkspace;
			}
		}

		private Project GetFirstSelectedProject()
		{
			var dte2 = (DTE2)serviceProvider.GetService(typeof(DTE));
			var solutionProjects = dte2.ActiveSolutionProjects as Array;

			if (solutionProjects.Length == 0)
			{
				throw new Exception("Project is not selected on Solution Explorer.");
			}

			return solutionProjects.GetValue(0) as Project;
		}

		public IEnumerable<string> GetSelectedFiles()
		{
			var dte = (DTE2)serviceProvider.GetService(typeof(DTE));
			var items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;

			return from item in items.Cast<UIHierarchyItem>()
				   let pi = item.Object as ProjectItem
				   select pi?.FileNames[1];
		}

		public void CreateMethodTree(GeneratedCodeInfo generatedCodeInfo)
		{
			AddItemTemplateToProjectNew(generatedCodeInfo.WrapperCodeInfo, false);

			var splittedByLinesArray = generatedCodeInfo.MethodCodeInfo.Code.Split(new string[] { "\r\n" }, StringSplitOptions.None);
			int index = 1;
			for (int i = 0; i < splittedByLinesArray.Length; i++)
			{
				if (splittedByLinesArray[i].Contains("#region MethodCode"))
				{
					index = i + 2;
				}
			}
			AddItemTemplateToProjectNew(generatedCodeInfo.MethodCodeInfo, true, index);
			AddItemTemplateToProjectNew(generatedCodeInfo.TestsCodeInfo, false);

			foreach (var partialCodeInfo in generatedCodeInfo.PartialCodeInfoList)
			{
				AddItemTemplateToProjectNew(partialCodeInfo, false);
			}

			foreach (var externalItemsInfo in generatedCodeInfo.ExternalItemsInfoList)
			{
				AddItemTemplateToProjectNew(externalItemsInfo, false);
			}
		}

		public string AddItemTemplateToProjectNew(CodeInfo codeInfo, bool openAfterCreation, int cursorIndex = -1)
		{
			string codeFilePath = !Path.HasExtension(codeInfo.Path) ? codeInfo.Path + ".cs" : codeInfo.Path;

			var folder = GetProjectFolder(codeFilePath);
			var fullPathToFolder = folder.Properties.Item("FullPath").Value.ToString();
			var codefileName = Path.GetFileName(codeFilePath);
			var fileFullPath = Path.Combine(fullPathToFolder, codefileName);

			Encoding witoutBom = new UTF8Encoding(true);
			this.iOWrapper.WriteAllTextIntoFile(fileFullPath, codeInfo.Code, witoutBom);
			folder.ProjectItems.AddFromFile(fileFullPath);
			if (openAfterCreation)
			{
				var window = folder.ProjectItems.Item(codefileName).Open(EnvDTE.Constants.vsViewKindCode);
				window.Visible = true;
				DTE dte = (DTE)this.vsPackageWrapper.GetGlobalService(typeof(DTE));
				dte.ExecuteCommand("Edit.Goto", cursorIndex.ToString());
			}

			return fileFullPath;
		}

		public void ExecuteCommand(string commandName)
		{
			DTE dte = (DTE)this.vsPackageWrapper.GetGlobalService(typeof(DTE));
			dte.ExecuteCommand(commandName);
		}

		public bool IsMethodExist(string methodName)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(methodName);
			ProjectItem folder;
			ProjectItems serverMethodFolderItems = ServerMethodFolderItems;
			if (serverMethodFolderItems.Exists(fileNameWithoutExtension))
			{
				folder = serverMethodFolderItems.Item(fileNameWithoutExtension);
				string methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;
				return folder.ProjectItems.Exists(methodNameWithExtension);
			}

			return false;
		}

		public bool IsFileExist(string path)
		{
			string[] pathParts = path.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);

			string fileName = pathParts.Last();
			var fileNameWithExtension = !Path.HasExtension(fileName) ? fileName + ".cs" : fileName;

			ProjectItem folder = LoadProjectFolder(path);
			if (folder == null || !folder.ProjectItems.Exists(fileNameWithExtension))
			{
				return false;
			}

			return true;
		}

		public void RemoveMethod(MethodInfo methodInfo)
		{
			var methodFolder = GetProjectFolder(methodInfo.MethodName);
			var methodNameWithExtension = !Path.HasExtension(methodInfo.MethodName) ? methodInfo.MethodName + ".cs" : methodInfo.MethodName;
			//remove method
			if (methodFolder.ProjectItems.Exists(methodNameWithExtension))
			{
				var methodItem = methodFolder.ProjectItems.Item(methodNameWithExtension);
				var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
				methodItem.Remove();
				this.iOWrapper.FileDelete(pathToItem);
			}
			//remove wrapper
			var methodName = methodInfo.MethodName + "Wrapper";
			methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;
			if (methodFolder.ProjectItems.Exists(methodNameWithExtension))
			{

				var methodItem = methodFolder.ProjectItems.Item(methodNameWithExtension);
				var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
				methodItem.Remove();
				this.iOWrapper.FileDelete(pathToItem);
			}

			//remove tests
			methodName = methodInfo.MethodName + "Tests";
			methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;
			if (methodFolder.ProjectItems.Exists(methodNameWithExtension))
			{

				var methodItem = methodFolder.ProjectItems.Item(methodNameWithExtension);
				var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
				methodItem.Remove();
				this.iOWrapper.FileDelete(pathToItem);
			}

			//remove partial classes
			var partialClassesForDelete = new List<string>();
			foreach (var partialCodeInfo in methodInfo.PartialClasses)
			{
				var folder = methodFolder.ProjectItems;
				string pathToFolder = string.Empty;
				var updatedPath = partialCodeInfo.Replace("\"", "");
				string[] partialFilePath = updatedPath.Split(new string[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);

				var partialFileName = !Path.HasExtension(partialFilePath.Last()) ? partialFilePath.Last() + ".cs" : partialFilePath.Last();
				for (int i = 0; i < partialFilePath.Length - 1; i++)
				{
					string partialMethodNameWithoutExtension = Path.GetFileNameWithoutExtension(partialFilePath[i]);
					if (folder.Exists(partialMethodNameWithoutExtension))
					{
						pathToFolder = folder.Item(partialMethodNameWithoutExtension).Properties.Item("FullPath").Value.ToString();
						folder = folder.Item(partialMethodNameWithoutExtension).ProjectItems;
					}
				}

				if (folder.Exists(partialFileName))
				{
					var methodItem = folder.Item(partialFileName);
					var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
					methodItem.Remove();
					this.iOWrapper.FileDelete(pathToItem);
				}

				partialClassesForDelete.Add(partialCodeInfo);
			}

			foreach (var pcfd in partialClassesForDelete)
			{
				methodInfo.PartialClasses.Remove(pcfd);
			}

			//remove external codes
			var externalItemsForDelete = new List<string>();
			foreach (var externalItemInfo in methodInfo.ExternalItems)
			{
				var folder = methodFolder.ProjectItems;
				string pathToFolder = string.Empty;
				var updatedPath = externalItemInfo.Replace("\"", "");
				string[] externalFilePath = updatedPath.Split(new string[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);

				var externalFileName = !Path.HasExtension(externalFilePath.Last()) ? externalFilePath.Last() + ".cs" : externalFilePath.Last();
				for (int i = 0; i < externalFilePath.Length - 1; i++)
				{
					string externalMethodNameWithoutExtension = Path.GetFileNameWithoutExtension(externalFilePath[i]);
					if (folder.Exists(externalMethodNameWithoutExtension))
					{
						pathToFolder = folder.Item(externalMethodNameWithoutExtension).Properties.Item("FullPath").Value.ToString();
						folder = folder.Item(externalMethodNameWithoutExtension).ProjectItems;
					}
				}

				if (folder.Exists(externalFileName))
				{
					var methodItem = folder.Item(externalFileName);
					var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
					methodItem.Remove();
					this.iOWrapper.FileDelete(pathToItem);
				}

				externalItemsForDelete.Add(externalItemInfo);
			}

			foreach (var pcfd in externalItemsForDelete)
			{
				methodInfo.ExternalItems.Remove(pcfd);
			}

			//remove folder if no files inside
		}

		public bool SaveDirtyFiles(List<MethodInfo> methodInfos)
		{
			bool saveIsApproved = false;

			foreach (MethodInfo methodInfo in methodInfos)
			{
				var methodPaths = new List<string>();
				methodPaths.Add(methodInfo.MethodName + "\\" + methodInfo.MethodName);
				methodPaths.AddRange(methodInfo.PartialClasses);
				methodPaths.AddRange(methodInfo.ExternalItems);
				foreach (string methodPath in methodPaths)
				{
					ProjectItem fileProjectItem = LoadItemFolder(methodPath);
					if (fileProjectItem != null && fileProjectItem.IsDirty)
					{
						if (saveIsApproved)
						{
							fileProjectItem.Save();
						}
						else
						{
							var messageWindow = dialogFactory.GetMessageBoxWindow(UIShell);
							var messageDialogResult = messageWindow.ShowDialog(
								"One or more method files is not saved. Do you want to save changes?",
								"Aras VS method plugin",
								MessageButtons.YesNoCancel,
								MessageIcon.Question);

							if (messageDialogResult == MessageDialogResult.Yes)
							{
								saveIsApproved = true;
								fileProjectItem.Save();
							}
							else if (messageDialogResult == MessageDialogResult.No)
							{
								return true;
							}
							else if (messageDialogResult == MessageDialogResult.Cancel)
							{
								return false;
							}
						}
					}
				}
			}

			return true;
		}

		private ProjectItem GetProjectFolder(string path)
		{
			string[] pathParts = path.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
			string fileName = pathParts.Last();
			ProjectItem folder = this.ServerMethodsFolderItem;
			for (int i = 0; i < pathParts.Length; i++)
			{
				if (string.IsNullOrEmpty(pathParts[i]))
				{
					continue;
				}

				if (Path.HasExtension(pathParts[i]))
				{
					break;
				}

				if (folder.ProjectItems.Exists(pathParts[i]))
				{
					folder = folder.ProjectItems.Item(pathParts[i]);
				}
				else
				{
					folder = folder.ProjectItems.AddFolder(pathParts[i]);
				}
			}

			return folder;
		}

		private ProjectItem LoadProjectFolder(string path)
		{
			string[] pathParts = path.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
			ProjectItem folder = this.ServerMethodsFolderItem;
			for (int i = 0; i < pathParts.Length - 1; i++)
			{
				if (folder.ProjectItems.Exists(pathParts[i]))
				{
					folder = folder.ProjectItems.Item(pathParts[i]);
				}
				else
				{
					folder = null;
					break;
				}
			}

			return folder;
		}

		private ProjectItem LoadItemFolder(string path)
		{
			string[] pathParts = path.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);
			string methodName = pathParts.Last();
			string methodNameWithoutExtension = Path.GetFileNameWithoutExtension(methodName);
			string methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;

			ProjectItem fileProjectItem = null;
			ProjectItem folderProjectItem = LoadProjectFolder(path);
			if (folderProjectItem != null && folderProjectItem.ProjectItems.Exists(methodNameWithExtension))
			{
				fileProjectItem = folderProjectItem.ProjectItems.Item(methodNameWithExtension);
			}

			return fileProjectItem;
		}

		public void AttachToProcess(System.Diagnostics.Process process)
		{
			DTE dte = (DTE)this.vsPackageWrapper.GetGlobalService(typeof(DTE));
			foreach (EnvDTE.Process processToAttach in dte.Debugger.LocalProcesses)
			{
				if (processToAttach.ProcessID == process.Id)
				{
					processToAttach.Attach();
				}
			}
		}

		public void AddSuppression(string suppressName, string ruleCategory, string ruleId, string scope = "", string target = "")
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("[{0}(\"{1}\", \"{2}\"", suppressName, ruleCategory, ruleId);
			if (!string.IsNullOrEmpty(scope))
			{
				stringBuilder.AppendFormat(", Scope = \"{0}\"", scope);
			}
			if (!string.IsNullOrEmpty(target))
			{
				stringBuilder.AppendFormat(", Target = \"{0}\"", target);
			}
			stringBuilder.Append(")]");

			var customAttribute = stringBuilder.ToString();

			string globalSuppressionsContent = this.iOWrapper.ReadAllText(GlobalSuppressionsPath, new UTF8Encoding(true));
			var tree = CSharpSyntaxTree.ParseText(globalSuppressionsContent);
			CompilationUnitSyntax root = (CompilationUnitSyntax)tree.GetRoot();

			stringBuilder.Clear();

			foreach (var attribute in root.AttributeLists)
			{
				stringBuilder.Append(attribute.ToFullString());
			}

			stringBuilder.Append(customAttribute);
			stringBuilder.Append(Environment.NewLine);

			this.iOWrapper.WriteAllTextIntoFile(GlobalSuppressionsPath, stringBuilder.ToString(), new UTF8Encoding(true));
		}

		private static string GetMethodPath(string selectedFilePath)
		{
			string mainFilePath = string.Empty;
			if (!string.IsNullOrEmpty(selectedFilePath))
			{
				var directoryInfo = new DirectoryInfo(selectedFilePath);
				while (directoryInfo.Parent != null)
				{
					DirectoryInfo parrentDirectoryInfo = directoryInfo.Parent;
					string rootFolderName = parrentDirectoryInfo.Name;
					string methodFolderName = directoryInfo.Name;

					if (rootFolderName == serverMethodsFolderName && !Path.HasExtension(methodFolderName))
					{
						mainFilePath = Path.Combine(directoryInfo.FullName, methodFolderName + ".cs");
						break;
					}

					directoryInfo = parrentDirectoryInfo;
				}
			}

			if (string.IsNullOrEmpty(mainFilePath))
			{
				return null;
			}

			return mainFilePath;
		}

		private IWpfTextView GetTextView()
		{
			IVsTextManager textManager = (IVsTextManager)serviceProvider.GetService(typeof(SVsTextManager));
			IVsTextView textView;
			textManager.GetActiveView(1, null, out textView);
			return GetEditorAdaptersFactoryService().GetWpfTextView(textView);
		}

		private IVsEditorAdaptersFactoryService GetEditorAdaptersFactoryService()
		{
			IComponentModel componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
			return componentModel.GetService<IVsEditorAdaptersFactoryService>();
		}
	}
}
