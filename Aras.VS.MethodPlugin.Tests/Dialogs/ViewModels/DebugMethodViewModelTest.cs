using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using Aras.VS.MethodPlugin.Tests.Dialogs.SubAdapters;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.ViewModels
{
	[TestFixture]
	class DebugMethodViewModelTest
	{
		private string methodCode;
		private List<ConnectionInfo> connectionInfos;

		private IAuthenticationManager authenticationManager;
		private IProjectConfigurationManager projectConfigurationManager;
		private IDialogFactory dialogFactory;
		private MethodInfo methodInformation;

		[OneTimeSetUp]
		public void LoadMethodInfo()
		{
			string currentPath = AppDomain.CurrentDomain.BaseDirectory;
			string methodFullPath = Path.Combine(currentPath, @"Dialogs\ViewModels\TestData\TestMethodItem.xml");

			XmlDocument testMethodItemXMLDoc = new XmlDocument();
			testMethodItemXMLDoc.Load(methodFullPath);

			this.methodCode = testMethodItemXMLDoc.SelectSingleNode("//method_code").InnerText;
			this.methodInformation = new MethodInfo()
			{
				InnovatorMethodConfigId = testMethodItemXMLDoc.SelectSingleNode("//config_id").InnerText,
				MethodType = "server",
				MethodName = testMethodItemXMLDoc.SelectSingleNode("//name").InnerText,
				MethodLanguage = "C#",
				EventData = EventSpecificData.None
			};
		}

		[SetUp]
		public void Setup()
		{
			this.authenticationManager = Substitute.For<IAuthenticationManager>();
			IProjectConfiguraiton projectConfiguration = Substitute.For<IProjectConfiguraiton>();
			this.projectConfigurationManager = Substitute.For<IProjectConfigurationManager>();
			this.projectConfigurationManager.CurrentProjectConfiguraiton.Returns(projectConfiguration);
			this.dialogFactory = Substitute.For<IDialogFactory>();
			this.methodInformation = new MethodInfo();

			this.connectionInfos = new List<ConnectionInfo>()
			{
				new ConnectionInfo()
				{
					Login = "userName",
					Database = "testDataBase",
					ServerUrl = "testServerUrl",
					LastConnection = true
				}
			};

			projectConfiguration.Connections.Returns(this.connectionInfos);
			
		}

		[Test]
		public void Ctor_ShouldAuthenticationManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				DebugMethodViewModel debugMethodViewModel = new DebugMethodViewModel(null,
					this.projectConfigurationManager,
					this.methodInformation,
					this.dialogFactory,
					"testMethodCode",
					"testProjectConfigPath",
					"testProjectName",
					"testProjectFullName");
			});
		}

		[Test]
		public void Ctor_ShouldProjectConfigurationManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				DebugMethodViewModel debugMethodViewModel = new DebugMethodViewModel(this.authenticationManager,
					null,
					this.methodInformation,
					this.dialogFactory,
					"testMethodCode",
					"testProjectConfigPath",
					"testProjectName",
					"testProjectFullName");
			});
		}

		[Test]
		public void Ctor_ShouldMethodInfoThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				DebugMethodViewModel debugMethodViewModel = new DebugMethodViewModel(this.authenticationManager,
					this.projectConfigurationManager,
					null,
					this.dialogFactory,
					"testMethodCode",
					"testProjectConfigPath",
					"testProjectName",
					"testProjectFullName");
			});
		}

		[Test]
		public void Ctor_ShouldDialogFactoryThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				DebugMethodViewModel debugMethodViewModel = new DebugMethodViewModel(this.authenticationManager,
					this.projectConfigurationManager,
					this.methodInformation,
					null,
					"testMethodCode",
					"testProjectConfigPath",
					"testProjectName",
					"testProjectFullName");
			});
		}

		[Test]
		public void Ctor_ShouldInitExpectedProperty()
		{
			//Arange
			string expectedMethodContext = $"<Item action=\"{methodInformation.MethodName}\" type=\"Method\" />";

			//Act
			DebugMethodViewModel debugMethodViewModel = new DebugMethodViewModel(authenticationManager,
				this.projectConfigurationManager,
				this.methodInformation,
				this.dialogFactory,
				this.methodCode,
				"testProjectConfigPath",
				"testProjectName",
				"testProjectFullName");

			//Assert
			Assert.AreEqual(this.methodCode, debugMethodViewModel.MethodCode);
			Assert.AreEqual(expectedMethodContext, debugMethodViewModel.MethodContext);
			Assert.AreEqual(this.methodInformation.MethodType, debugMethodViewModel.MethodType);
			Assert.AreEqual(this.methodInformation.MethodLanguage, debugMethodViewModel.MethodLanguage);
			Assert.AreEqual(this.methodInformation.EventData.ToString(), debugMethodViewModel.SelectedEventSpecificData);
			Assert.AreEqual(this.methodInformation.MethodName, debugMethodViewModel.MethodName);
			Assert.AreEqual(this.connectionInfos.First(), debugMethodViewModel.ConnectionInformation);
		}

		[Test]
		public void EditConnectionInfoCommandExecute_ShouldLeaveStartConnection()
		{
			//Arange
			string projectConfigPath = "testProjectConfigPath";
			string projectName = "testProjectName";
			string projectFullName = "testProjectFullName";
			DebugMethodViewModel debugMethodViewModel = new DebugMethodViewModel(authenticationManager,
				this.projectConfigurationManager,
				this.methodInformation,
				this.dialogFactory,
				this.methodCode,
				projectConfigPath,
				projectName,
				projectFullName);

			LoginViewTestAdapter adapter = new LoginViewTestAdapter(false);
			this.dialogFactory.GetLoginView(this.projectConfigurationManager.CurrentProjectConfiguraiton, projectName, projectFullName).Returns(adapter);

			//Act
			debugMethodViewModel.EditConnectionInfoCommand.Execute(null);

			//Assert
			Assert.AreEqual(this.connectionInfos.First(), debugMethodViewModel.ConnectionInformation);
		}

		[Test]
		public void EditConnectionInfoCommandExecute_ShouldSetNewConnectionInConnectionInformation()
		{
			//Arange
			string projectConfigPath = "testProjectConfigPath";
			string projectName = "testProjectName";
			string projectFullName = "testProjectFullName";

			DebugMethodViewModel debugMethodViewModel = new DebugMethodViewModel(authenticationManager,
				this.projectConfigurationManager,
				this.methodInformation,
				this.dialogFactory,
				this.methodCode,
				projectConfigPath,
				projectName,
				projectFullName);

			ConnectionInfo newConnection = new ConnectionInfo()
			{
				Login = "newUserName",
				Database = "newTestDataBase",
				ServerUrl = "newTestServerUrl",
				LastConnection = true
			};

			LoginViewTestAdapter adapter =new LoginViewTestAdapter(true);
			this.dialogFactory.GetLoginView(this.projectConfigurationManager.CurrentProjectConfiguraiton, projectName, projectFullName).Returns(adapter);
			this.dialogFactory.When(x => x.GetLoginView(this.projectConfigurationManager.CurrentProjectConfiguraiton, projectName, projectFullName)).Do(callback =>
				{
					connectionInfos[0].LastConnection = false;
					connectionInfos.Add(newConnection);
				});

			//Act
			debugMethodViewModel.EditConnectionInfoCommand.Execute(null);

			//Assert
			Assert.AreEqual(newConnection, debugMethodViewModel.ConnectionInformation);
		}
	}
}
