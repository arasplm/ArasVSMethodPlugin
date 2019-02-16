//------------------------------------------------------------------------------
// <copyright file="ConnectionInfo.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.Configurations.ProjectConfigurations
{
	public class ConnectionInfo
	{
		public string ServerUrl { get; set; }

		public string Database { get; set; }

		public string Login { get; set; }

		public bool LastConnection { get; set; }
	}
}
