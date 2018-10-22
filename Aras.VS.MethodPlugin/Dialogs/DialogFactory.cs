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
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;
using OfficeConnector.Dialogs;

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

		public LoginViewAdapter GetLoginView(IProjectManager projectManager, ProjectConfiguraiton projectConfiguration)
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

		public CreateMethodViewAdapter GetCreateView(IVsUIShell uiShell, ProjectConfiguraiton projectConfiguration, TemplateLoader templateLoader, PackageManager packageManager, IProjectManager projectManager, string projectLanguage)
		{
			CreateMethodView view = new CreateMethodView();
			CreateMethodViewModel viewModel = new CreateMethodViewModel(authManager, this, projectConfiguration, templateLoader, packageManager, projectManager, arasDataProvider, projectLanguage);
			view.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new CreateMethodViewAdapter(view);
		}

		public ConnectionInfoViewAdapter GetConnectionInfoView(IProjectManager projectManager, ProjectConfigurationManager configurationManager)
		{
			ProjectConfiguraiton projectConfiguration = configurationManager.Load(projectManager.ProjectConfigPath);

			var viewModel = new ConnectionInfoViewModel(authManager, this, configurationManager, projectManager, projectConfiguration);
			var view = new ConnectionInfoView();
			view.DataContext = viewModel;

			IntPtr hwnd;
			projectManager.UIShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new ConnectionInfoViewAdapter(view);
		}

		public OpenFromArasViewAdapter GetOpenFromArasView(IVsUIShell uiShell, ProjectConfigurationManager configurationManager,
			ProjectConfiguraiton projectConfiguration,
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

		public OpenFromPackageViewAdapter GetOpenFromPackageView(IVsUIShell uiShell, TemplateLoader templateLoader, string projectLanguage, string lastSelectedDirectory, bool useVSFormattingCode)
		{
			var viewModel = new OpenFromPackageViewModel(templateLoader, projectLanguage, lastSelectedDirectory, useVSFormattingCode);
			var view = new OpenFromPackageView();
			view.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new OpenFromPackageViewAdapter(view);
		}

		public SaveMethodViewAdapter GetSaveToArasView(IVsUIShell uiShell, ProjectConfigurationManager projectConfigurationManager, ProjectConfiguraiton projectConfiguration, PackageManager packageManager, MethodInfo methodInformation, string methodCode, string projectConfigPath, string projectName, string projectFullName)
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

		public SaveToPackageViewAdapter GetSaveToPackageView(IVsUIShell uiShell, ProjectConfiguraiton projectConfiguration, TemplateLoader templateLoader, PackageManager packageManager, ICodeProvider codeProvider,IProjectManager projectManager, MethodInfo methodInformation, string pathToFileForSave)
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

		public UpdateFromArasViewAdapter GetUpdateFromArasView(IVsUIShell uiShell, ProjectConfigurationManager projectConfigurationManager, ProjectConfiguraiton projectConfiguration, TemplateLoader templateLoader, PackageManager packageManager, MethodInfo methodInfo, string projectConfigPath, string projectName, string projectFullName)
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

		public CreatePartialElementViewAdapter GetCreatePartialClassView(IVsUIShell uiShell, bool usedVSFormatting)
		{
			var viewModel = new CreatePartialElementViewModel(usedVSFormatting);
			var view = new CreatePartialElementView();
			view.DataContext = viewModel;

			IntPtr hwnd;
			uiShell.GetDialogOwnerHwnd(out hwnd);
			var windowInteropHelper = new WindowInteropHelper(view);
			windowInteropHelper.Owner = hwnd;

			return new CreatePartialElementViewAdapter(view);
		}

		public DebugMethodViewAdapter GetDebugMethodView(IVsUIShell uiShell, ProjectConfigurationManager projectConfigurationManager, ProjectConfiguraiton projectConfiguration, MethodInfo methodInformation, string methodCode, string projectConfigPath, string projectName, string projectFullName)
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

		public MessageBoxWindow GetMessageBoxWindow(IVsUIShell uiShell)
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
