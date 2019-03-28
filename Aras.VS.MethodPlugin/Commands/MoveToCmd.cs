using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.Commands
{
	/// <summary>
	/// Command handler
	/// </summary>
	public sealed class MoveToCmd : CmdBase
	{
		private readonly ICodeProviderFactory codeProviderFactory;

		/// <summary>
		/// Command ID.
		/// </summary>
		public const int CommandId = 0x0105;

		/// <summary>
		/// Command menu group (command set GUID).
		/// </summary>
		public static readonly Guid CommandSet = CommandIds.MoveTo;

		private MoveToCmd(IProjectManager projectManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, IMessageManager messageManager)
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
		public static MoveToCmd Instance
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes the singleton instance of the command.
		/// </summary>
		/// <param name="projectManager"></param>
		/// <param name="dialogFactory"></param>
		/// <param name="projectConfigurationManager"></param>
		/// <param name="codeProviderFactory"></param>
		public static void Initialize(IProjectManager projectManager, IDialogFactory dialogFactory, IProjectConfigurationManager projectConfigurationManager, ICodeProviderFactory codeProviderFactory, IMessageManager messageManager)
		{
			Instance = new MoveToCmd(projectManager, dialogFactory, projectConfigurationManager, codeProviderFactory, messageManager);
		}

		public override void ExecuteCommandImpl(object sender, EventArgs args)
		{
			Document activeDocument = projectManager.ActiveDocument;
			SyntaxNode activeSyntaxNode = projectManager.ActiveSyntaxNode;
			string activeDocumentMethodName = projectManager.ActiveDocumentMethodName;
			string activeDocumentMethodFullPath = projectManager.ActiveDocumentMethodFullPath;
			string activeDocumentMethodFolderPath = projectManager.ActiveDocumentMethodFolderPath;
			string serverMethodFolderPath = projectManager.ServerMethodFolderPath;

			string projectConfigPath = projectManager.ProjectConfigPath;
			IProjectConfiguraiton projectConfiguration = projectConfigurationManager.Load(projectConfigPath);

			if (string.Equals(activeDocument.FilePath, activeDocumentMethodFullPath, StringComparison.InvariantCultureIgnoreCase))
			{
				// main to external
				var moveToViewAdapter = this.dialogFactory.GetMoveToView(activeDocumentMethodFolderPath, activeSyntaxNode);
				var moveToViewResult = moveToViewAdapter.ShowDialog();
				if (moveToViewResult.DialogOperationResult == false)
				{
					return;
				}

				ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(activeDocument.Project.Language, projectConfiguration);
				CodeInfo activeDocumentCodeInfo = codeProvider.RemoveActiveNodeFromActiveDocument(activeDocument, activeSyntaxNode, serverMethodFolderPath);
				CodeInfo itemCodeInfo = null;

				if (moveToViewResult.SelectedCodeType == CodeType.Partial)
				{
					itemCodeInfo = codeProvider.InsertActiveNodeToPartial(moveToViewResult.SelectedFullPath, serverMethodFolderPath, activeDocumentMethodName, activeSyntaxNode);
				}
				else
				{
					itemCodeInfo = codeProvider.InsertActiveNodeToExternal(moveToViewResult.SelectedFullPath, serverMethodFolderPath, activeDocumentMethodName, activeSyntaxNode);
				}

				projectManager.AddItemTemplateToProjectNew(activeDocumentCodeInfo, false);
				projectManager.AddItemTemplateToProjectNew(itemCodeInfo, false);

				MethodInfo methodInformation = projectConfiguration.MethodInfos.FirstOrDefault(m => m.MethodName == activeDocumentMethodName);
				if (moveToViewResult.SelectedCodeType == CodeType.Partial)
				{
					bool nodePathExists = methodInformation.PartialClasses.Exists(x => string.Equals(x, itemCodeInfo.Path, StringComparison.InvariantCultureIgnoreCase));
					if (!nodePathExists)
					{
						methodInformation.PartialClasses.Add(itemCodeInfo.Path);
					}
				}
				else
				{
					bool nodePathExists = methodInformation.ExternalItems.Exists(x => string.Equals(x, itemCodeInfo.Path, StringComparison.InvariantCultureIgnoreCase));
					if (!nodePathExists)
					{
						methodInformation.ExternalItems.Add(itemCodeInfo.Path);
					}
				}

				projectConfigurationManager.Save(projectConfigPath, projectConfiguration);
			}
			else
			{
				// external to main
				var messageBoxWindow = dialogFactory.GetMessageBoxWindow();
				var messageBoxWindowResult = messageBoxWindow.ShowDialog(
					this.messageManager.GetMessage("SelectedCodeWillBeMovedToMainMethodFileClickOKToContinue"),
					this.messageManager.GetMessage("MoveToMainMethod"),
					MessageButtons.OKCancel,
					MessageIcon.Question);

				if (messageBoxWindowResult == MessageDialogResult.OK)
				{
					ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(activeDocument.Project.Language , projectConfiguration);
					CodeInfo activeDocumentCodeInfo = codeProvider.RemoveActiveNodeFromActiveDocument(activeDocument, activeSyntaxNode, serverMethodFolderPath);
					CodeInfo methodDocumentCodeInfo = codeProvider.InsertActiveNodeToMainMethod(activeDocumentMethodFullPath, serverMethodFolderPath, activeSyntaxNode, activeDocument.FilePath);
					projectManager.AddItemTemplateToProjectNew(activeDocumentCodeInfo, false);
					projectManager.AddItemTemplateToProjectNew(methodDocumentCodeInfo, false);
				}
			}
		}

		protected override void CheckCommandAccessibility(object sender, EventArgs e)
		{
			OleMenuCommand command = (OleMenuCommand)sender;
			if (!projectManager.IsArasProject | string.IsNullOrEmpty(projectManager.ActiveDocumentMethodName))
			{
				command.Enabled = false;
				return;
			}

			SyntaxNode activeSyntaxNode = projectManager.ActiveSyntaxNode;

			if (activeSyntaxNode is TypeDeclarationSyntax typeDeclarationNode)
			{
				command.Enabled = !typeDeclarationNode.Modifiers.Any(SyntaxKind.PartialKeyword);
			}
			else
			{
				command.Enabled = activeSyntaxNode is MethodDeclarationSyntax || activeSyntaxNode is EnumDeclarationSyntax;
			}
		}
	}
}
