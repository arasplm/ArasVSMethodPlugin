//------------------------------------------------------------------------------
// <copyright file="ConnectionInfo.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.Method.Libs.Configurations.ProjectConfigurations
{
	public class ConnectionInfo
	{
		public string ServerUrl { get; set; }

		public string Database { get; set; }

		public string Login { get; set; }

		public bool LastConnection { get; set; }
	}
}
