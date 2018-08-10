//------------------------------------------------------------------------------
// <copyright file="IDialogFactory.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Dialogs
{
	public interface IDialogFactory
	{
		ItemSearchPresenter GetItemSearchPresenter(string itemTypeName, string itemTypeSingularLabel);

		ItemSearchPresenter GetItemSearchPresenter(ISearcher searcher);

		LoginViewAdapter GetLoginView(IProjectManager projectManager, ProjectConfiguraiton projectConfiguration);

		CreateMethodViewAdapter GetCreateView(IVsUIShell uiShell,
			ProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			IProjectManager projectManager,
			string projectLanguage);

		ConnectionInfoViewAdapter GetConnectionInfoView(IProjectManager projectManager, ProjectConfigurationManager configurationManager);

		OpenFromArasViewAdapter GetOpenFromArasView(IVsUIShell uiShell, ProjectConfigurationManager configurationManager,
		   ProjectConfiguraiton projectConfiguration,
		   TemplateLoader templateLoader,
		   PackageManager packageManager,
		   string pathToProjectConfigFile,
		   string projectName,
		   string projectFullName,
		   string projectLanguage);

		OpenFromPackageViewAdapter GetOpenFromPackageView(IVsUIShell uiShell, TemplateLoader templateLoader, string projectLanguage, string lastSelectedDirectory);

		SaveMethodViewAdapter GetSaveToArasView(IVsUIShell uiShell,
			ProjectConfigurationManager projectConfigurationManager,
			ProjectConfiguraiton projectConfiguration,
			PackageManager packageManager,
			MethodInfo methodInformation,
			string methodCode,
			string projectConfigPath,
			string projectName,
			string projectFullName);

		SaveToPackageViewAdapter GetSaveToPackageView(IVsUIShell uiShell,
			ProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			ICodeProvider codeProvider,
			IProjectManager projectManager,
			MethodInfo methodInformation,
			string pathToFileForSave);

		UpdateFromArasViewAdapter GetUpdateFromArasView(IVsUIShell uiShell,
			ProjectConfigurationManager projectConfigurationManager,
			ProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			MethodInfo methodInfo,
			string projectConfigPath,
			string projectName,
			string projectFullName);

		CreatePartialElementViewAdapter GetCreatePartialClassView(IVsUIShell uiShell);

		DebugMethodViewAdapter GetDebugMethodView(IVsUIShell uiShell,
			ProjectConfigurationManager projectConfigurationManager,
			ProjectConfiguraiton projectConfiguration,
			MethodInfo methodInformation,
			string methodCode,
			string projectConfigPath,
			string projectName,
			string projectFullName);

		MessageBoxWindow GetMessageBoxWindow(IVsUIShell uiShell);
	}
}
