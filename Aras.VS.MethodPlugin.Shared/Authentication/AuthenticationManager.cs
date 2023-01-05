//------------------------------------------------------------------------------
// <copyright file="AuthenticationManager.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Windows;
using Aras.Method.Libs;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.SolutionManagement;

namespace Aras.VS.MethodPlugin.Authentication
{
	public class AuthenticationManager : IAuthenticationManager
	{
		private readonly MessageManager messageManager;
		private readonly IProjectManager projectManager;
		private readonly IProjectConfigurationManager projectConfigurationManager;

		private dynamic serverConnection;
		private dynamic innovator;
		private InnovatorUser innovatorUser;
		private IIOMWrapper iOMWrapper;

		public AuthenticationManager(MessageManager messageManager, IProjectManager projectManager)
		{
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
			this.projectManager = projectManager ?? throw new ArgumentNullException(nameof(projectManager));
		}

		public dynamic InnovatorInstance
		{
			get
			{
				return innovator;
			}
		}

		public dynamic ServerConnection
		{
			get
			{
				return serverConnection;
			}
		}

		public InnovatorUser InnovatorUser
		{
			get
			{
				return innovatorUser;
			}
			private set
			{
				innovatorUser = value;
			}
		}

		public IIOMWrapper IOMWrapperInstance
		{
			get
			{
				return iOMWrapper;
			}
		}

		public string[] GetBases(string innovatorURL, string projectFullName)
		{
			try
			{
				LoadIOMWrapper(projectFullName);

				dynamic connection = IOMWrapperInstance.IomFactory_CreateHttpServerConnection(innovatorURL);
				return connection.GetDatabases();
			}
			catch
			{
				return new string[0];
			}
		}

		public string GetServerUrl()
		{
			return InnovatorUser.serverUrl;
		}

		public string GetServerDatabaseName()
		{
			return InnovatorUser.databaseName;
		}

		public List<string> GetUserIdentityList()
		{
			return new List<string>();
		}

		public bool IsLoginedForCurrentProject(string projectName, ConnectionInfo lastConnectoinInfo)
		{
			if (InnovatorUser != null && lastConnectoinInfo != null)
			{
				if (InnovatorUser.currentProjectName == projectName
						&& InnovatorUser.serverUrl == lastConnectoinInfo.ServerUrl
						&& InnovatorUser.databaseName == lastConnectoinInfo.Database
						&& InnovatorUser.userName == lastConnectoinInfo.Login)
				{
					return true;
				}
				else
				{
					LogOut();
					return false;
				}
			}

			return false;
		}

		public bool TryWindowsLogin(string projectName, string projectFullName, string serverUrl, string databaseName)
		{
			LoadIOMWrapper(projectFullName);

			try
			{
				var сonnection = IOMWrapperInstance.IomFactory_CreateWinAuthHttpServerConnection(serverUrl, databaseName);
				var loginItem = сonnection.Login();

				if (!loginItem.isError())
				{
					сonnection.Logout();

					InnovatorUser = new InnovatorUser()
					{
						userName = Environment.UserName
					};

					return true;
				}
			}
			catch (Exception ex)
			{

			}

			return false;
		}

		public bool WindowsLogin(string projectName, string projectFullName, string serverUrl, string databaseName)
		{
			LoadIOMWrapper(projectFullName);

			try
			{
				serverConnection = IOMWrapperInstance.IomFactory_CreateWinAuthHttpServerConnection(serverUrl, databaseName);
				var loginItem = serverConnection.Login();
				if (!loginItem.isError())
				{
					innovator = IOMWrapperInstance.Innovator_Ctor(serverConnection);

					InnovatorUser = new InnovatorUser()
					{
						currentProjectName = projectName,
						serverUrl = serverUrl,
						databaseName = databaseName,
						userName = Environment.UserName
					};

					return true;
				}
			}
			catch (Exception ex)
			{

			}

			return false;
		}

		public bool Login(string projectName, string projectFullName, string serverUrl, string databaseName, string login, string password, Window window)
		{
			LoadIOMWrapper(projectFullName);

			bool isWindowsLogin = WindowsLogin(projectName, projectFullName, serverUrl, databaseName);
			if (isWindowsLogin)
			{
				return true;
			}

			string passwordHash = IOMWrapperInstance.Innovator_ScalcMD5(password);
			serverConnection = IOMWrapperInstance.IomFactory_CreateHttpServerConnection(serverUrl, databaseName, login, passwordHash);

			var loginItem = serverConnection.Login();
			if (loginItem.isError())
			{
				var messageWindow = new MessageBoxWindow();
				string message = loginItem.getErrorString();
				if (message.Contains("Authentication failed for"))
				{
					message = string.Format(this.messageManager.GetMessage("AuthenticationFailedFor"), login);
				}

				messageWindow.ShowDialog(window, message, string.Empty, Dialogs.MessageButtons.OK, Dialogs.MessageIcon.None);

				return false;
			}

			innovator = IOMWrapperInstance.Innovator_Ctor(serverConnection);
			InnovatorUser = new InnovatorUser()
			{
				currentProjectName = projectName,
				serverUrl = serverUrl,
				databaseName = databaseName,
				userName = login,
				passwordHash = passwordHash
			};

			//Login succesed
			return true;
		}

		public void LogOut()
		{
			if (serverConnection != null)
			{
				serverConnection.Logout();
			}

			InnovatorUser = null;
			innovator = null;
		}

		private void LoadIOMWrapper(string projectFullName)
		{
			if (iOMWrapper == null || iOMWrapper.ProjectFullName != projectFullName)
			{
				iOMWrapper = new IOMWrapper(messageManager, projectFullName, projectManager.IOMFilePath);
			}
		}
	}
}
