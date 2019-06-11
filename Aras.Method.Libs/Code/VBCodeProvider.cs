//------------------------------------------------------------------------------
// <copyright file="VBCodeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Microsoft.CodeAnalysis;

namespace Aras.Method.Libs.Code
{
	public class VBCodeProvider : ICodeProvider
	{
		public string Language
		{
			get { return "VB"; }
		}

		public VBCodeProvider()
		{

		}

		public CodeInfo CreateCodeItemInfo(MethodInfo methodInformation, string fileName, CodeType codeType, CodeElementType codeElementType, bool useVSFormatting, string serverMethodFolderPath, string selectedFolderPath, string methodName, string MethodConfigPath, string MethodPath, string defaultCodeTemplatesPath)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreateMainNew(GeneratedCodeInfo generatedCodeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert, string defaultCodeTemplatesPath)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreatePartialClasses(GeneratedCodeInfo methodInfo)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreateTestsNew(GeneratedCodeInfo generatedCodeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string defaultCodeTemplatesPath)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo CreateWrapper(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useCodeFormatting, string defaultCodeTemplatesPath)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert, bool useCodeFormatting, string defaultCodeTemplatesPath)
		{
			throw new NotImplementedException();
		}

		public CodeInfo InsertActiveNodeToExternal(string externalFullPath, string serverMethodFolderPath, string methodName, SyntaxNode activeSyntaxNode, string activeDocumentMethodFullPath)
		{
			throw new NotImplementedException();
		}

		public CodeInfo InsertActiveNodeToMainMethod(string mainMethodFullPath, string serverMethodFolderPath, SyntaxNode activeSyntaxNode, string activeDocumentPath)
		{
			throw new NotImplementedException();
		}

		public CodeInfo InsertActiveNodeToPartial(string partialfullPath, string serverMethodFolderPath, string methodName, SyntaxNode activeSyntaxNode, string activeDocumentMethodFullPath)
		{
			throw new NotImplementedException();
		}

		public string LoadMethodCode(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath)
		{
			throw new NotImplementedException();
		}

		public CodeInfo RemoveActiveNodeFromActiveDocument(Document activeDocument, SyntaxNode activeSyntaxNode, string serverMethodFolderPath)
		{
			throw new NotImplementedException();
		}

		public CodeInfo UpdateSourceCodeToInsertExternalItems(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath)
		{
			throw new NotImplementedException();
		}
	}
}
