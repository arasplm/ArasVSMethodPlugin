//------------------------------------------------------------------------------
// <copyright file="ICodeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Microsoft.CodeAnalysis;

namespace Aras.Method.Libs.Code
{
	public interface ICodeProvider
	{
		string Language { get; }

		string LoadMethodCode(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath);

		GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert, bool useCodeFormatting, string defaultCodeTemplatesPath);

		GeneratedCodeInfo CreateWrapper(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useCodeFormatting, string defaultCodeTemplatesPath);

		GeneratedCodeInfo CreateMainNew(GeneratedCodeInfo generatedCodeInfo,
			TemplateInfo template,
			EventSpecificDataType eventData,
			string methodName,
			bool useAdvancedCode,
			string codeToInsert,
			string defaultCodeTemplatesPath);

		GeneratedCodeInfo CreatePartialClasses(GeneratedCodeInfo methodInfo);

		CodeInfo CreateCodeItemInfo(MethodInfo methodInformation,
			string fileName,
			CodeType codeType,
			CodeElementType codeElementType,
			bool useVSFormatting,
			string serverMethodFolderPath,
			string selectedFolderPath,
			string methodName,
			string MethodConfigPath,
			string MethodPath,
			string defaultCodeTemplatesPath);

		GeneratedCodeInfo CreateTestsNew(
			GeneratedCodeInfo generatedCodeInfo,
			TemplateInfo template, EventSpecificDataType
			eventData,
			string methodName,
			bool useAdvancedCode,
			string defaultCodeTemplatesPath);

		CodeInfo RemoveActiveNodeFromActiveDocument(Document activeDocument, SyntaxNode activeSyntaxNode, string serverMethodFolderPath);

		CodeInfo InsertActiveNodeToMainMethod(string mainMethodFullPath, string serverMethodFolderPath, SyntaxNode activeSyntaxNode, string activeDocumentPath);

		CodeInfo InsertActiveNodeToPartial(string partialfullPath, string serverMethodFolderPath, string methodName, SyntaxNode activeSyntaxNode, string activeDocumentMethodFullPath);

		CodeInfo InsertActiveNodeToExternal(string externalFullPath, string serverMethodFolderPath, string methodName, SyntaxNode activeSyntaxNode, string activeDocumentMethodFullPath);

		CodeInfo UpdateSourceCodeToInsertExternalItems(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath);
	}
}
