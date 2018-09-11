//------------------------------------------------------------------------------
// <copyright file="RefreshConfigCmd.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


using System;
using System.ComponentModel.Design;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class RefreshConfigCmd : CmdBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0101;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = new Guid("DB77AE9E-9CB5-4C13-9EB3-ED388DC94B66");

		private RefreshConfigCmd(IProjectManager projectManager, IDialogFactory dialogFactory, ProjectConfigurationManager projectConfigurationManager)
			: base(projectManager, dialogFactory, projectConfigurationManager)
		{
			if (projectManager.CommandService != null)
			{
				var menuCommandID = new CommandID(CommandSet, CommandId);
				var menuItem = new MenuCommand(this.ExecuteCommand, menuCommandID);

				projectManager.CommandService.AddCommand(menuItem);
			}
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static RefreshConfigCmd Instance
		{
			get;
			private set;
		}

		/// <summary>
		///  Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="projectManager"></param>
		/// <param name="dialogFactory"></param>
		/// <param name="projectConfigurationManager"></param>
		public static void Initialize(IProjectManager projectManager, IDialogFactory dialogFactory, ProjectConfigurationManager projectConfigurationManager)
		{
			Instance = new RefreshConfigCmd(projectManager, dialogFactory, projectConfigurationManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args, IVsUIShell uiShell)
		{

		}
	}
}
