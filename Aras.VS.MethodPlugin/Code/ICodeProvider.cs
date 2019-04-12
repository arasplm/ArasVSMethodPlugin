//------------------------------------------------------------------------------
// <copyright file="ICodeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Aras.VS.MethodPlugin.Code
{
	public interface ICodeProvider
	{
		string Language { get; }

		string LoadMethodCode(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath);

		GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert, bool useCodeFormatting);

		GeneratedCodeInfo CreateWrapper(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useCodeFormatting);

		GeneratedCodeInfo CreateMainNew(GeneratedCodeInfo generatedCodeInfo,
			TemplateInfo template,
			EventSpecificDataType eventData,
			string methodName,
			bool useAdvancedCode,
			string codeToInsert);

		GeneratedCodeInfo CreatePartialClasses(GeneratedCodeInfo methodInfo);

		CodeInfo CreateCodeItemInfo(MethodInfo methodInfo, string fileName, CodeType codeType, CodeElementType codeElementType, bool isUseVSFormattingCode);

		GeneratedCodeInfo CreateTestsNew(GeneratedCodeInfo generatedCodeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode);

		CodeInfo RemoveActiveNodeFromActiveDocument(Document activeDocument, SyntaxNode activeSyntaxNode, string serverMethodFolderPath);

		CodeInfo InsertActiveNodeToMainMethod(string mainMethodFullPath, string serverMethodFolderPath, SyntaxNode activeSyntaxNode, string activeDocumentPath);

		CodeInfo InsertActiveNodeToPartial(string partialfullPath, string serverMethodFolderPath, string methodName, SyntaxNode syntaxNode);

		CodeInfo InsertActiveNodeToExternal(string externalFullPath, string serverMethodFolderPath, string methodName, SyntaxNode syntaxNode);

		CodeInfo UpdateSourceCodeToInsertExternalItems(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath);
	}
}
