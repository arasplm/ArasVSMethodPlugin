//------------------------------------------------------------------------------
// <copyright file="OpenFromArasViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.Templates;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class OpenFromArasViewModel : BaseViewModel
	{
		private readonly IAuthenticationManager authenticationManager;
		private readonly IDialogFactory dialogFactory;
		private readonly ProjectConfigurationManager configurationManager;
		private readonly TemplateLoader templateLoader;
		private readonly PackageManager packageManager;
		private string pathToProjectConfigFile;
		private string projectName;
		private string projectFullName;
		private string projectLanguage;

		private ProjectConfiguraiton projectConfiguration;
		private ConnectionInfo connectionInfo;

		private string methodComment;
		private string methodType;
		private string methodLanguage;
		private TemplateInfo selectedTemplate;
		private EventSpecificDataType selectedEventSpecificData;
		private string methodName;
		private string methodId;
		private string methodConfigId;
		private string methodCode;
		private string identityKeyedName;
		private string identityId;
		private string package;
		private bool isUseVSFormattingCode;

		private ICommand editConnectionInfoCommand;
		private ICommand searchMethodDialogCommand;
		private ICommand okCommand;
		private ICommand closeCommand;

		public OpenFromArasViewModel(
			IAuthenticationManager authenticationManager,
			IDialogFactory dialogFactory,
			ProjectConfigurationManager configurationManager,
			ProjectConfiguraiton projectConfiguration,
			TemplateLoader templateLoader,
			PackageManager packageManager,
			string pathToProjectConfigFile,
			string projectName,
			string projectFullName,
			string projectLanguage)
		{
			if (authenticationManager == null) throw new ArgumentNullException(nameof(authenticationManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (configurationManager == null) throw new ArgumentNullException(nameof(configurationManager));
			if (projectConfiguration == null) throw new ArgumentNullException(nameof(projectConfiguration));
			if (templateLoader == null) throw new ArgumentNullException(nameof(templateLoader));
			if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));

			this.authenticationManager = authenticationManager;
			this.dialogFactory = dialogFactory;
			this.configurationManager = configurationManager;
			this.projectConfiguration = projectConfiguration;
			this.templateLoader = templateLoader;
			this.packageManager = packageManager;

			this.pathToProjectConfigFile = pathToProjectConfigFile;
			this.projectName = projectName;
			this.projectFullName = projectFullName;
			this.projectLanguage = projectLanguage;
            this.isUseVSFormattingCode = projectConfiguration.UseVSFormatting;


            this.editConnectionInfoCommand = new RelayCommand<object>(EditConnectionInfoCommandClicked);
			this.searchMethodDialogCommand = new RelayCommand<object>(SearchMethodDialogCommandClicked);
			this.okCommand = new RelayCommand<object>(OkCommandCliked, IsEnabledOkButton);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);

			ConnectionInformation = projectConfiguration.Connections.First(c => c.LastConnection);
			SelectedEventSpecificData = EventSpecificData.First();
		}

		#region Properties

		public ConnectionInfo ConnectionInformation
		{
			get { return connectionInfo; }
			set
			{
				connectionInfo = value;
				RaisePropertyChanged(nameof(ConnectionInformation));
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

		public string MethodType
		{
			get { return methodType; }
			set
			{
				methodType = value;
				RaisePropertyChanged(nameof(MethodType));
			}
		}

		public string MethodLanguage
		{
			get { return methodLanguage; }
			set
			{
				methodLanguage = value;
				RaisePropertyChanged(nameof(MethodLanguage));
			}
		}

		public string TemplateName
		{
			get { return selectedTemplate != null ? selectedTemplate.TemplateName : "None"; }
			set { }
		}

		public TemplateInfo SelectedTemplate
		{
			get { return selectedTemplate; }
			set
			{
				selectedTemplate = value;
				RaisePropertyChanged(nameof(TemplateName));
			}
		}

		public List<EventSpecificDataType> EventSpecificData { get { return CommonData.EventSpecificDataTypeList; } }

		public EventSpecificDataType SelectedEventSpecificData
		{
			get { return selectedEventSpecificData; }
			set
			{
				selectedEventSpecificData = value;
				RaisePropertyChanged(nameof(SelectedEventSpecificData));
			}
		}

		public string MethodName
		{
			get { return methodName; }
			set
			{
				methodName = value;
				RaisePropertyChanged(nameof(MethodName));
			}
		}

		public string MethodId
		{
			get { return methodId; }
			set { methodId = value; }
		}

		public string MethodConfigId
		{
			get { return methodConfigId; }
			set { methodConfigId = value; }
		}

		public string MethodCode
		{
			get { return methodCode; }
			set
			{
				methodCode = value;
				RaisePropertyChanged(nameof(MethodCode));
			}
		}

		public string IdentityKeyedName
		{
			get { return identityKeyedName; }
			set
			{
				identityKeyedName = value;
				RaisePropertyChanged(nameof(IdentityKeyedName));
			}
		}

		public string IdentityId
		{
			get { return identityId; }
			set { identityId = value; }
		}

		public string Package
		{
			get { return package; }
			set
			{
				package = value;
				RaisePropertyChanged(nameof(Package));
			}
		}

		public bool IsUseVSFormattingCode
		{ 
			get{ return isUseVSFormattingCode; }
			set{ isUseVSFormattingCode = value; }
		}

		#endregion

		#region Commands

		public ICommand EditConnectionInfoCommand { get { return editConnectionInfoCommand; } }

		public ICommand SearchMethodDialogCommand { get { return searchMethodDialogCommand; } }

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		#endregion

		private void EditConnectionInfoCommandClicked(object window)
		{
			var loginView = new LoginView();
			var loginViewModel = new LoginViewModel(authenticationManager, projectConfiguration, projectName, projectFullName);
			loginView.DataContext = loginViewModel;
			loginView.Owner = window as Window;

			if (loginView.ShowDialog() == true)
			{
				configurationManager.Save(pathToProjectConfigFile, projectConfiguration);
				ConnectionInformation = projectConfiguration.Connections.First(c => c.LastConnection);

				this.MethodName = string.Empty;
				this.MethodId = string.Empty;
				this.MethodConfigId = string.Empty;
				this.MethodType = string.Empty;
				this.MethodLanguage = string.Empty;
				this.SelectedTemplate = null;
				this.MethodCode = string.Empty;
				this.IdentityKeyedName = string.Empty;
				this.IdentityId = string.Empty;
				this.Package = string.Empty;
				this.MethodComment = string.Empty;
			}
		}

		private void SearchMethodDialogCommandClicked(object window)
		{
			string itemTypeName = "Method";
			string itemTypeSingularLabel = "Method";

			var predefinedPropertyValues = new List<PropertyInfo> { new PropertyInfo() { PropertyName = "method_type", PropertyValue = projectLanguage, IsReadonly = true } };

			List<PropertyInfo> predefinedSearchItems;
			if (!projectConfiguration.LastSavedSearch.TryGetValue(itemTypeName, out predefinedSearchItems))
			{
				predefinedSearchItems = new List<PropertyInfo>();
			}

			foreach (var searchItem in predefinedSearchItems)
			{
				if (predefinedPropertyValues.Any(x => x.PropertyName == searchItem.PropertyName))
				{
					continue;
				}

				predefinedPropertyValues.Add(new PropertyInfo() { PropertyName = searchItem.PropertyName, PropertyValue = searchItem.PropertyValue });
			}

			var presenter = dialogFactory.GetItemSearchPresenter(itemTypeName, itemTypeSingularLabel);
			var searchArguments = new ItemSearchPresenterArgs()
			{
				Title = "Search dialog",
				PredefinedPropertyValues = predefinedPropertyValues
			};

			var result = presenter.Run(searchArguments);
			if (result.DialogResult == DialogResult.OK)
			{
				dynamic item = authenticationManager.InnovatorInstance.newItem(result.ItemType, "get");
				item.setID(result.ItemId);
				item = item.apply();

				this.MethodName = item.getProperty("name", string.Empty);
				this.MethodId = item.getProperty("id", string.Empty);
				this.MethodConfigId = item.getProperty("config_id", string.Empty);
				this.MethodLanguage = item.getProperty("method_type", string.Empty);
				this.IdentityKeyedName = item.getPropertyAttribute("execution_allowed_to", "keyed_name", string.Empty);
				this.IdentityId = item.getProperty("execution_allowed_to", string.Empty);
				this.MethodComment = item.getProperty("comments", string.Empty);

                var methodCode = item.getProperty("method_code", string.Empty);
                this.MethodCode = Regex.Replace(methodCode, @"//MethodTemplateName=[\S]+\r\n", "");
             


                if (methodLanguage == "C#" || methodLanguage == "VB")
				{
					this.MethodType = "server";
				}
				else
				{
					this.MethodType = "client";
				}

				var packageName = string.Empty;

				try
				{
					packageName = packageManager.GetPackageDefinitionByElementName(methodName);
				}
				catch (Exception ex) { }

				this.Package = packageName;

				// TODO : duplicated with OpenFromPackageView
				TemplateInfo template = null;
				string methodTemplatePattern = @"//MethodTemplateName\s*=\s*(?<templatename>[^\W]*)\s*";
				Match methodTemplateNameMatch = Regex.Match(methodCode, methodTemplatePattern);
				if (methodTemplateNameMatch.Success)
				{
					string templateName = methodTemplateNameMatch.Groups["templatename"].Value;
					template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodLanguage && t.TemplateName == templateName).FirstOrDefault();

					if (template == null)
					{
						var messageWindow = new MessageBoxWindow();
						messageWindow.ShowDialog(window as Window,
							$"The template {templateName} from selected method not found. Default template will be used.",
							"Open method from Aras Innovator",
							MessageButtons.OK,
							MessageIcon.Information);
					}
				}
				if (template == null)
				{
					template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodLanguage && t.IsSupported).FirstOrDefault();
				}

				this.SelectedTemplate = template;

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


		private void OkCommandCliked(object view)
		{
			var window = view as Window;
			window.DialogResult = true;
			window.Close();
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private bool IsEnabledOkButton(object obj)
		{
			if (string.IsNullOrEmpty(methodName))
			{
				return false;
			}

			return true;
		}
	}
}
