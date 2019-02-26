//------------------------------------------------------------------------------
// <copyright file="IDialogFactory.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using OfficeConnector.Dialogs;
using Aras.VS.MethodPlugin.Configurations;

namespace Aras.VS.MethodPlugin.Dialogs
{
	public interface IDialogFactory
	{
		ItemSearchPresenter GetItemSearchPresenter(string itemTypeName, string itemTypeSingularLabel);

		ItemSearchPresenter GetItemSearchPresenter(ISearcher searcher);

		IViewAdaper<LoginView, ViewResult> GetLoginView(IProjectManager projectManager, IProjectConfiguraiton projectConfiguration);

		IViewAdaper<CreateMethodView, CreateMethodViewResult> GetCreateView(IVsUIShell uiShell,
			IProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			IProjectManager projectManager,
			ICodeProvider codeProvider,
			IGlobalConfiguration globalConfiguration);

		IViewAdaper<ConnectionInfoView, ViewResult> GetConnectionInfoView(IProjectManager projectManager, IProjectConfigurationManager configurationManager);

		IViewAdaper<OpenFromArasView, OpenFromArasViewResult> GetOpenFromArasView(IVsUIShell uiShell, IProjectConfigurationManager configurationManager,
		   IProjectConfiguraiton projectConfiguration,
		   TemplateLoader templateLoader,
		   PackageManager packageManager,
		   string pathToProjectConfigFile,
		   string projectName,
		   string projectFullName,
		   string projectLanguage);

		IViewAdaper<OpenFromPackageView, OpenFromPackageViewResult> GetOpenFromPackageView(IVsUIShell uiShell, TemplateLoader templateLoader, string projectLanguage, IProjectConfiguraiton projectConfiguraiton);

		IViewAdaper<SaveMethodView, SaveMethodViewResult> GetSaveToArasView(IVsUIShell uiShell,
			IProjectConfigurationManager projectConfigurationManager,
			IProjectConfiguraiton projectConfiguration,
			PackageManager packageManager,
			MethodInfo methodInformation,
			string methodCode,
			string projectConfigPath,
			string projectName,
			string projectFullName);

		IViewAdaper<SaveToPackageView, SaveToPackageViewResult> GetSaveToPackageView(IVsUIShell uiShell,
			IProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			ICodeProvider codeProvider,
			IProjectManager projectManager,
			MethodInfo methodInformation,
			string pathToFileForSave);

		IViewAdaper<UpdateFromArasView, UpdateFromArasViewResult> GetUpdateFromArasView(IVsUIShell uiShell,
			IProjectConfigurationManager projectConfigurationManager,
			IProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			MethodInfo methodInfo,
			string projectConfigPath,
			string projectName,
			string projectFullName);

		IViewAdaper<CreateCodeItemView, CreateCodeItemViewResult> GetCreateCodeItemView(IVsUIShell uiShell, ICodeItemProvider codeItemProvider, bool usedVSFormatting);

		IViewAdaper<DebugMethodView, DebugMethodViewResult> GetDebugMethodView(IVsUIShell uiShell,
			IProjectConfigurationManager projectConfigurationManager,
			IProjectConfiguraiton projectConfiguration,
			MethodInfo methodInformation,
			string methodCode,
			string projectConfigPath,
			string projectName,
			string projectFullName);

		IMessageBoxWindow GetMessageBoxWindow(IVsUIShell uiShell);
	}
}
