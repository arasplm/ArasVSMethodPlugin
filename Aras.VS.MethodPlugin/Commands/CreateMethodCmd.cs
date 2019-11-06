//------------------------------------------------------------------------------
// <copyright file="CreateMethodCmd.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.IO;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Configurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class CreateMethodCmd : AuthenticationCommandBase
	{
		protected readonly IGlobalConfiguration globalConfiguration;

		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0103;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = CommandIds.CreateMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="CreateMethodCmd"/> class.
		/// Adds our command handlers for menu (commands must exist in the command table file)
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		private CreateMethodCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, IGlobalConfiguration userConfiguration, MessageManager messageManager) : base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory, messageManager)
		{
			this.globalConfiguration = userConfiguration ?? throw new ArgumentNullException(nameof(userConfiguration));

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
		public static CreateMethodCmd Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="package">Owner package, not null.</param>
		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, IGlobalConfiguration userConfiguration, MessageManager messageManager)
		{
			Instance = new CreateMethodCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, userConfiguration, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			TemplateLoader templateLoader = new TemplateLoader();
			templateLoader.Load(projectManager.MethodConfigPath);

			PackageManager packageManager = new PackageManager(authManager, this.messageManager);
			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectManager.Language);

			var createView = dialogFactory.GetCreateView(projectConfigurationManager.CurrentProjectConfiguraiton, templateLoader, packageManager, projectManager, codeProvider, globalConfiguration);
			var createViewResult = createView.ShowDialog();
			if (createViewResult?.DialogOperationResult != true)
			{
				return;
			}

			GeneratedCodeInfo codeInfo = codeProvider.GenerateCodeInfo(createViewResult.SelectedTemplate, createViewResult.SelectedEventSpecificData, createViewResult.MethodName, createViewResult.SelectedUserCodeTemplate.Code, createViewResult.IsUseVSFormattingCode);
			projectManager.CreateMethodTree(codeInfo, createViewResult.SelectedPackage);
			projectManager.AddSuppression("assembly: System.Diagnostics.CodeAnalysis.SuppressMessage", "Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", "namespace", codeInfo.Namespace);

			string newInnovatorMethodId = authManager.InnovatorInstance.getNewID();
			var methodInfo = new MethodInfo()
			{
				InnovatorMethodConfigId = newInnovatorMethodId,
				InnovatorMethodId = newInnovatorMethodId,
				MethodLanguage = createViewResult.SelectedLanguage.Value,
				MethodName = createViewResult.MethodName,
				MethodType = createViewResult.SelectedActionLocation.Value,
				MethodComment = createViewResult.MethodComment,
				Package = createViewResult.SelectedPackage,
				TemplateName = createViewResult.SelectedTemplate.TemplateName,
				EventData = createViewResult.SelectedEventSpecificData.EventSpecificData,
				ExecutionAllowedToId = createViewResult.SelectedIdentityId,
				ExecutionAllowedToKeyedName = createViewResult.SelectedIdentityKeyedName
			};

			projectConfigurationManager.CurrentProjectConfiguraiton.AddMethodInfo(methodInfo);
			projectConfigurationManager.CurrentProjectConfiguraiton.UseVSFormatting = createViewResult.IsUseVSFormattingCode;
			projectConfigurationManager.Save(projectManager.ProjectConfigPath);
		}
	}
}
