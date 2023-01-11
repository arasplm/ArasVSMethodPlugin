//------------------------------------------------------------------------------
// <copyright file="CreateCodeItemCmd.cs" company="Aras Corporation">
//     Copyright © 2023 Aras Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;

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
		private CreateCodeItemCmd(IProjectManager projectManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager)
			: base(projectManager, dialogFactory, projectConfigurationManager, messageManager)
		{
			this.codeProviderFactory = codeProviderFactory ?? throw new ArgumentNullException(nameof(codeProviderFactory));

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
		public static void Initialize(IProjectManager projectManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager)
		{
			Instance = new CreateCodeItemCmd(projectManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			string selectedMethodName = projectManager.MethodName;
			string serverMethodFolderPath = projectManager.ServerMethodFolderPath;
			string selectedFolderPath = projectManager.SelectedFolderPath;

			MethodInfo methodInformation = projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.FirstOrDefault(m => m.MethodName == selectedMethodName);
			if (methodInformation == null)
			{
				throw new Exception(this.messageManager.GetMessage("ConfigurationsForTheMethodNotFound", selectedMethodName));
			}

			var view = dialogFactory.GetCreateCodeItemView(this.codeProviderFactory.GetCodeItemProvider(projectManager.Language), projectConfigurationManager.CurrentProjectConfiguraiton.UseVSFormatting);
			var viewResult = view.ShowDialog();
			if (viewResult?.DialogOperationResult != true)
			{
				return;
			}

			string packageMethodFolderPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? methodInformation.Package.MethodFolderPath : string.Empty;
			string methodWorkingFolder = Path.Combine(serverMethodFolderPath, packageMethodFolderPath, methodInformation.MethodName);

			string codeItemPath = selectedFolderPath.Substring(methodWorkingFolder.Length).TrimStart('\\', '/');
			codeItemPath = Path.Combine(codeItemPath, viewResult.FileName);

			string newFilePath = Path.Combine(methodWorkingFolder, codeItemPath) + GlobalConsts.CSExtension;
			if (File.Exists(newFilePath))
			{
				throw new Exception(this.messageManager.GetMessage("CodeItemAlreadyExists"));
			}

			TemplateLoader templateLoader = new TemplateLoader();
			templateLoader.Load(projectManager.MethodConfigPath);

			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectManager.Language);
			CodeInfo codeItemInfo = null;
			if (viewResult.SelectedCodeType == CodeType.Partial)
			{
				codeItemInfo = codeProvider.CreatePartialCodeItemInfo(methodInformation,
					viewResult.FileName,
					viewResult.SelectedElementType,
					viewResult.IsUseVSFormattingCode,
					methodWorkingFolder,
					projectManager.SelectedFolderPath,
					projectManager.MethodName,
					templateLoader,
					projectManager.MethodPath);
			}
			else if (viewResult.SelectedCodeType == CodeType.External)
			{
				codeItemInfo = codeProvider.CreateExternalCodeItemInfo(methodInformation,
					viewResult.FileName,
					viewResult.SelectedElementType,
					viewResult.IsUseVSFormattingCode,
					methodWorkingFolder,
					projectManager.SelectedFolderPath,
					projectManager.MethodName,
					templateLoader,
					projectManager.MethodPath);
			}

			projectManager.AddItemTemplateToProjectNew(codeItemInfo, packageMethodFolderPath, true, 0);

			projectConfigurationManager.CurrentProjectConfiguraiton.UseVSFormatting = viewResult.IsUseVSFormattingCode;
			projectConfigurationManager.Save(projectManager.ProjectConfigPath);
		}
	}
}
