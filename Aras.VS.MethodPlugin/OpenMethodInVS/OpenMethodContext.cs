//------------------------------------------------------------------------------
// <copyright file="OpenMethodContext.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Aras.Method.Libs.Configurations.ProjectConfigurations;

namespace Aras.VS.MethodPlugin.OpenMethodInVS
{
	public class OpenMethodContext : EventArgs
	{
		public OpenMethodContext(string arasInnovatorVersion, string methodId, ConnectionInfo connectionInfo)
		{
			this.ArasInnovatorVersion = arasInnovatorVersion ?? throw new ArgumentNullException(nameof(arasInnovatorVersion));
			this.MethodId = methodId ?? throw new ArgumentNullException(nameof(methodId));
			this.ConnectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));
		}

		public string ArasInnovatorVersion { get; }

		public string MethodId { get; }

		public ConnectionInfo ConnectionInfo { get; }
	}
}
