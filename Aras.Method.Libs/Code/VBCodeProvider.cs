//------------------------------------------------------------------------------
// <copyright file="VBCodeProvider.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
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

		public string LoadMethodCode(string methodFolderPath, string sourceCode)
		{
			throw new NotImplementedException();
		}

		public GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, string methodCode, bool useCodeFormatting)
		{
			throw new NotImplementedException();
		}

		public CodeInfo CreatePartialCodeItemInfo(MethodInfo methodInformation, string fileName, CodeElementType codeElementType, bool useVSFormatting, string methodFolderPath, string selectedFolderPath, string methodName, TemplateLoader templateLoader, string MethodPath)
		{
			throw new NotImplementedException();
		}

		public CodeInfo CreateExternalCodeItemInfo(MethodInfo methodInformation, string fileName, CodeElementType codeElementType, bool useVSFormatting, string methodFolderPath, string selectedFolderPath, string methodName, TemplateLoader templateLoader, string MethodPath)
		{
			throw new NotImplementedException();
		}

		public CodeInfo RemoveActiveNodeFromActiveDocument(Document activeDocument, SyntaxNode activeSyntaxNode, string serverMethodFolderPath)
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

		public CodeInfo InsertActiveNodeToExternal(string externalFullPath, string serverMethodFolderPath, string methodName, SyntaxNode activeSyntaxNode, string activeDocumentMethodFullPath)
		{
			throw new NotImplementedException();
		}

		public CodeInfo UpdateSourceCodeToInsertExternalItems(string methodFolderPath, string sourceCode, MethodInfo methodInformation, bool useVSFormatting)
		{
			throw new NotImplementedException();
		}
	}
}
