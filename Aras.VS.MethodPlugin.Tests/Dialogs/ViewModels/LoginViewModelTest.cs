using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aras.IOM;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using Aras.VS.MethodPlugin.Tests.Authentication;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Code
{
	class LoginViewModelTest
	{
		private LoginViewModel loginViewModel;
		private AuthenticationManagerProxy authenticationManager;
		private IProjectConfiguraiton projectConfiguraiton;

		private IServerConnection serverConnection;
		private Innovator innovator;
		private InnovatorUser innovatorUser;

		[SetUp]
		public void Setup()
		{
			this.innovatorUser = new InnovatorUser();
			this.serverConnection = Substitute.For<IServerConnection>();
			this.innovator = new Innovator(this.serverConnection);
			this.authenticationManager = new AuthenticationManagerProxy(this.serverConnection,
				this.innovator,
				this.innovatorUser,
				Substitute.For<IIOMWrapper>());

			List<ConnectionInfo> connectionInfo = new List<ConnectionInfo>();
			this.projectConfiguraiton = Substitute.For<IProjectConfiguraiton>();
			this.projectConfiguraiton.Connections.Returns(connectionInfo);

			this.loginViewModel = new LoginViewModel(this.authenticationManager,
				this.projectConfiguraiton,
				"testProjectName",
				"testProjectFullName");
		}

		[Test]
		public void Ctor_ThrowAuthenticationManagerArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				var loginViewModel = new LoginViewModel(null, this.projectConfiguraiton, string.Empty, string.Empty);
			});
		}

		[Test]
		public void Ctor_ThrowProjectConfiguraitonArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				var loginViewModel = new LoginViewModel(this.authenticationManager, null, string.Empty, string.Empty);
			});
		}

		[Test]
		public void SetSelectedDB_ShouldEnabledPasswordAndDisabledLogin()
		{
			//Arange
			this.authenticationManager.TryWindowsLogin_SetResult(false);

			//Act
			this.loginViewModel.SelectedDatabase = "selectedDB";

			//Assert
			Assert.IsFalse(this.loginViewModel.IsLoginEnabled);
			Assert.IsTrue(this.loginViewModel.IsPasswordEnabled);
		}

		[Test]
		public void SetSelectedDB_ShouldSetExpectedUserNameAndFieldsEnabled()
		{
			//Arange
			this.authenticationManager.TryWindowsLogin_SetResult(true);
			this.innovatorUser.userName = "TestUserName";

			//Act
			this.loginViewModel.SelectedDatabase = "selectedDB";

			//Assert
			Assert.AreEqual("TestUserName", this.loginViewModel.Login);
			Assert.IsTrue(this.loginViewModel.IsLoginEnabled);
			Assert.IsFalse(this.loginViewModel.IsPasswordEnabled);
		}

		[Test]
		public void SetSelectedUrl_ShouldSetExpectedDataBase()
		{
			//Arange
			string[] dataBases = new string[] { "DdName1", "DdName2" };
			this.authenticationManager.GetBases_SetResult(dataBases);

			//Act
			this.loginViewModel.SelectedUrl = "testUrl";

			//Assert
			Assert.AreEqual(dataBases, this.loginViewModel.Databases);
		}

		[Test]
		public void SetDatabases_ShouldSetFirstDBIntoSelectedDatabase()
		{
			//Arange
			string[] dataBases = new string[] { "DdName" };

			//Act
			this.loginViewModel.Databases = new ObservableCollection<string>(dataBases);

			//Assert
			Assert.AreEqual(dataBases.First(), this.loginViewModel.SelectedDatabase);
		}
	}
}
