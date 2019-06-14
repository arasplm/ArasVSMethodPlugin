//------------------------------------------------------------------------------
// <copyright file="IProjectManager.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using EnvDTE;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.SolutionManagement
{
	public interface IProjectManager
	{
		string MethodConfigPath { get; }

		string IOMFilePath { get; }

		string ProjectFolderPath { get; }

		string ProjectConfigPath { get; }

		string ServerMethodFolderPath { get; }

		string MethodPath { get; }

		string MethodName { get; }

		Microsoft.CodeAnalysis.Document ActiveDocument { get; }

		string ActiveDocumentMethodFullPath { get; }

		string ActiveDocumentFilePath { get; }

		string ActiveDocumentMethodName { get; }

		string ActiveDocumentMethodFolderPath { get; }

		Microsoft.CodeAnalysis.SyntaxNode ActiveSyntaxNode { get; }

		string SelectedFilePath { get; }

		string SelectedFileName { get; }

		string SelectedFolderPath { get; }

		Project SelectedProject { get; }

		IVsUIShell UIShell { get; }

		ProjectItem ServerMethodsFolderItem { get; }

		ProjectItems ServerMethodFolderItems { get; }

		OleMenuCommandService CommandService { get; }

		VisualStudioWorkspace VisualStudioWorkspace { get; }

		bool IsArasProject { get; }

		bool SolutionHasProject{ get; }

		string Language { get; }

		bool IsCommandForMethod(Guid commandId);

		IEnumerable<string> GetSelectedFiles();

		string AddItemTemplateToProjectNew(CodeInfo codeInfo, string packagePath, bool openAfterCreation, int cursorIndex = -1);

		bool IsMethodExist(string packagePath, string methodName);

		bool IsFileExist(string path);

		void RemoveMethod(MethodInfo methodInfo);

		void CreateMethodTree(GeneratedCodeInfo generatedCodeInfo, PackageInfo packageInfo);

		bool SaveDirtyFiles(IDialogFactory dialogFactory, List<MethodInfo> methodInfos);

		void ExecuteCommand(string commandName);

		void AttachToProcess(System.Diagnostics.Process process);

		void AddSuppression(string suppressName, string ruleCategory, string ruleId, string scope = "", string target = "");
	}
}
