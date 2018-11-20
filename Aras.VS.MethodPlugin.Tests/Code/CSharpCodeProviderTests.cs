using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MethodInfo = Aras.VS.MethodPlugin.ProjectConfigurations.MethodInfo;

namespace Aras.VS.MethodPlugin.Tests.Code
{
    [TestFixture]
    public class CSharpCodeProviderTests
    {
        CSharpCodeProvider codeProvider;
        IProjectManager projectManager;
        IProjectConfiguraiton projectConfiguration;
        DefaultCodeProvider defaultCodeProvider;

        [SetUp]
        public void Init()
        {
            projectManager = Substitute.For<IProjectManager>();
            projectConfiguration = new ProjectConfiguraiton();
            defaultCodeProvider = new DefaultCodeProvider();
            codeProvider = new CSharpCodeProvider(projectManager, projectConfiguration, defaultCodeProvider);
        }

        [Test]
        public void Ctor_CSharpCodeProvider_ShouldIProjectManagerThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
            {
                // Act
                new CSharpCodeProvider(null, projectConfiguration, defaultCodeProvider);
            }));
        }

        [Test]
        public void Ctor_CSharpCodeProvider_ShouldProjectConfiguraitonThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
            {
                // Act
                new CSharpCodeProvider(projectManager, null, defaultCodeProvider);
            }));
        }

        [Test]
        public void Ctor_CSharpCodeProvider_ShouldDefaultCodeProviderThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(new TestDelegate(() =>
            {
                // Act
                new CSharpCodeProvider(projectManager, projectConfiguration, null);
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
        public void LoadMethodCode_ShouldReturnCorrectCode()
        {
            //Arrange
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var sourceCode = File.ReadAllText(Path.Combine(path, "Code\\TestData\\LoadMethodCode\\SourceCode.txt"));
            var expected = File.ReadAllText(Path.Combine(path, "Code\\TestData\\LoadMethodCode\\ExpectedSourceCode.txt"));

            //Act
            var actual = codeProvider.LoadMethodCode(sourceCode, Substitute.For<MethodInfo>(), projectManager.ServerMethodFolderPath);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void LoadMethodCode_ShouldThrowExceptionEmptyCode()
        {
            //Arrange
            var sourceCode = "";

            //Act
            var testDelegate = new TestDelegate(() =>
            {
                codeProvider.LoadMethodCode(sourceCode, Substitute.For<MethodInfo>(), projectManager.ServerMethodFolderPath);
            });

            //Assert
            Assert.Throws<Exception>(testDelegate);
        }

        [Test]
        public void CreateWrapper_ShouldReturnCorrectInfo()
        {
            //Arrange
            var curentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            projectManager.DefaultCodeTemplatesPath.Returns(Path.Combine(curentPath, "TestData"));
            var templateLoader = new TemplateLoader();
            templateLoader.Load(Path.Combine(curentPath, "TestData\\method-config.xml"));
            var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
            var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
            var methodName = "TestMethod";
            var isUsedVSFormatting = false;
            var expectedWrapper = File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreateWrapper\\WrapperCodeInfo.txt"));

            //Act
            var expected = codeProvider.CreateWrapper(template, eventData, methodName, isUsedVSFormatting);

            //Assert
            Assert.AreEqual(expected.ClassName, "ArasCLS" + methodName);
            Assert.AreEqual(expected.IsUseVSFormatting, isUsedVSFormatting);
            Assert.AreEqual(expected.MethodCodeParentClassName, "ItemMethod");
            Assert.AreEqual(expected.MethodName, methodName);
            Assert.AreEqual(expected.Namespace, "ArasPKG" + methodName);
            Assert.AreEqual(expected.PartialCodeInfoList.Count, 0);
            Assert.AreEqual(expected.WrapperCodeInfo.Code, expectedWrapper);
            Assert.AreEqual(expected.WrapperCodeInfo.Path, methodName + "\\" + methodName + "Wrapper.cs");
        }

        [Test]
        public void CreateWrapper_ShouldThrowArgumentException()
        {
            //Arrange
            var curentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            projectManager.DefaultCodeTemplatesPath.Returns(Path.Combine(curentPath, "TestData"));
            var templateLoader = new TemplateLoader();
            templateLoader.Load(Path.Combine(curentPath, "TestData\\method-config.xml"));
            var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
            var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
            var methodName = "";
            var isUsedVSFormatting = false;

            //Act
            var testDelegate = new TestDelegate(() =>
            {
                codeProvider.CreateWrapper(template, eventData, methodName, isUsedVSFormatting);
            });

            //Assert
            Assert.Throws<ArgumentException>(testDelegate);
        }


        [Test]
        public void CreateWrapper_EventData_ShouldThrowNullReferenceException()
        {
            //Arrange
            var curentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            projectManager.DefaultCodeTemplatesPath.Returns(Path.Combine(curentPath, "TestData"));
            var templateLoader = new TemplateLoader();
            templateLoader.Load(Path.Combine(curentPath, "TestData\\method-config.xml"));
            var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
            var methodName = "TestMethod";
            var isUsedVSFormatting = false;

            //Act
            var testDelegate = new TestDelegate(() =>
            {
                codeProvider.CreateWrapper(template, null, methodName, isUsedVSFormatting);
            });

            //Assert
            Assert.Throws<NullReferenceException>(testDelegate);
        }

        [Test]
        public void CreateWrapper_Template_ShouldThrowNullReferenceException()
        {
            //Arrange
            var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
            var methodName = "TestMethod";
            var isUsedVSFormatting = false;

            //Act
            var testDelegate = new TestDelegate(() =>
            {
                codeProvider.CreateWrapper(null, eventData, methodName, isUsedVSFormatting);
            });

            //Assert
            Assert.Throws<NullReferenceException>(testDelegate);
        }

        [Test]
        public void CreateMainNew_ShouldReturnCorrectCodeInfo()
        {
            //Arrange
            var curentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            projectManager.DefaultCodeTemplatesPath.Returns(Path.Combine(curentPath, "TestData"));
            var templateLoader = new TemplateLoader();
            templateLoader.Load(Path.Combine(curentPath, "TestData\\method-config.xml"));
            var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
            var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
            var methodName = "TestMethod";
            var generatedCodeInfo = new GeneratedCodeInfo()
            {
                ClassName = "ArasCLS" + methodName,
                IsUseVSFormatting = false,
                Namespace = "ArasPKG" + methodName,
                MethodName = methodName,
                MethodCodeParentClassName = "ItemMethod",
            };

            //Act
            var expected = codeProvider.CreateMainNew(generatedCodeInfo, template, eventData, methodName, false, "\r\n\r\n");

            //Assert
            Assert.AreEqual(expected.IsUseVSFormatting, generatedCodeInfo.IsUseVSFormatting);
            Assert.AreEqual(expected.MethodCodeParentClassName, generatedCodeInfo.MethodCodeParentClassName);
            Assert.AreEqual(expected.ClassName, generatedCodeInfo.ClassName);
            Assert.AreEqual(expected.Namespace, generatedCodeInfo.Namespace);
            Assert.AreEqual(expected.MethodName, generatedCodeInfo.MethodName);
            Assert.AreEqual(expected.MethodCodeInfo.Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreateMainNew\\MethodCode.txt")));
            Assert.AreEqual(expected.MethodCodeInfo.Path, methodName + "\\" + methodName + ".cs");
        }

        [Test]
        public void CreateMainNew_ShouldReturnTemplateCode()
        {
            //Arrange
            var curentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            projectManager.DefaultCodeTemplatesPath.Returns(Path.Combine(curentPath, "TestData"));
            var templateLoader = new TemplateLoader();
            templateLoader.Load(Path.Combine(curentPath, "TestData\\method-config.xml"));
            var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
            var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
            var methodName = "TestMethod";
            var generatedCodeInfo = new GeneratedCodeInfo()
            {
                ClassName = "ArasCLS" + methodName,
                IsUseVSFormatting = false,
                Namespace = "ArasPKG" + methodName,
            };

            //Act
            var expected = codeProvider.CreateMainNew(generatedCodeInfo, template, eventData, methodName, false, "");

            //Assert
            Assert.AreEqual(expected.MethodCodeInfo.Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreateMainNew\\DefaultTemplateCode.txt")));
        }

        [Test]
        public void CreatePartialClasses_ShouldReturnCorrectCodeWithOnePartial()
        {
            // Arrange
            var curentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            var methodName = "TestMethod";
            var generatedCodeInfo = new GeneratedCodeInfo()
            {
                ClassName = "ArasCLS" + methodName,
                IsUseVSFormatting = false,
                Namespace = "ArasPKG" + methodName,
                MethodName = methodName,
                MethodCodeParentClassName = "ItemMethod",
                MethodCodeInfo = new CodeInfo
                {
                    Code = File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\MethodCodeInfo.txt"))
                },                
            };

            //Act
            var expected = codeProvider.CreatePartialClasses(generatedCodeInfo);

            //Assert
            Assert.AreEqual(expected.IsUseVSFormatting, generatedCodeInfo.IsUseVSFormatting);
            Assert.AreEqual(expected.MethodCodeParentClassName, generatedCodeInfo.MethodCodeParentClassName);
            Assert.AreEqual(expected.ClassName, generatedCodeInfo.ClassName);
            Assert.AreEqual(expected.Namespace, generatedCodeInfo.Namespace);
            Assert.AreEqual(expected.MethodName, generatedCodeInfo.MethodName);
            Assert.AreEqual(expected.MethodCodeInfo.Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\ExpectedMainPart.txt")));
            Assert.AreEqual(expected.PartialCodeInfoList.Count, 1);
            Assert.AreEqual(expected.PartialCodeInfoList.First().Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\ExpectedPartialPart.txt")));
            Assert.AreEqual(expected.PartialCodeInfoList.First().Path, "TestMethod\\TestPartial");
        }

        [Test]
        public void CreatePartialClasses_ShouldReturnCorrectCodeWithTwoPartials()
        {
            // Arrange
            var curentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            var methodName = "TestMethod";
            var generatedCodeInfo = new GeneratedCodeInfo()
            {
                ClassName = "ArasCLS" + methodName,
                IsUseVSFormatting = false,
                Namespace = "ArasPKG" + methodName,
                MethodName = methodName,
                MethodCodeParentClassName = "ItemMethod",
                MethodCodeInfo = new CodeInfo
                {
                    Code = File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\MethodCodeInfo2Partial.txt"))
                },
            };

            //Act
            var expected = codeProvider.CreatePartialClasses(generatedCodeInfo);

            //Assert
            Assert.AreEqual(expected.PartialCodeInfoList.Count, 2);
            Assert.AreEqual(expected.MethodCodeInfo.Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\ExpectedMainPartFor2Partial.txt")));
            Assert.AreEqual(expected.PartialCodeInfoList.First().Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\ExpectedFirstPartialClass.txt")));
            Assert.AreEqual(expected.PartialCodeInfoList.First().Path, "TestMethod\\FirstTestPartial");
            Assert.AreEqual(expected.PartialCodeInfoList.Last().Code, File.ReadAllText(Path.Combine(curentPath, "Code\\TestData\\CreatePartialClasses\\ExpectedSecondPartialClass.txt")));
            Assert.AreEqual(expected.PartialCodeInfoList.Last().Path, "TestMethod\\Second\\SeconfTestPartial");
        }

        [Test]
        public void CreatePartialCodeInfo_ShouldReturnEmptyCode()
        {
            //Arrange
            var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            projectManager.MethodConfigPath.Returns(Path.Combine(currentPath, "TestData\\method-config.xml"));
            projectManager.ServerMethodFolderPath.Returns(Path.Combine(currentPath, "Code\\TestData\\"));
            projectManager.MethodName.Returns(Path.Combine(currentPath, "TestMethod"));
            projectManager.SelectedFolderPath.Returns(Path.Combine(currentPath, "Code\\TestData\\CreatePartialCodeInfo"));
            projectManager.MethodName.Returns("CreatePartialCodeInfo");
            projectManager.DefaultCodeTemplatesPath.Returns(Path.Combine(currentPath, "TestData"));
            projectManager.MethodPath.Returns(Path.Combine(currentPath, "Code\\TestData\\CreatePartialCodeInfo\\MethodCode.txt"));
            var fileName = "TestFile";
            var methodInfo = new MethodInfo
            {
                MethodLanguage = @"C#",
                TemplateName = "CSharp",
                EventData = EventSpecificData.None,
            };

            //Act
            var expected = codeProvider.CreatePartialCodeInfo(methodInfo, fileName, false);

            //Assert
            Assert.AreEqual(expected.Path, @"CreatePartialCodeInfo\TestFile");
            Assert.AreEqual(expected.Code, File.ReadAllText(Path.Combine(currentPath, "Code\\TestData\\CreatePartialCodeInfo\\ExpectedEmptyCode.txt")));
        }

        [Test]
        public void CreatePartialCodeInfo_ShouldAddNewUsing()
        {
            //Arrange
            var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            projectManager.MethodConfigPath.Returns(Path.Combine(currentPath, "TestData\\method-config.xml"));
            projectManager.ServerMethodFolderPath.Returns(Path.Combine(currentPath, "Code\\TestData\\"));
            projectManager.MethodName.Returns(Path.Combine(currentPath, "TestMethod"));
            projectManager.SelectedFolderPath.Returns(Path.Combine(currentPath, "Code\\TestData\\CreatePartialCodeInfo"));
            projectManager.MethodName.Returns("CreatePartialCodeInfo");
            projectManager.DefaultCodeTemplatesPath.Returns(Path.Combine(currentPath, "TestData"));
            projectManager.MethodPath.Returns(Path.Combine(currentPath, "Code\\TestData\\CreatePartialCodeInfo\\MethodCode.txt"));
            var fileName = "TestFile";
            var methodInfo = new MethodInfo
            {
                MethodLanguage = @"C#",
                TemplateName = "CSharp",
                EventData = EventSpecificData.None,
            };

            //Act
            var expected = codeProvider.CreatePartialCodeInfo(methodInfo, fileName, false);

            //Assert
            Assert.AreEqual(expected.Path, @"CreatePartialCodeInfo\TestFile");
            Assert.AreEqual(expected.Code, File.ReadAllText(Path.Combine(currentPath, "Code\\TestData\\CreatePartialCodeInfo\\ExpectedEmptyCode.txt")));
        }

        [Test]
        public void CreatePartialCodeInfo_ShouldReturnPathWithSlashes()
        {
            //Arrange
            var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            projectManager.MethodConfigPath.Returns(Path.Combine(currentPath, "TestData\\method-config.xml"));
            projectManager.ServerMethodFolderPath.Returns(Path.Combine(currentPath, "Code\\TestData"));
            projectManager.MethodName.Returns(Path.Combine(currentPath, "TestMethod"));
            projectManager.SelectedFolderPath.Returns(Path.Combine(currentPath, "Code\\TestData\\CreatePartialCodeInfo"));
            projectManager.MethodName.Returns("CreatePartialCodeInfo");
            projectManager.DefaultCodeTemplatesPath.Returns(Path.Combine(currentPath, "TestData"));
            projectManager.MethodPath.Returns(Path.Combine(currentPath, "Code\\TestData\\CreatePartialCodeInfo\\MethodCode.txt"));
            var fileName = "TestFile";
            var methodInfo = new MethodInfo
            {
                MethodLanguage = @"C#",
                TemplateName = "CSharp",
                EventData = EventSpecificData.None,
            };

            //Act
            var expected = codeProvider.CreatePartialCodeInfo(methodInfo, fileName, false);

            //Assert
            Assert.AreEqual(expected.Path, @"\CreatePartialCodeInfo\TestFile");
            Assert.AreEqual(expected.Code, File.ReadAllText(Path.Combine(currentPath, "Code\\TestData\\CreatePartialCodeInfo\\ExpectedEmptyCode.txt")));
        }


        [Test]
        public void CreateTestsNew_ShouldReturnCorrectTestCode()
        {
            //Arrange
            var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
            projectManager.DefaultCodeTemplatesPath.Returns(Path.Combine(currentPath, "TestData"));
            var templateLoader = new TemplateLoader();
            templateLoader.Load(Path.Combine(currentPath, "TestData\\method-config.xml"));
            var template = templateLoader.Templates.FirstOrDefault(tmp => tmp.TemplateName == "CSharp");
            var methodName = "MethodTest";
            var eventData = CommonData.EventSpecificDataTypeList.FirstOrDefault(ed => ed.EventSpecificData == EventSpecificData.None);
            var generatedCodeInfo = new GeneratedCodeInfo()
            {
                ClassName = "ArasCLS" + methodName,
                IsUseVSFormatting = false,
                Namespace = "ArasPKG" + methodName,
            };

            //Act
            var expected = codeProvider.CreateTestsNew(generatedCodeInfo, template, eventData, methodName, false );

            //Assert
            Assert.AreEqual(expected.TestsCodeInfo.Path, @"MethodTest\MethodTestTests.cs");
            Assert.AreEqual(expected.TestsCodeInfo.Code, File.ReadAllText(Path.Combine(currentPath, "Code\\TestData\\CreateTestsNew\\ExpectedTestCode.txt")));
        }








    }
}