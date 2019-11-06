//------------------------------------------------------------------------------
// <copyright file="XmlMethodInfo.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.Method.Libs.Code
{
	public class XmlMethodInfo
	{
		public string Path { get; set; }

		public string MethodName { get; set; }

		public string MethodType { get; internal set; }

		public string Code { get; set; }
	}
}
