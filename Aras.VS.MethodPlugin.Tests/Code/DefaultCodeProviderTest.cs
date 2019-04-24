using System.IO;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Code
{
	[TestFixture]
	public class DefaultCodeProviderTest
	{
		private IIOWrapper iOWrapper;
		private DefaultCodeProvider defaultCodeProvider;

		[OneTimeSetUp]
		public void Setup()
		{
			this.iOWrapper = Substitute.For<IIOWrapper>();
			defaultCodeProvider = new DefaultCodeProvider(iOWrapper);
		}

		[Test]
		public void GetDefaultCodeTemplate_DirectoryNotExist_ShouldReturnNull()
		{
			//Arrange
			this.iOWrapper.DirectoryExists(string.Empty).Returns(false);

			//Act
			DefaultCodeTemplate result = this.defaultCodeProvider.GetDefaultCodeTemplate(string.Empty, string.Empty, string.Empty);

			//Assert
			Assert.IsNull(result);
		}

		[Test]
		public void GetDefaultCodeTemplate_ShouldReturnNull()
		{
			//Arrange
			string templateName = "CSharp";
			string eventName = "None";
			this.iOWrapper.DirectoryExists(Arg.Any<string>()).Returns(true);
			this.iOWrapper.DirectoryGetFiles(Arg.Any<string>()).Returns(new string[] { "TemlateFilePath" });

			//Act
			DefaultCodeTemplate result = this.defaultCodeProvider.GetDefaultCodeTemplate(Arg.Any<string>(), templateName, eventName);

			//Assert
			Assert.IsNull(result);
		}

		[Test]
		public void GetDefaultCodeTemplate_ShouldReturnExpected()
		{
			//Arrange
			string templateName = "CSharp";
			string eventName = "None";

			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var cSharpNonePath = Path.Combine(currentPath, @"TestData\CSharpNone.xml");

			this.iOWrapper.DirectoryExists(Arg.Any<string>()).Returns(true);
			this.iOWrapper.DirectoryGetFiles(Arg.Any<string>()).Returns(new string[] { cSharpNonePath });

			//Act
			DefaultCodeTemplate result = this.defaultCodeProvider.GetDefaultCodeTemplate(Arg.Any<string>(), templateName, eventName);

			//Assert
			Assert.IsNotNull(result);
			Assert.IsNull(result.AdvancedSourceCode);
			Assert.IsNull(result.AdvancedUnitTestsCode);
			Assert.AreEqual("CSharp", result.TempalteName);
			Assert.AreEqual(EventSpecificData.None, result.EventDataType);
			Assert.IsNotEmpty(result.WrapperSourceCode);
			Assert.IsNotEmpty(result.SimpleSourceCode);
			Assert.IsNotEmpty(result.SimpleUnitTestsCode);
		}
	}
}
