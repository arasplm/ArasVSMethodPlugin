using Aras.Method.Libs.Templates;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Templates
{
	[TestFixture]
	public class TemplateMapperTests
	{
		[TestCase("CSharp", "CSharp")]
		[TestCase("CSharpInOut", "CSharpIO")]
		[TestCase("ConflictDetectionLocalRuleCSharp.version:1", "CDLocRule")]
		[TestCase("ConflictDetectionLocalRuleSimplifiedCSharp.version:1", "CDLRSimpl")]
		[TestCase("CSharp:Aras.Server.Core.AccessControl.EnvironmentAttributeMethod", "CoreAccess")]
		[TestCase("CSharp:Aras.Server.Core.Configurator", "CoreConfig")]
		[TestCase("CSharp:Aras.Server.Core.Configurator.ScopeStructureOutput", "CoreScope")]
		[TestCase("CSharp:Aras.TDF.ContentGenerator(Extended)", "TDFExtend")]
		[TestCase("CSharp:Aras.TDF.ContentGenerator(Strict)", "TDFStrict")]
		[TestCase("CSharp:Aras.TreeGridView.BuilderMethod", "TreeGrid")]
		[TestCase("CSharp:Aras.Server.Core.GraphNavigation.PropertyGetter", "GNPropGet")]
		[TestCase("CSharp:Aras.Server.Core.GraphNavigation.NodeValuesGetter", "GNNodeValGet")]
		[TestCase("CSharp:Aras.Server.Core.GraphNavigation.KeyStringGetter", "GNKeyStrGet")]
		public void GetAliasTemplateName_ShouldReturnExpectedValue(string fullName, string expected)
		{
			// Act
			string actual = TemplateMapper.GetAliasTemplateName(fullName);

			// Assert
			Assert.AreEqual(expected, actual);
		}
	}
}
