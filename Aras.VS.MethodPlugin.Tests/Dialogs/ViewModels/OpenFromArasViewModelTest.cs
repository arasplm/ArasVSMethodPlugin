using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Aras.IOM;
using Aras.Method.Libs;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.Tests.Authentication;
using NSubstitute;
using NUnit.Framework;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.ViewModels
{
	[TestFixture]
	public class OpenFromArasViewModelTest
	{
		private string currentPath = AppDomain.CurrentDomain.BaseDirectory;

		private OpenFromArasViewModel openFromArasViewModel;

		private IAuthenticationManager authManager;
		private IDialogFactory dialogFactory;
		private IProjectConfigurationManager projectConfigurationManager;
		private MessageManager messageManager;
		private PackageManager packageManager;
		private TemplateLoader templateLoader;

		private IServerConnection serverConnection;
		private Innovator innovatorInstance;
		private InnovatorUser innovatorUser;
		private IIOMWrapper iOMWrapper;

		[SetUp]
		public void Setup()
		{
			this.innovatorUser = new InnovatorUser();
			this.serverConnection = Substitute.For<IServerConnection>();
			this.innovatorInstance = new Innovator(this.serverConnection);
			this.iOMWrapper = Substitute.For<IIOMWrapper>();

			this.authManager = new AuthenticationManagerProxy(serverConnection, innovatorInstance, innovatorUser, iOMWrapper);
			this.dialogFactory = Substitute.For<IDialogFactory>();
			IProjectConfiguraiton projectConfiguration = Substitute.For<IProjectConfiguraiton>();
			this.projectConfigurationManager = Substitute.For<IProjectConfigurationManager>();
			this.projectConfigurationManager.CurrentProjectConfiguraiton.Returns(projectConfiguration);
			this.messageManager = Substitute.For<MessageManager>();
			this.templateLoader = new TemplateLoader();
			this.packageManager = new PackageManager(authManager, messageManager);

			ConnectionInfo testConnectionInfo = new ConnectionInfo()
			{
				LastConnection = true
			};

			projectConfiguration.Connections.Returns(new List<ConnectionInfo>() { testConnectionInfo });

			this.openFromArasViewModel = new OpenFromArasViewModel(authManager,
				dialogFactory,
				projectConfigurationManager,
				templateLoader,
				packageManager,
				messageManager,
				"tesPathToProjectConfigFile",
				"testProjectName",
				"testProjectFullName",
				"testProjectLanguage",
				null);
		}

		[Test]
		public void Ctor_ThrowAuthenticationManagerArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				OpenFromArasViewModel openFromArasView = new OpenFromArasViewModel(null,
					dialogFactory,
					projectConfigurationManager,
					templateLoader,
					packageManager,
					messageManager,
					"tesPathToProjectConfigFile",
					"testProjectName",
					"testProjectFullName",
					"testProjectLanguage",
					null);
			});
		}

		[Test]
		public void Ctor_ThrowDialogFactoryArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				OpenFromArasViewModel openFromArasView = new OpenFromArasViewModel(authManager,
					null,
					projectConfigurationManager,
					templateLoader,
					packageManager,
					messageManager,
					"tesPathToProjectConfigFile",
					"testProjectName",
					"testProjectFullName",
					"testProjectLanguage",
					null);
			});
		}

		[Test]
		public void Ctor_ThrowProjectConfigurationManagerArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				OpenFromArasViewModel openFromArasView = new OpenFromArasViewModel(authManager,
					dialogFactory,
					null,
					templateLoader,
					packageManager,
					messageManager,
					"tesPathToProjectConfigFile",
					"testProjectName",
					"testProjectFullName",
					"testProjectLanguage",
					null);
			});
		}

		[Test]
		public void Ctor_ThrowTemplateLoaderArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				OpenFromArasViewModel openFromArasView = new OpenFromArasViewModel(null,
					dialogFactory,
					projectConfigurationManager,
					null,
					packageManager,
					messageManager,
					"tesPathToProjectConfigFile",
					"testProjectName",
					"testProjectFullName",
					"testProjectLanguage",
					null);
			});
		}

		[Test]
		public void Ctor_ThrowPackageManagerArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				OpenFromArasViewModel openFromArasView = new OpenFromArasViewModel(null,
					dialogFactory,
					projectConfigurationManager,
					templateLoader,
					null,
					messageManager,
					"tesPathToProjectConfigFile",
					"testProjectName",
					"testProjectFullName",
					"testProjectLanguage",
					null);
			});
		}

		[Test]
		public void SelectedTemplate_ShouldReceiveMessageBox()
		{
			//Arange
			TemplateInfo testTemplateInfo = new TemplateInfo()
			{
				Message = "messageText",
				IsSuccessfullySupported = false
			};

			IMessageBoxWindow messageBox = Substitute.For<IMessageBoxWindow>();
			this.dialogFactory.GetMessageBoxWindow().Returns(messageBox);
			//Act
			this.openFromArasViewModel.SelectedTemplate = testTemplateInfo;

			//Assert
			messageBox.Received().ShowDialog(testTemplateInfo.Message,
				messageManager.GetMessage("OpenMethodArasInnovator"),
				MessageButtons.OK,
				MessageIcon.None);
		}

		[Test]
		public void SearchMethodDialogCommandExcecute_ShouldDoNotSetExpectedProperties()
		{
			//Arange
			ISearcher searcher = Substitute.For<ISearcher>();
			IItemSearchView searchView = Substitute.For<IItemSearchView>();
			ItemSearchPresenter itemSearchPresenter = Substitute.ForPartsOf<ItemSearchPresenter>(searchView, searcher);
			itemSearchPresenter.When(x => x.Run(Arg.Any<ItemSearchPresenterArgs>())).DoNotCallBase();

			this.dialogFactory.GetItemSearchPresenter("Method", "Method").Returns(itemSearchPresenter);
			this.projectConfigurationManager.CurrentProjectConfiguraiton.LastSavedSearch.Returns(new Dictionary<string, List<PropertyInfo>>());

			ItemSearchPresenterResult searchResult = new ItemSearchPresenterResult()
			{
				DialogResult = DialogResult.Cancel
			};

			itemSearchPresenter.Run(Arg.Any<ItemSearchPresenterArgs>()).Returns(searchResult);

			//Act
			this.openFromArasViewModel.SearchMethodDialogCommand.Execute(null);

			//Assert
			Assert.IsNull(this.openFromArasViewModel.MethodName);
			Assert.IsNull(this.openFromArasViewModel.MethodId);
			Assert.IsNull(this.openFromArasViewModel.MethodConfigId);
			Assert.IsNull(this.openFromArasViewModel.MethodLanguage);
			Assert.IsNull(this.openFromArasViewModel.IdentityKeyedName);
			Assert.IsNull(this.openFromArasViewModel.IdentityId);
			Assert.IsNull(this.openFromArasViewModel.MethodComment);
			Assert.IsNull(this.openFromArasViewModel.MethodType);
			Assert.IsNull(this.openFromArasViewModel.MethodCode);
			Assert.IsNull(this.openFromArasViewModel.SelectedTemplate);
		}

		[Test]
		[Ignore("Should be updated")]
		public void SearchMethodDialogCommandExcecute_ShouldSetExpectedPropertyValue()
		{
			//Arange
			ISearcher searcher = Substitute.For<ISearcher>();
			IItemSearchView searchView = Substitute.For<IItemSearchView>();
			ItemSearchPresenter itemSearchPresenter = Substitute.ForPartsOf<ItemSearchPresenter>(searchView, searcher);
			itemSearchPresenter.When(x => x.Run(Arg.Any<ItemSearchPresenterArgs>())).DoNotCallBase();

			this.dialogFactory.GetItemSearchPresenter("Method", "Method").Returns(itemSearchPresenter);
			this.projectConfigurationManager.CurrentProjectConfiguraiton.LastSavedSearch.Returns(new Dictionary<string, List<PropertyInfo>>());

			ItemSearchPresenterResult searchResult = new ItemSearchPresenterResult()
			{
				DialogResult = DialogResult.OK,
				ItemType = "Method",
				ItemId = "1BF96D4255962F7EA5970426401A841E"
			};

			itemSearchPresenter.Run(Arg.Any<ItemSearchPresenterArgs>()).Returns(searchResult);

			this.serverConnection.When(x => x.CallAction("ApplyItem", Arg.Any<XmlDocument>(), Arg.Any<XmlDocument>()))
				.Do(callBack =>
				{
					(callBack[2] as XmlDocument).Load(Path.Combine(currentPath, @"Dialogs\ViewModels\TestData\TestMethodItem.xml"));
				});

			//Act
			this.openFromArasViewModel.SearchMethodDialogCommand.Execute(null);

			//Assert
			Assert.AreEqual("ReturnNullMethodName", this.openFromArasViewModel.MethodName);
			Assert.AreEqual("1BF96D4255962F7EA5970426401A841E", this.openFromArasViewModel.MethodId);
			Assert.AreEqual("616634B3DC344D51964CD7AD988051D7", this.openFromArasViewModel.MethodConfigId);
			Assert.AreEqual("C#", this.openFromArasViewModel.MethodLanguage);
			Assert.AreEqual("World", this.openFromArasViewModel.IdentityKeyedName);
			Assert.AreEqual("A73B655731924CD0B027E4F4D5FCC0A9", this.openFromArasViewModel.IdentityId);
			Assert.IsEmpty(this.openFromArasViewModel.MethodComment);
			Assert.AreEqual("server", this.openFromArasViewModel.MethodType);
			Assert.AreEqual("return null;", this.openFromArasViewModel.MethodCode);
			Assert.IsNull(this.openFromArasViewModel.SelectedTemplate);
			Assert.AreEqual(1, this.projectConfigurationManager.CurrentProjectConfiguraiton.LastSavedSearch.Count);
		}

		[Test]
		public void OkCommand_CanExecute_ReturnFalse()
		{
			//Act
			bool result = this.openFromArasViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsFalse(result);
		}

		[Test]
		[Ignore("Should be updated")]
		public void OkCommand_CanExecute_ReturnTrue()
		{
			//Arange
			this.openFromArasViewModel.MethodName = "testMethodName";

			//Act
			bool result = this.openFromArasViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void SetSelectedTemplate_ShouldShowExpectedMessage()
		{
			//Arange
			TemplateInfo testTemplateInfo = new TemplateInfo()
			{
				IsSuccessfullySupported = false,
				Message = "message"
			};

			IMessageBoxWindow messageBoxWindow = Substitute.For<IMessageBoxWindow>();
			this.dialogFactory.GetMessageBoxWindow().Returns(messageBoxWindow);

			//Act
			this.openFromArasViewModel.SelectedTemplate = testTemplateInfo;

			//Assert
			messageBoxWindow.Received().ShowDialog("message",
						messageManager.GetMessage("OpenMethodArasInnovator"),
						MessageButtons.OK,
						MessageIcon.None);
		}
	}
}
