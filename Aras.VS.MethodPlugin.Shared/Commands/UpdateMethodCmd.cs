﻿//------------------------------------------------------------------------------
// <copyright file="UpdateMethodCmd.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
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
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;

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
		private UpdateMethodCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager)
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
		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager)
		{
			Instance = new UpdateMethodCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			var project = projectManager.SelectedProject;

			string projectConfigPath = projectManager.ProjectConfigPath;
			string selectedMethodPath = projectManager.MethodPath;
			string selectedMethodName = Path.GetFileNameWithoutExtension(selectedMethodPath);
			MethodInfo methodInformation = projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.FirstOrDefault(m => m.MethodName == selectedMethodName);
			if (methodInformation == null)
			{
				throw new Exception();
			}

			TemplateLoader templateLoader = new TemplateLoader();
			templateLoader.Load(projectManager.MethodConfigPath);

			var packageManager = new PackageManager(authManager, this.messageManager);

			var updateView = dialogFactory.GetUpdateFromArasView(projectConfigurationManager, templateLoader, packageManager, methodInformation, projectConfigPath, project.Name, project.FullName);
			var updateViewResult = updateView.ShowDialog();
			if (updateViewResult?.DialogOperationResult != true)
			{
				return;
			}

			var eventData = CommonData.EventSpecificDataTypeList.First(x => x.EventSpecificData == updateViewResult.EventSpecificData);

			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectManager.Language);
			GeneratedCodeInfo codeInfo = codeProvider.GenerateCodeInfo(updateViewResult.SelectedTemplate, eventData, updateViewResult.MethodName, updateViewResult.MethodCode, updateViewResult.IsUseVSFormattingCode);
			projectManager.CreateMethodTree(codeInfo, updateViewResult.Package);

			var methodInfo = new MethodInfo()
			{
				InnovatorMethodConfigId = updateViewResult.MethodConfigId,
				InnovatorMethodId = updateViewResult.MethodId,
				MethodLanguage = updateViewResult.MethodLanguage,
				MethodName = updateViewResult.MethodName,
				MethodType = updateViewResult.MethodType,
				Package = updateViewResult.Package,
				TemplateName = updateViewResult.SelectedTemplate.TemplateName,
				EventData = updateViewResult.EventSpecificData,
				ExecutionAllowedToId = updateViewResult.ExecutionIdentityId,
				ExecutionAllowedToKeyedName = updateViewResult.ExecutionIdentityKeyedName,
				MethodComment = updateViewResult.MethodComment
			};

			projectConfigurationManager.CurrentProjectConfiguraiton.AddMethodInfo(methodInfo);
			projectConfigurationManager.CurrentProjectConfiguraiton.UseVSFormatting = updateViewResult.IsUseVSFormattingCode;
			projectConfigurationManager.Save(projectConfigPath);
		}
	}
}
