//------------------------------------------------------------------------------
// <copyright file="ArasMainMenuCmdPackage.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Configurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the
	/// IVsPackage interface and uses the registration attributes defined in the framework to
	/// register itself and its components with the shell. These attributes tell the pkgdef creation
	/// utility what data to put into .pkgdef file.
	/// </para>
	/// <para>
	/// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
	/// </para>
	/// </remarks>
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(ArasMainMenuCmdPackage.PackageGuidString)]
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
	public sealed class ArasMainMenuCmdPackage : AsyncPackage
	{
		/// <summary>
		/// ArasMainMenuCommandPackagePackage GUID string.
		/// </summary>
		public const string PackageGuidString = "7afa5f12-ad2b-4fda-85d3-818f2d1e6c8c";

		private IProjectConfigurationManager projectConfigurationManager;
		private IAuthenticationManager authManager;
		private IArasDataProvider arasDataProvider;
		private IDialogFactory dialogFactory;
		private IProjectManager projectManager;
		private ICodeProviderFactory codeProviderFactory;
		private IIOWrapper iOWrapper;
		private IVsPackageWrapper vsPackageWrapper;
		private IGlobalConfiguration globalConfiguration;
		private MessageManager messageManager;

		private ProjectItemsEvents projectItemsEvents;

		/// <summary>
		/// Initializes a new instance of the <see cref="ArasMainMenuCmdPackage"/> class.
		/// </summary>
		public ArasMainMenuCmdPackage()
		{
			// Inside this method you can place any initialization code that does not require
			// any Visual Studio service because at this point the package object is created but
			// not sited yet inside Visual Studio environment. The place to do all the other
			// initialization is the Initialize method.
		}

		#region Package Members

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override async System.Threading.Tasks.Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await base.InitializeAsync(cancellationToken, progress);

			// When initialized asynchronously, we *may* be on a background thread at this point.
			// Do any initialization that requires the UI thread after switching to the UI thread.
			// Otherwise, remove the switch to the UI thread if you don't need it.
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			IVisualStudioServiceProvider serviceProvider = new VisualStudioServiceProvider(this);

			this.messageManager = new VisualStudioMessageManager();
			this.iOWrapper = new IOWrapper();
			this.projectConfigurationManager = new ProjectConfigurationManager();
			this.vsPackageWrapper = new VsPackageWrapper();
			this.projectManager = new ProjectManager(serviceProvider, iOWrapper, vsPackageWrapper, messageManager, projectConfigurationManager);
			this.authManager = new AuthenticationManager(messageManager, projectManager);
			this.arasDataProvider = new ArasDataProvider(authManager, messageManager);
			this.dialogFactory = new DialogFactory(authManager, arasDataProvider, serviceProvider, iOWrapper, messageManager);
			ICodeFormatter codeFormatter = new VisualStudioCodeFormatter(this.projectManager);
			this.codeProviderFactory = new CodeProviderFactory(codeFormatter, messageManager, iOWrapper);
			this.globalConfiguration = new GlobalConfiguration(iOWrapper);

			Commands.OpenFromArasCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
			Commands.OpenFromPackageCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
			Commands.CreateMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, globalConfiguration, messageManager);
			Commands.SaveToArasCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
			Commands.SaveToPackageCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
			Commands.UpdateMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
			Commands.ConnectionInfoCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, messageManager);
			Commands.CreateCodeItemCmd.Initialize(projectManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
			Commands.RefreshConfigCmd.Initialize(projectManager, dialogFactory, projectConfigurationManager, messageManager);
			Commands.DebugMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
			Commands.MoveToCmd.Initialize(projectManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);

			var dte = (DTE)serviceProvider.GetService(typeof(DTE));
			this.projectItemsEvents = dte.Events.GetObject("CSharpProjectItemsEvents") as ProjectItemsEvents;
			if (this.projectItemsEvents != null)
			{
				this.projectItemsEvents.ItemRemoved += this.ProjectItemsEvents_ItemRemoved;
				this.projectItemsEvents.ItemRenamed += this.ProjectItemsEvents_ItemRenamed;
			}
		}

		private void ProjectItemsEvents_ItemRemoved(ProjectItem ProjectItem)
		{
			try
			{
				if (!this.projectManager.IsArasProject)
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
				if (!this.projectManager.IsArasProject)
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

		private void RemoveFromMethodInfo(string methodName, ProjectItem projectItem)
		{
			MethodInfo methodInfos = projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.FirstOrDefault(m => m.MethodName == methodName);
			if (methodInfos == null)
			{
				return;
			}

			string methodFolderPath = iOWrapper.PathCombine(projectConfigurationManager.CurrentProjectConfiguraiton.MethodsFolderPath, methodInfos.Package.MethodFolderPath);
			string filePath = projectItem.FileNames[0];
			int index = filePath.IndexOf(projectConfigurationManager.CurrentProjectConfiguraiton.MethodsFolderPath);
			if (index == 1)
			{
				return;
			}

			string attributePath = filePath.Substring(index + methodFolderPath.Length);
			attributePath = Path.ChangeExtension(attributePath, null);

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

			string methodFolderPath = iOWrapper.PathCombine(projectConfigurationManager.CurrentProjectConfiguraiton.MethodsFolderPath, methodInformation.Package.MethodFolderPath);

			string newAttributePath = newFilePath.Substring(index + methodFolderPath.Length);
			newAttributePath = Path.ChangeExtension(newAttributePath, null);
			string oldAttributePath = iOWrapper.PathCombine(iOWrapper.PathGetDirectoryName(newAttributePath), oldName);
			oldAttributePath = Path.ChangeExtension(oldAttributePath, null);

			string oldPartialAttribute = oldAttributePath.Substring(oldAttributePath.IndexOf("\\") + 1).Replace("\\", "/");
			string newPartialAttribute = newAttributePath.Substring(newAttributePath.IndexOf("\\") + 1).Replace("\\", "/");
			string oldExternalAttribute = oldAttributePath.Substring(oldAttributePath.IndexOf("\\") + 1).Replace("\\", "/");
			string newExternalAttribute = newAttributePath.Substring(newAttributePath.IndexOf("\\") + 1).Replace("\\", "/");

			Encoding witoutBom = new UTF8Encoding(true);
			string code = File.ReadAllText(newFilePath, witoutBom);
			code = code.Replace($"[PartialPath(\"{oldPartialAttribute}\"", $"[PartialPath(\"{newPartialAttribute}\"");
			code = code.Replace($"[ExternalPath(\"{oldExternalAttribute}\"", $"[ExternalPath(\"{newExternalAttribute}\"");
			File.WriteAllText(newFilePath, code, witoutBom);
		}

		#endregion
	}
}
