//------------------------------------------------------------------------------
// <copyright file="ProjectManager.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Commands;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Extensions;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Aras.VS.MethodPlugin.SolutionManagement
{
	public class ProjectManager : IProjectManager
	{
		private readonly IVisualStudioServiceProvider serviceProvider;
		private readonly IIOWrapper iOWrapper;
		private readonly IVsPackageWrapper vsPackageWrapper;
		private readonly MessageManager messageManager;
		private readonly IProjectConfigurationManager projectConfigurationManager;

		private VisualStudioWorkspace visualStudioWorkspace;

		public ProjectManager(IVisualStudioServiceProvider serviceProvider,
			IIOWrapper iOWrapper,
			IVsPackageWrapper vsPackageWrapper,
			MessageManager messageManager,
			IProjectConfigurationManager projectConfigurationManager)
		{
			this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			this.iOWrapper = iOWrapper ?? throw new ArgumentNullException(nameof(iOWrapper));
			this.vsPackageWrapper = vsPackageWrapper ?? throw new ArgumentNullException(nameof(vsPackageWrapper));
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
			this.projectConfigurationManager = projectConfigurationManager ?? throw new ArgumentNullException(nameof(projectConfigurationManager));
		}

		public string MethodConfigPath
		{
			get
			{
				string methodConfigPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.MethodConfigPath;
				if (!iOWrapper.PathIsPathRooted(methodConfigPath))
				{
					methodConfigPath = iOWrapper.PathCombine(this.ProjectFolderPath, methodConfigPath);
				}

				return methodConfigPath;
			}
		}

		public string IOMFilePath
		{
			get
			{
				string iOMFilePath = this.projectConfigurationManager.CurrentProjectConfiguraiton.IOMFilePath;
				if (!iOWrapper.PathIsPathRooted(iOMFilePath))
				{
					iOMFilePath = iOWrapper.PathCombine(this.ProjectFolderPath, iOMFilePath);
				}

				return iOMFilePath;
			}
		}

		public string ProjectFolderPath
		{
			get
			{
				return Path.GetDirectoryName(GetFirstSelectedProject().FullName);
			}
		}

		public string ProjectConfigPath
		{
			get
			{
				var project = GetFirstSelectedProject();
				ProjectItem projectConfigItem = project.ProjectItems.Item(GlobalConsts.projectConfigFileName);
				return projectConfigItem.FileNames[0];
			}
		}

		public string GlobalSuppressionsPath
		{
			get
			{
				var project = GetFirstSelectedProject();
				ProjectItem defaultmethodConfigFile = project.ProjectItems.Item(GlobalConsts.globalSuppressionsFileName);
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
				return Path.GetFileNameWithoutExtension(this.MethodPath);
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

		public string ActiveDocumentFilePath
		{
			get
			{
				return this.ActiveDocument.FilePath;
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
				return ForceLoadProjectFolder(this.projectConfigurationManager.CurrentProjectConfiguraiton.MethodsFolderPath);
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
				ProjectItem defaultmethodConfigFile = this.ServerMethodsFolderItem;
				return defaultmethodConfigFile.FileNames[0];
			}
		}

		public IMenuCommandService CommandService
		{
			get
			{
				return serviceProvider.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
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

				if (selectedProject.ProjectItems.Exists(GlobalConsts.projectConfigFileName))
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

		public string Language { get { return SelectedProject.CodeModel.Language; } }

		public bool IsCommandForMethod(Guid commandId)
		{
			List<Guid> listOfMethodNotDependentOfSelection = new List<Guid>
			{
				CommandIds.CreateMethod,
				CommandIds.OpenFromAras,
				CommandIds.OpenFromPackage,
				CommandIds.ConnectionInfo,
				CommandIds.RefreshConfig,
				CommandIds.ImportOpenInVSAction
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
				throw new Exception(messageManager.GetMessage("ProjectIsNotSelectedOnSolutionExplorer"));
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

		public void CreateMethodTree(GeneratedCodeInfo generatedCodeInfo, PackageInfo packageInfo)
		{
			string packageMethodFolderPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? packageInfo.MethodFolderPath : string.Empty;

			AddItemTemplateToProjectNew(generatedCodeInfo.WrapperCodeInfo, packageMethodFolderPath, false);

			var splittedByLinesArray = generatedCodeInfo.MethodCodeInfo.Code.Split(new string[] { "\r\n" }, StringSplitOptions.None);
			int index = 1;
			for (int i = 0; i < splittedByLinesArray.Length; i++)
			{
				if (splittedByLinesArray[i].Contains(GlobalConsts.RegionMethodCode))
				{
					index = i + 2;
				}
			}
			AddItemTemplateToProjectNew(generatedCodeInfo.MethodCodeInfo, packageMethodFolderPath, true, index);

			foreach (var partialCodeInfo in generatedCodeInfo.PartialCodeInfoList)
			{
				AddItemTemplateToProjectNew(partialCodeInfo, packageMethodFolderPath, false);
			}

			foreach (var externalItemsInfo in generatedCodeInfo.ExternalItemsInfoList)
			{
				AddItemTemplateToProjectNew(externalItemsInfo, packageMethodFolderPath, false);
			}
		}

		public string AddItemTemplateToProjectNew(CodeInfo codeInfo, string packagePath, bool openAfterCreation, int cursorIndex = -1)
		{
			string codeFilePath = !Path.HasExtension(codeInfo.Path) ? codeInfo.Path + GlobalConsts.CSExtension : codeInfo.Path;
			codeFilePath = Path.Combine(packagePath, codeFilePath);

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

		public bool IsMethodExist(string packagePath, string methodName)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(methodName);
			ProjectItem folder;
			ProjectItem serverMethodFolderItems = LoadProjectFolder(packagePath);
			if (serverMethodFolderItems == null)
			{
				return false;
			}

			if (serverMethodFolderItems.ProjectItems.Exists(fileNameWithoutExtension))
			{
				folder = serverMethodFolderItems.ProjectItems.Item(fileNameWithoutExtension);
				string methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + GlobalConsts.CSExtension : methodName;
				return folder.ProjectItems.Exists(methodNameWithExtension);
			}

			return false;
		}

		public bool IsFileExist(string filePath)
		{
			string[] pathParts = filePath.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries);

			string fileName = pathParts.Last();
			var fileNameWithExtension = !Path.HasExtension(fileName) ? fileName + GlobalConsts.CSExtension : fileName;

			string folderPath = iOWrapper.PathGetDirectoryName(filePath);
			ProjectItem folder = LoadProjectFolder(folderPath);
			if (folder == null || !folder.ProjectItems.Exists(fileNameWithExtension))
			{
				return false;
			}

			return true;
		}

		public void RemoveMethod(MethodInfo methodInfo)
		{
			string packageMethodFolderPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? methodInfo.Package.MethodFolderPath : string.Empty;
			var methodFolder = GetProjectFolder(Path.Combine(packageMethodFolderPath, methodInfo.MethodName));
			var methodNameWithExtension = !Path.HasExtension(methodInfo.MethodName) ? methodInfo.MethodName + GlobalConsts.CSExtension : methodInfo.MethodName;
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
			methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + GlobalConsts.CSExtension : methodName;
			if (methodFolder.ProjectItems.Exists(methodNameWithExtension))
			{

				var methodItem = methodFolder.ProjectItems.Item(methodNameWithExtension);
				var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
				methodItem.Remove();
				this.iOWrapper.FileDelete(pathToItem);
			}
		}

		public bool SaveDirtyFiles(IDialogFactory dialogFactory, List<MethodInfo> methodInfos)
		{
			string serverMethodFolderPath = ServerMethodFolderPath;

			bool saveIsApproved = false;

			foreach (MethodInfo methodInfo in methodInfos)
			{
				string packageMethodFolderPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? methodInfo.Package.MethodFolderPath : string.Empty;
				string methodWorkingFolder = Path.Combine(serverMethodFolderPath, packageMethodFolderPath, methodInfo.MethodName);
				List<string> methodPaths = iOWrapper.DirectoryGetFiles(methodWorkingFolder, $"*{GlobalConsts.CSExtension}", SearchOption.AllDirectories)
					.Select(x => x.Substring(serverMethodFolderPath.Length))
					.ToList();

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
							var messageWindow = dialogFactory.GetMessageBoxWindow();
							var messageDialogResult = messageWindow.ShowDialog(
								messageManager.GetMessage("OneOrMoreMethodFilesIsNotSavedDoYouWantToSaveChanges"),
								messageManager.GetMessage("ArasVSMethodPlugin"),
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
			string[] pathParts = path.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries);
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

		private ProjectItem ForceLoadProjectFolder(string path)
		{
			Project root = SelectedProject;

			string[] pathParts = path.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries);
			int count = iOWrapper.PathHasExtension(path) ? pathParts.Length - 1 : pathParts.Length;

			if (count == 0)
			{
				return null;
			}


			if (!root.ProjectItems.Exists(pathParts[0]))
			{
				root.ProjectItems.AddFolder(pathParts[0]);
			}

			ProjectItem projectItemFolder = root.ProjectItems.Item(pathParts[0]);

			for (int i = 1; i < count; i++)
			{
				if (!projectItemFolder.ProjectItems.Exists(pathParts[i]))
				{
					projectItemFolder.ProjectItems.AddFolder(pathParts[i]);
				}

				projectItemFolder = projectItemFolder.ProjectItems.Item(pathParts[i]);
			}

			return projectItemFolder;
		}

		private ProjectItem LoadProjectFolder(string path)
		{
			string[] pathParts = path.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries);
			ProjectItem folder = this.ServerMethodsFolderItem;

			int count = iOWrapper.PathHasExtension(path) ? pathParts.Length - 1 : pathParts.Length;

			for (int i = 0; i < count; i++)
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
			string[] pathParts = path.Split(new string[] { @"\", "/" }, StringSplitOptions.RemoveEmptyEntries);
			string methodName = pathParts.Last();
			string methodNameWithoutExtension = Path.GetFileNameWithoutExtension(methodName);
			string methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + GlobalConsts.CSExtension : methodName;

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

		private string GetMethodPath(string selectedFilePath)
		{
			string expectedRootFolderName = projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? "Method" : this.projectConfigurationManager.CurrentProjectConfiguraiton.MethodsFolderPath;
			string mainFilePath = string.Empty;

			if (!string.IsNullOrEmpty(selectedFilePath))
			{
				var directoryInfo = new DirectoryInfo(selectedFilePath);
				while (directoryInfo.Parent != null)
				{
					DirectoryInfo parrentDirectoryInfo = directoryInfo.Parent;
					string rootFolderName = parrentDirectoryInfo.Name;
					string methodFolderName = directoryInfo.Name;

					if (rootFolderName == expectedRootFolderName && !Path.HasExtension(methodFolderName))
					{
						mainFilePath = Path.Combine(directoryInfo.FullName, methodFolderName + GlobalConsts.CSExtension);
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
