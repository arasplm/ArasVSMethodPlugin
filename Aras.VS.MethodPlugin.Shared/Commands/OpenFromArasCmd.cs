//------------------------------------------------------------------------------
// <copyright file="OpenFromArasCmd.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
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
using Aras.VS.MethodPlugin.OpenMethodInVS;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class OpenFromArasCmd : AuthenticationCommandBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x101;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = CommandIds.OpenFromAras;

		/// <summary>
		/// Initializes a new instance of the <see cref="OpenFromArasCmd"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public OpenFromArasCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager) : base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory, messageManager)
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
		public static OpenFromArasCmd Instance
		{
			get;
			private set;
		}

		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager)
		{
			Instance = new OpenFromArasCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			var project = projectManager.SelectedProject;
			string projectConfigPath = projectManager.ProjectConfigPath;

			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectManager.Language);

			TemplateLoader templateLoader = new TemplateLoader();
			templateLoader.Load(projectManager.MethodConfigPath);

			var packageManager = new PackageManager(authManager, this.messageManager);
			OpenMethodContext openContext = args as OpenMethodContext;
			var openView = dialogFactory.GetOpenFromArasView(projectConfigurationManager, templateLoader, packageManager, projectConfigPath, project.Name, project.FullName, codeProvider.Language, openContext?.MethodId);

			var openViewResult = openView.ShowDialog();
			if (openViewResult?.DialogOperationResult != true)
			{
				return;
			}

			MethodInfo methodInformation = projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.FirstOrDefault(m => m.MethodName == openViewResult.MethodName);
			if (methodInformation != null)
			{
				string packageMethodFolderPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? methodInformation.Package.MethodFolderPath : string.Empty;
				if (projectManager.IsMethodExist(packageMethodFolderPath, methodInformation.MethodName))
				{
					var messageWindow = this.dialogFactory.GetMessageBoxWindow();
					var dialogReuslt = messageWindow.ShowDialog(this.messageManager.GetMessage("MethodAlreadyAddedToProjectDoYouWantReplaceMethod"),
						this.messageManager.GetMessage("Warning"),
						MessageButtons.YesNo,
						MessageIcon.None);

					if (dialogReuslt == MessageDialogResult.Yes)
					{
						projectManager.RemoveMethod(methodInformation);
						projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.Remove(methodInformation);
					}
					else
					{
						return;
					}
				}
			}

			GeneratedCodeInfo codeInfo = codeProvider.GenerateCodeInfo(openViewResult.SelectedTemplate, openViewResult.SelectedEventSpecificData, openViewResult.MethodName, openViewResult.MethodCode, openViewResult.IsUseVSFormattingCode);
			projectManager.CreateMethodTree(codeInfo, openViewResult.Package);

			var methodInfo = new MethodInfo()
			{
				InnovatorMethodConfigId = openViewResult.MethodConfigId,
				InnovatorMethodId = openViewResult.MethodId,
				MethodLanguage = openViewResult.MethodLanguage,
				MethodName = openViewResult.MethodName,
				MethodType = openViewResult.MethodType ?? "server",
				MethodComment = openViewResult.MethodComment,
				Package = openViewResult.Package,
				TemplateName = openViewResult.SelectedTemplate.TemplateName,
				EventData = openViewResult.SelectedEventSpecificData.EventSpecificData,
				ExecutionAllowedToId = openViewResult.SelectedIdentityId,
				ExecutionAllowedToKeyedName = openViewResult.SelectedIdentityKeyedName
			};

			projectManager.AddSuppression("assembly: System.Diagnostics.CodeAnalysis.SuppressMessage", "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", "namespace", codeInfo.Namespace);

			projectConfigurationManager.CurrentProjectConfiguraiton.AddMethodInfo(methodInfo);
			projectConfigurationManager.CurrentProjectConfiguraiton.UseVSFormatting = openViewResult.IsUseVSFormattingCode;
			projectConfigurationManager.Save(projectConfigPath);
		}
	}
}
