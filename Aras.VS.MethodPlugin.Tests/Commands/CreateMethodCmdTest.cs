using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using NUnit.Framework;
using Aras.VS.MethodPlugin.Commands;
using NSubstitute;
using Aras.VS.MethodPlugin.Authentication;
using Microsoft.VisualStudio.Shell.Interop;
using Aras.VS.MethodPlugin.Dialogs.Views;
using System.Threading;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.PackageManagement;
using System.IO;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Dynamic;
using Aras.VS.MethodPlugin.Templates;
using Aras.VS.MethodPlugin.Tests.Stubs;

namespace Aras.VS.MethodPlugin.Tests.Commands
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class CreateMethodCmdTest
    {
        IAuthenticationManager authManager;
        IProjectManager projectManager;
        IDialogFactory dialogFactory;
        ProjectConfigurationManager projectConfigurationManager;
        CreateMethodCmd createMethodCmd;
        IVsUIShell iVsUIShell;
        ICodeProviderFactory codeProviderFactory;
        static TemplateInfo template;
        static EventSpecificDataType eventSpecificDataType;
        TemplateLoader templateLoader;
        PackageManager packageManager;
        ICodeProvider codeProvider;
        IProjectConfiguraiton projectConfiguration;


        [SetUp]
        public void Init()
        {
            projectManager = Substitute.For<IProjectManager>();
            projectConfigurationManager = new ProjectConfigurationManager();
            dialogFactory = Substitute.For<IDialogFactory>();
            authManager = new AuthManagerStub();
            codeProviderFactory = Substitute.For<ICodeProviderFactory>(); 
            CreateMethodCmd.Initialize(projectManager, authManager, dialogFactory, projectConfigurationManager, codeProviderFactory);
            createMethodCmd = CreateMethodCmd.Instance;
            iVsUIShell = Substitute.For<IVsUIShell>();
            var currentPath = AppDomain.CurrentDomain.BaseDirectory;
            projectManager.ProjectConfigPath.Returns(Path.Combine(currentPath, "TestData\\projectConfig.xml"));
            projectManager.MethodConfigPath.Returns(Path.Combine(currentPath, "TestData\\method-config.xml"));
            template = new TemplateInfo { TemplateName = string.Empty };
            eventSpecificDataType = new EventSpecificDataType { EventSpecificData = EventSpecificData.None };
            templateLoader = new TemplateLoader();
            packageManager = new PackageManager(authManager);
            codeProvider = Substitute.For<ICodeProvider>();
            projectConfiguration = projectConfigurationManager.Load(projectManager.ProjectConfigPath);
            templateLoader.Load(projectManager.MethodConfigPath);
            codeProviderFactory.GetCodeProvider(null, null).ReturnsForAnyArgs(codeProvider);
        }


        [Test]
        public void ExecuteCommandImpl_ShouldReceivedGetCreateView()
        {
            // Arrange
           
            codeProvider.GenerateCodeInfo(null, Arg.Any<EventSpecificDataType>(), null, Arg.Any<bool>(), null, Arg.Any<bool>()).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());
            dialogFactory.GetCreateView(iVsUIShell, projectConfiguration, templateLoader, packageManager, projectManager, codeProvider.Language).ReturnsForAnyArgs(Substitute.For<CreateMethodViewAdapterTest>());
         
            //Act
            createMethodCmd.ExecuteCommandImpl(null, null, iVsUIShell);

            // Assert
            dialogFactory.Received().GetCreateView(iVsUIShell, Arg.Any<ProjectConfiguraiton>(), Arg.Any<TemplateLoader>(), Arg.Any<PackageManager>(), projectManager, codeProvider.Language);
        }

        [Test]
        public void ExecuteCommandImpl_ShouldReceivedGenerateCodeInfo()
        {
            // Arrange
            var createMethodViewAdapterTest = Substitute.For<CreateMethodViewAdapterTest>();
            codeProvider.GenerateCodeInfo(null, Arg.Any<EventSpecificDataType>(), null, Arg.Any<bool>(), null, Arg.Any<bool>()).ReturnsForAnyArgs(Substitute.For<GeneratedCodeInfo>());
            dialogFactory.GetCreateView(iVsUIShell, projectConfiguration, templateLoader, packageManager, projectManager, codeProvider.Language).ReturnsForAnyArgs(createMethodViewAdapterTest);
            var showDialogResult = createMethodViewAdapterTest.ShowDialog();

            //Act
            createMethodCmd.ExecuteCommandImpl(null, null, iVsUIShell);

            // Assert
            codeProvider.Received().GenerateCodeInfo(template, eventSpecificDataType, showDialogResult.MethodName, false, string.Empty, false);
        }


        public class CreateMethodViewAdapterTest : IViewAdaper<CreateMethodView, CreateMethodViewResult>
        {
            public CreateMethodViewResult ShowDialog()
            {
                return new CreateMethodViewResult
                {
                    DialogOperationResult = true,
                    SelectedLanguage = new FilteredListInfo(string.Empty, string.Empty, string.Empty),
                    MethodName = string.Empty,
                    SelectedActionLocation = new ListInfo(),
                    MethodComment = string.Empty,
                    SelectedPackage = string.Empty,
                    SelectedTemplate = template,
                    SelectedEventSpecificData = eventSpecificDataType,
                    SelectedIdentityId = string.Empty,
                    SelectedIdentityKeyedName = string.Empty,
                    UseRecommendedDefaultCode = false,
                    IsUseVSFormattingCode = false
                };
            }
        }
    }
}
