using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Aras.IOM;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using Aras.VS.MethodPlugin.Tests.Authentication;
using NSubstitute;
using NUnit.Framework;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.ViewModels
{
	[TestFixture]
	class SaveToPackageViewModelTest
	{
		private readonly string currentPath = AppDomain.CurrentDomain.BaseDirectory;
		private SaveToPackageViewModel saveToPackageViewModel;

		private IAuthenticationManager authManager;
		private IDialogFactory dialogFactory;
		private IProjectConfigurationManager projectConfigurationManager;
		private IProjectConfiguraiton projectConfiguration;
		private IMessageManager messageManager;
		private PackageManager packageManager;
		private TemplateLoader templateLoader;
		private ICodeProvider codeProvider;
		private IProjectManager projectManager;
		private IArasDataProvider arasDataProvider;
		private IIOWrapper iOWrapper;
		private MethodInfo methodInformation;

		private IServerConnection serverConnection;

		[OneTimeSetUp]
		public void Init()
		{
			serverConnection = Substitute.For<IServerConnection>();
			Innovator innovatorIns = new Innovator(serverConnection);

			this.authManager = new AuthenticationManagerProxy(this.serverConnection, innovatorIns, new InnovatorUser(), Substitute.For<IIOMWrapper>());
			this.dialogFactory = Substitute.For<IDialogFactory>();
			this.projectConfigurationManager = Substitute.For<IProjectConfigurationManager>();
			this.projectConfiguration = Substitute.For<IProjectConfiguraiton>();
			this.messageManager = Substitute.For<IMessageManager>();
			this.packageManager = new PackageManager(this.authManager, this.messageManager);
			this.templateLoader = new TemplateLoader(this.dialogFactory, this.messageManager);
			this.codeProvider = Substitute.For<ICodeProvider>();
			this.projectManager = Substitute.For<IProjectManager>();
			this.arasDataProvider = Substitute.For<IArasDataProvider>();
			this.iOWrapper = Substitute.For<IIOWrapper>();
			this.methodInformation = new MethodInfo();

			this.projectConfiguration.LastSavedSearch.Returns(new Dictionary<string, List<PropertyInfo>>());

			XmlDocument methodItemTypeDoc = new XmlDocument();
			methodItemTypeDoc.Load(Path.Combine(currentPath, @"Dialogs\ViewModels\TestData\MethodItemType.xml"));

			Item methodItemType = innovatorIns.newItem();
			methodItemType.loadAML(methodItemTypeDoc.OuterXml);
			MethodItemTypeInfo methodItemTypeInfo = new MethodItemTypeInfo(methodItemType, messageManager);
			this.arasDataProvider.GetMethodItemTypeInfo().Returns(methodItemTypeInfo);

			string pathToFileForSave = Path.Combine(currentPath, @"Code\TestData\MethodAml\ReturnNullMethodAml.xml");
			this.saveToPackageViewModel = new SaveToPackageViewModel(authManager,
				dialogFactory,
				projectConfiguration,
				templateLoader,
				packageManager,
				codeProvider,
				projectManager,
				arasDataProvider,
				iOWrapper,
				messageManager,
				methodInformation,
				pathToFileForSave);
		}

		[Test]
		public void Ctor_ShouldAuthenticationManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				SaveToPackageViewModel saveToPackageViewModel = new SaveToPackageViewModel(null,
				dialogFactory,
				projectConfiguration,
				templateLoader,
				packageManager,
				codeProvider,
				projectManager,
				arasDataProvider,
				iOWrapper,
				messageManager,
				methodInformation,
				"testPathToFileForSave");
			});
		}

		[Test]
		public void Ctor_ShouldDialogFactoryThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				SaveToPackageViewModel saveToPackageViewModel = new SaveToPackageViewModel(authManager,
				null,
				projectConfiguration,
				templateLoader,
				packageManager,
				codeProvider,
				projectManager,
				arasDataProvider,
				iOWrapper,
				messageManager,
				methodInformation,
				"testPathToFileForSave");
			});
		}

		[Test]
		public void Ctor_ShouldProjectConfigurationThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				SaveToPackageViewModel saveToPackageViewModel = new SaveToPackageViewModel(authManager,
				dialogFactory,
				null,
				templateLoader,
				packageManager,
				codeProvider,
				projectManager,
				arasDataProvider,
				iOWrapper,
				messageManager,
				methodInformation,
				"testPathToFileForSave");
			});
		}

		[Test]
		public void Ctor_ShouldTemplateLoaderThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				SaveToPackageViewModel saveToPackageViewModel = new SaveToPackageViewModel(authManager,
				dialogFactory,
				projectConfiguration,
				null,
				packageManager,
				codeProvider,
				projectManager,
				arasDataProvider,
				iOWrapper,
				messageManager,
				methodInformation,
				"testPathToFileForSave");
			});
		}

		[Test]
		public void Ctor_ShouldPackageManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				SaveToPackageViewModel saveToPackageViewModel = new SaveToPackageViewModel(authManager,
				dialogFactory,
				projectConfiguration,
				templateLoader,
				null,
				codeProvider,
				projectManager,
				arasDataProvider,
				iOWrapper,
				messageManager,
				methodInformation,
				"testPathToFileForSave");
			});
		}

		[Test]
		public void Ctor_ShouldCodeProviderThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				SaveToPackageViewModel saveToPackageViewModel = new SaveToPackageViewModel(authManager,
				dialogFactory,
				projectConfiguration,
				templateLoader,
				packageManager,
				null,
				projectManager,
				arasDataProvider,
				iOWrapper,
				messageManager,
				methodInformation,
				"testPathToFileForSave");
			});
		}

		[Test]
		public void Ctor_ShouldProjectManagerThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				SaveToPackageViewModel saveToPackageViewModel = new SaveToPackageViewModel(authManager,
				dialogFactory,
				projectConfiguration,
				templateLoader,
				packageManager,
				codeProvider,
				null,
				arasDataProvider,
				iOWrapper,
				messageManager,
				methodInformation,
				"testPathToFileForSave");
			});
		}

		[Test]
		public void Ctor_ShouldArasDataProviderThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				SaveToPackageViewModel saveToPackageViewModel = new SaveToPackageViewModel(authManager,
				dialogFactory,
				projectConfiguration,
				templateLoader,
				packageManager,
				codeProvider,
				projectManager,
				null,
				iOWrapper,
				messageManager,
				methodInformation,
				"testPathToFileForSave");
			});
		}

		[Test]
		public void Ctor_ShouldIOWrapperThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				SaveToPackageViewModel saveToPackageViewModel = new SaveToPackageViewModel(authManager,
				dialogFactory,
				projectConfiguration,
				templateLoader,
				packageManager,
				codeProvider,
				projectManager,
				arasDataProvider,
				null,
				messageManager,
				methodInformation,
				"testPathToFileForSave");
			});
		}

		[Test]
		public void Ctor_ShouldMethodInformationThrowArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				SaveToPackageViewModel saveToPackageViewModel = new SaveToPackageViewModel(authManager,
				dialogFactory,
				projectConfiguration,
				templateLoader,
				packageManager,
				codeProvider,
				projectManager,
				arasDataProvider,
				iOWrapper,
				messageManager,
				null,
				"testPathToFileForSave");
			});
		}

		[Test]
		public void OkCommand_CanExecute_ShouldReturnTrue()
		{
			//Arange
			this.saveToPackageViewModel.SelectedPackage = "testSelectedPackageName";
			this.saveToPackageViewModel.MethodName = "testMethodName";
			this.saveToPackageViewModel.PackagePath = "testPackagePath";
			this.saveToPackageViewModel.SelectedIdentityKeyedName = "testSelectedIdentityKeyedName";

			//Act
			bool result = this.saveToPackageViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsTrue(result);
		}

		[Test]
		public void OkCommand_CanExecute_ShouldReturnFalse_WhenSelectedPackageEmpty()
		{
			//Arange
			this.saveToPackageViewModel.SelectedPackage = string.Empty;
			this.saveToPackageViewModel.MethodName = "testMethodName";
			this.saveToPackageViewModel.PackagePath = "testPackagePath";
			this.saveToPackageViewModel.SelectedIdentityKeyedName = "testSelectedIdentityKeyedName";

			//Act
			bool result = this.saveToPackageViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void OkCommand_CanExecute_ShouldReturnFalse_WhenMethodNameEmpty()
		{
			//Arange
			this.saveToPackageViewModel.SelectedPackage = "testSelectedPackageName";
			this.saveToPackageViewModel.MethodName = string.Empty;
			this.saveToPackageViewModel.PackagePath = "testPackagePath";
			this.saveToPackageViewModel.SelectedIdentityKeyedName = "testSelectedIdentityKeyedName";

			//Act
			bool result = this.saveToPackageViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void OkCommand_CanExecute_ShouldReturnFalse_WhenPackagePathEmpty()
		{
			//Arange
			this.saveToPackageViewModel.SelectedPackage = "testSelectedPackageName";
			this.saveToPackageViewModel.MethodName = "testMethodName";
			this.saveToPackageViewModel.PackagePath = string.Empty;
			this.saveToPackageViewModel.SelectedIdentityKeyedName = "testSelectedIdentityKeyedName";

			//Act
			bool result = this.saveToPackageViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsFalse(result);
		}

		[Test]
		public void OkCommand_CanExecute_ShouldReturnFalse_WhenSelectedIdentityKeyedNameEmpty()
		{
			//Arange
			this.saveToPackageViewModel.SelectedPackage = "testSelectedPackageName";
			this.saveToPackageViewModel.MethodName = "testMethodName";
			this.saveToPackageViewModel.PackagePath = "testPackagePath";
			this.saveToPackageViewModel.SelectedIdentityKeyedName = string.Empty;

			//Act
			bool result = this.saveToPackageViewModel.OkCommand.CanExecute(null);

			//Assert
			Assert.IsFalse(result);
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
			this.saveToPackageViewModel.SelectedIdentityCommand.Execute(null);

			//Assert
			Assert.AreEqual("World", this.saveToPackageViewModel.SelectedIdentityKeyedName);
			Assert.AreEqual("A73B655731924CD0B027E4F4D5FCC0A9", this.saveToPackageViewModel.SelectedIdentityId);
			Assert.AreEqual(1, this.projectConfiguration.LastSavedSearch.Count);
		}
	}
}
