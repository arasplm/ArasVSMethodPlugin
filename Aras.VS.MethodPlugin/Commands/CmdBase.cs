//------------------------------------------------------------------------------
// <copyright file="CmdBase.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Aras.Method.Libs;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;
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
		protected readonly MessageManager messageManager;

		public CmdBase(
			IProjectManager projectManager,
			IDialogFactory dialogFactory, 
			IProjectConfigurationManager projectConfigurationManager,
			MessageManager messageManager)
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
				projectConfigurationManager.Load(projectConfigPath);

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

		public void UpdateProjectConfiguration(IProjectConfiguraiton projectConfiguration, IProjectManager projectManager)
		{
			ProjectItems serverMethods = projectManager.ServerMethodFolderItems;
			if (serverMethods.Count == 0)
			{
				projectConfiguration.MethodInfos.Clear();
				return;
			}

			var updatedMethodInfos = new List<MethodInfo>();

			foreach (MethodInfo methodInfo in projectConfiguration.MethodInfos)
			{
				if (!projectManager.IsMethodExist(methodInfo.Package.MethodFolderPath, methodInfo.MethodName))
				{
					continue;
				}

				MethodInfo updatedMethodInfo;
				if (methodInfo is PackageMethodInfo)
				{
					updatedMethodInfo = new PackageMethodInfo((PackageMethodInfo)methodInfo);
				}
				else
				{
					updatedMethodInfo = new MethodInfo(methodInfo);
				}

				updatedMethodInfos.Add(updatedMethodInfo);
			}

			projectConfiguration.MethodInfos = updatedMethodInfos;
		}
	}
}
