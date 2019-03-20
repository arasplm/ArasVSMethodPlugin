//------------------------------------------------------------------------------
// <copyright file="IProjectManager.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using EnvDTE;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Aras.VS.MethodPlugin.SolutionManagement
{
	public interface IProjectManager
	{
		string ProjectConfigPath { get; }

		string MethodConfigPath { get; }

		string DefaultCodeTemplatesPath { get; }

		string ServerMethodFolderPath { get; }

		string MethodPath { get; }

		string MethodName { get; }

		Microsoft.CodeAnalysis.Document ActiveDocument { get; }

		string ActiveDocumentMethodFullPath { get; }

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

		bool IsCommandForMethod(Guid commandId);

		IEnumerable<string> GetSelectedFiles();

		string AddItemTemplateToProjectNew(CodeInfo codeInfo, bool openAfterCreation, int cursorIndex = -1);

		bool IsMethodExist(string methodName);

		bool IsFileExist(string path);

		void RemoveMethod(MethodInfo methodInfo);

		void CreateMethodTree(GeneratedCodeInfo generatedCodeInfo);

		bool SaveDirtyFiles(List<MethodInfo> methodInfos);

		void ExecuteCommand(string commandName);

		void AttachToProcess(System.Diagnostics.Process process);

		void AddSuppression(string suppressName, string ruleCategory, string ruleId, string scope = "", string target = "");
	}
}
