using System;
using System.IO;
using System.Text;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using NSubstitute;
using NUnit.Framework;
using MethodInfo = Aras.Method.Libs.Configurations.ProjectConfigurations.MethodInfo;

namespace Aras.VS.MethodPlugin.Tests.Code
{
	[TestFixture]
	public class CSharpCodeProviderTests
	{
		private const string partialClassTemplate = "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal class {4}\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}";
		private const string externalClassTemplate = "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    [ExternalPath(\"{2}\")]\r\n    internal class {4}\r\n    {{\r\n\r\n    }}\r\n}}";

		ICodeItemProvider codeItemProvider;
		ICodeFormatter codeFormatter;
		IIOWrapper iOWrapper;
		MessageManager messageManager;
		CSharpCodeProvider codeProvider;

		[SetUp]
		public void Init()
		{
			iOWrapper = Substitute.For<IIOWrapper>();
			codeItemProvider = Substitute.For<ICodeItemProvider>();
			codeFormatter = Substitute.For<ICodeFormatter>();
			messageManager = Substitute.For<MessageManager>();

			codeProvider = new CSharpCodeProvider( codeItemProvider, codeFormatter, iOWrapper, messageManager);
		}

		[Test]
		public void Ctor_CodeItemProvider_ShouldDefaultCodeProviderThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new CSharpCodeProvider(null, codeFormatter, iOWrapper, messageManager);
			}));
		}

		[Test]
		public void Ctor_CodeFormatter_ShouldDefaultCodeProviderThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new CSharpCodeProvider(codeItemProvider, null, iOWrapper, messageManager);
			}));
		}

		[Test]
		public void Ctor_IOWrapper_ShouldDefaultCodeProviderThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new CSharpCodeProvider(codeItemProvider, codeFormatter, null, messageManager);
			}));
		}

		[Test]
		public void Ctor_MessageManager_ShouldDefaultCodeProviderThrowArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
			{
				// Act
				new CSharpCodeProvider(codeItemProvider, codeFormatter, iOWrapper, null);
			}));
		}

		[Test]
		public void Language_ShouldBeCSharp()
		{
			//Arrange
			string expected = "C#";

			//Act
			var actual = codeProvider.Language;

			//Assert
			Assert.AreEqual(expected, actual);
		}

		[Test]
		[Ignore("Should be updated")]
		public void LoadMethodCode_ShouldReturnCorrectCode()
		{
			//Arrange
			var path = AppDomain.CurrentDomain.BaseDirectory;
			var sourceCode = File.ReadAllText(Path.Combine(path, "Code\\TestData\\LoadMethodCode\\SourceCode.txt"));
			var expected = File.ReadAllText(Path.Combine(path, "Code\\TestData\\LoadMethodCode\\ExpectedSourceCode.txt"));

			//Act
			var actual = codeProvider.LoadMethodCode(sourceCode, string.Empty);

			//Assert
			Assert.AreEqual(actual, expected);
		}

		[Test]
		public void LoadMethodCode_ShouldThrowExceptionEmptyCode()
		{
			//Arrange
			var sourceCode = "";

			//Act
			var testDelegate = new TestDelegate(() =>
			{
				codeProvider.LoadMethodCode(sourceCode, string.Empty);
			});

			//Assert
			Assert.Throws<ArgumentException>(testDelegate);
		}


		[Test]
		[Ignore("Should be updated")]
		public void LoadMethodCode_WithSinglePartial_ShouldReturnCorrectCode()
		{
			//Arrange
			var sourceCode = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Code\\TestData\\LoadMethodCode\\SourceCode.txt"));
			var partialCode = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Code\\TestData\\LoadMethodCode\\PartialsGetLicenseInfo.txt"));
			var expected = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Code\\TestData\\LoadMethodCode\\ExpectedSourceCodeWithSinglePartial.txt"));

			iOWrapper.PathCombine(Arg.Any<string>(), "Partials\\GetLicenseInfo.cs")
				.Returns("C:/Partials/GetLicenseInfo.cs");
			iOWrapper.FileReadAllText(Arg.Is("C:/Partials/GetLicenseInfo.cs"), Arg.Any<UTF8Encoding>())
				.Returns(partialCode);

			//Act
			var actual = codeProvider.LoadMethodCode(sourceCode, string.Empty);

			//Assert
			Assert.AreEqual(actual, expected);
		}

		[Test]
		[Ignore("Should be updated")]
		public void LoadMethodCode_WithPartials_ShouldReturnCorrectCode()
		{
			//Arrange
			var sourceCode = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Code\\TestData\\LoadMethodCode\\SourceCode.txt"));
			var getLicenseInfoCode = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Code\\TestData\\LoadMethodCode\\PartialsGetLicenseInfo.txt"));
			var itemTypeInfoCode = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Code\\TestData\\LoadMethodCode\\PartialsItemTypeInfo.txt"));
			var expected = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Code\\TestData\\LoadMethodCode\\ExpectedSourceCodeWithTwoPartials.txt"));

			iOWrapper.PathCombine(Arg.Any<string>(), "Partials\\GetLicenseInfo.cs")
				.Returns("C:/Partials/GetLicenseInfo.cs");
			iOWrapper.PathCombine(Arg.Any<string>(), "Partials\\ItemTypeInfo.cs")
				.Returns("C:/Partials/ItemTypeInfo.cs");
			iOWrapper.FileReadAllText(Arg.Is("C:/Partials/GetLicenseInfo.cs"), Arg.Any<UTF8Encoding>())
				.Returns(getLicenseInfoCode);
			iOWrapper.FileReadAllText(Arg.Is("C:/Partials/ItemTypeInfo.cs"), Arg.Any<UTF8Encoding>())
				.Returns(itemTypeInfoCode);

			//Act
			var actual = codeProvider.LoadMethodCode(sourceCode, string.Empty);

			//Assert
			Assert.AreEqual(actual, expected);
		}

		//[Test]
		//public void CreateWrapper_ShouldReturnCorrectInfo()
		//{
		//	//Arrange
		//	var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
		//	string defaultCodeTemplatesPath = Path.Combine(currentPath, "TestData");

		//	this.iOWrapper.DirectoryExists(defaultCodeTemplatesPath).Returns(true);
		//	this.iOWrapper.DirectoryGetFiles(defaultCodeTemplatesPath).Returns(Directory.GetFiles(defaultCodeTemplatesPath));

		//	var templateLoader = new TemplateLoader();
		//	templateLoader.Load(Path.Combine(currentPath, "TestData\\method-config.xml"));
		//	var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
		//	var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
		//	var methodName = "TestMethod";
		//	var isUsedVSFormatting = false;
		//	var expectedWrapper = File.ReadAllText(Path.Combine(currentPath, "Code\\TestData\\CreateWrapper\\WrapperCodeInfo.txt"));

		//	//Act
		//	var expected = codeProvider.CreateWrapper(template, eventData, methodName, isUsedVSFormatting, defaultCodeTemplatesPath);

		//	//Assert
		//	Assert.AreEqual(expected.ClassName, "ArasCLS" + methodName);
		//	Assert.AreEqual(expected.IsUseVSFormatting, isUsedVSFormatting);
		//	Assert.AreEqual(expected.MethodCodeParentClassName, "ItemMethod");
		//	Assert.AreEqual(expected.MethodName, methodName);
		//	Assert.AreEqual(expected.Namespace, "ArasPKG" + methodName);
		//	Assert.AreEqual(expected.PartialCodeInfoList.Count, 0);
		//	Assert.AreEqual(expected.WrapperCodeInfo.Code, expectedWrapper);
		//	Assert.AreEqual(expected.WrapperCodeInfo.Path, methodName + "\\" + methodName + "Wrapper.cs");
		//}

		//[Test]
		//public void CreateWrapper_ShouldThrowArgumentException()
		//{
		//	//Arrange
		//	var curentPath = AppDomain.CurrentDomain.BaseDirectory;
		//	var defaultCodeTemplatesPath = Path.Combine(curentPath, "TestData");
		//	var templateLoader = new TemplateLoader();
		//	templateLoader.Load(Path.Combine(curentPath, "TestData\\method-config.xml"));
		//	var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
		//	var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
		//	var methodName = "";
		//	var isUsedVSFormatting = false;

		//	//Act
		//	var testDelegate = new TestDelegate(() =>
		//	{
		//		codeProvider.CreateWrapper(template, eventData, methodName, isUsedVSFormatting, defaultCodeTemplatesPath);
		//	});

		//	//Assert
		//	Assert.Throws<ArgumentException>(testDelegate);
		//}


		//[Test]
		//public void CreateWrapper_EventData_ShouldThrowNullReferenceException()
		//{
		//	//Arrange
		//	var curentPath = AppDomain.CurrentDomain.BaseDirectory;
		//	var defaultCodeTemplatesPath = Path.Combine(curentPath, "TestData");
		//	var templateLoader = new TemplateLoader();
		//	templateLoader.Load(Path.Combine(curentPath, "TestData\\method-config.xml"));
		//	var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
		//	var methodName = "TestMethod";
		//	var isUsedVSFormatting = false;

		//	//Act
		//	var testDelegate = new TestDelegate(() =>
		//	{
		//		codeProvider.CreateWrapper(template, null, methodName, isUsedVSFormatting, defaultCodeTemplatesPath);
		//	});

		//	//Assert
		//	Assert.Throws<NullReferenceException>(testDelegate);
		//}

		//[Test]
		//public void CreateWrapper_Template_ShouldThrowNullReferenceException()
		//{
		//	//Arrange
		//	var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
		//	var methodName = "TestMethod";
		//	var isUsedVSFormatting = false;

		//	//Act
		//	var testDelegate = new TestDelegate(() =>
		//	{
		//		codeProvider.CreateWrapper(null, eventData, methodName, isUsedVSFormatting, string.Empty);
		//	});

		//	//Assert
		//	Assert.Throws<NullReferenceException>(testDelegate);
		//}

		//[Test]
		//public void CreateMainNew_ShouldReturnCorrectCodeInfo()
		//{
		//	//Arrange
		//	var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
		//	var defaultCodeTemplatesPath = Path.Combine(currentPath, "TestData");

		//	this.iOWrapper.DirectoryExists(defaultCodeTemplatesPath).Returns(true);
		//	this.iOWrapper.DirectoryGetFiles(defaultCodeTemplatesPath).Returns(Directory.GetFiles(defaultCodeTemplatesPath));

		//	var templateLoader = new TemplateLoader();
		//	templateLoader.Load(Path.Combine(currentPath, "TestData\\method-config.xml"));
		//	var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");

		//	var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
		//	var methodName = "TestMethod";
		//	var generatedCodeInfo = new GeneratedCodeInfo()
		//	{
		//		ClassName = "ArasCLS" + methodName,
		//		IsUseVSFormatting = false,
		//		Namespace = "ArasPKG" + methodName,
		//		MethodName = methodName,
		//		MethodCodeParentClassName = "ItemMethod",
		//	};

		//	//Act
		//	var expected = codeProvider.CreateMainNew(generatedCodeInfo, template, eventData, methodName, false, "\r\n\r\n", defaultCodeTemplatesPath);

		//	//Assert
		//	Assert.AreEqual(expected.IsUseVSFormatting, generatedCodeInfo.IsUseVSFormatting);
		//	Assert.AreEqual(expected.MethodCodeParentClassName, generatedCodeInfo.MethodCodeParentClassName);
		//	Assert.AreEqual(expected.ClassName, generatedCodeInfo.ClassName);
		//	Assert.AreEqual(expected.Namespace, generatedCodeInfo.Namespace);
		//	Assert.AreEqual(expected.MethodName, generatedCodeInfo.MethodName);
		//	Assert.AreEqual(expected.MethodCodeInfo.Code, File.ReadAllText(Path.Combine(currentPath, "Code\\TestData\\CreateMainNew\\MethodCode.txt")));
		//	Assert.AreEqual(expected.MethodCodeInfo.Path, methodName + "\\" + methodName + ".cs");
		//}

		//[Test]
		//public void CreateMainNew_ShouldReturnTemplateCode()
		//{
		//	//Arrange
		//	var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
		//	string defaultCodeTemplatesPath = Path.Combine(currentPath, "TestData");

		//	this.iOWrapper.DirectoryExists(defaultCodeTemplatesPath).Returns(true);
		//	this.iOWrapper.DirectoryGetFiles(defaultCodeTemplatesPath).Returns(Directory.GetFiles(defaultCodeTemplatesPath));

		//	var templateLoader = new TemplateLoader();
		//	templateLoader.Load(Path.Combine(currentPath, "TestData\\method-config.xml"));

		//	var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
		//	var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
		//	var methodName = "TestMethod";
		//	var generatedCodeInfo = new GeneratedCodeInfo()
		//	{
		//		ClassName = "ArasCLS" + methodName,
		//		IsUseVSFormatting = false,
		//		Namespace = "ArasPKG" + methodName,
		//	};

		//	//Act
		//	var expected = codeProvider.CreateMainNew(generatedCodeInfo, template, eventData, methodName, false, "", defaultCodeTemplatesPath);

		//	//Assert
		//	Assert.AreEqual(expected.MethodCodeInfo.Code, File.ReadAllText(Path.Combine(currentPath, "Code\\TestData\\CreateMainNew\\DefaultTemplateCode.txt")));
		//}

		//[Test]
		//public void CreatePartialClasses_ShouldReturnCorrectCodeWithOnePartial()
		//{
		//	// Arrange
		//	var curentPath = System.AppDomain.CurrentDomain.BaseDirectory;
		//	var methodName = "TestMethod";
		//	var generatedCodeInfo = new GeneratedCodeInfo()
		//	{
		//		ClassName = "ArasCLS" + methodName,
		//		IsUseVSFormatting = false,
		//		Namespace = "ArasPKG" + methodName,
		//		MethodName = methodName,
		//		MethodCodeParentClassName = "ItemMethod",
		//		MethodCodeInfo = new CodeInfo
		//		{
		//			Code = File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\MethodCodeInfo.txt"))
		//		},
		//	};

		//	//Act
		//	var expected = codeProvider.CreatePartialClasses(generatedCodeInfo);

		//	//Assert
		//	Assert.AreEqual(expected.IsUseVSFormatting, generatedCodeInfo.IsUseVSFormatting);
		//	Assert.AreEqual(expected.MethodCodeParentClassName, generatedCodeInfo.MethodCodeParentClassName);
		//	Assert.AreEqual(expected.ClassName, generatedCodeInfo.ClassName);
		//	Assert.AreEqual(expected.Namespace, generatedCodeInfo.Namespace);
		//	Assert.AreEqual(expected.MethodName, generatedCodeInfo.MethodName);
		//	Assert.AreEqual(expected.MethodCodeInfo.Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\ExpectedMainPart.txt")));
		//	Assert.AreEqual(expected.PartialCodeInfoList.Count, 1);
		//	Assert.AreEqual(expected.PartialCodeInfoList.First().Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\ExpectedPartialPart.txt")));
		//	Assert.AreEqual(expected.PartialCodeInfoList.First().Path, "TestMethod\\TestPartial");
		//}

		//[Test]
		//public void CreatePartialClasses_ShouldReturnCorrectCodeWithTwoPartials()
		//{
		//	// Arrange
		//	var curentPath = System.AppDomain.CurrentDomain.BaseDirectory;
		//	var methodName = "TestMethod";
		//	var generatedCodeInfo = new GeneratedCodeInfo()
		//	{
		//		ClassName = "ArasCLS" + methodName,
		//		IsUseVSFormatting = false,
		//		Namespace = "ArasPKG" + methodName,
		//		MethodName = methodName,
		//		MethodCodeParentClassName = "ItemMethod",
		//		MethodCodeInfo = new CodeInfo
		//		{
		//			Code = File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\MethodCodeInfo2Partial.txt"))
		//		},
		//	};

		//	//Act
		//	var expected = codeProvider.CreatePartialClasses(generatedCodeInfo);

		//	//Assert
		//	Assert.AreEqual(expected.PartialCodeInfoList.Count, 2);
		//	Assert.AreEqual(expected.MethodCodeInfo.Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\ExpectedMainPartFor2Partial.txt")));
		//	Assert.AreEqual(expected.PartialCodeInfoList.First().Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\ExpectedFirstPartialClass.txt")));
		//	Assert.AreEqual(expected.PartialCodeInfoList.First().Path, "TestMethod\\FirstTestPartial");
		//	Assert.AreEqual(expected.PartialCodeInfoList.Last().Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\ExpectedSecondPartialClass.txt")));
		//	Assert.AreEqual(expected.PartialCodeInfoList.Last().Path, "TestMethod\\Second\\SeconfTestPartial");
		//}

		//[Test]
		//public void CreateCodeItemInfo_Partial_ShouldReturnExpectedCode()
		//{
		//	// Arrange
		//	codeItemProvider
		//		.GetCodeElementTypeTemplate(CodeType.Partial, CodeElementType.Class)
		//		.Returns(partialClassTemplate);

		//	var currentPath = AppDomain.CurrentDomain.BaseDirectory;
		//	string defaultCodeTemplatesPath = Path.Combine(currentPath, "TestData");
		//	string methodConfigPath = Path.Combine(currentPath, "TestData\\method-config.xml");
		//	string serverMethodFolderPath = Path.Combine(currentPath, "Code\\TestData\\");
		//	string selectedFolderPath = Path.Combine(currentPath, "Code\\TestData\\CreateCodeItemInfo");
		//	string methodName = "CreateCodeItemInfo";
		//	string methodPath = Path.Combine(currentPath, "Code\\TestData\\CreateCodeItemInfo\\MethodCode.txt");

		//	this.iOWrapper.DirectoryExists(defaultCodeTemplatesPath).Returns(true);
		//	this.iOWrapper.DirectoryGetFiles(defaultCodeTemplatesPath).Returns(Directory.GetFiles(defaultCodeTemplatesPath));

		//	var fileName = "TestFile";
		//	var methodInfo = new MethodInfo
		//	{
		//		MethodLanguage = @"C#",
		//		TemplateName = "CSharp",
		//		EventData = EventSpecificData.None,
		//	};

		//	// Act
		//	var expected = codeProvider.CreateCodeItemInfo(methodInfo, fileName, CodeType.Partial, CodeElementType.Class, false, serverMethodFolderPath, selectedFolderPath, methodName, methodConfigPath, methodPath, defaultCodeTemplatesPath);

		//	// Asseer
		//	Assert.AreEqual(expected.Path, @"CreateCodeItemInfo\TestFile");
		//	Assert.AreEqual(expected.Code, File.ReadAllText(Path.Combine(currentPath, "Code\\TestData\\CreateCodeItemInfo\\PartialClassEmpty.txt")));
		//}

		//[Test]
		//public void CreateCodeItemInfo_External_ShouldReturnExpectedCode()
		//{
		//	// Arrange
		//	codeItemProvider
		//		.GetCodeElementTypeTemplate(CodeType.External, CodeElementType.Class)
		//		.Returns(externalClassTemplate);

		//	var currentPath = AppDomain.CurrentDomain.BaseDirectory;
		//	string defaultCodeTemplatesPath = Path.Combine(currentPath, "TestData");
		//	string methodConfigPath = Path.Combine(currentPath, "TestData\\method-config.xml");
		//	string serverMethodFolderPath = Path.Combine(currentPath, "Code\\TestData\\");
		//	string selectedFolderPath = Path.Combine(currentPath, "Code\\TestData\\CreateCodeItemInfo");
		//	string methodName = "CreateCodeItemInfo";
		//	string methodPath = Path.Combine(currentPath, "Code\\TestData\\CreateCodeItemInfo\\MethodCode.txt");

		//	this.iOWrapper.DirectoryExists(defaultCodeTemplatesPath).Returns(true);
		//	this.iOWrapper.DirectoryGetFiles(defaultCodeTemplatesPath).Returns(Directory.GetFiles(defaultCodeTemplatesPath));

		//	var fileName = "TestFile";
		//	var methodInfo = new MethodInfo
		//	{
		//		MethodLanguage = @"C#",
		//		TemplateName = "CSharp",
		//		EventData = EventSpecificData.None,
		//	};

		//	// Act
		//	var expected = codeProvider.CreateCodeItemInfo(methodInfo, fileName, CodeType.External, CodeElementType.Class, false, serverMethodFolderPath, selectedFolderPath, methodName, methodConfigPath, methodPath, defaultCodeTemplatesPath);

		//	// Asseer
		//	Assert.AreEqual(expected.Path, @"CreateCodeItemInfo\TestFile");
		//	Assert.AreEqual(expected.Code, File.ReadAllText(Path.Combine(currentPath, "Code\\TestData\\CreateCodeItemInfo\\ExternalClassEmpty.txt")));
		//}

		//[Test]
		//public void CreateTestsNew_ShouldReturnCorrectTestCode()
		//{
		//	//Arrange
		//	var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
		//	string defaultCodeTemplatesPath = Path.Combine(currentPath, "TestData");

		//	this.iOWrapper.DirectoryExists(defaultCodeTemplatesPath).Returns(true);
		//	this.iOWrapper.DirectoryGetFiles(defaultCodeTemplatesPath).Returns(Directory.GetFiles(defaultCodeTemplatesPath));

		//	var templateLoader = new TemplateLoader();
		//	templateLoader.Load(Path.Combine(currentPath, "TestData\\method-config.xml"));
		//	var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
		//	var methodName = "MethodTest";
		//	var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
		//	var generatedCodeInfo = new GeneratedCodeInfo()
		//	{
		//		ClassName = "ArasCLS" + methodName,
		//		IsUseVSFormatting = false,
		//		Namespace = "ArasPKG" + methodName,
		//	};

		//	//Act
		//	var expected = codeProvider.CreateTestsNew(generatedCodeInfo, template, eventData, methodName, false, defaultCodeTemplatesPath);

		//	//Assert
		//	Assert.AreEqual(expected.TestsCodeInfo.Path, @"MethodTest\MethodTestTests.cs");
		//	Assert.AreEqual(expected.TestsCodeInfo.Code, File.ReadAllText(Path.Combine(currentPath, "Code\\TestData\\CreateTestsNew\\ExpectedTestCode.txt")));
		//}
	}
}