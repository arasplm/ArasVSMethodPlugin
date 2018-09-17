//------------------------------------------------------------------------------
// <copyright file="OpenFromPackageViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.Templates;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class OpenFromPackageViewModel : BaseViewModel
	{
		private readonly TemplateLoader templateLoader;
		private string projectLanguage;
		private string selectedFolderPath;

		private EventSpecificDataType selectedEventSpecificData;
		private bool isOkButtonEnabled;

		private string methodComment;
		private string methodPath;
		private string methodType;
		private string methodLanguage;
		private TemplateInfo selectedTemplate;
		private string methodCode;
		private string identityKeyedName;
		private string identityId;
		private string package;
		private string methodName;
		private string methodConfigId;
		private string methodId;

		private ICommand folderBrowserCommand;
		private ICommand okCommand;
		private ICommand closeCommand;

		public OpenFromPackageViewModel(TemplateLoader templateLoader, string projectLanguage, string lastSelectedDirectory)
		{
			if (templateLoader == null) throw new ArgumentNullException(nameof(templateLoader));

			this.templateLoader = templateLoader;
			this.projectLanguage = projectLanguage;
			this.selectedFolderPath = lastSelectedDirectory;

			folderBrowserCommand = new RelayCommand<object>(OnFolderBrowserCommandClicked);
			okCommand = new RelayCommand<object>(OnOkClicked);
			closeCommand = new RelayCommand<object>(OnCloseCliked);

			SelectedEventSpecificData = EventSpecificData.First();
		}

		#region Properties

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

		public bool IsOkButtonEnabled
		{
			get { return isOkButtonEnabled; }
			set
			{
				isOkButtonEnabled = value;
				RaisePropertyChanged(nameof(IsOkButtonEnabled));
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

		public string MethodPath
		{
			get { return methodPath; }
			set
			{
				methodPath = value;
				RaisePropertyChanged(nameof(MethodPath));
			}
		}

		public string SelectedFolderPath
		{
			get { return selectedFolderPath; }
			set { selectedFolderPath = value; }
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

		public string MethodConfigId
		{
			get { return methodConfigId; }
			set { methodConfigId = value; }
		}

		public string MethodId
		{
			get { return methodId; }
			set { methodId = value; }
		}

		public string SelectedManifestFile { get; set; }

		#endregion

		#region Commands

		public ICommand FolderBrowserCommand { get { return folderBrowserCommand; } }

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		#endregion

		private void OnFolderBrowserCommandClicked(object window)
		{
		    var actualFolderPath = !string.IsNullOrWhiteSpace(MethodPath)
		        ? Path.GetDirectoryName(MethodPath)
		        : selectedFolderPath;
            var viewModel = new OpenFromPackageTreeViewModel(actualFolderPath);
			var view = new OpenFromPackageTreeView();
			view.DataContext = viewModel;
			view.Owner = window as Window;

			if (view.ShowDialog() == true)
			{
				this.MethodPath = viewModel.SelectedMethodValue;
				this.SelectedFolderPath = Path.GetDirectoryName(viewModel.SelectPathViewModel.SelectedPath);
				this.Package = viewModel.SelectedPackageValue.Split('\\')[0];
				this.SelectedManifestFile = Path.GetFileName(viewModel.SelectPathViewModel.SelectedPath);

				var xmlDocument = new XmlDocument();
				xmlDocument.Load(this.methodPath);
				XmlNode itemXmlNode = xmlDocument.SelectSingleNode("AML/Item");
				XmlNode commentsXmlNode = itemXmlNode.SelectSingleNode("comments");
				XmlNode methodCodeXmlNode = itemXmlNode.SelectSingleNode("method_code");
				XmlNode executionAllowedToXmlNode = itemXmlNode.SelectSingleNode("execution_allowed_to");
				XmlNode methodTypeXmlNode = itemXmlNode.SelectSingleNode("method_type");
				XmlNode nameTypeXmlNode = itemXmlNode.SelectSingleNode("name");

				this.MethodLanguage = methodTypeXmlNode.InnerText;

				if (methodLanguage == "C#" || methodLanguage == "VB")
				{
					this.MethodType = "server";
				}
				else
				{
					this.MethodType = "client";
				}

				this.MethodComment = commentsXmlNode?.InnerText;
				this.MethodCode = methodCodeXmlNode.InnerText;
				this.IdentityKeyedName = executionAllowedToXmlNode.Attributes["keyed_name"].InnerText;
				this.IdentityId = executionAllowedToXmlNode.InnerText;
				this.MethodName = nameTypeXmlNode.InnerText;
				this.MethodConfigId = itemXmlNode.Attributes["id"].InnerText;
				this.MethodId = itemXmlNode.Attributes["id"].InnerText;
                

				// TODO : duplicated with OpenFromArasViewModel
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
							"Open method from AML package",
							MessageButtons.OK,
							MessageIcon.Information);
					}
				}
				if (template == null)
				{
					template = templateLoader.Templates.Where(t => t.TemplateLanguage == methodLanguage && t.IsSupported).FirstOrDefault();
				}
                

				this.SelectedTemplate = template;
                
				ValidateOkButton();
			}
		}

		private void OnOkClicked(object view)
		{
			var window = view as Window;

			if (projectLanguage != methodLanguage)
			{
				var messageWindow = new MessageBoxWindow();
				messageWindow.ShowDialog(window,
					"Current project and method types are different.",
					"Open method from AML package",
					MessageButtons.OK,
					MessageIcon.None);

				return;
			}

			window.DialogResult = true;
			window.Close();
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private void ValidateOkButton()
		{
			if (string.IsNullOrEmpty(this.methodPath))
			{
				IsOkButtonEnabled = false;
			}
			else
			{
				IsOkButtonEnabled = true;
			}
		}
	}
}
