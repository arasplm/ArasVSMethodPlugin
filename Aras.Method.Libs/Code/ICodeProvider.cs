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

		string LoadMethodCode(string methodFolderPath, string sourceCode);

		GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, string methodCode, bool useCodeFormatting);

		CodeInfo CreatePartialCodeItemInfo(MethodInfo methodInformation,
			string fileName,
			CodeElementType codeElementType,
			bool useVSFormatting,
			string serverMethodFolderPath,
			string selectedFolderPath,
			string methodName,
			TemplateLoader templateLoader,
			string MethodPath);

		CodeInfo CreateExternalCodeItemInfo(MethodInfo methodInformation,
			string fileName,
			CodeElementType codeElementType,
			bool useVSFormatting,
			string serverMethodFolderPath,
			string selectedFolderPath,
			string methodName,
			TemplateLoader templateLoader,
			string MethodPath);

		CodeInfo RemoveActiveNodeFromActiveDocument(Document activeDocument, SyntaxNode activeSyntaxNode, string serverMethodFolderPath);

		CodeInfo InsertActiveNodeToMainMethod(string mainMethodFullPath, string serverMethodFolderPath, SyntaxNode activeSyntaxNode, string activeDocumentPath);

		CodeInfo InsertActiveNodeToPartial(string partialfullPath, string serverMethodFolderPath, string methodName, SyntaxNode activeSyntaxNode, string activeDocumentMethodFullPath);

		CodeInfo InsertActiveNodeToExternal(string externalFullPath, string serverMethodFolderPath, string methodName, SyntaxNode activeSyntaxNode, string activeDocumentMethodFullPath);

		CodeInfo UpdateSourceCodeToInsertExternalItems(string methodFolderPath, string sourceCode, MethodInfo methodInformation);
	}
}
