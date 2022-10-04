//------------------------------------------------------------------------------
// <copyright file="MethodInformationViewModel.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Code;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.Method.Libs.Templates;
using Aras.VS.MethodPlugin.PackageManagement;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class MethodInformationViewModel : BaseViewModel
	{
		private readonly TemplateLoader templateLoader;
		private readonly dynamic innovatorInstance;
		private readonly PackageManager packageManager;

		private MethodInfo currentMethodInfo;

		private List<FilteredListInfo> allLanguages;
		private ListInfo selectedActionLocation;
		private ObservableCollection<ListInfo> actionLocations;
		private FilteredListInfo selectedLanguage;
		private ObservableCollection<FilteredListInfo> languages;
		private TemplateInfo selectedTemplate;
		private ObservableCollection<TemplateInfo> templates;

		private ICommand okCommand;
		private ICommand cancelCommand;
		private ICommand closeCommand;
		public MethodInformationViewModel(dynamic innovatorInstance, TemplateLoader templateLoader, MethodInfo currentMethodInfo, PackageManager packageManager)
		{
			if (innovatorInstance == null) throw new ArgumentNullException(nameof(innovatorInstance));
			if (templateLoader == null) throw new ArgumentNullException(nameof(templateLoader));
			if (packageManager == null) throw new ArgumentNullException(nameof(templateLoader));

			this.innovatorInstance = innovatorInstance;
			this.templateLoader = templateLoader;
			this.currentMethodInfo = currentMethodInfo;
			this.packageManager = packageManager;

			actionLocations = new ObservableCollection<ListInfo>();
			foreach (var localtion in Utilities.Utils.GetValueListByName(innovatorInstance, "Action Locations"))
			{
				actionLocations.Add(new ListInfo(localtion.getProperty("value"), localtion.getProperty("label")));
			}

			allLanguages = new List<FilteredListInfo>();
			foreach (var language in Utilities.Utils.GetFilterValueListByName(innovatorInstance, "Method Types"))
			{
				allLanguages.Add(new FilteredListInfo(language.getProperty("value"), language.getProperty("label"), language.getProperty("filter")));
			}

			SelectedActionLocation = actionLocations.First(al=>al.Value ==  currentMethodInfo.MethodType);
			SelectedLanguage = allLanguages.First(l => l.Value == currentMethodInfo.MethodLanguage);
			SelectedTemplate = Templates.First(t=>t.TemplateName == currentMethodInfo.TemplateName);
			SelectedPackageText = currentMethodInfo.Package.Name;

			okCommand = new RelayCommand<object>(OnOkClick);
			cancelCommand = new RelayCommand<object>(OnCancelClick);
			closeCommand = new RelayCommand<object>(OnCloseCliked);
		}

		public ListInfo SelectedActionLocation
		{
			get { return selectedActionLocation; }
			set
			{
				if (!string.Equals(value.Value, selectedActionLocation?.Value))
				{
					selectedActionLocation = value;
					RaisePropertyChanged(nameof(SelectedActionLocation));

					Languages = new ObservableCollection<FilteredListInfo>(GetLanguagesList(value.Value));
					SelectedLanguage = null;
				}
			}
		}

		public ObservableCollection<ListInfo> ActionLocations
		{
			get { return actionLocations; }
			set { actionLocations = value; RaisePropertyChanged(nameof(ActionLocations)); }
		}

		public bool MethodTypeEnabled { get { return false; } }

		public FilteredListInfo SelectedLanguage
		{
			get { return selectedLanguage; }
			set
			{
				selectedLanguage = value;
				RaisePropertyChanged(nameof(SelectedLanguage));
				var templatesForLanguage = templateLoader.Templates.Where(t => string.Equals(t.TemplateLanguage.ToLowerInvariant(), selectedLanguage?.Value.ToLowerInvariant()));
				Templates = new ObservableCollection<TemplateInfo>(templatesForLanguage);
			}
		}

		public ObservableCollection<FilteredListInfo> Languages
		{
			get { return languages; }
			set
			{
				languages = value;
				RaisePropertyChanged(nameof(Languages));
				SelectedLanguage = null;
				Templates = null;
				SelectedTemplate = null;
			}
		}

		public List<PackageInfo> AvaliablePackages { get { return packageManager.GetPackageDefinitionList(); } }


		private string selectedPackageText;

		public string SelectedPackageText
		{
			get { return selectedPackageText; }
			set { selectedPackageText = value;  RaisePropertyChanged(nameof(SelectedPackageText)); }
		}


		public TemplateInfo SelectedTemplate
		{
			get { return selectedTemplate; }
			set
			{
				selectedTemplate = value;
				RaisePropertyChanged(nameof(SelectedTemplate));
			}
		}

		public ObservableCollection<TemplateInfo> Templates
		{
			get { return templates; }
			set { templates = value; RaisePropertyChanged(nameof(Templates)); }
		}

		private List<FilteredListInfo> GetLanguagesList(string value)
		{
			return allLanguages.Where(l => string.Equals(l.Filter.ToLowerInvariant(), value)).ToList();
		}


		#region Commands

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CancelCommand { get { return cancelCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }
		private void OnCancelClick(object window)
		{
			var wnd = window as Window;
			wnd.DialogResult = false;
			wnd.Close();
		}

		private void OnOkClick(object window)
		{
			//validation Package etc

			var wnd = window as Window;
			wnd.DialogResult = true;
			wnd.Close();
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		#endregion Commands
	}
}
