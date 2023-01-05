//------------------------------------------------------------------------------
// <copyright file="MethodOpener.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Aras.Method.Libs;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.SolutionManagement;

namespace Aras.VS.MethodPlugin.OpenMethodInVS
{
	public class MethodOpener : IMethodOpener
	{
		private readonly IProjectManager projectManager;
		private readonly IDialogFactory dialogFactory;
		private readonly IOpenContextParser openContextParser;
		private readonly MessageManager messageManager;

		public MethodOpener(IProjectManager projectManager,
			IDialogFactory dialogFactory,
			IOpenContextParser openContextParser,
			MessageManager messageManager)
		{
			this.projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
			this.dialogFactory = dialogFactory ?? throw new ArgumentNullException(nameof(dialogFactory));
			this.openContextParser = openContextParser ?? throw new ArgumentNullException(nameof(openContextParser));
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
		}

		public void OpenMethodFromAras(string openRequestString)
		{
			OpenMethodContext openContext = openContextParser.Parse(openRequestString);
			if (openContext == null)
			{
				return;
			}

			IMessageBoxWindow messageBox = dialogFactory.GetMessageBoxWindow();
			MessageDialogResult result = messageBox.ShowDialog(messageManager.GetMessage("CreateNewOrOpenExistingProject"),
				messageManager.GetMessage("ArasVSMethodPlugin"), MessageButtons.YesNo, MessageIcon.Question);

			if (result == MessageDialogResult.Yes)
			{
				projectManager.ExecuteCommand("File.NewProject");
			}
			else
			{
				projectManager.ExecuteCommand("File.OpenProject");
			}

			if (projectManager.SolutionHasProject && projectManager.IsArasProject)
			{
				Commands.OpenFromArasCmd.Instance.ExecuteCommand(this, openContext);
			}
		}
	}
}
