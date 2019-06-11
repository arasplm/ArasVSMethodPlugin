using System;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using EnvDTE;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Code
{
	[TestFixture]
	public class CodeProviderFactoryTests
	{
		private DefaultCodeProvider defaultCodeProvider;
		private ICodeFormatter codeFormatter;
		private MessageManager messageManager;
		private IIOWrapper iOWrapper;
		private CodeProviderFactory codeProviderFactory;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			iOWrapper = Substitute.For<IIOWrapper>();
			defaultCodeProvider = new DefaultCodeProvider(iOWrapper);
			codeFormatter = Substitute.For<ICodeFormatter>();
			messageManager = Substitute.For<MessageManager>();
			messageManager.GetMessage("CurrentProjectTypeIsNotSupported").Returns("CurrentProjectTypeIsNotSupported");

			codeProviderFactory = new CodeProviderFactory(defaultCodeProvider, codeFormatter, messageManager, iOWrapper);
		}

		[Test]
		public void Ctor_DefaultCodeProviderIsNull_ShouldThrowArgumentNullException()
		{
			// Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				var codeProviderFactory =
					new CodeProviderFactory(null, codeFormatter, messageManager, iOWrapper);
			}), nameof(defaultCodeProvider));
		}

		[Test]
		public void Ctor_CodeFormatterIsNull_ShouldThrowArgumentNullException()
		{
			// Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				var codeProviderFactory =
					new CodeProviderFactory(defaultCodeProvider, null, messageManager, iOWrapper);
			}), nameof(messageManager));
		}

		[Test]
		public void Ctor_MessageManagerIsNull_ShouldThrowArgumentNullException()
		{
			// Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				var codeProviderFactory =
					new CodeProviderFactory(defaultCodeProvider, codeFormatter, null, iOWrapper);
			}), nameof(messageManager));
		}

		[Test]
		public void Ctor_IOWrapperIsNull_ShouldThrowArgumentNullException()
		{
			// Assert
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				var codeProviderFactory =
					new CodeProviderFactory(defaultCodeProvider, codeFormatter, messageManager, null);
			}), nameof(iOWrapper));
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
			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectLanguageCode);

			// Assert
			Assert.IsInstanceOf(typeof(CSharpCodeProvider), codeProvider);
		}

		[TestCase(CodeModelLanguageConstants.vsCMLanguageVB)]
		public void GetCodeProvider_ShouldReturnVBCodeProvider(string projectLanguageCode)
		{
			// Act
			ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectLanguageCode);

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
				ICodeProvider codeProvider = codeProviderFactory.GetCodeProvider(projectLanguageCode);
			}));

			Assert.AreEqual(messageManager.GetMessage("CurrentProjectTypeIsNotSupported"), exception.Message);
		}
	}
}
