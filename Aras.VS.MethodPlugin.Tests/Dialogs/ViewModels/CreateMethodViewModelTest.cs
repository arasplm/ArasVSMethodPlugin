using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Aras.IOM;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Configurations;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Tests.Authentication;
using Aras.VS.MethodPlugin.Tests.Dialogs.SubAdapters;
using NSubstitute;
using NUnit.Framework;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.ViewModels
{
	[TestFixture]
	class CreateMethodViewModelTest
	{
		private string currentPath = AppDomain.CurrentDomain.BaseDirectory;

		private IAuthenticationManager authManager;
		private IDialogFactory dialogFactory;
		private IProjectConfigurationManager projectConfigurationManager;
		private IProjectConfiguraiton projectConfiguration;
		private MessageManager messageManager;
		private PackageManager packageManager;
		private IArasDataProvider arasDataProvider;
		private MethodInfo methodInformation;
		private TemplateLoader templateLoader;
		private IProjectManager projectManager;
		private ICodeProvider codeProvider;
		private IGlobalConfiguration globalConfiguration;

		private IServerConnection serverConnection;
		private Innovator innovatorInstance;
		private InnovatorUser innovatorUser;
		private IIOMWrapper iOMWrapper;

		private CreateMethodViewModel createMethodViewModel;

		[SetUp]
		public void Setup()
		{
			this.innovatorUser = new InnovatorUser();
			this.serverConnection = Substitute.For<IServerConnection>();
			this.innovatorInstance = new Innovator(this.serverConnection);
			this.iOMWrapper = Substitute.For<IIOMWrapper>();

			this.authManager = new AuthenticationManagerProxy(serverConnection, innovatorInstance, innovatorUser, iOMWrapper);
			this.dialogFactory = Substitute.For<IDialogFactory>();
			this.projectConfigurationManager = Substitute.For<IProjectConfigurationManager>();
			this.projectConfiguration = Substitute.For<IProjectConfiguraiton>();
			this.messageManager = Substitute.For<MessageManager>();
			this.packageManager = new PackageManager(authManager, messageManager);
			this.arasDataProvider = Substitute.For<IArasDataProvider>();
			this.methodInformation = new MethodInfo();
			this.templateLoader = new TemplateLoader();
			this.projectManager = Substitute.For<IProjectManager>();
			this.codeProvider = Substitute.For<ICodeProvider>();
			this.globalConfiguration = Substitute.For<IGlobalConfiguration>();

			this.globalConfiguration.GetUserCodeTemplatesPaths().Returns(new List<string>());

			this.serverConnection.When(x => x.CallAction("ApplyItem", Arg.Is<XmlDocument>(doc => doc.DocumentElement.Attributes["type"].Value == "Value"), Arg.Any<XmlDocument>()))
								.Do(x =>
								{
									(x[2] as XmlDocument).Load(Path.Combine(currentPath, @"Dialogs\ViewModels\TestData\ActionLocationsListValue.xml"));
								});

			this.serverConnection.When(x => x.CallAction("ApplyItem", Arg.Is<XmlDocument>(doc => doc.DocumentElement.Attributes["type"].Value == "Filter Value"), Arg.Any<XmlDocument>()))
								.Do(x =>
								{
									(x[2] as XmlDocument).Load(Path.Combine(currentPath, @"Dialogs\ViewModels\TestData\MethodTypesListFilterValue.xml"));
								});

			this.projectConfiguration.LastSavedSearch.Returns(new Dictionary<string, List<PropertyInfo>>());

			XmlDocument methodItemTypeAML = new XmlDocument();
			methodItemTypeAML.Load(Path.Combine(currentPath, @"Dialogs\ViewModels\TestData\MethodItemType.xml"));

			Item methodItemType = this.innovatorInstance.newItem();
			methodItemType.loadAML(methodItemTypeAML.OuterXml);

			this.arasDataProvider.GetMethodItemTypeInfo().Returns(new MethodItemTypeInfo(methodItemType, messageManager));

			this.createMethodViewModel = new CreateMethodViewModel(authManager,
					dialogFactory,
					projectConfiguration,
					templateLoader,
					packageManager,
					projectManager,
					arasDataProvider,
					codeProvider,
					globalConfiguration,
					messageManager);

		}

		[Test]
		public void Ctor_CreateMethodViewModel_ShouldAuthenticationManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				CreateMethodViewModel createMethodViewModel = new CreateMethodViewModel(null,
								dialogFactory,
								projectConfiguration,
								templateLoader,
								packageManager,
								projectManager,
								arasDataProvider,
								codeProvider,
								globalConfiguration,
								messageManager);
			});

		}

		[Test]
		public void Ctor_CreateMethodViewModel_ShouldDialogFactoryThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				CreateMethodViewModel createMethodViewModel = new CreateMethodViewModel(authManager,
								null,
								projectConfiguration,
								templateLoader,
								packageManager,
								projectManager,
								arasDataProvider,
								codeProvider,
								globalConfiguration,
								messageManager);
			});

		}

		[Test]
		public void Ctor_CreateMethodViewModel_ShouldProjectConfigurationThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				CreateMethodViewModel createMethodViewModel = new CreateMethodViewModel(authManager,
								dialogFactory,
								null,
								templateLoader,
								packageManager,
								projectManager,
								arasDataProvider,
								codeProvider,
								globalConfiguration,
								messageManager);
			});

		}

		[Test]
		public void Ctor_CreateMethodViewModel_ShouldTemplateLoaderThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				CreateMethodViewModel createMethodViewModel = new CreateMethodViewModel(authManager,
								dialogFactory,
								projectConfiguration,
								null,
								packageManager,
								projectManager,
								arasDataProvider,
								codeProvider,
								globalConfiguration,
								messageManager);
			});

		}

		[Test]
		public void Ctor_CreateMethodViewModel_ShouldPackageManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				CreateMethodViewModel createMethodViewModel = new CreateMethodViewModel(authManager,
								dialogFactory,
								projectConfiguration,
								templateLoader,
								null,
								projectManager,
								arasDataProvider,
								codeProvider,
								globalConfiguration,
								messageManager);
			});

		}

		[Test]
		public void Ctor_CreateMethodViewModel_ShouldProjectManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				CreateMethodViewModel createMethodViewModel = new CreateMethodViewModel(authManager,
								dialogFactory,
								projectConfiguration,
								templateLoader,
								packageManager,
								null,
								arasDataProvider,
								codeProvider,
								globalConfiguration,
								messageManager);
			});

		}

		[Test]
		public void Ctor_CreateMethodViewModel_ShouldArasDataProviderThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				CreateMethodViewModel createMethodViewModel = new CreateMethodViewModel(authManager,
								dialogFactory,
								projectConfiguration,
								templateLoader,
								packageManager,
								projectManager,
								null,
								codeProvider,
								globalConfiguration,
								messageManager);
			});

		}

		[Test]
		public void Ctor_CreateMethodViewModel_ShouldCodeProviderThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				CreateMethodViewModel createMethodViewModel = new CreateMethodViewModel(null,
								dialogFactory,
								projectConfiguration,
								templateLoader,
								packageManager,
								projectManager,
								arasDataProvider,
								null,
								globalConfiguration,
								messageManager);
			});

		}

		[Test]
		public void Ctor_CreateMethodViewModel_ShouldGlobalConfigurationThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				CreateMethodViewModel createMethodViewModel = new CreateMethodViewModel(null,
								dialogFactory,
								projectConfiguration,
								templateLoader,
								packageManager,
								projectManager,
								arasDataProvider,
								codeProvider,
								null,
								messageManager);
			});

		}

		[Test]
		public void SelectedIdentityCommandExecute_ShouldFillExpected()
		{
			//Arange
			IItemSearchView view = Substitute.For<IItemSearchView>();
			ISearcher iSearcher = Substitute.For<ISearcher>();

			ItemSearchPresenterResult searchPresenterResult = new ItemSearchPresenterResult()
			{
				DialogResult = System.Windows.Forms.DialogResult.OK,
				ItemId = "A73B655731924CD0B027E4F4D5FCC0A9",
				ItemType = "Identity",
				LastSavedSearch = new List<PropertyInfo>()
			};

			ItemSearchPresenter itemSearchPresenter = Substitute.ForPartsOf<ItemSearchPresenter>(view, iSearcher);
			itemSearchPresenter.When(x => x.Run(Arg.Any<ItemSearchPresenterArgs>())).DoNotCallBase();
			itemSearchPresenter.Run(Arg.Any<ItemSearchPresenterArgs>()).Returns(searchPresenterResult);

			this.dialogFactory.GetItemSearchPresenter("Identity", "Identity").Returns(itemSearchPresenter);

			this.serverConnection.When(x => x.CallAction("ApplyItem", Arg.Is<XmlDocument>(doc => doc.DocumentElement.Attributes["type"].Value == "Identity"), Arg.Any<XmlDocument>()))
								.Do(x =>
								{
									(x[2] as XmlDocument).Load(Path.Combine(currentPath, @"Dialogs\ViewModels\TestData\WorldItentityItem.xml"));
								});

			//Act
			this.createMethodViewModel.SelectedIdentityCommand.Execute(null);

			//Assert
			Assert.AreEqual("World", this.createMethodViewModel.SelectedIdentityKeyedName);
			Assert.AreEqual("A73B655731924CD0B027E4F4D5FCC0A9", this.createMethodViewModel.SelectedIdentityId);
			Assert.AreEqual(1, this.projectConfiguration.LastSavedSearch.Count);
		}

		[Test]
		public void Execute_SelectedIdentityCommand_LeaveEmptyAndNullProperty()
		{
			//Arange
			IItemSearchView view = Substitute.For<IItemSearchView>();
			ISearcher iSearcher = Substitute.For<ISearcher>();

			ItemSearchPresenterResult searchPresenterResult = new ItemSearchPresenterResult()
			{
				DialogResult = System.Windows.Forms.DialogResult.Cancel
			};

			ItemSearchPresenter itemSearchPresenter = Substitute.ForPartsOf<ItemSearchPresenter>(view, iSearcher);
			itemSearchPresenter.When(x => x.Run(Arg.Any<ItemSearchPresenterArgs>())).DoNotCallBase();
			itemSearchPresenter.Run(Arg.Any<ItemSearchPresenterArgs>()).Returns(searchPresenterResult);

			this.dialogFactory.GetItemSearchPresenter("Identity", "Identity").Returns(itemSearchPresenter);

			//Act
			this.createMethodViewModel.SelectedIdentityCommand.Execute(null);

			//Assert
			Assert.IsNull(this.createMethodViewModel.SelectedIdentityKeyedName);
			Assert.IsNull(this.createMethodViewModel.SelectedIdentityId);
			Assert.AreEqual(0, this.projectConfiguration.LastSavedSearch.Count);
		}

		[Test]
		public void BrowseCodeTemplateCommandExecute_ShouldShowTemplateInvalidMessage()
		{
			//Arange
			string filePath = "filePathTest";
			OpenFileDialogTestAdapter openFileDialog = new OpenFileDialogTestAdapter(DialogResult.OK, filePath);
			this.dialogFactory.GetOpenFileDialog("XML Files (*.xml)|*.xml", "xml").Returns(openFileDialog);

			IMessageBoxWindow messageBox = Substitute.For<IMessageBoxWindow>();
			this.dialogFactory.GetMessageBoxWindow().Returns(messageBox);

			//Act
			this.createMethodViewModel.BrowseCodeTemplateCommand.Execute(null);

			//Assert
			messageBox.Received().ShowDialog(messageManager.GetMessage("UserCodeTemplateInvalidFormat"),
					messageManager.GetMessage("Warning"),
					MessageButtons.OK,
					MessageIcon.Warning);
		}

		[Test]
		public void BrowseCodeTemplateCommandExecute_ShouldShowExpectedMessageee()
		{
			//Arange
			string currentFilePath = AppDomain.CurrentDomain.BaseDirectory;
			string amlMethodFilePath = Path.Combine(currentFilePath, @"Code\TestData\MethodAml\ReturnNullMethodAml.xml");
			this.codeProvider.Language.Returns("VB");

			OpenFileDialogTestAdapter openFileDialog = new OpenFileDialogTestAdapter(DialogResult.OK, amlMethodFilePath);
			this.dialogFactory.GetOpenFileDialog("XML Files (*.xml)|*.xml", "xml").Returns(openFileDialog);

			IMessageBoxWindow messageBox = Substitute.For<IMessageBoxWindow>();
			this.dialogFactory.GetMessageBoxWindow().Returns(messageBox);

			//Act
			this.createMethodViewModel.BrowseCodeTemplateCommand.Execute(null);

			//Assert
			messageBox.Received().ShowDialog(messageManager.GetMessage("UserCodeTamplateMustBeMethodType", this.codeProvider.Language),
					messageManager.GetMessage("Warning"),
					MessageButtons.OK,
					MessageIcon.Warning);
		}

		[Test]
		public void BrowseCodeTemplateCommandExecute_ShouldAddNewUserCodeTemplate()
		{
			//Arange
			string currentFilePath = AppDomain.CurrentDomain.BaseDirectory;
			string amlMethodFilePath = Path.Combine(currentFilePath, @"Code\TestData\MethodAml\ReturnNullMethodAml.xml");
			this.codeProvider.Language.Returns("C#");

			OpenFileDialogTestAdapter openFileDialog = new OpenFileDialogTestAdapter(DialogResult.OK, amlMethodFilePath);
			this.dialogFactory.GetOpenFileDialog("XML Files (*.xml)|*.xml", "xml").Returns(openFileDialog);
			int userCodeTemplateCount = this.createMethodViewModel.UserCodeTemplates.Count;

			//Act
			this.createMethodViewModel.BrowseCodeTemplateCommand.Execute(null);

			//Assert

			this.globalConfiguration.Received().AddUserCodeTemplatePath(amlMethodFilePath);
			Assert.AreEqual(1, this.createMethodViewModel.UserCodeTemplates.Count - userCodeTemplateCount);
			Assert.IsNotNull(this.createMethodViewModel.SelectedUserCodeTemplate);
		}

		[Test]
		public void DeleteUserCodeTemplateCommandExecute_ShouldDeleteUserCodeTemplate()
		{
			//Arange
			string currentFilePath = AppDomain.CurrentDomain.BaseDirectory;
			string amlMethodFilePath = Path.Combine(currentFilePath, @"Code\TestData\MethodAml\ReturnNullMethodAml.xml");
			string key = "ReturnNullMethodAml";
			XmlMethodInfo methodInfo = new XmlMethodInfo()
			{
				Path = amlMethodFilePath
			};

			KeyValuePair<string, XmlMethodInfo> userCodeTemplate = new KeyValuePair<string, XmlMethodInfo>(key, methodInfo);
			this.globalConfiguration.GetUserCodeTemplatesPaths().Returns(new List<string>() { amlMethodFilePath });

			//Act
			this.createMethodViewModel.DeleteUserCodeTemplateCommand.Execute(userCodeTemplate);

			//Assert
			this.globalConfiguration.Received().RemoveUserCodeTemplatePath(methodInfo.Path);
			Assert.AreEqual(1, this.createMethodViewModel.UserCodeTemplates.Count);
		}
	}
}
