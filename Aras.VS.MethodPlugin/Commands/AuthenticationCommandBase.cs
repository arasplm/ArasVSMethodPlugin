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
	public abstract class AuthenticationCommandBase : CmdBase
	{
		protected readonly IAuthenticationManager authManager;
		protected readonly ICodeProviderFactory codeProviderFactory;

		public AuthenticationCommandBase(
			IAuthenticationManager authManager,
			IDialogFactory dialogFactory,
			IProjectManager projectManager,
			ProjectConfigurationManager projectConfigurationManager,
			ICodeProviderFactory codeProviderFactory) : base(projectManager, dialogFactory, projectConfigurationManager)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (codeProviderFactory == null) throw new ArgumentNullException(nameof(codeProviderFactory));

			this.authManager = authManager;
			this.codeProviderFactory = codeProviderFactory;
		}

		public override void ExecuteCommand(object sender, EventArgs args)
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
	}
}
