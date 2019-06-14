using System;
using System.Collections.Generic;
using System.Windows;
using Aras.IOM;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Authentication;

namespace Aras.VS.MethodPlugin.Tests.Authentication
{
	class AuthenticationManagerProxy : IAuthenticationManager
	{
		private string[] getBasesResult;
		private string getServerDatabaseNameResult;
		private string getServerUrlResult;
		private List<string> getUserIdentityListResult;
		private bool isLoginedForCurrentProjectResult;
		private bool loginResult;
		private bool tryWindowsLoginResult;
		private bool windowsLoginResult;

		public AuthenticationManagerProxy(IServerConnection serverConnection, Innovator innovator, InnovatorUser innovatorUser, IIOMWrapper iOMWrapperInstance)
		{
			this.ServerConnection = serverConnection;
			this.InnovatorInstance = innovator;
			this.InnovatorUser = innovatorUser;
			this.IOMWrapperInstance = iOMWrapperInstance;
		}

		public dynamic ServerConnection { get; private set; }

		public dynamic InnovatorInstance { get; private set; }

		public InnovatorUser InnovatorUser { get; private set; }

		public IIOMWrapper IOMWrapperInstance { get; private set; }

		public string[] GetBases(string innovatorURL, string projectFullName)
		{
			return getBasesResult;
		}

		public string GetServerDatabaseName()
		{
			return this.getServerDatabaseNameResult;
		}

		public string GetServerUrl()
		{
			return this.getServerUrlResult;
		}

		public List<string> GetUserIdentityList()
		{
			return this.getUserIdentityListResult;
		}

		public bool IsLoginedForCurrentProject(string projectName, ConnectionInfo lastConnectoinInfo)
		{
			return this.isLoginedForCurrentProjectResult;
		}

		public bool Login(string projectName, string projectFullName, string serverUrl, string databaseName, string login, string password, Window window)
		{
			return this.loginResult;
		}

		public void LogOut()
		{
			throw new NotImplementedException();
		}

		public bool TryWindowsLogin(string projectName, string projectFullName, string serverUrl, string databaseName)
		{
			return this.tryWindowsLoginResult;
		}

		public bool WindowsLogin(string projectName, string projectFullName, string serverUrl, string databaseName)
		{
			return this.windowsLoginResult;
		}

		internal void GetBases_SetResult(string[] result)
		{
			this.getBasesResult = result;
		}

		internal void GetServerDatabaseName_SetResult(string result)
		{
			this.getServerDatabaseNameResult = result;
		}

		internal void GetServerUrl_SetResult(string result)
		{
			this.getServerDatabaseNameResult = result;
		}

		internal void GetUserIdentityList_SetResult(List<string> result)
		{
			this.getUserIdentityListResult = result;
		}

		internal void IsLoginedForCurrentProject_SetResult(bool result)
		{
			this.isLoginedForCurrentProjectResult = result;
		}

		internal void TryWindowsLogin_SetResult(bool tryWindowsLoginResult)
		{
			this.tryWindowsLoginResult = tryWindowsLoginResult;
		}

		internal void WindowsLogin_SetResult(bool windowsLoginResult)
		{
			this.windowsLoginResult = windowsLoginResult;
		}
	}
}
