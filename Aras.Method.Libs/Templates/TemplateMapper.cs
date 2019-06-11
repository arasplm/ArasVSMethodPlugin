//------------------------------------------------------------------------------
// <copyright file="TemplateMapper.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.Method.Libs.Templates
{
	public static class TemplateMapper
	{
		private static readonly Dictionary<string, string> templateNameMapper = new Dictionary<string, string>
		{
			{"CSharp", "CSharp"},
			{"CSharpInOut", "CSharpIO"},
			{"ConflictDetectionLocalRuleCSharp.version:1", "CDLocRule"},
			{"ConflictDetectionLocalRuleSimplifiedCSharp.version:1", "CDLRSimpl"},
			{"CSharp:Aras.Server.Core.AccessControl.EnvironmentAttributeMethod", "CoreAccess"},
			{"CSharp:Aras.Server.Core.Configurator", "CoreConfig"},
			{"CSharp:Aras.Server.Core.Configurator.ScopeStructureOutput", "CoreScope"},
			{"CSharp:Aras.TDF.ContentGenerator(Extended)", "TDFExtend"},
			{"CSharp:Aras.TDF.ContentGenerator(Strict)", "TDFStrict"},
			{"CSharp:Aras.TreeGridView.BuilderMethod", "TreeGrid"},
			{"CSharp:Aras.Server.Core.GraphNavigation.PropertyGetter", "GNPropGet"},
			{"CSharp:Aras.Server.Core.GraphNavigation.NodeValuesGetter", "GNNodeValGet"},
			{"CSharp:Aras.Server.Core.GraphNavigation.KeyStringGetter", "GNKeyStrGet"}
		};

		public static string GetAliasTemplateName(string fullName)
		{
			return templateNameMapper[fullName];
		}

	}
}