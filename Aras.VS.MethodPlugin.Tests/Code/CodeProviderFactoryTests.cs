using System;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.SolutionManagement;
using EnvDTE;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Code
{
	[TestFixture]
	public class CodeProviderFactoryTests
	{
		private IProjectManager projectManager;
		private DefaultCodeProvider defaultCodeProvider;
		private IIOWrapper iOWrapper;
		private IDialogFactory dialogFactory;
		private IMessageManager messageManager;
		private IProjectConfiguraiton projectConfiguration;
		private CodeProviderFactory codeProviderFactory;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			projectManager = Substitute.For<IProjectManager>();
			iOWrapper = Substitute.For<IIOWrapper>();
			defaultCodeProvider = Substitute.For<DefaultCodeProvider>(iOWrapper);
			dialogFactory = Substitute.For<IDialogFactory>();
			messageManager = Substitute.For<IMessageManager>();
			messageManager.GetMessage("CurrentProjectTypeIsNotSupported").Returns("CurrentProjectTypeIsNotSupported");
			projectConfiguration = Substitute.For<IProjectConfiguraiton>();

			codeProviderFactory = new CodeProviderFactory(projectManager, defaultCodeProvider, iOWrapper, dialogFactory, messageManager);
		}

		[Test]
		public void Ctor_ProjectManagerIsNull_ShouldThrowArgumentNullException()
		{
			// Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				var codeProviderFactory =
					new CodeProviderFactory(null, defaultCodeProvider, iOWrapper, dialogFactory, messageManager);
			}), nameof(projectManager));
		}

		[Test]
		public void Ctor_DefaultCodeProviderIsNull_ShouldThrowArgumentNullException()
		{
			// Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				var codeProviderFactory =
					new CodeProviderFactory(projectManager, null, iOWrapper, dialogFactory, messageManager);
			}), nameof(defaultCodeProvider));
		}

		[Test]
		public void Ctor_IOWrapperIsNull_ShouldThrowArgumentNullException()
		{
			// Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				var codeProviderFactory =
					new CodeProviderFactory(projectManager, defaultCodeProvider, null, dialogFactory, messageManager);
			}), nameof(iOWrapper));
		}

		[Test]
		public void Ctor_DialogFactoryIsNull_ShouldThrowArgumentNullException()
		{
			// Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				var codeProviderFactory =
					new CodeProviderFactory(projectManager, defaultCodeProvider, iOWrapper, null, messageManager);
			}), nameof(dialogFactory));
		}

		[Test]
		public void Ctor_MessageManagerIsNull_ShouldThrowArgumentNullException()
		{
			// Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				var codeProviderFactory =
					new CodeProviderFactory(projectManager, defaultCodeProvider, iOWrapper, dialogFactory, null);
			}), nameof(messageManager));
		}

		[Test]
		public void GetCodeItemProvider_ShouldReturnCSharpCodeItemProvider()
		{
			// Act
			ICodeItemProvider codeItemProvider = codeProviderFactory.GetCodeItemProvider(CodeModelLanguageConstants.vsCMLanguageCSharp);

			// Assert
			Assert.IsInstanceOf(typeof(CSharpCodeItemProvider), codeItemProvider);
		}

		[TestCase("")]
		[TestCase(CodeModelLanguageConstants.vsCMLanguageVC)]
		[TestCase(CodeModelLanguageConstants.vsCMLanguageVB)]
		[TestCase(CodeModelLanguageConstants.vsCMLanguageIDL)]
		[TestCase(CodeModelLanguageConstants.vsCMLanguageMC)]
		public void GetCodeItemProvider_NotSupporteCode_ShouldThrowNotSupportedException(string projectLanguageCode)
		{
			// Assert
			NotSupportedException exception = Assert.Throws<NotSupportedException>(new TestDelegate(() =>
			{
				// Act
				ICodeItemProvider codeItemProvider = codeProviderFactory.GetCodeItemProvider(projectLanguageCode);
			}));

			Assert.AreEqual(messageManager.GetMessage("CurrentProjectTypeIsNotSupported"), exception.Message);
		}

		[TestCase(CodeModelLanguageConstants.vsCMLanguageCSharp)]
		[TestCase(GlobalConsts.CSharp)]
		public void GetCodeProvider_ShouldReturnCSharpCodeProvider(string projectLanguageCode)
		{
			// Act
			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectLanguageCode, projectConfiguration);

			// Assert
			Assert.IsInstanceOf(typeof(CSharpCodeProvider), codeProvider);
		}

		[TestCase(CodeModelLanguageConstants.vsCMLanguageVB)]
		public void GetCodeProvider_ShouldReturnVBCodeProvider(string projectLanguageCode)
		{
			// Act
			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectLanguageCode, projectConfiguration);

			// Assert
			Assert.IsInstanceOf(typeof(VBCodeProvider), codeProvider);
		}

		[TestCase("")]
		[TestCase(CodeModelLanguageConstants.vsCMLanguageVC)]
		[TestCase(CodeModelLanguageConstants.vsCMLanguageIDL)]
		[TestCase(CodeModelLanguageConstants.vsCMLanguageMC)]
		public void GetCodeProvider_NotSupporteCode_ShouldThrowNotSupportedException(string projectLanguageCode)
		{
			// Assert
			NotSupportedException exception = Assert.Throws<NotSupportedException>(new TestDelegate(() =>
			{
				// Act
				ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectLanguageCode, projectConfiguration);
			}));

			Assert.AreEqual(messageManager.GetMessage("CurrentProjectTypeIsNotSupported"), exception.Message);
		}
	}
}
