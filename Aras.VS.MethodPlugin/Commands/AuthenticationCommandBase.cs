using System;
using System.Linq;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	public abstract class AuthenticationCommandBase
	{
		protected readonly IAuthenticationManager authManager;
		protected readonly IDialogFactory dialogFactory;
		protected readonly IProjectManager projectManager;
		protected readonly ProjectConfigurationManager projectConfigurationManager;
		protected readonly ICodeProviderFactory codeProviderFactory;

		public AuthenticationCommandBase(
			IAuthenticationManager authManager,
			IDialogFactory dialogFactory,
			IProjectManager projectManager,
			ProjectConfigurationManager projectConfigurationManager,
			ICodeProviderFactory codeProviderFactory)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (projectConfigurationManager == null) throw new ArgumentNullException(nameof(projectConfigurationManager));
			if (codeProviderFactory == null) throw new ArgumentNullException(nameof(codeProviderFactory));

			this.authManager = authManager;
			this.dialogFactory = dialogFactory;
			this.projectManager = projectManager;
			this.projectConfigurationManager = projectConfigurationManager;
			this.codeProviderFactory = codeProviderFactory;
		}

		public void ExecuteCommand(object sender, EventArgs args)
		{
			IVsUIShell uiShell = projectManager.UIShell;
			uiShell.EnableModeless(0);

			try
			{
				string projectConfigPath = projectManager.ProjectConfigPath;
				ProjectConfiguraiton projectConfiguration = projectConfigurationManager.Load(projectConfigPath);

				var lastConnection = projectConfiguration.Connections.FirstOrDefault(c => c.LastConnection);
				if (!authManager.IsLoginedForCurrentProject(projectManager.SelectedProject.Name, lastConnection))
				{
					var loginView = dialogFactory.GetLoginView(projectManager, projectConfiguration);
					if (loginView.ShowDialog()?.DialogOperationResult != true)
					{
						return;
					}
				}

				projectConfiguration.Update(projectManager);
				projectConfigurationManager.Save(projectConfigPath, projectConfiguration);

				bool saved = projectManager.SaveDirtyFiles(projectConfiguration.MethodInfos);
				if (saved)
				{
					ExecuteCommandImpl(sender, args, uiShell);
				}
			}
			catch (Exception ex)
			{
				var messageWindow = dialogFactory.GetMessageBoxWindow(projectManager.UIShell);
				messageWindow.ShowDialog(
					ex.Message,
					"Aras VS method plugin",
					MessageButtons.OK,
					MessageIcon.Error);
			}
			finally
			{
				uiShell.EnableModeless(1);
			}
		}

		public abstract void ExecuteCommandImpl(object sender, EventArgs args, IVsUIShell uiShell);
	}
}
