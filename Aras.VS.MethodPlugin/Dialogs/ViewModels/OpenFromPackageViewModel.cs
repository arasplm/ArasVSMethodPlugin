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
using Aras.VS.MethodPlugin.ProjectConfigurations;
using Aras.VS.MethodPlugin.Templates;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class OpenFromPackageViewModel : BaseViewModel
	{
		private readonly TemplateLoader templateLoader;
		private string projectLanguage;
		private string selectedFolderPath;
        private string lastSelectedMFFile;

        private EventSpecificDataType selectedEventSpecificData;

		private string methodComment;
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
		private string selectedManifestFilePath;
		private bool isUseVSFormattingCode;

		private ICommand folderBrowserCommand;
		private ICommand okCommand;
		private ICommand closeCommand;

		public OpenFromPackageViewModel(TemplateLoader templateLoader, string projectLanguage, IProjectConfiguraiton projectConfiguration)
		{
			if (templateLoader == null) throw new ArgumentNullException(nameof(templateLoader));

			this.templateLoader = templateLoader;
			this.projectLanguage = projectLanguage;
			this.selectedFolderPath = projectConfiguration.LastSelectedDir;
            this.lastSelectedMFFile = projectConfiguration.LastSelectedMfFile;
            this.isUseVSFormattingCode = projectConfiguration.UseVSFormatting;

            folderBrowserCommand = new RelayCommand<object>(OnFolderBrowserCommandClicked);
			okCommand = new RelayCommand<object>(OnOkClicked, IsOkButtonEnabled);
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

		public string SelectedManifestFilePath
		{
			get { return selectedManifestFilePath; }
			set
			{
				selectedManifestFilePath = value;
				RaisePropertyChanged(nameof(SelectedManifestFilePath));
			}
		}

		public bool IsUseVSFormattingCode
		{
			get { return isUseVSFormattingCode; }
			set { isUseVSFormattingCode = value; }
		}

		#endregion

		#region Commands

		public ICommand FolderBrowserCommand { get { return folderBrowserCommand; } }

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		#endregion

		private void OnFolderBrowserCommandClicked(object window)
		{
            var actualFolderPath = GetActualFolderPath();
			var viewModel = new OpenFromPackageTreeViewModel(actualFolderPath, this.Package, this.MethodName);
			var view = new OpenFromPackageTreeView();
			view.DataContext = viewModel;
			view.Owner = window as Window;

			if (view.ShowDialog() == true)
			{
				this.Package = viewModel.SelectedPackageName.Split('\\')[0];
				this.SelectedManifestFilePath = viewModel.SelectPathViewModel.SelectedPath;

				var xmlDocument = new XmlDocument();
				xmlDocument.Load(viewModel.SelectedMethod.FullName);
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

                this.SelectedTemplate = templateLoader.GetTemplateFromCodeString(methodCode, methodLanguage, "Open method from AML package", window as Window);

            }
		}

        private string GetActualFolderPath()
        {
            if (!string.IsNullOrWhiteSpace(SelectedManifestFilePath))
            {
                return SelectedManifestFilePath;
            }
            if (!string.IsNullOrWhiteSpace(lastSelectedMFFile))
            {
                return lastSelectedMFFile;
            }
            if (!string.IsNullOrWhiteSpace(selectedFolderPath))
            {
                return selectedFolderPath;
            }
            return @"C:\";
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

		private bool IsOkButtonEnabled(object window)
		{
			if (string.IsNullOrEmpty(this.selectedManifestFilePath))
			{
				return false;
			}

			return true;
		}
	}
}
