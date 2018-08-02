//------------------------------------------------------------------------------
// <copyright file="CreateMethodViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.Extensions;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class CreateMethodViewModel : BaseViewModel
	{
		private readonly TemplateLoader templateLoader;
		private readonly IAuthenticationManager authenticationManager;
		private readonly IDialogFactory dialogFactory;
		private readonly ProjectConfiguraiton projectConfiguration;
		private readonly PackageManager packageManager;
		IProjectManager projectManager;

		private List<FilteredListInfo> allLanguages;

		private ListInfo selectedActionLocation;
		private ObservableCollection<ListInfo> actionLocations;
		private FilteredListInfo selectedLanguage;
		private ObservableCollection<FilteredListInfo> languages;
		private TemplateInfo selectedTemplate;
		private ObservableCollection<TemplateInfo> templates;
		private string methodName = "Method1";
		private string methodComment;
		private bool isOkButtonEnabled;

		private string selectedIdentityKeyedName;
		private string selectedIdentityId;

		private ICommand okCommand;
		private ICommand cancelCommand;
		private ICommand closeCommand;
		private ICommand selectedIdentityCommand;

		public CreateMethodViewModel(
			IAuthenticationManager authenticationManager,
			IDialogFactory dialogFactory,
			ProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			IProjectManager projectManager,
			string projectLanguage)
		{
			if (authenticationManager == null) throw new ArgumentNullException(nameof(authenticationManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));
			if (templateLoader == null) throw new ArgumentNullException(nameof(templateLoader));
			if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));

			this.authenticationManager = authenticationManager;
			this.dialogFactory = dialogFactory;
			this.projectConfiguration = projectConfiguration;
			this.templateLoader = templateLoader;
			this.packageManager = packageManager;
			this.projectManager = projectManager;

			actionLocations = new ObservableCollection<ListInfo>();
			foreach (var localtion in Utilities.Utils.GetValueListByName(authenticationManager.InnovatorInstance, "Action Locations"))
			{
				actionLocations.Add(new ListInfo(localtion.getProperty("value"), localtion.getProperty("label")));
			}

			allLanguages = new List<FilteredListInfo>();
			foreach (var language in Utilities.Utils.GetFilterValueListByName(authenticationManager.InnovatorInstance, "Method Types"))
			{
				string value = language.getProperty("value");
				string label = language.getProperty("label");
				string filter = language.getProperty("filter");

				if (string.Equals(filter, "server", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(value, projectLanguage, StringComparison.CurrentCultureIgnoreCase))
				{
					continue;
				}

				allLanguages.Add(new FilteredListInfo(value, label, filter));
			}

			SelectedActionLocation = actionLocations.First(al => string.Equals(al.Value.ToLowerInvariant(), "server"));

			okCommand = new RelayCommand<object>(OnOkClick);
			cancelCommand = new RelayCommand(OnCancelClick);
			closeCommand = new RelayCommand<object>(OnCloseCliked);
			selectedIdentityCommand = new RelayCommand(SelectedIdentityCommandClick);

			SelectedEventSpecificData = EventSpecificData.First();
			ValidateOkButton();
		}

		#region Properties

		public List<EventSpecificDataType> EventSpecificData { get { return CommonData.EventSpecificDataTypeList; } }

		private EventSpecificDataType selectedEventSpecificData;
		public EventSpecificDataType SelectedEventSpecificData
		{
			get { return selectedEventSpecificData; }
			set
			{
				selectedEventSpecificData = value;

				var resultCode = SelectedTemplate.TemplateCode;
				resultCode = resultCode.Replace("$(interfacename)", value.InterfaceName);
				resultCode = resultCode.Replace("$(EventDataClass)", value.EventDataClass);
				TemplatePreviewWithEventData = resultCode;

				RaisePropertyChanged(nameof(SelectedEventSpecificData));
			}
		}

		private bool useRecommendedDefaultCode;

		public bool UseRecommendedDefaultCode
		{
			get { return useRecommendedDefaultCode; }
			set { useRecommendedDefaultCode = value; RaisePropertyChanged(nameof(UseRecommendedDefaultCode)); }
		}


		private string templatePreviewWithEventData;

		public string TemplatePreviewWithEventData
		{
			get { return templatePreviewWithEventData; }
			set { templatePreviewWithEventData = value; RaisePropertyChanged(nameof(TemplatePreviewWithEventData)); }
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
				SelectedTemplate = Templates.FirstOrDefault(t => t.IsSupported);
			}
		}

		public ObservableCollection<FilteredListInfo> Languages
		{
			get { return languages; }
			set
			{
				languages = value;

				RaisePropertyChanged(nameof(Languages));
				Templates = null;
				SelectedTemplate = null;
				SelectedLanguage = languages.Count == 1 ? languages[0] : null;
			}
		}
		private string selectedPackage;

		public string SelectedPackage
		{
			get { return selectedPackage; }
			set
			{
				selectedPackage = value;
				RaisePropertyChanged(nameof(SelectedPackage));
				ValidateOkButton();
			}
		}

		public List<string> AvaliablePackages { get { return packageManager.GetPackageDefinitionList(); } }

		public TemplateInfo SelectedTemplate
		{
			get { return selectedTemplate; }
			set
			{
				selectedTemplate = value;
				if (value != null && SelectedEventSpecificData != null)
				{
					var resultCode = value.TemplateCode;
					resultCode = resultCode.Replace("$(interfacename)", SelectedEventSpecificData.InterfaceName);
					resultCode = resultCode.Replace("$(EventDataClass)", SelectedEventSpecificData.EventDataClass);
					TemplatePreviewWithEventData = resultCode;
				}

				RaisePropertyChanged(nameof(SelectedTemplate));
			}
		}

		public ObservableCollection<TemplateInfo> Templates
		{
			get { return templates; }
			set { templates = value; RaisePropertyChanged(nameof(Templates)); }
		}

		public string MethodName
		{
			get { return methodName; }
			set
			{
				methodName = value;
				RaisePropertyChanged(nameof(MethodName));
				ValidateOkButton();
			}
		}

		public string MethodComment
		{
			get { return methodComment; }
			set
			{
				methodComment = value;
				RaisePropertyChanged(nameof(MethodComment));
			}
		}

		public bool IsOkButtonEnabled
		{
			get { return isOkButtonEnabled; }
			set
			{
				isOkButtonEnabled = value;
				RaisePropertyChanged(nameof(IsOkButtonEnabled));
			}
		}

		public string SelectedIdentityKeyedName
		{
			get { return this.selectedIdentityKeyedName; }
			set
			{
				this.selectedIdentityKeyedName = value;
				RaisePropertyChanged(nameof(SelectedIdentityKeyedName));
			}
		}

		public string SelectedIdentityId
		{
			get { return this.selectedIdentityId; }
			set { }
		}

		#endregion Properties

		#region Commands

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CancelCommand { get { return cancelCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		public ICommand SelectedIdentityCommand { get { return selectedIdentityCommand; } }

		#endregion Commands

		private void OnCancelClick()
		{
			throw new NotImplementedException();
		}

		private void OnCloseCliked(object view)
		{
			(view as System.Windows.Window).Close();
		}

		private void OnOkClick(object window)
		{
			var wnd = window as System.Windows.Window;

			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(methodName);
			if (projectManager.ServerMethodFolderItems.Exists(fileNameWithoutExtension))
			{
				ProjectItem folder = projectManager.ServerMethodFolderItems.Item(fileNameWithoutExtension);
				string methodNameWithExtension = !Path.HasExtension(methodName) ? methodName + ".cs" : methodName;
				if (folder.ProjectItems.Exists(methodNameWithExtension))
				{
					var messageWindow = new MessageBoxWindow();
					var dialogReuslt = messageWindow.ShowDialog(wnd,
						"Method already added to project. Do you want replace method?",
						"Warning",
						MessageButtons.YesNo,
						MessageIcon.None);

					if (dialogReuslt != MessageDialogResult.Yes)
					{
						return;
					}
				}
			}

			
			wnd.DialogResult = true;
			wnd.Close();
		}

		private List<FilteredListInfo> GetLanguagesList(string value)
		{
			return allLanguages.Where(l => string.Equals(l.Filter.ToLowerInvariant(), value)).ToList();
		}

		private void ValidateOkButton()
		{
			if (string.IsNullOrEmpty(this.methodName))
			{
				IsOkButtonEnabled = false;
			}
			else
			{
				IsOkButtonEnabled = true;
			}
		}

		private void SelectedIdentityCommandClick()
		{
			string itemTypeName = "Identity";
			string itemTypeSingularLabel = "Identity";

			List<PropertyInfo> predefinedSearchItems;
			if (!projectConfiguration.LastSavedSearch.TryGetValue(itemTypeName, out predefinedSearchItems))
			{
				predefinedSearchItems = new List<PropertyInfo>();
			}

			var searchArguments = new ItemSearchPresenterArgs()
			{
				Title = "Search dialog",
				PredefinedPropertyValues = predefinedSearchItems
			};

			ItemSearchPresenter presenter = dialogFactory.GetItemSearchPresenter(itemTypeName, itemTypeSingularLabel);
			var result = presenter.Run(searchArguments);
			if (result.DialogResult == System.Windows.Forms.DialogResult.OK)
			{
				// TODO : should be replaced by better approach
				dynamic item = authenticationManager.InnovatorInstance.newItem(itemTypeName, "get");
				item.setID(result.ItemId);
				item.setAttribute("select", "keyed_name");
				item = item.apply();

				this.SelectedIdentityKeyedName = item.getProperty("keyed_name");
				this.selectedIdentityId = result.ItemId;

				if (projectConfiguration.LastSavedSearch.ContainsKey(result.ItemType))
				{
					projectConfiguration.LastSavedSearch[result.ItemType] = result.LastSavedSearch;
				}
				else
				{
					projectConfiguration.LastSavedSearch.Add(result.ItemType, result.LastSavedSearch);
				}
			}
		}
	}
}
