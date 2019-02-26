//------------------------------------------------------------------------------
// <copyright file="GeneratedCodeInfo.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.Code
{
	public class GeneratedCodeInfo
	{
		public GeneratedCodeInfo(GeneratedCodeInfo codeInfo)
		{
			this.ClassName = codeInfo.ClassName;
			this.Namespace = codeInfo.Namespace;
			this.MethodName = codeInfo.MethodName;
			this.MethodCodeParentClassName = codeInfo.MethodCodeParentClassName;
			this.IsUseVSFormatting = codeInfo.IsUseVSFormatting;
			this.WrapperCodeInfo = new CodeInfo() { Code = codeInfo.WrapperCodeInfo.Code, Path = codeInfo.WrapperCodeInfo.Path };
			this.MethodCodeInfo = new CodeInfo() { Code = codeInfo.MethodCodeInfo.Code, Path = codeInfo.MethodCodeInfo.Path };
			this.PartialCodeInfoList = new List<CodeInfo>(codeInfo.PartialCodeInfoList);
			this.ExternalItemsInfoList = new List<CodeInfo>(codeInfo.ExternalItemsInfoList);
			this.TestsCodeInfo = new CodeInfo() { Code = codeInfo.TestsCodeInfo.Code, Path = codeInfo.TestsCodeInfo.Path };
		}

		public GeneratedCodeInfo()
		{
			this.WrapperCodeInfo = new CodeInfo();
			this.MethodCodeInfo = new CodeInfo();
			this.PartialCodeInfoList = new List<CodeInfo>();
			this.ExternalItemsInfoList = new List<CodeInfo>();
			this.TestsCodeInfo = new CodeInfo();
		}

		public string Namespace { get; set; }

		public string ClassName { get; set; }

		public string MethodName { get; set; }

		public string MethodCodeParentClassName { get; set; }


		public CodeInfo WrapperCodeInfo { get; set; }

		public CodeInfo MethodCodeInfo { get; set; }

		public List<CodeInfo> PartialCodeInfoList { get; set; }

		public List<CodeInfo> ExternalItemsInfoList { get; set; }

		public CodeInfo TestsCodeInfo { get; set; }

		public bool IsUseVSFormatting{ get; set; }
	}
}
