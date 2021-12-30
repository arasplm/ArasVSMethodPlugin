//------------------------------------------------------------------------------
// <copyright file="OpenContextParser.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Aras.Method.Libs.Configurations.ProjectConfigurations;

namespace Aras.VS.MethodPlugin.OpenMethodInVS
{
	public class OpenContextParser : IOpenContextParser
	{
		public OpenMethodContext Parse(string openRequestString)
		{
			if (string.IsNullOrEmpty(openRequestString))
			{
				return null;
			}

			string[] parameterStrings = openRequestString.Replace($"{OpenInVSConstants.ProtocolName}://", "").Trim('/').Split('&');
			if (parameterStrings.Length < 2)
			{
				return null;
			}

			Dictionary<string, string> parameters = new Dictionary<string, string>();
			foreach (string parameterString in parameterStrings)
			{
				string[] parameterPair = parameterString.Split('=');
				if (parameterPair.Length == 2)
				{
					parameters.Add(parameterPair[0], parameterPair[1]);
				}
			}

			parameters.TryGetValue(OpenInVSConstants.ServerUrlParam, out string serverUrl);
			parameters.TryGetValue(OpenInVSConstants.DatabaseParam, out string database);
			parameters.TryGetValue(OpenInVSConstants.UserNameParam, out string userName);
			parameters.TryGetValue(OpenInVSConstants.ArasVersionParam, out string arasVersion);
			parameters.TryGetValue(OpenInVSConstants.MethodIdParam, out string methodId);

			if (string.IsNullOrEmpty(arasVersion) ||
				string.IsNullOrEmpty(methodId) ||
				string.IsNullOrEmpty(userName) ||
				string.IsNullOrEmpty(database) ||
				string.IsNullOrEmpty(serverUrl))
			{
				return null;
			}

			ConnectionInfo connectionInfo = new ConnectionInfo
			{
				ServerUrl = serverUrl,
				Database = database,
				Login = userName,
				LastConnection = true
			};

			return new OpenMethodContext(arasVersion, methodId, connectionInfo);
		}
	}
}
