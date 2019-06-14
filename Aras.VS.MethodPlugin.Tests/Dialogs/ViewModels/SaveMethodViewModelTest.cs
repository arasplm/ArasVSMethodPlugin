using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Aras.IOM;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.Tests.Authentication;
using NSubstitute;
using NUnit.Framework;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.ViewModels
{
	[TestFixture]
	class SaveMethodViewModelTest
	{
		private readonly string currentPath = AppDomain.CurrentDomain.BaseDirectory;

		private SaveMethodViewModel saveMethodViewModel;

		private IServerConnection serverConnection;
		private Innovator innovator;

		private IAuthenticationManager authManager;
		private IDialogFactory dialogFactory;
		private IProjectConfigurationManager projectConfigurationManager;
		private MessageManager messageManager;
		private PackageManager packageManager;
		private IArasDataProvider arasDataProvider;
		private MethodInfo methodInformation;

		[OneTimeSetUp]
		public void Setup()
		{
			this.serverConnection = Substitute.For<IServerConnection>();
			this.innovator = new Innovator(this.serverConnection);

			this.authManager = new AuthenticationManagerProxy(this.serverConnection, innovator, new InnovatorUser(), Substitute.For<IIOMWrapper>());
			this.dialogFactory = Substitute.For<IDialogFactory>();
			IProjectConfiguraiton projectConfiguration = Substitute.For<IProjectConfiguraiton>();
			this.projectConfigurationManager = Substitute.For<IProjectConfigurationManager>();
			projectConfigurationManager.CurrentProjectConfiguraiton.Returns(projectConfiguration);
			this.messageManager = Substitute.For<MessageManager>();
			this.packageManager = new PackageManager(authManager, messageManager);
			this.arasDataProvider = Substitute.For<IArasDataProvider>();
			this.methodInformation = new MethodInfo()
			{
				Package = new PackageInfo(string.Empty)
			};

			XmlDocument methodItemTypeXmlDocument = new XmlDocument();
			methodItemTypeXmlDocument.Load(Path.Combine(currentPath, @"Dialogs\ViewModels\TestData\MethodItemType.xml"));

			Item methodItem = this.innovator.newItem();
			methodItem.loadAML(methodItemTypeXmlDocument.OuterXml);
			MethodItemTypeInfo methodItemTypeInfo = new MethodItemTypeInfo(methodItem, messageManager);

			arasDataProvider.GetMethodItemTypeInfo().Returns(methodItemTypeInfo);

			List<ConnectionInfo> connectionInfos = new List<ConnectionInfo>()
			{
				new ConnectionInfo()
				{
					Login = "userName",
					Database = "testDataBase",
					ServerUrl = "testServerUrl",
					LastConnection = true
				}
			};

			projectConfiguration.Connections.Returns(connectionInfos);

			saveMethodViewModel = new SaveMethodViewModel(this.authManager,
				this.dialogFactory,
				this.projectConfigurationManager,
				this.packageManager,
				this.arasDataProvider,
				this.methodInformation,
				this.messageManager,
				"methodCode",
				"projectConfPath",
				"projectName",
				"projectFullName");
		}

		[Test]
		public void Ctor_ShouldAuthenticationManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				SaveMethodViewModel viewModel = new SaveMethodViewModel(null,
					this.dialogFactory,
					this.projectConfigurationManager,
					this.packageManager, 
					this.arasDataProvider, 
					this.methodInformation,
					this.messageManager,
					"methodCode",
					"projectConfPath",
					"projectName",
					"projectFullName");
			});
		}

		[Test]
		public void Ctor_ShouldDialogFactoryThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				SaveMethodViewModel viewModel = new SaveMethodViewModel(this.authManager,
					null,
					this.projectConfigurationManager,
					this.packageManager,
					this.arasDataProvider,
					this.methodInformation,
					this.messageManager,
					"methodCode",
					"projectConfPath",
					"projectName",
					"projectFullName");
			});
		}

		[Test]
		public void Ctor_ShouldProjectConfigurationManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				SaveMethodViewModel viewModel = new SaveMethodViewModel(this.authManager,
					this.dialogFactory,
					null,
					this.packageManager,
					this.arasDataProvider,
					this.methodInformation,
					this.messageManager,
					"methodCode",
					"projectConfPath",
					"projectName",
					"projectFullName");
			});
		}

		[Test]
		public void Ctor_ShouldPackageManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				SaveMethodViewModel viewModel = new SaveMethodViewModel(this.authManager,
					this.dialogFactory,
					this.projectConfigurationManager,
					null,
					this.arasDataProvider,
					this.methodInformation,
					this.messageManager,
					"methodCode",
					"projectConfPath",
					"projectName",
					"projectFullName");
			});
		}

		[Test]
		public void Ctor_ShouldArasDataProviderThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				SaveMethodViewModel viewModel = new SaveMethodViewModel(this.authManager,
					this.dialogFactory,
					this.projectConfigurationManager,
					this.packageManager,
					null,
					this.methodInformation,
					this.messageManager,
					"methodCode",
					"projectConfPath",
					"projectName",
					"projectFullName");
			});
		}

		[Test]
		public void Ctor_ShouldMethodInformationThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				SaveMethodViewModel viewModel = new SaveMethodViewModel(this.authManager,
					this.dialogFactory,
					this.projectConfigurationManager,
					this.packageManager,
					this.arasDataProvider,
					null,
					this.messageManager,
					"methodCode",
					"projectConfPath",
					"projectName",
					"projectFullName");
			});
		}

		[Test]
		public void SelectedIdentityCommand()
		{
			//Arange
			projectConfigurationManager.CurrentProjectConfiguraiton.LastSavedSearch.Returns(new Dictionary<string, List<PropertyInfo>>());

			List<PropertyInfo> lastSavedSearch = new List<PropertyInfo>();
			IItemSearchView view = Substitute.For<IItemSearchView>();
			ISearcher iSearcher = Substitute.For<ISearcher>();
			ItemSearchPresenterResult searchPresenterResult = new ItemSearchPresenterResult()
			{
				DialogResult = System.Windows.Forms.DialogResult.OK,
				ItemType = "Identity",
				ItemId = "A73B655731924CD0B027E4F4D5FCC0A9",
				LastSavedSearch = lastSavedSearch
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
			saveMethodViewModel.SelectedIdentityCommand.Execute(null);

			//Assert
			Assert.AreEqual("A73B655731924CD0B027E4F4D5FCC0A9", saveMethodViewModel.SelectedIdentityId);
			Assert.AreEqual("World", saveMethodViewModel.SelectedIdentityKeyedName);
			Assert.AreEqual(lastSavedSearch, projectConfigurationManager.CurrentProjectConfiguraiton.LastSavedSearch["Identity"]);
		}

		[Test]
		public void OkCommand_CanExecute_ShouldReturnTrue()
		{
			//Arange
			saveMethodViewModel.SelectedPackage = "SelectedPackage";
			saveMethodViewModel.MethodName = "MethodName";
			saveMethodViewModel.SelectedIdentityKeyedName = "SelectedIdentityKeyedName";

			//Act
			bool canExecute = saveMethodViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsTrue(canExecute);
		}

		[Test]
		public void OkCommand_CanExecute_ShouldReturnFalse_WhenSelectedPackageEmpty()
		{
			//Arange
			saveMethodViewModel.SelectedPackage = string.Empty;
			saveMethodViewModel.MethodName = "MethodName";
			saveMethodViewModel.SelectedIdentityKeyedName = "SelectedIdentityKeyedName";

			//Act
			bool canExecute = saveMethodViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsFalse(canExecute);
		}

		[Test]
		public void OkCommand_CanExecute_ShouldReturnFalse_WhenMethodNameEmpty()
		{
			//Arange
			saveMethodViewModel.SelectedPackage = "SelectedPackage";
			saveMethodViewModel.MethodName = string.Empty;
			saveMethodViewModel.SelectedIdentityKeyedName = "SelectedIdentityKeyedName";

			//Act
			bool canExecute = saveMethodViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsFalse(canExecute);
		}

		[Test]
		public void OkCommand_CanExecute_ShouldReturnFalse_WhenSelectedIdentityKeyedNameEmpty()
		{
			//Arange
			saveMethodViewModel.SelectedPackage = "SelectedPackage";
			saveMethodViewModel.MethodName = "MethodName";
			saveMethodViewModel.SelectedIdentityKeyedName = string.Empty;

			//Act
			bool canExecute = saveMethodViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsFalse(canExecute);
		}
	}
}
