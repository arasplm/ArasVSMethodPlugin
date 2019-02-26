//------------------------------------------------------------------------------
// <copyright file="MethodInfo.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Aras.VS.MethodPlugin.Code;

namespace Aras.VS.MethodPlugin.Configurations.ProjectConfigurations
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
			this.PackageName = methodInfo.PackageName;
			this.EventData = methodInfo.EventData;
			this.PartialClasses = new List<string>(methodInfo.PartialClasses);
			this.ExternalItems = new List<string>(methodInfo.ExternalItems);
			this.ExecutionAllowedToKeyedName = methodInfo.ExecutionAllowedToKeyedName;
			this.ExecutionAllowedToId = methodInfo.ExecutionAllowedToId;
			this.MethodPath = methodInfo.MethodPath;
		}

		public MethodInfo()
		{
			this.PartialClasses = new List<string>();
			this.ExternalItems = new List<string>();
		}

		public string InnovatorMethodId { get; set; }

		public string InnovatorMethodConfigId { get; set; }

		public string MethodName { get; set; }

		public string MethodType { get; set; }

		public string MethodLanguage { get; set; }

		public string MethodComment { get; set; }

		public string TemplateName { get; set; }

		public string PackageName { get; set; }

		public EventSpecificData EventData { get; set; }

		public List<string> PartialClasses { get; set; }

		public List<string> ExternalItems { get; set; }

		public string ExecutionAllowedToKeyedName { get; set; }

		public string ExecutionAllowedToId { get; set; }

		public string MethodPath { get; set; }
	}
}
