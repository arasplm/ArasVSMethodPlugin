//------------------------------------------------------------------------------
// <copyright file="ICodeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Templates;

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
		
	}
}
