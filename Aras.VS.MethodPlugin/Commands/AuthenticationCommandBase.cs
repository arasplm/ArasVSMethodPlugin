﻿//------------------------------------------------------------------------------
// <copyright file="AuthenticationCommandBase.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Linq;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;

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
			try
			{
				string projectConfigPath = projectManager.ProjectConfigPath;
				var projectConfiguration = projectConfigurationManager.Load(projectConfigPath);

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
		}
	}
}
