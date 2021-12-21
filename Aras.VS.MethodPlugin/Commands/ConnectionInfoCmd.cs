//------------------------------------------------------------------------------
// <copyright file="ConnectionInfoCmd.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using Aras.Method.Libs;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class ConnectionInfoCmd : CmdBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x104;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = CommandIds.ConnectionInfo;

		private readonly IAuthenticationManager authManager;

		private ConnectionInfoCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, MessageManager messageManager)
			: base(projectManager, dialogFactory, projectConfigurationManager, messageManager)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));

			this.authManager = authManager;

			if (projectManager.CommandService != null)
			{
				var menuCommandID = new CommandID(CommandSet, CommandId);
				var menuItem = new OleMenuCommand(this.ExecuteCommand, menuCommandID);
				menuItem.BeforeQueryStatus += CheckCommandAccessibility;
				projectManager.CommandService.AddCommand(menuItem);
			}
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static ConnectionInfoCmd Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, MessageManager messageManager)
		{
			Instance = new ConnectionInfoCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			var view = dialogFactory.GetConnectionInfoView(projectManager, projectConfigurationManager);
			view.ShowDialog();
		}
	}
}
