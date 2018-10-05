using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.Templates
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
            {"CSharp:Aras.TreeGridView.BuilderMethod", "TreeGrid"}
        };

        public static string GetAliasTemplateName(string fullName)
        {
            return templateNameMapper[fullName];}

    }
}