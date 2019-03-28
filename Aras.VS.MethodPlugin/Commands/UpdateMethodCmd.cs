//------------------------------------------------------------------------------
// <copyright file="UpdateMethodCmd.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class UpdateMethodCmd : AuthenticationCommandBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0104;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = CommandIds.UpdateMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="UpdateMethodCmd"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		private UpdateMethodCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, IMessageManager messageManager)
			: base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory, messageManager)
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
		public static UpdateMethodCmd Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, IMessageManager messageManager)
		{
			Instance = new UpdateMethodCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			var project = projectManager.SelectedProject;

			string projectConfigPath = projectManager.ProjectConfigPath;
			string methodConfigPath = projectManager.MethodConfigPath;

			var projectConfiguration = projectConfigurationManager.Load(projectConfigPath);

			string selectedMethodPath = projectManager.MethodPath;
			string selectedMethodName = Path.GetFileNameWithoutExtension(selectedMethodPath);
			MethodInfo methodInformation = projectConfiguration.MethodInfos.FirstOrDefault(m => m.MethodName == selectedMethodName);
			if (methodInformation == null)
			{
				throw new Exception();
			}

			var templateLoader = new TemplateLoader(this.dialogFactory, this.messageManager);
			templateLoader.Load(methodConfigPath);

			var packageManager = new PackageManager(authManager, this.messageManager);

			var updateView = dialogFactory.GetUpdateFromArasView(projectConfigurationManager, projectConfiguration, templateLoader, packageManager, methodInformation, projectConfigPath, project.Name, project.FullName);
			var updateViewResult = updateView.ShowDialog();
			if (updateViewResult?.DialogOperationResult != true)
			{
				return;
			}

			var eventData = CommonData.EventSpecificDataTypeList.First(x => x.EventSpecificData == updateViewResult.EventSpecificData);

			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(project.CodeModel.Language, projectConfiguration);
			GeneratedCodeInfo codeInfo = codeProvider.GenerateCodeInfo(updateViewResult.SelectedTemplate, eventData, updateViewResult.MethodName, false, updateViewResult.MethodCode, updateViewResult.IsUseVSFormattingCode);
			projectManager.CreateMethodTree(codeInfo);

			var methodInfo = new MethodInfo()
			{
				InnovatorMethodConfigId = updateViewResult.MethodConfigId,
				InnovatorMethodId = updateViewResult.MethodId,
				MethodLanguage = updateViewResult.MethodLanguage,
				MethodName = updateViewResult.MethodName,
				MethodType = updateViewResult.MethodType,
				PackageName = updateViewResult.PackageName,
				TemplateName = updateViewResult.SelectedTemplate.TemplateName,
				EventData = updateViewResult.EventSpecificData,
				ExecutionAllowedToId = updateViewResult.ExecutionIdentityId,
				ExecutionAllowedToKeyedName = updateViewResult.ExecutionIdentityKeyedName,
				MethodComment = updateViewResult.MethodComment,
				PartialClasses = codeInfo.PartialCodeInfoList.Select(pci => pci.Path).ToList(),
				ExternalItems = codeInfo.ExternalItemsInfoList.Select(pci => pci.Path).ToList()
			};

			projectConfiguration.AddMethodInfo(methodInfo);
			projectConfiguration.UseVSFormatting = updateViewResult.IsUseVSFormattingCode;
			projectConfigurationManager.Save(projectConfigPath, projectConfiguration);
		}
	}
}
