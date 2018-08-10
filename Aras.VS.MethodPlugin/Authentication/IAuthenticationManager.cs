//------------------------------------------------------------------------------
// <copyright file="IAuthenticationManager.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Windows;
using Aras.VS.MethodPlugin.ProjectConfigurations;

namespace Aras.VS.MethodPlugin.Authentication
{
	public interface IAuthenticationManager
	{
		dynamic ServerConnection { get; }

		dynamic InnovatorInstance { get; }

		InnovatorUser InnovatorUser { get; }

		IOMWrapper IOMWrapperInstance { get; }

		string[] GetBases(string innovatorURL, string projectFullName);

		bool IsLoginedForCurrentProject(string projectName, ConnectionInfo lastConnectoinInfo);

		bool TryWindowsLogin(string projectName, string projectFullName, string serverUrl, string databaseName);

		bool WindowsLogin(string projectName, string projectFullName, string serverUrl, string databaseName);

		bool Login(string projectName, string projectFullName, string serverUrl, string databaseName, string login, string password, Window window);

		void LogOut();

		string GetServerUrl();

		string GetServerDatabaseName();

		List<string> GetUserIdentityList();
	}
}
