//------------------------------------------------------------------------------
// <copyright file="SaveToArasCmd.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class SaveToArasCmd : AuthenticationCommandBase
	{
		/// <summary>
		/// Command ID.
		/// </summary>
		public const int ItemCommandId = 0x101;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid ItemCommandSet = CommandIds.SaveToAras;

		private static IVsPackageWrapper VsPackageWrapper;


		private SaveToArasCmd(IProjectManager projectManager, IAuthenticationManager authManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, MessageManager messageManager) : base(authManager, dialogFactory, projectManager, projectConfigurationManager, codeProviderFactory, messageManager)
		{
			if (projectManager.CommandService != null)
			{
				var itemCommandID = new CommandID(ItemCommandSet, ItemCommandId);
				var itemMenu = new OleMenuCommand(this.ExecuteCommand, itemCommandID);
				itemMenu.BeforeQueryStatus += CheckCommandAccessibility;

				projectManager.CommandService.AddCommand(itemMenu);
			}
		}

		/// <summary>
		/// Gets the instance of the command.
		/// </summary>
		public static SaveToArasCmd Instance
		{
			get;
			private set;
		}

		public static void Initialize(IProjectManager projectManager, IAuthenticationManager authManager,
			IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager,
			ICodeProviderFactory codeProviderFactory, MessageManager messageManager, IVsPackageWrapper vsPackageWrapper)
		{
			VsPackageWrapper = vsPackageWrapper;
			Instance = new SaveToArasCmd(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}

		public string GetUpdateAction(int lockStatus)
		{
			string action;

			switch (lockStatus)
			{
				case 0:
					action = "edit";
					break;
				case 1:
					action = "update";
					break;
				case 2:
					throw new Exception(this.messageManager.GetMessage("ItemHasBeenLockedBySomeone"));
				default:
					throw new Exception(this.messageManager.GetMessage("GetLockStatusError"));
			}

			return action;
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			var project = projectManager.SelectedProject;

			string selectedMethodPath = projectManager.MethodPath;
			string sourceCode = File.ReadAllText(selectedMethodPath, new UTF8Encoding(true));
			string selectedMethodName = Path.GetFileNameWithoutExtension(selectedMethodPath);

			string projectConfigPath = projectManager.ProjectConfigPath;

			MethodInfo methodInformation = projectConfigurationManager.CurrentProjectConfiguraiton.MethodInfos.FirstOrDefault(m => m.MethodName == selectedMethodName);
			if (methodInformation == null)
			{
				throw new Exception();
			}

			string packageMethodFolderPath = this.projectConfigurationManager.CurrentProjectConfiguraiton.UseCommonProjectStructure ? methodInformation.Package.MethodFolderPath : string.Empty;
			string methodWorkingFolder = Path.Combine(projectManager.ServerMethodFolderPath, packageMethodFolderPath, methodInformation.MethodName);

			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectManager.Language);

			CodeInfo codeItemInfo = codeProvider.UpdateSourceCodeToInsertExternalItems(methodWorkingFolder, sourceCode, methodInformation, projectConfigurationManager.CurrentProjectConfiguraiton.UseVSFormatting);
			if (codeItemInfo != null)
			{
				var dialogResult = dialogFactory.GetMessageBoxWindow().ShowDialog(messageManager.GetMessage("CouldNotInsertExternalItemsInsideOfMethodCodeSection"),
					messageManager.GetMessage("ArasVSMethodPlugin"),
					MessageButtons.OKCancel,
					MessageIcon.Question);
				if (dialogResult == MessageDialogResult.Cancel)
				{
					return;
				}

				projectManager.AddItemTemplateToProjectNew(codeItemInfo, packageMethodFolderPath, true, 0);
				sourceCode = codeItemInfo.Code;
			}

			string methodCode = codeProvider.LoadMethodCode(methodWorkingFolder, sourceCode);

			var packageManager = new PackageManager(authManager, this.messageManager);
			var saveView = dialogFactory.GetSaveToArasView(projectConfigurationManager, packageManager, methodInformation, methodCode, projectConfigPath, project.Name, project.FullName);
			var saveViewResult = saveView.ShowDialog();
			if (saveViewResult?.DialogOperationResult != true)
			{
				return;
			}

			TemplateLoader templateLoader = new TemplateLoader();
			templateLoader.Load(projectManager.MethodConfigPath);

			dynamic currentMethodItem = saveViewResult.MethodItem;

			if (!currentMethodItem.isError())
			{
				methodCode = saveViewResult.MethodCode;
				var template = templateLoader.Templates.FirstOrDefault(t => t.TemplateName == saveViewResult.TemplateName);
				if (template != null && !template.IsSupported)
				{
					if (!methodCode.Contains(string.Format("//MethodTemplateName={0}", template.TemplateName)))
					{
						methodCode = methodCode.Insert(0, string.Format("//MethodTemplateName={0};\r\n", template.TemplateName));
					}
				}

				currentMethodItem.setProperty("comments", saveViewResult.MethodComment);
				currentMethodItem.setProperty("method_code", methodCode);
				currentMethodItem.setProperty("name", saveViewResult.MethodName);
				currentMethodItem.setProperty("method_type", saveViewResult.MethodLanguage);
				currentMethodItem.setProperty("execution_allowed_to", saveViewResult.SelectedIdentityId);

				DTE dte = (DTE)VsPackageWrapper.GetGlobalService(typeof(DTE));
				if (dte.Debugger.CurrentMode == dbgDebugMode.dbgBreakMode)
				{
					var dialogResult = this.dialogFactory.GetMessageBoxWindow()
						.ShowDialog("Debug In Progress. Debugging will be terminated", "Debug In Progress", MessageButtons.OKCancel, MessageIcon.Warning);
					if (dialogResult == MessageDialogResult.OK)
					{
						dte.Debugger.Stop();
					}
					else
					{
						return;
					}
				}

				var action = GetUpdateAction(currentMethodItem.getLockStatus());
				currentMethodItem = currentMethodItem.apply(action);
			}
			else
			{
				methodCode = saveViewResult.MethodCode;
				var template = templateLoader.Templates.FirstOrDefault(t => t.TemplateName == saveViewResult.TemplateName);
				if (template != null && !template.IsSupported)
				{
					if (!methodCode.Contains(string.Format("//MethodTemplateName={0}", template.TemplateName)))
					{
						methodCode = methodCode.Insert(0, string.Format("//MethodTemplateName={0};\r\n", template.TemplateName));
					}
				}

				currentMethodItem = authManager.InnovatorInstance.newItem("Method", "add");
				currentMethodItem.setProperty("comments", saveViewResult.MethodComment);
				currentMethodItem.setProperty("method_code", methodCode);
				currentMethodItem.setProperty("name", saveViewResult.MethodName);
				currentMethodItem.setProperty("method_type", saveViewResult.MethodLanguage);
				currentMethodItem.setProperty("execution_allowed_to", saveViewResult.SelectedIdentityId);
				currentMethodItem = currentMethodItem.apply();
			}

			if (currentMethodItem.isError())
			{
				throw new Exception(currentMethodItem.getErrorString());
			}

			string newId = currentMethodItem.getID();
			string configId = currentMethodItem.getProperty("config_id");

			if (string.IsNullOrEmpty(saveViewResult.CurrentMethodPackage))
			{
				packageManager.AddPackageElementToPackageDefinition(configId, saveViewResult.MethodName, saveViewResult.SelectedPackageInfo.Name);
			}
			else
			{
				if (!string.Equals(saveViewResult.CurrentMethodPackage, saveViewResult.SelectedPackageInfo.ToString()))
				{
					packageManager.DeletePackageByElementIdFromPackageDefinition(configId);
					packageManager.AddPackageElementToPackageDefinition(configId, saveViewResult.MethodName, saveViewResult.SelectedPackageInfo.Name);
				}
			}

			if (methodInformation.MethodName == saveViewResult.MethodName &&
				methodInformation.Package.Name == saveViewResult.SelectedPackageInfo.Name)
			{
				methodInformation.InnovatorMethodConfigId = configId;
				methodInformation.InnovatorMethodId = newId;
				methodInformation.Package = saveViewResult.SelectedPackageInfo;
				methodInformation.ExecutionAllowedToKeyedName = saveViewResult.SelectedIdentityKeyedName;
				methodInformation.ExecutionAllowedToId = saveViewResult.SelectedIdentityId;
				methodInformation.MethodComment = saveViewResult.MethodComment;

				projectConfigurationManager.Save(projectConfigPath);
			}

			var messageBoxWindow = dialogFactory.GetMessageBoxWindow();
			messageBoxWindow.ShowDialog(this.messageManager.GetMessage("MethodSaved", saveViewResult.MethodName),
				string.Empty,
				MessageButtons.OK,
				MessageIcon.Information);
		}
	}
}
