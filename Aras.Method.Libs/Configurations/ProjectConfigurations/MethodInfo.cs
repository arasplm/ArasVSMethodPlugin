//------------------------------------------------------------------------------
// <copyright file="MethodInfo.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;

namespace Aras.Method.Libs.Configurations.ProjectConfigurations
{
	public class MethodInfo
	{
		public MethodInfo(MethodInfo methodInfo)
		{
			this.InnovatorMethodId = methodInfo.InnovatorMethodId;
			this.InnovatorMethodConfigId = methodInfo.InnovatorMethodConfigId;
			this.MethodName = methodInfo.MethodName;
			this.MethodType = methodInfo.MethodType;
			this.MethodLanguage = methodInfo.MethodLanguage;
			this.MethodComment = methodInfo.MethodComment;
			this.TemplateName = methodInfo.TemplateName;
			this.Package = methodInfo.Package;
			this.EventData = methodInfo.EventData;
			this.ExecutionAllowedToKeyedName = methodInfo.ExecutionAllowedToKeyedName;
			this.ExecutionAllowedToId = methodInfo.ExecutionAllowedToId;
		}

		public MethodInfo()
		{

		}

		public string InnovatorMethodId { get; set; }

		public string InnovatorMethodConfigId { get; set; }

		public string MethodName { get; set; }

		public string MethodType { get; set; }

		public string MethodLanguage { get; set; }

		public string MethodComment { get; set; }

		public string TemplateName { get; set; }

		public PackageInfo Package { get; set; }

		public EventSpecificData EventData { get; set; }

		public string ExecutionAllowedToKeyedName { get; set; }

		public string ExecutionAllowedToId { get; set; }
	}
}
