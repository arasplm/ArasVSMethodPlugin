using System;
using System.Linq;
using System.Text;
using Aras.Method.Libs;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin.SolutionManagement
{
	internal class EventListener
	{
		private readonly IProjectManager projectManager;
		private readonly ProjectUpdater projectUpdater;
		private readonly IProjectConfigurationManager projectConfigurationManager;
		private readonly IIOWrapper iOWrapper;

		private DTE2 dte2;
		private Events2 dteEvents;
		private ProjectItemsEvents projectItemsEvents;
		private SelectionEvents selectionEvents;

		public EventListener(IProjectManager projectManager, ProjectUpdater projectUpdater, IProjectConfigurationManager projectConfigurationManager, IIOWrapper iOWrapper)
		{
			this.projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
			this.projectUpdater = projectUpdater ?? throw new ArgumentNullException(nameof(projectUpdater));
			this.projectConfigurationManager = projectConfigurationManager ?? throw new ArgumentNullException(nameof(projectConfigurationManager));
			this.iOWrapper = iOWrapper ?? throw new ArgumentNullException(nameof(iOWrapper));

			this.dte2 = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;
			this.dteEvents = dte2.Events as Events2;
			this.projectItemsEvents = dteEvents.ProjectItemsEvents;
			this.selectionEvents = dteEvents.SelectionEvents;
		}

		public void StartListening()
		{
			if (projectItemsEvents != null)
			{
				projectItemsEvents.ItemRemoved += this.ProjectItemsEvents_ItemRemoved;
				projectItemsEvents.ItemRenamed += this.ProjectItemsEvents_ItemRenamed;
			}

			if (selectionEvents != null)
			{
				selectionEvents.OnChange += this.SelectionEvents_OnChange;
			}
		}

		private void ProjectItemsEvents_ItemRemoved(ProjectItem ProjectItem)
		{
			try
			{
				if (!this.projectManager.SolutionHasProject | !this.projectManager.IsArasProject)
				{
					return;
				}

				string projectConfigPath = this.projectManager.ProjectConfigPath;
				projectConfigurationManager.Load(projectConfigPath);

				string methodName = this.projectManager.MethodName;

				RemoveFromMethodInfo(methodName, ProjectItem);
				projectConfigurationManager.Save(projectConfigPath);
			}
			catch
			{

			}
		}

		private void ProjectItemsEvents_ItemRenamed(ProjectItem ProjectItem, string OldName)
		{
			try
			{
				if (!this.projectManager.SolutionHasProject | !this.projectManager.IsArasProject)
				{
					return;
				}

				string projectConfigPath = this.projectManager.ProjectConfigPath;
				projectConfigurationManager.Load(projectConfigPath);

				string methodName = this.projectManager.MethodName;

				UpdateMethodInfo(methodName, ProjectItem, OldName);
				projectConfigurationManager.Save(projectConfigPath);
			}
			catch
			{

			}
		}

		private string projectName = string.Empty;
		private void SelectionEvents_OnChange()
		{
			try
			{
				if (!this.projectManager.SolutionHasProject | !this.projectManager.IsArasProject)
				{
					return;
				}

				if (projectName != projectManager.SelectedProject.Name)
				{
					projectName = projectManager.SelectedProject.Name;
					projectConfigurationManager.Load(this.projectManager.ProjectConfigPath);

					projectUpdater.UpdateProjectAttributes(projectManager.ProjectFolderPath);
				}
			}
			catch
			{

			}
		}

		private void RemoveFromMethodInfo(string methodName, ProjectItem projectItem)
		{
			MethodInfo methodInfos = projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.FirstOrDefault(m => m.MethodName == methodName);
			if (methodInfos == null)
			{
				return;
			}

			string packageMethodFolderPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? methodInfos.Package.MethodFolderPath : string.Empty;
			string methodFolderPath = iOWrapper.PathCombine(projectConfigurationManager.CurrentProjectConfiguraiton.MethodsFolderPath, packageMethodFolderPath);
			string filePath = projectItem.FileNames[0];
			int index = filePath.IndexOf(projectConfigurationManager.CurrentProjectConfiguraiton.MethodsFolderPath);
			if (index == 1)
			{
				return;
			}

			string attributePath = filePath.Substring(index + methodFolderPath.Length);
			attributePath = iOWrapper.PathChangeExtension(attributePath, null);

			string fodlerPath = methodName + "\\";

			if (fodlerPath == attributePath)
			{
				projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.RemoveAll(x => x.MethodName == methodName);
				return;
			}
		}

		private void UpdateMethodInfo(string methodName, ProjectItem projectItem, string oldName)
		{
			MethodInfo methodInformation = projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.FirstOrDefault(m => m.MethodName == methodName);
			if (methodInformation == null)
			{
				return;
			}

			string newFilePath = projectItem.FileNames[0];
			int index = newFilePath.IndexOf(projectConfigurationManager.CurrentProjectConfiguraiton.MethodsFolderPath);
			if (index == -1)
			{
				return;
			}

			string packageMethodFolderPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? methodInformation.Package.MethodFolderPath : string.Empty;
			string methodFolderPath = iOWrapper.PathCombine(projectConfigurationManager.CurrentProjectConfiguraiton.MethodsFolderPath, packageMethodFolderPath);

			string newAttributePath = newFilePath.Substring(index + methodFolderPath.Length);
			newAttributePath = iOWrapper.PathChangeExtension(newAttributePath, null);
			string oldAttributePath = iOWrapper.PathCombine(iOWrapper.PathGetDirectoryName(newAttributePath), oldName);
			oldAttributePath = iOWrapper.PathChangeExtension(oldAttributePath, null);

			string oldPartialAttribute = oldAttributePath.Substring(oldAttributePath.IndexOf("\\") + 1).Replace("\\", "/");
			string newPartialAttribute = newAttributePath.Substring(newAttributePath.IndexOf("\\") + 1).Replace("\\", "/");
			string oldExternalAttribute = oldAttributePath.Substring(oldAttributePath.IndexOf("\\") + 1).Replace("\\", "/");
			string newExternalAttribute = newAttributePath.Substring(newAttributePath.IndexOf("\\") + 1).Replace("\\", "/");

			Encoding witoutBom = new UTF8Encoding(true);
			string code = iOWrapper.FileReadAllText(newFilePath, witoutBom);
			code = code.Replace($"[PartialPath(\"{oldPartialAttribute}\"", $"[PartialPath(\"{newPartialAttribute}\"");
			code = code.Replace($"[ExternalPath(\"{oldExternalAttribute}\"", $"[ExternalPath(\"{newExternalAttribute}\"");
			iOWrapper.WriteAllTextIntoFile(newFilePath, code, witoutBom);
		}
	}
}
