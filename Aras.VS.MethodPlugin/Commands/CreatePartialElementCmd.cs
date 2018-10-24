//------------------------------------------------------------------------------
// <copyright file="CreatePartialElementCmd.cs" company="Aras Corporation">
//     Copyright © 2018 Aras Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class CreatePartialElementCmd : CmdBase
	{
		private readonly ICodeProviderFactory codeProviderFactory;

		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0101;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = CommandIds.CreatePartialElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreatePartialElementCmd"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private CreatePartialElementCmd(IProjectManager projectManager,IDialogFactory dialogFactory, ProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory)
			: base(projectManager, dialogFactory, projectConfigurationManager)
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
		public static CreatePartialElementCmd Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(IProjectManager projectManager, IDialogFactory dialogFactory, ProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory)
		{
			Instance = new CreatePartialElementCmd(projectManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args, IVsUIShell uiShell)
		{
			var project = projectManager.SelectedProject;

			string selectedMethodName = projectManager.MethodName;
			string serverMethodFolderPath = projectManager.ServerMethodFolderPath;
			string selectedFolderPath = projectManager.SelectedFolderPath;
			string projectConfigPath = projectManager.ProjectConfigPath;

			ProjectConfiguraiton projectConfiguration = projectConfigurationManager.Load(projectConfigPath);
			MethodInfo methodInformation = projectConfiguration.MethodInfos.FirstOrDefault(m => m.MethodName == selectedMethodName);
			if (methodInformation == null)
			{
				throw new Exception($"Configurations for the {selectedMethodName} method not found.");
			}
			
			CreatePartialElementViewAdapter view = dialogFactory.GetCreatePartialClassView(uiShell, projectConfiguration.UseVSFormatting);
			var viewResult = view.ShowDialog();
			if (viewResult?.DialogOperationResult != true)
			{
				return;
			}

			string partialPath = selectedFolderPath.Substring(serverMethodFolderPath.IndexOf(serverMethodFolderPath) + serverMethodFolderPath.Length);
			partialPath = Path.Combine(partialPath, viewResult.FileName);

			if (methodInformation.PartialClasses.Contains(partialPath, StringComparer.InvariantCultureIgnoreCase))
			{
				throw new Exception($"Partial element already exist.");
			}

			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(project.CodeModel.Language, projectConfiguration);
			CodeInfo partialCodeInfo = codeProvider.CreatePartialCodeInfo(methodInformation, viewResult.FileName, viewResult.IsUseVSFormattingCode);

			projectManager.AddItemTemplateToProjectNew(partialCodeInfo, true, 0);
			methodInformation.PartialClasses.Add(partialCodeInfo.Path);
            projectConfiguration.UseVSFormatting = viewResult.IsUseVSFormattingCode;
			projectConfigurationManager.Save(projectManager.ProjectConfigPath, projectConfiguration);
		}
	}
}
