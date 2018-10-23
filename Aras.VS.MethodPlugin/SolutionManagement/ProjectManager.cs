//------------------------------------------------------------------------------
// <copyright file="ProjectManager.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.Extensions;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.SolutionManagement
{
	public class ProjectManager : IProjectManager
	{
		private const string arasLibsFolderName = "DefaultCodeTemplates";
		private const string defaultCodeTemplatesFolderName = "DefaultCodeTemplates";
		private const string serverMethodsFolderName = "ServerMethods";
		private const string projectConfigFileName = "projectConfig.xml";
		private const string methodConfigFileName = "method-config.xml";

		private VisualStudioWorkspace visualStudioWorkspace;

		private readonly IServiceProvider serviceProvider;
		private readonly IDialogFactory dialogFactory;

		public ProjectManager(IServiceProvider serviceProvider, IDialogFactory dialogFactory)
		{
			if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));

			this.serviceProvider = serviceProvider;
			this.dialogFactory = dialogFactory;
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

		public string MethodPath
		{
			get
			{
				var mainFilePath = string.Empty;
				string selectedFilePath = this.GetSelectedFiles().FirstOrDefault();

				if (!string.IsNullOrEmpty(selectedFilePath))
				{
					var directoryInfo = new DirectoryInfo(selectedFilePath);
					while (directoryInfo.Parent != null)
					{
						DirectoryInfo parrentDirectoryInfo = directoryInfo.Parent;
						string rootFolderName = parrentDirectoryInfo.Name;
						string methodFolderName = directoryInfo.Name;

						if (rootFolderName == "ServerMethods" && !Path.HasExtension(methodFolderName))
						{
							mainFilePath = Path.Combine(directoryInfo.FullName, methodFolderName + ".cs");
							break;
						}

						directoryInfo = parrentDirectoryInfo;
					}
				}

				if (string.IsNullOrEmpty(mainFilePath))
				{
					throw new Exception("Method not found.");
				}

				return mainFilePath;
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
				Project selectedProject = SelectedProject;
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

		public VisualStudioWorkspace VisualStudioWorkspace
		{
			get
			{
				if (this.visualStudioWorkspace == null)
				{
					var componentModel = (IComponentModel)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SComponentModel));
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
		}

		public string AddItemTemplateToProjectNew(CodeInfo codeInfo, bool openAfterCreation, int cursorIndex = -1)
		{
			string path = codeInfo.Path;
			var folder = GetProjectFolder(path);
			var methodName = Path.GetFileName(path);
			var methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;
			var pathToFolder = folder.Properties.Item("FullPath").Value.ToString();
			var filePath = Path.Combine(pathToFolder, methodNameWithExtension);
			Encoding witoutBom = new UTF8Encoding(true);
			File.WriteAllText(filePath, codeInfo.Code, witoutBom);
			folder.ProjectItems.AddFromFile(filePath);
			string filePathNew = folder.ProjectItems.Item(methodNameWithExtension).FileNames[0].ToString();
			if (openAfterCreation)
			{
				var window = folder.ProjectItems.Item(methodNameWithExtension).Open(EnvDTE.Constants.vsViewKindCode);
				window.Visible = true;
				DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));
				dte.ExecuteCommand("Edit.Goto", cursorIndex.ToString());
			}

			return filePath;
		}

		public void ExecuteCommand(string commandName)
		{
			DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));
			dte.ExecuteCommand(commandName);
		}

		public bool IsMethodExist(string methodName)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(methodName);
			ProjectItem folder;
			if (ServerMethodFolderItems.Exists(fileNameWithoutExtension))
			{
				folder = ServerMethodFolderItems.Item(fileNameWithoutExtension);
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
				File.Delete(pathToItem);
			}
			//remove wrapper
			var methodName = methodInfo.MethodName + "Wrapper";
			methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;
			if (methodFolder.ProjectItems.Exists(methodNameWithExtension))
			{

				var methodItem = methodFolder.ProjectItems.Item(methodNameWithExtension);
				var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
				methodItem.Remove();
				File.Delete(pathToItem);
			}

			//remove tests
			methodName = methodInfo.MethodName + "Tests";
			methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;
			if (methodFolder.ProjectItems.Exists(methodNameWithExtension))
			{

				var methodItem = methodFolder.ProjectItems.Item(methodNameWithExtension);
				var pathToItem = methodItem.Properties.Item("FullPath").Value.ToString();
				methodItem.Remove();
				File.Delete(pathToItem);
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
					File.Delete(pathToItem);
				}

				partialClassesForDelete.Add(partialCodeInfo);
			}

			foreach (var pcfd in partialClassesForDelete)
			{
				methodInfo.PartialClasses.Remove(pcfd);
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
			for (int i = 0; i < pathParts.Length - 1; i++)
			{
				if (string.IsNullOrEmpty(pathParts[i]))
				{
					continue;
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
			DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));
			foreach (EnvDTE.Process processToAttach in dte.Debugger.LocalProcesses)
			{
				if (processToAttach.ProcessID == process.Id)
				{
					processToAttach.Attach();
				}
			}
		}
	}
}
