//------------------------------------------------------------------------------
// <copyright file="DialogFactory.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Windows.Interop;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.ItemSearch.Preferences;
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
	internal class DialogFactory : IDialogFactory
	{
		private IAuthenticationManager authManager;
		private readonly IArasDataProvider arasDataProvider;

		public DialogFactory(IAuthenticationManager authManager, IArasDataProvider arasDataProvider)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (arasDataProvider == null) throw new ArgumentNullException(nameof(arasDataProvider));

			this.arasDataProvider = arasDataProvider;
			this.authManager = authManager;
		}

		public ItemSearchPresenter GetItemSearchPresenter(string itemTypeName, string itemTypeSingularLabel)
		{
			var preferenceProvider = new PreferenceProvider(authManager.InnovatorInstance);
			var savedSearchProvider = new SavedSearchProvider(authManager.InnovatorInstance);
			var searcher = new DefaultSearcher(authManager.InnovatorInstance, preferenceProvider, savedSearchProvider, itemTypeName, itemTypeSingularLabel);

			ItemSearchPresenter presenter = GetItemSearchPresenter(searcher);
			return presenter;
		}

		public ItemSearchPresenter GetItemSearchPresenter(ISearcher searcher)
		{
			var itemSearchColumnProvider = new ItemSearchColumnProvider();
			var itemSearchCellProvider = new ItemSearchCellProvider();
			var dialog = new ItemSearchDialog(itemSearchColumnProvider, itemSearchCellProvider);
			var presenter = new ItemSearchPresenter(dialog, searcher);
			return presenter;
		}

		public IViewAdaper<LoginView, ViewResult> GetLoginView(IProjectManager projectManager, IProjectConfiguraiton projectConfiguration)
		{
			var view = new LoginView();
			var viewModel = new LoginViewModel(authManager, projectConfiguration, projectManager.SelectedProject.Name, projectManager.SelectedProject.FullName);
			view.DataContext = viewModel;

			IntPtr hwnd;
			projectManager.UIShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new LoginViewAdapter(view);
		}

		public IViewAdaper<CreateMethodView, CreateMethodViewResult> GetCreateView(IVsUIShell uiShell, IProjectConfiguraiton projectConfiguration, TemplateLoader templateLoader, PackageManager packageManager, IProjectManager projectManager, ICodeProvider codeProvider, IGlobalConfiguration globalConfiguration)
		{
			CreateMethodView view = new CreateMethodView();
			CreateMethodViewModel viewModel = new CreateMethodViewModel(authManager, this, projectConfiguration, templateLoader, packageManager, projectManager, arasDataProvider, codeProvider, globalConfiguration);
			view.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new CreateMethodViewAdapter(view);
		}

		public IViewAdaper<ConnectionInfoView, ViewResult> GetConnectionInfoView(IProjectManager projectManager, IProjectConfigurationManager configurationManager)
		{
            var projectConfiguration = configurationManager.Load(projectManager.ProjectConfigPath);

			var viewModel = new ConnectionInfoViewModel(authManager, this, configurationManager, projectManager, projectConfiguration);
			var view = new ConnectionInfoView();
			view.DataContext = viewModel;

			IntPtr hwnd;
			projectManager.UIShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new ConnectionInfoViewAdapter(view);
		}

		public IViewAdaper<OpenFromArasView, OpenFromArasViewResult> GetOpenFromArasView(IVsUIShell uiShell, IProjectConfigurationManager configurationManager,
			IProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			string pathToProjectConfigFile,
			string projectName,
			string projectFullName,
			string projectLanguage)
		{
			var viewModel = new OpenFromArasViewModel(authManager, this, configurationManager, projectConfiguration, templateLoader, packageManager, pathToProjectConfigFile, projectName, projectFullName, projectLanguage);
			var view = new OpenFromArasView();
			view.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new OpenFromArasViewAdapter(view);
		}

		public IViewAdaper<OpenFromPackageView, OpenFromPackageViewResult> GetOpenFromPackageView(IVsUIShell uiShell, TemplateLoader templateLoader, string projectLanguage, IProjectConfiguraiton projectConfiguration)
		{
			var viewModel = new OpenFromPackageViewModel(templateLoader, projectLanguage, projectConfiguration);
			var view = new OpenFromPackageView();
			view.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new OpenFromPackageViewAdapter(view);
		}

        public IViewAdaper<SaveMethodView, SaveMethodViewResult> GetSaveToArasView(IVsUIShell uiShell, IProjectConfigurationManager projectConfigurationManager, IProjectConfiguraiton projectConfiguration, PackageManager packageManager, MethodInfo methodInformation, string methodCode, string projectConfigPath, string projectName, string projectFullName)
		{
			var view = new SaveMethodView();
			var viewModel = new SaveMethodViewModel(
				authManager,
				this,
				projectConfigurationManager,
				projectConfiguration,
				packageManager,
				arasDataProvider,
				methodInformation,
				methodCode,
				projectConfigPath,
				projectName,
				projectFullName);

			view.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new SaveMethodViewAdapter(view);
		}

        public IViewAdaper<SaveToPackageView, SaveToPackageViewResult> GetSaveToPackageView(IVsUIShell uiShell, IProjectConfiguraiton projectConfiguration, TemplateLoader templateLoader, PackageManager packageManager, ICodeProvider codeProvider,IProjectManager projectManager, MethodInfo methodInformation, string pathToFileForSave)
		{
			var saveToLocalPackageView = new SaveToPackageView();
			var viewModel = new SaveToPackageViewModel(authManager, this, projectConfiguration, templateLoader, packageManager, codeProvider, projectManager, arasDataProvider, methodInformation, pathToFileForSave);
			saveToLocalPackageView.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(saveToLocalPackageView);
			windowInteropHelper.Owner = hwnd;

			return new SaveToPackageViewAdapter(saveToLocalPackageView);
		}

        public IViewAdaper<UpdateFromArasView, UpdateFromArasViewResult> GetUpdateFromArasView(IVsUIShell uiShell, IProjectConfigurationManager projectConfigurationManager, IProjectConfiguraiton projectConfiguration, TemplateLoader templateLoader, PackageManager packageManager, MethodInfo methodInfo, string projectConfigPath, string projectName, string projectFullName)
		{
			var viewModel = new UpdateFromArasViewModel(authManager, projectConfigurationManager, projectConfiguration, templateLoader, packageManager, methodInfo, projectConfigPath, projectName, projectFullName);
			var view = new UpdateFromArasView();
			view.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new UpdateFromArasViewAdapter(view);
		}

		public IViewAdaper<CreateCodeItemView, CreateCodeItemViewResult> GetCreateCodeItemView(IVsUIShell uiShell, ICodeItemProvider codeItemProvider, bool usedVSFormatting)
		{
			var viewModel = new CreateCodeItemViewModel(codeItemProvider, usedVSFormatting);
			var view = new CreateCodeItemView();
			view.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new CreateCodeItemViewAdapter(view);
		}

        public IViewAdaper<DebugMethodView, DebugMethodViewResult> GetDebugMethodView(IVsUIShell uiShell, IProjectConfigurationManager projectConfigurationManager, IProjectConfiguraiton projectConfiguration, MethodInfo methodInformation, string methodCode, string projectConfigPath, string projectName, string projectFullName)
		{
			var viewModel = new DebugMethodViewModel(authManager, projectConfigurationManager, projectConfiguration, methodInformation, methodCode, projectConfigPath, projectName, projectFullName);
			var view = new DebugMethodView();
			view.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new DebugMethodViewAdapter(view);
		}

		public IMessageBoxWindow GetMessageBoxWindow(IVsUIShell uiShell)
		{
			var view = new MessageBoxWindow();

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return view;
		}
	}
}
