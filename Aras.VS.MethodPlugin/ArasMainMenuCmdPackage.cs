//------------------------------------------------------------------------------
// <copyright file="ArasMainMenuCmdPackage.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.ProjectConfigurations;
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
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(ArasMainMenuCmdPackage.PackageGuidString)]
	[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
	[ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]
	public sealed class ArasMainMenuCmdPackage : Package
	{
		/// <summary>
		/// ArasMainMenuCommandPackagePackage GUID string.
		/// </summary>
		public const string PackageGuidString = "7afa5f12-ad2b-4fda-85d3-818f2d1e6c8c";

		private IAuthenticationManager authManager;
		private IArasDataProvider arasDataProvider;
		private IDialogFactory dialogFactory;
		private ProjectConfigurationManager projectConfigurationManager;
		private IProjectManager projectManager;
		private DefaultCodeProvider defaultCodeProvider;
		private ICodeProviderFactory codeProviderFactory;

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
		protected override void Initialize()
		{
			base.Initialize();
			var dllPath = Assembly.GetExecutingAssembly().Location;
			
			this.authManager = new AuthenticationManager();
			this.arasDataProvider = new ArasDataProvider(authManager);
			this.dialogFactory = new DialogFactory(authManager, arasDataProvider);
			this.projectConfigurationManager = new ProjectConfigurationManager();
			this.projectManager = new ProjectManager(this, dialogFactory);
			this.defaultCodeProvider = new DefaultCodeProvider();
			this.codeProviderFactory = new CodeProviderFactory(projectManager, defaultCodeProvider);

			Commands.OpenFromArasCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
			Commands.OpenFromPackageCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
			Commands.CreateMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
			Commands.SaveToArasCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
			Commands.SaveToPackageCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
			Commands.UpdateMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
			Commands.ConnectionInfoCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager);
			Commands.CreatePartialElementCmd.Initialize(projectManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
			Commands.RefreshConfigCmd.Initialize(projectManager, dialogFactory, projectConfigurationManager);
			Commands.DebugMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);

			var dte = (DTE)this.GetService(typeof(DTE));
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
				ProjectConfiguraiton projectConfiguration = projectConfigurationManager.Load(projectConfigPath);

				string methodName = this.projectManager.MethodName;

				projectConfiguration.RemoveFromMethodInfo(methodName, ProjectItem);
				projectConfigurationManager.Save(projectConfigPath, projectConfiguration);
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
				ProjectConfiguraiton projectConfiguration = projectConfigurationManager.Load(projectConfigPath);

				string methodName = this.projectManager.MethodName;

				projectConfiguration.UpdateMethodInfo(methodName, ProjectItem, OldName);
				projectConfigurationManager.Save(projectConfigPath, projectConfiguration);
			}
			catch
			{

			}
		}
		#endregion
	}
}
