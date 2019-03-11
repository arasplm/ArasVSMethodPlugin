using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;

namespace Aras.VS.MethodPlugin.Tests.Stubs
{
	public class AuthManagerStub : IAuthenticationManager
	{
		public dynamic ServerConnection => throw new NotImplementedException();

		public InnovatorUser InnovatorUser
		{
			get
			{
				return new InnovatorUser
				{
					userName = "admin"
				};
			}
		}

		public IIOMWrapper IOMWrapperInstance => throw new NotImplementedException();

		dynamic IAuthenticationManager.InnovatorInstance
		{
			get
			{
				dynamic expandoObejct = new ExpandoObject();
				expandoObejct.getNewID = new Func<dynamic>(() => { return String.Empty; });
				expandoObejct.applyAML = new Func<string, dynamic>((item) => { return new MethodItemStub(); });
				return expandoObejct;
			}
		}

		public string[] GetBases(string innovatorURL, string projectFullName)
		{
			throw new NotImplementedException();
		}

		public string GetServerDatabaseName()
		{
			return string.Empty;
		}

		public string GetServerUrl()
		{
			return string.Empty;
		}

		public List<string> GetUserIdentityList()
		{
			throw new NotImplementedException();
		}

		public bool IsLoginedForCurrentProject(string projectName, ConnectionInfo lastConnectoinInfo)
		{
			throw new NotImplementedException();
		}

		public bool Login(string projectName, string projectFullName, string serverUrl, string databaseName, string login, string password, Window window)
		{
			throw new NotImplementedException();
		}

		public void LogOut()
		{
			throw new NotImplementedException();
		}

		public bool TryWindowsLogin(string projectName, string projectFullName, string serverUrl, string databaseName)
		{
			throw new NotImplementedException();
		}

		public bool WindowsLogin(string projectName, string projectFullName, string serverUrl, string databaseName)
		{
			throw new NotImplementedException();
		}
	}
}
