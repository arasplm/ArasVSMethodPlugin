//------------------------------------------------------------------------------
// <copyright file="CmdBase.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	public abstract class CmdBase
	{
		/// <summary>
		/// VS Package that provides this command, not null.
		/// </summary>
		protected readonly IDialogFactory dialogFactory;
		protected readonly IProjectManager projectManager;
		protected readonly IProjectConfigurationManager projectConfigurationManager;
		protected readonly IMessageManager messageManager;

		public CmdBase(
			IProjectManager projectManager,
			IDialogFactory dialogFactory, 
			IProjectConfigurationManager projectConfigurationManager,
			IMessageManager messageManager)
		{
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (projectConfigurationManager == null) throw new ArgumentNullException(nameof(projectConfigurationManager));
			if (messageManager == null) throw new ArgumentNullException(nameof(messageManager));

			this.dialogFactory = dialogFactory;
			this.projectManager = projectManager;
			this.projectConfigurationManager = projectConfigurationManager;
			this.messageManager = messageManager;
		}

		public virtual void ExecuteCommand(object sender, EventArgs args)
		{
			IVsUIShell uiShell = projectManager.UIShell;
			uiShell.EnableModeless(0);

			try
			{
				string projectConfigPath = projectManager.ProjectConfigPath;
				var projectConfiguration = projectConfigurationManager.Load(projectConfigPath);

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
			finally
			{
				uiShell.EnableModeless(1);
			}
		}
		//TODO: remove uiShell from parameters
		public abstract void ExecuteCommandImpl(object sender, EventArgs args);

		protected virtual void CheckCommandAccessibility(object sender, EventArgs e)
		{
			var command = (OleMenuCommand)sender;
			if (this.projectManager.SolutionHasProject && this.projectManager.IsArasProject && this.projectManager.IsCommandForMethod(command.CommandID.Guid))
			{
				command.Enabled = true;
				return;
			}

			command.Enabled = false;
		}
	}
}
