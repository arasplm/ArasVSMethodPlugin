//------------------------------------------------------------------------------
// <copyright file="ImportOpenInVSActionCmd.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class ImportOpenInVSActionCmd : AuthenticationCommandBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x106;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = CommandIds.ImportOpenInVSAction;

		/// <summary>
		/// Initializes a new instance of the <see cref="ImportOpenInVSActionCmd"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public ImportOpenInVSActionCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager) :
			base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory, messageManager)
		{
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
		public static ImportOpenInVSActionCmd Instance
		{
			get;
			private set;
		}

		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager)
		{
			Instance = new ImportOpenInVSActionCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			dynamic result = authManager.InnovatorInstance.applyAML(Properties.Resources.ImportOpenInVSActionAML);

			IMessageBoxWindow messageBox = dialogFactory.GetMessageBoxWindow();
			string title = messageManager.GetMessage("ArasVSMethodPlugin");

			if (result.isError())
			{
				string errorMessage = messageManager.GetMessage("OpenInVSActionImportFailed", result.getErrorString());
				messageBox.ShowDialog(errorMessage, title, MessageButtons.OK, MessageIcon.Error);
			}
			else
			{
				messageBox.ShowDialog(messageManager.GetMessage("OpenInVSActionImported"), title, MessageButtons.OK, MessageIcon.Information);
			}
		}
	}
}
