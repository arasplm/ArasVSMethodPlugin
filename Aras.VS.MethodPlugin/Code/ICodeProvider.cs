//------------------------------------------------------------------------------
// <copyright file="ICodeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.Templates;

namespace Aras.VS.MethodPlugin.Code
{
	public interface ICodeProvider
	{
		string Language { get; }

		void LoadCodeToProject(string methodLanguage, string methodCode, string methodLocation, string methodName, string innovatorMethodConfigId, string innovatorMethodId, EventSpecificDataType eventData, string packageName, string executionAllowedToId, string executionAllowedToKeyedName);

		//void LoadCodeToProject(dynamic methodItem, EventSpecificDataType eventData, string packageName);

		string LoadMethodCode(string sourceCode, MethodInfo methodInformation, string serverMethodFolderPath);


		GeneratedCodeInfo GenerateCodeInfo(TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode, string codeToInsert);

		GeneratedCodeInfo CreateWrapper(TemplateInfo template, EventSpecificDataType eventData, string methodName);

		GeneratedCodeInfo CreateMainNew(GeneratedCodeInfo generatedCodeInfo,
			TemplateInfo template,
			EventSpecificDataType eventData,
			string methodName,
			bool useAdvancedCode,
			string codeToInsert);

		GeneratedCodeInfo CreatePartialClasses(GeneratedCodeInfo methodInfo);

		CodeInfo CreatePartialCodeInfo(MethodInfo methodInfo, string fileName);

		GeneratedCodeInfo CreateTestsNew(GeneratedCodeInfo generatedCodeInfo, TemplateInfo template, EventSpecificDataType eventData, string methodName, bool useAdvancedCode);
	}
}
