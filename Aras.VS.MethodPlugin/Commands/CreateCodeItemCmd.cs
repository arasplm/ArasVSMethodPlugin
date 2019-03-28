//------------------------------------------------------------------------------
// <copyright file="CreateCodeItemCmd.cs" company="Aras Corporation">
//     Copyright © 2018 Aras Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class CreateCodeItemCmd : CmdBase
	{
		private readonly ICodeProviderFactory codeProviderFactory;

		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0101;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = CommandIds.CreateCodeItemElement;

		/// <summary>
		/// Initializes a new instance of the <see cref="CreateCodeItemCmd"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		private CreateCodeItemCmd(IProjectManager projectManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, IMessageManager messageManager)
			: base(projectManager, dialogFactory, projectConfigurationManager, messageManager)
		{
			if (codeProviderFactory == null) throw new ArgumentNullException(nameof(codeProviderFactory));

			this.codeProviderFactory = codeProviderFactory;

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
		public static CreateCodeItemCmd Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(IProjectManager projectManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, IMessageManager messageManager)
		{
			Instance = new CreateCodeItemCmd(projectManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			var project = projectManager.SelectedProject;

			string selectedMethodName = projectManager.MethodName;
			string serverMethodFolderPath = projectManager.ServerMethodFolderPath;
			string selectedFolderPath = projectManager.SelectedFolderPath;
			string projectConfigPath = projectManager.ProjectConfigPath;

			var projectConfiguration = projectConfigurationManager.Load(projectConfigPath);
			MethodInfo methodInformation = projectConfiguration.MethodInfos.FirstOrDefault(m => m.MethodName == selectedMethodName);
			if (methodInformation == null)
			{
				throw new Exception(this.messageManager.GetMessage("ConfigurationsForTheMethodNotFound", selectedMethodName));
			}

			var view = dialogFactory.GetCreateCodeItemView(this.codeProviderFactory.GetCodeItemProvider(project.CodeModel.Language), projectConfiguration.UseVSFormatting);
			var viewResult = view.ShowDialog();
			if (viewResult?.DialogOperationResult != true)
			{
				return;
			}

			string codeItemPath = selectedFolderPath.Substring(serverMethodFolderPath.IndexOf(serverMethodFolderPath) + serverMethodFolderPath.Length);
			codeItemPath = Path.Combine(codeItemPath, viewResult.FileName);

			if (methodInformation.PartialClasses.Contains(codeItemPath, StringComparer.InvariantCultureIgnoreCase) ||
				methodInformation.ExternalItems.Contains(codeItemPath, StringComparer.InvariantCultureIgnoreCase))
			{
				throw new Exception(this.messageManager.GetMessage("CodeItemAlreadyExists"));
			}

			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(project.CodeModel.Language, projectConfiguration);
			CodeInfo codeItemInfo = codeProvider.CreateCodeItemInfo(methodInformation, viewResult.FileName, viewResult.SelectedCodeType, viewResult.SelectedElementType, viewResult.IsUseVSFormattingCode);

			projectManager.AddItemTemplateToProjectNew(codeItemInfo, true, 0);

			if (viewResult.SelectedCodeType == CodeType.Partial)
			{
				methodInformation.PartialClasses.Add(codeItemInfo.Path);
			}
			else if (viewResult.SelectedCodeType == CodeType.External)
			{
				methodInformation.ExternalItems.Add(codeItemInfo.Path);
			}

			projectConfiguration.UseVSFormatting = viewResult.IsUseVSFormattingCode;
			projectConfigurationManager.Save(projectManager.ProjectConfigPath, projectConfiguration);
		}
	}
}
