//------------------------------------------------------------------------------
// <copyright file="OpenFromPackageCmd.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
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
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class OpenFromPackageCmd : CmdBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x102;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = CommandIds.OpenFromPackage;


		private readonly IAuthenticationManager authManager;
		private readonly ICodeProviderFactory codeProviderFactory;

		private OpenFromPackageCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager)
			: base(projectManager, dialogFactory, projectConfigurationManager, messageManager)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (projectConfigurationManager == null) throw new ArgumentNullException(nameof(projectConfigurationManager));
			if (codeProviderFactory == null) throw new ArgumentNullException(nameof(codeProviderFactory));

			this.authManager = authManager;
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
		public static OpenFromPackageCmd Instance
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
			Instance = new OpenFromPackageCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}


		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectManager.Language);

			TemplateLoader templateLoader = new TemplateLoader();
			templateLoader.Load(projectManager.MethodConfigPath);

			var openView = dialogFactory.GetOpenFromPackageView(templateLoader, codeProvider.Language, projectConfigurationManager.CurrentProjectConfiguraiton);
			var openViewResult = openView.ShowDialog();
			if (openViewResult?.DialogOperationResult != true)
			{
				return;
			}

			MethodInfo methodInformation = projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.FirstOrDefault(m => m.MethodName == openViewResult.MethodName);
			if (projectManager.IsMethodExist(openViewResult.Package.MethodFolderPath, openViewResult.MethodName))
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

			GeneratedCodeInfo codeInfo = codeProvider.GenerateCodeInfo(openViewResult.SelectedTemplate, openViewResult.SelectedEventSpecificData, openViewResult.MethodName, openViewResult.MethodCode, openViewResult.IsUseVSFormattingCode);
			projectManager.CreateMethodTree(codeInfo, openViewResult.Package);

			var methodInfo = new PackageMethodInfo()
			{
				InnovatorMethodConfigId = openViewResult.MethodConfigId,
				InnovatorMethodId = openViewResult.MethodId,
				MethodLanguage = openViewResult.MethodLanguage,
				MethodName = openViewResult.MethodName,
				MethodType = openViewResult.MethodType,
				MethodComment = openViewResult.MethodComment,
				Package = openViewResult.Package,
				TemplateName = openViewResult.SelectedTemplate.TemplateName,
				EventData = openViewResult.SelectedEventSpecificData.EventSpecificData,
				ExecutionAllowedToId = openViewResult.IdentityId,
				ExecutionAllowedToKeyedName = openViewResult.IdentityKeyedName,
				ManifestFileName = openViewResult.SelectedManifestFileName
			};

			projectManager.AddSuppression("assembly: System.Diagnostics.CodeAnalysis.SuppressMessage", "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", "namespace", codeInfo.Namespace);

			projectConfigurationManager.CurrentProjectConfiguraiton.LastSelectedDir = openViewResult.SelectedFolderPath;
			projectConfigurationManager.CurrentProjectConfiguraiton.LastSelectedMfFile = openViewResult.SelectedManifestFullPath;
			projectConfigurationManager.CurrentProjectConfiguraiton.UseVSFormatting = openViewResult.IsUseVSFormattingCode;
			projectConfigurationManager.CurrentProjectConfiguraiton.LastSelectedSearchTypeInOpenFromPackage = openViewResult.SelectedSearchType;
			projectConfigurationManager.CurrentProjectConfiguraiton.AddMethodInfo(methodInfo);
			projectConfigurationManager.Save(projectManager.ProjectConfigPath);
		}
	}
}
