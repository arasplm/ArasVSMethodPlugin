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
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.Extensions;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.SolutionManagement;
using Aras.VS.MethodPlugin.Templates;
using EnvDTE;
using OfficeConnector.Dialogs;
using Aras.VS.MethodPlugin.Configurations;
using System.Windows.Forms;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class CreateMethodViewModel : BaseViewModel
	{
		private const string None = "None";

		private readonly TemplateLoader templateLoader;
		private readonly IAuthenticationManager authenticationManager;
		private readonly IDialogFactory dialogFactory;
		private readonly IProjectConfiguraiton projectConfiguration;
		private readonly PackageManager packageManager;
		private readonly IProjectManager projectManager;
		private readonly IArasDataProvider arasDataProvider;
		private readonly ICodeProvider codeProvider;
		private readonly IGlobalConfiguration globalConfiguration;

		private MethodItemTypeInfo methodItemTypeInfo;

		private List<FilteredListInfo> allLanguages;

		private ListInfo selectedActionLocation;
		private ObservableCollection<ListInfo> actionLocations;
		private FilteredListInfo selectedLanguage;
		private ObservableCollection<FilteredListInfo> languages;
		private TemplateInfo selectedTemplate;
		private ObservableCollection<TemplateInfo> templates;
		private string methodName = "Method1";
		private int methodCommentMaxLength;
		private string methodComment;
		private int methodNameMaxLength;
		private ObservableCollection<KeyValuePair<string, XmlMethodInfo>> userCodeTemplates;
		private KeyValuePair<string, XmlMethodInfo> selectedUserCodeTemplate;

		private string selectedIdentityKeyedName;
		private string selectedIdentityId;
		private bool isUseVSFormattingCode;

		private ICommand okCommand;
		private ICommand cancelCommand;
		private ICommand closeCommand;
		private ICommand selectedIdentityCommand;
		private ICommand browseCodeTemplateCommand;
		private ICommand deleteUserCodeTemplateCommand;

		public CreateMethodViewModel(
			IAuthenticationManager authenticationManager,
			IDialogFactory dialogFactory,
			IProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			IProjectManager projectManager,
			IArasDataProvider arasDataProvider,
			ICodeProvider codeProvider,
			IGlobalConfiguration userConfiguration)
		{
			if (authenticationManager == null) throw new ArgumentNullException(nameof(authenticationManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));
			if (templateLoader == null) throw new ArgumentNullException(nameof(templateLoader));
			if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
			if (projectManager == null) throw new ArgumentNullException(nameof(projectManager));
			if (arasDataProvider == null) throw new ArgumentNullException(nameof(arasDataProvider));
			if (codeProvider == null) throw new ArgumentNullException(nameof(codeProvider));
			if (userConfiguration == null) throw new ArgumentNullException(nameof(userConfiguration));

			this.authenticationManager = authenticationManager;
			this.dialogFactory = dialogFactory;
			this.projectConfiguration = projectConfiguration;
			this.templateLoader = templateLoader;
			this.packageManager = packageManager;
			this.projectManager = projectManager;
			this.arasDataProvider = arasDataProvider;
			this.codeProvider = codeProvider;
			this.globalConfiguration = userConfiguration;
			this.isUseVSFormattingCode = projectConfiguration.UseVSFormatting;

			this.UserCodeTemplates = LoadUserCodeTemplates();
			this.SelectedUserCodeTemplate = this.userCodeTemplates.First();

			this.methodItemTypeInfo = arasDataProvider.GetMethodItemTypeInfo();
			this.MethodNameMaxLength = methodItemTypeInfo.NameStoredLength;
			this.MethodCommentMaxLength = methodItemTypeInfo.CommentsStoredLength;

			actionLocations = new ObservableCollection<ListInfo>();
			foreach (var localtion in Utilities.Utils.GetValueListByName(authenticationManager.InnovatorInstance, "Action Locations"))
			{
				string value = localtion.getProperty("value", string.Empty);
				if (string.Equals(value, "client"))
				{
					continue;
				}

				actionLocations.Add(new ListInfo(value, localtion.getProperty("label", string.Empty)));
			}

			allLanguages = new List<FilteredListInfo>();
			foreach (var language in Utilities.Utils.GetFilterValueListByName(authenticationManager.InnovatorInstance, "Method Types"))
			{
				string value = language.getProperty("value");
				string label = language.getProperty("label");
				string filter = language.getProperty("filter");

				if (string.Equals(filter, "server", StringComparison.CurrentCultureIgnoreCase) && !string.Equals(value, this.codeProvider.Language, StringComparison.CurrentCultureIgnoreCase))
				{
					continue;
				}

				allLanguages.Add(new FilteredListInfo(value, label, filter));
			}

			SelectedActionLocation = actionLocations.First(al => string.Equals(al.Value.ToLowerInvariant(), "server"));

			okCommand = new RelayCommand<object>(OnOkClick, IsEnabledOkButton);
			cancelCommand = new RelayCommand<object>(OnCancelClick);
			closeCommand = new RelayCommand<object>(OnCloseCliked);
			selectedIdentityCommand = new RelayCommand(SelectedIdentityCommandClick);
			browseCodeTemplateCommand = new RelayCommand(BrowseCodeTemplateCommandClick);
			deleteUserCodeTemplateCommand = new RelayCommand<KeyValuePair<string, XmlMethodInfo>>(DeleteUserCodeTemplateCommandClick);

			SelectedEventSpecificData = EventSpecificData.First();
		}

		#region Properties

		public List<EventSpecificDataType> EventSpecificData { get { return CommonData.EventSpecificDataTypeList; } }

		private EventSpecificDataType selectedEventSpecificData;
		public EventSpecificDataType SelectedEventSpecificData
		{
			get { return this.selectedEventSpecificData; }
			set
			{
				this.selectedEventSpecificData = value;

				RaisePropertyChanged(nameof(SelectedEventSpecificData));
				RaisePropertyChanged(nameof(MethodCodePreview));
			}
		}

		private bool useRecommendedDefaultCode;

		public bool UseRecommendedDefaultCode
		{
			get { return useRecommendedDefaultCode; }
			set { useRecommendedDefaultCode = value; RaisePropertyChanged(nameof(UseRecommendedDefaultCode)); }
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
			}
		}

		public List<string> AvaliablePackages { get { return packageManager.GetPackageDefinitionList(); } }

		public TemplateInfo SelectedTemplate
		{
			get { return selectedTemplate; }
			set
			{
				selectedTemplate = value;

				if (selectedTemplate != null)
				{
					if (!selectedTemplate.IsSuccessfullySupported)
					{
						var messageWindow = new MessageBoxWindow();
						var dialogReuslt = messageWindow.ShowDialog(null,
							selectedTemplate.Message,
							"Create new method",
							MessageButtons.OK,
							MessageIcon.None);
					}
				}

				RaisePropertyChanged(nameof(SelectedTemplate));
				RaisePropertyChanged(nameof(MethodCodePreview));
			}
		}

		public ObservableCollection<TemplateInfo> Templates
		{
			get { return templates; }
			set
			{
				templates = value;
				RaisePropertyChanged(nameof(Templates));
			}
		}

		public string MethodName
		{
			get { return methodName; }
			set
			{
				methodName = value;
				RaisePropertyChanged(nameof(MethodName));
				RaisePropertyChanged(nameof(MethodCodePreview));
			}
		}

		public int MethodNameMaxLength
		{
			get { return methodNameMaxLength; }
			set
			{
				methodNameMaxLength = value;
				RaisePropertyChanged(nameof(MethodNameMaxLength));
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

		public int MethodCommentMaxLength
		{
			get { return methodCommentMaxLength; }
			set
			{
				methodCommentMaxLength = value;
				RaisePropertyChanged(nameof(MethodCommentMaxLength));
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

		public bool IsUseVSFormattingCode
		{
			get { return isUseVSFormattingCode; }
			set
			{
				isUseVSFormattingCode = value;
				RaisePropertyChanged(nameof(MethodCodePreview));
			}
		}

		public ObservableCollection<KeyValuePair<string, XmlMethodInfo>> UserCodeTemplates
		{
			get { return userCodeTemplates; }
			set
			{
				userCodeTemplates = value;
				RaisePropertyChanged(nameof(UserCodeTemplates));
			}
		}

		public string MethodCodePreview
		{
			get
			{
				if(string.IsNullOrEmpty(methodName))
				{
					return string.Empty;
				}
				else
				{
					GeneratedCodeInfo codeInfo = codeProvider.CreateWrapper(selectedTemplate, selectedEventSpecificData, methodName, isUseVSFormattingCode);
					codeInfo = codeProvider.CreateMainNew(codeInfo, selectedTemplate, selectedEventSpecificData, methodName, false, selectedUserCodeTemplate.Value?.Code);
					return codeInfo.MethodCodeInfo.Code;
				}
			}
			set { }
		}

		public KeyValuePair<string, XmlMethodInfo> SelectedUserCodeTemplate
		{
			get { return selectedUserCodeTemplate; }
			set
			{
				selectedUserCodeTemplate = value;

				RaisePropertyChanged(nameof(SelectedUserCodeTemplate));
				RaisePropertyChanged(nameof(MethodCodePreview));
			}
		}

		#endregion Properties

		#region Commands

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CancelCommand { get { return cancelCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		public ICommand SelectedIdentityCommand { get { return selectedIdentityCommand; } }

		public ICommand BrowseCodeTemplateCommand { get { return browseCodeTemplateCommand; } }

		public ICommand DeleteUserCodeTemplateCommand { get { return deleteUserCodeTemplateCommand; } }

		#endregion Commands

		private void OnCancelClick(object window)
		{
			var wnd = window as System.Windows.Window;
			wnd.DialogResult = false;
			wnd.Close();
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

		private bool IsEnabledOkButton(object obj)
		{
			if (string.IsNullOrEmpty(this.methodName))
			{
				return false;
			}

			return true;
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

		private void BrowseCodeTemplateCommandClick()
		{
			OpenFileDialog openFileDialog = new OpenFileDialog()
			{
				Filter = "XML Files (*.xml)|*.xml",
				FilterIndex = 0,
				DefaultExt = "xml"
			};

			DialogResult dialogResult = openFileDialog.ShowDialog();
			if (dialogResult == DialogResult.Cancel)
			{
				return;
			}

			XmlMethodInfo xmlMethodInfo = new XmlMethodLoader().LoadMethod(openFileDialog.FileName);
			if (xmlMethodInfo == null)
			{
				var messageWindow = new MessageBoxWindow();
				var dialogReuslt = messageWindow.ShowDialog(null,
					$"User code template invalid format.",
					"Warning",
					MessageButtons.OK,
					MessageIcon.Warning);

				return;
			}

			if (xmlMethodInfo.MethodType != codeProvider.Language)
			{
				var messageWindow = new MessageBoxWindow();
				var dialogReuslt = messageWindow.ShowDialog(null,
					$"User code tamplate must be {codeProvider.Language} method type.",
					"Warning",
					MessageButtons.OK,
					MessageIcon.Warning);

				return;
			}

			KeyValuePair<string, XmlMethodInfo> userCodeTemplate = this.UserCodeTemplates.FirstOrDefault(x => x.Key == xmlMethodInfo.MethodName);
			if (userCodeTemplate.Key == default(KeyValuePair<string, XmlMethodInfo>).Key)
			{
				this.globalConfiguration.AddUserCodeTemplatePath(xmlMethodInfo.Path);
				this.globalConfiguration.Save();

				userCodeTemplate = new KeyValuePair<string, XmlMethodInfo>(xmlMethodInfo.MethodName, xmlMethodInfo);
				UserCodeTemplates.Add(userCodeTemplate);
			}

			SelectedUserCodeTemplate = userCodeTemplate;
		}

		private void DeleteUserCodeTemplateCommandClick(KeyValuePair<string, XmlMethodInfo> userCodeTemplate)
		{
			if (userCodeTemplate.Key == None)
			{
				return;
			}

			if (this.SelectedUserCodeTemplate.Key == userCodeTemplate.Key)
			{
				SelectedUserCodeTemplate = this.UserCodeTemplates.First(); // None
			}

			this.globalConfiguration.RemoveUserCodeTemplatePath(userCodeTemplate.Value.Path);
			this.globalConfiguration.Save();
			this.UserCodeTemplates.Remove(userCodeTemplate);
		}

		private ObservableCollection<KeyValuePair<string, XmlMethodInfo>> LoadUserCodeTemplates()
		{
			List<string> paths = this.globalConfiguration.GetUserCodeTemplatesPaths();
			List<XmlMethodInfo> xmlMethodInfos = new XmlMethodLoader().LoadMethods(paths).Where(x => x.MethodType == this.codeProvider.Language).ToList();
			ObservableCollection<KeyValuePair<string, XmlMethodInfo>> templates = new ObservableCollection<KeyValuePair<string, XmlMethodInfo>>();
			templates.Add(new KeyValuePair<string, XmlMethodInfo>(None, new XmlMethodInfo()));
			foreach (XmlMethodInfo xmlMethodInfo in xmlMethodInfos)
			{
				templates.Add(new KeyValuePair<string, XmlMethodInfo>(xmlMethodInfo.MethodName, xmlMethodInfo));
			}

			return templates;
		}
	}
}
