//------------------------------------------------------------------------------
// <copyright file="InnovatorUser.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace Aras.VS.MethodPlugin.Authentication
{
	public class InnovatorUser
	{
		public string userName;
		public string passwordHash;
		public string databaseName;
		public string serverUrl;
		public string serverName;
		public string keyedName;
		public string userId;
		public string aliasIdentityId;
		public string currentProjectName;

		public bool IsEmpty()
		{
			return string.IsNullOrEmpty(passwordHash);
		}
	}
}
