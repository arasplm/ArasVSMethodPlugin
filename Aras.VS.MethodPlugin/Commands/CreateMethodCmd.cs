//------------------------------------------------------------------------------
// <copyright file="CreateMethodCmd.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Linq;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Aras.VS.MethodPlugin.Configurations;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	internal sealed class CreateMethodCmd : AuthenticationCommandBase
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
		private CreateMethodCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, IGlobalConfiguration userConfiguration) : base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory)
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
		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, IGlobalConfiguration userConfiguration)
		{
			Instance = new CreateMethodCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, userConfiguration);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args, IVsUIShell uiShell)
		{
			var project = projectManager.SelectedProject;
			var projectConfiguration = projectConfigurationManager.Load(projectManager.ProjectConfigPath);

			var templateLoader = new Templates.TemplateLoader();
			templateLoader.Load(projectManager.MethodConfigPath);

			PackageManager packageManager = new PackageManager(authManager);
			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(project.CodeModel.Language, projectConfiguration);

			var createView = dialogFactory.GetCreateView(uiShell, projectConfiguration, templateLoader, packageManager, projectManager, codeProvider, globalConfiguration);
			var createViewResult = createView.ShowDialog();
			if (createViewResult?.DialogOperationResult != true)
			{
				return;
			}

			GeneratedCodeInfo codeInfo = codeProvider.GenerateCodeInfo(createViewResult.SelectedTemplate, createViewResult.SelectedEventSpecificData, createViewResult.MethodName, createViewResult.UseRecommendedDefaultCode, createViewResult.SelectedUserCodeTemplate.Code, createViewResult.IsUseVSFormattingCode);
			projectManager.CreateMethodTree(codeInfo);

			string newInnovatorMethodId = authManager.InnovatorInstance.getNewID();
			var methodInfo = new MethodInfo()
			{
				InnovatorMethodConfigId = newInnovatorMethodId,
				InnovatorMethodId = newInnovatorMethodId,
				MethodLanguage = createViewResult.SelectedLanguage.Value,
				MethodName = createViewResult.MethodName,
				MethodType = createViewResult.SelectedActionLocation.Value,
				MethodComment = createViewResult.MethodComment,
				PackageName = createViewResult.SelectedPackage,
				TemplateName = createViewResult.SelectedTemplate.TemplateName,
				EventData = createViewResult.SelectedEventSpecificData.EventSpecificData,
				ExecutionAllowedToId = createViewResult.SelectedIdentityId,
				ExecutionAllowedToKeyedName = createViewResult.SelectedIdentityKeyedName,
				PartialClasses = codeInfo.PartialCodeInfoList.Select(pci => pci.Path).ToList(),
				ExternalItems = codeInfo.ExternalItemsInfoList.Select(pci => pci.Path).ToList()
			};

			projectConfiguration.AddMethodInfo(methodInfo);
			projectConfiguration.UseVSFormatting = createViewResult.IsUseVSFormattingCode;
			projectConfigurationManager.Save(projectManager.ProjectConfigPath, projectConfiguration);
		}
	}
}
