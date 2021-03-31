//------------------------------------------------------------------------------
// <copyright file="AuthenticationCommandBase.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.OpenMethodInVS;
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
			IProjectConfigurationManager projectConfigurationManager,
			ICodeProviderFactory codeProviderFactory,
			MessageManager messageManager) : base(projectManager, dialogFactory, projectConfigurationManager, messageManager)
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
				projectConfigurationManager.Load(projectConfigPath);

				if (args is OpenMethodContext openMethodContext)
				{
					projectConfigurationManager.CurrentProjectConfiguraiton.AddConnection(openMethodContext.ConnectionInfo);
				}

				var lastConnection = projectConfigurationManager.CurrentProjectConfiguraiton.Connections.FirstOrDefault(c => c.LastConnection);
				if (!authManager.IsLoginedForCurrentProject(projectManager.SelectedProject.Name, lastConnection))
				{
					var loginView = dialogFactory.GetLoginView(projectManager, projectConfigurationManager.CurrentProjectConfiguraiton);
					if (loginView.ShowDialog()?.DialogOperationResult != true)
					{
						return;
					}
				}

				UpdateProjectConfiguration(projectConfigurationManager.CurrentProjectConfiguraiton, projectManager);
				projectConfigurationManager.Save(projectConfigPath);

				bool saved = projectManager.SaveDirtyFiles(dialogFactory, projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos);
				if (saved)
				{
					ExecuteCommandImpl(sender, args);
				}
			}
			catch (Exception ex)
			{
				var messageWindow = dialogFactory.GetMessageBoxWindow();
				messageWindow.ShowDialog(ex.Message,
					messageManager.GetMessage("ArasVSMethodPlugin"),
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
