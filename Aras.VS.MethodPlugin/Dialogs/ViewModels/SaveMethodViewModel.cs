//------------------------------------------------------------------------------
// <copyright file="SaveMethodViewModel.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.Method.Libs.Configurations.ProjectConfigurations;
using Aras.VS.MethodPlugin.ArasInnovator;
using Aras.VS.MethodPlugin.Authentication;
using Aras.VS.MethodPlugin.ItemSearch;
using Aras.VS.MethodPlugin.PackageManagement;
using OfficeConnector.Dialogs;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class SaveMethodViewModel : BaseViewModel
	{
		private readonly IAuthenticationManager authManager;
		private readonly IDialogFactory dialogFactory;
		private readonly IProjectConfigurationManager projectConfigurationManager;
		private readonly PackageManager packageManager;
		private readonly IArasDataProvider arasDataProvider;
		private readonly MessageManager messageManager;

		private MethodItemTypeInfo methodItemTypeInfo;

		private string projectConfigPath;
		private string projectName;
		private string projectFullName;

		private ConnectionInfo connectionInfo;
		private string innovatorMethodId;

		private string methodComment;
		private int methodCommentMaxLength;
		private string methodType;
		private string methodLanguage;
		private string templateName;
		private string eventData;
		private string methodCode;
		private string selectedIdentityKeyedName;
		private string selectedIdentityId;
		private string selectedPackage;
		private string currentMethodPackage;
		private string methodName;
		private string methodConfigId;
		private int methodNameMaxLength;

		private ICommand okCommand;
		private ICommand closeCommand;
		private ICommand editConnectionInfoCommand;
		private ICommand selectedIdentityCommand;

		public SaveMethodViewModel(
			IAuthenticationManager authManager,
			IDialogFactory dialogFactory,
			IProjectConfigurationManager projectConfigurationManager,
			PackageManager packageManager,
			IArasDataProvider arasDataProvider,
			MethodInfo methodInformation,
			MessageManager messageManager,
			string methodCode,
			string projectConfigPath,
			string projectName,
			string projectFullName)
		{
			if (authManager == null) throw new ArgumentNullException(nameof(authManager));
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (projectConfigurationManager == null) throw new ArgumentNullException(nameof(projectConfigurationManager));
			if (packageManager == null) throw new ArgumentNullException(nameof(packageManager));
			if (arasDataProvider == null) throw new ArgumentNullException(nameof(arasDataProvider));
			if (methodInformation == null) throw new ArgumentNullException(nameof(methodInformation));
			if (messageManager == null) throw new ArgumentNullException(nameof(messageManager));

			this.authManager = authManager;
			this.dialogFactory = dialogFactory;
			this.projectConfigurationManager = projectConfigurationManager;
			this.packageManager = packageManager;
			this.arasDataProvider = arasDataProvider;
			this.messageManager = messageManager;

			this.projectConfigPath = projectConfigPath;
			this.projectName = projectName;
			this.projectFullName = projectFullName;

			this.okCommand = new RelayCommand<object>(OkCommandClick, IsEnabledOkButton);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
			this.editConnectionInfoCommand = new RelayCommand<object>(EditConnectionInfoCommandClick);
			this.selectedIdentityCommand = new RelayCommand(SelectedIdentityCommandClick);

			this.methodItemTypeInfo = arasDataProvider.GetMethodItemTypeInfo();
			this.MethodNameMaxLength = methodItemTypeInfo.NameStoredLength;
			this.MethodCommentMaxLength = methodItemTypeInfo.CommentsStoredLength;

			this.MethodComment = methodInformation.MethodComment;
			this.innovatorMethodId = methodInformation.InnovatorMethodId;

			this.methodCode = methodCode;
			this.methodType = methodInformation.MethodType;
			this.methodLanguage = methodInformation.MethodLanguage;
			this.templateName = methodInformation.TemplateName;
			this.eventData = methodInformation.EventData.ToString();
			this.selectedIdentityKeyedName = methodInformation.ExecutionAllowedToKeyedName;
			this.selectedIdentityId = methodInformation.ExecutionAllowedToId;
			this.selectedPackage = methodInformation.Package.Name;
			this.MethodName = methodInformation.MethodName;
			this.methodConfigId = methodInformation.InnovatorMethodConfigId;

			//TODO: How to know current connection?
			ConnectionInformation = projectConfigurationManager.CurrentProjectConfiguraiton.Connections.First(c => c.LastConnection);
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

		public string MethodType
		{
			get { return methodType; }
			set { }
		}

		public string MethodLanguage
		{
			get { return methodLanguage; }
			set { }
		}

		public string TemplateName
		{
			get { return templateName; }
			set { }
		}

		public string SelectedEventSpecificData
		{
			get { return eventData; }
			set { }
		}

		public string MethodCode
		{
			get { return methodCode; }
			set { }
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

		public List<PackageInfo> AvaliablePackages { get { return packageManager.GetPackageDefinitionList(); } }

		public string SelectedPackage
		{
			get { return selectedPackage; }
			set
			{
				selectedPackage = value;
				RaisePropertyChanged(nameof(SelectedPackage));
			}
		}

		public PackageInfo SelectedPackageInfo { get { return new PackageInfo(selectedPackage ?? string.Empty); } }

		public string CurrentMethodPackage
		{
			get { return currentMethodPackage; }
		}

		public string MethodName
		{
			get { return methodName; }
			set
			{
				if (value != null && value.Length > this.MethodNameMaxLength)
				{
					methodName = value.Substring(0, this.MethodNameMaxLength);
				}
				else
				{
					methodName = value;
				}

				RaisePropertyChanged(nameof(MethodName));
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
				if (value != null && value.Length > this.MethodCommentMaxLength)
				{
					methodComment = value.Substring(0, this.MethodCommentMaxLength);
				}
				else
				{
					methodComment = value;
				}

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

		private dynamic methodItem;

		public dynamic MethodItem
		{
			get { return methodItem; }
			set { methodItem = value; }
		}

		#endregion

		#region Commands

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		//public ICommand EditMethodInfoCommand { get { return editMethodInfoCommand; } }

		public ICommand EditConnectionInfoCommand { get { return editConnectionInfoCommand; } }

		public ICommand SelectedIdentityCommand { get { return selectedIdentityCommand; } }

		#endregion Commands

		private void OkCommandClick(object window)
		{
			var wnd = window as Window;

			try
			{
				this.currentMethodPackage = packageManager.GetPackageDefinitionByElementId(this.methodConfigId)?.Name;
				if (!string.IsNullOrEmpty(currentMethodPackage) && !string.Equals(this.currentMethodPackage, this.selectedPackage))
				{
					var messageWindow = this.dialogFactory.GetMessageBoxWindow();
					var dialogReuslt = messageWindow.ShowDialog(
						messageManager.GetMessage("TheMethodAlreadyAttachedToDiffererntPackageClickOKToReasignPackageForThisMethod", this.methodName),
						messageManager.GetMessage("SaveMethodToArasInnovator"),
						MessageButtons.OKCancel,
						MessageIcon.None);

					if (dialogReuslt == MessageDialogResult.Cancel)
					{
						this.SelectedPackage = this.currentMethodPackage;
						return;
					}
				}

				methodItem = authManager.InnovatorInstance.newItem("Method", "get");
				methodItem.setProperty("name", methodName);
				methodItem = methodItem.apply();

				if (!methodItem.isError() && methodItem.getID() != innovatorMethodId)
				{
					var messageWindow = this.dialogFactory.GetMessageBoxWindow();
					var dialogResult = messageWindow.ShowDialog(
						messageManager.GetMessage("LatestVersionInArasIsDifferrentThatYouHaveClickOKToRewriteArasMethodCode"),
						messageManager.GetMessage("SaveMethodToArasInnovator"),
						MessageButtons.OKCancel,
						MessageIcon.None);

					if (dialogResult != MessageDialogResult.OK)
					{
						return;
					}
				}

				wnd.DialogResult = true;
				wnd.Close();
			}
			catch (Exception ex)
			{
				var messageWindow = this.dialogFactory.GetMessageBoxWindow();
				messageWindow.ShowDialog(ex.Message,
					messageManager.GetMessage("ArasVSMethodPlugin"),
					MessageButtons.OK,
					MessageIcon.Error);
			}
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private void EditConnectionInfoCommandClick(object window)
		{
			var dialogAdapter = this.dialogFactory.GetLoginView(projectConfigurationManager.CurrentProjectConfiguraiton, projectName, projectFullName);
			var dialogResult = dialogAdapter.ShowDialog();

			if (dialogResult.DialogOperationResult == true)
			{
				projectConfigurationManager.Save(projectConfigPath);
				ConnectionInformation = projectConfigurationManager.CurrentProjectConfiguraiton.Connections.First(c => c.LastConnection);

				this.methodItemTypeInfo = arasDataProvider.GetMethodItemTypeInfo();
				this.MethodNameMaxLength = methodItemTypeInfo.NameStoredLength;
				this.MethodCommentMaxLength = methodItemTypeInfo.CommentsStoredLength;
				this.MethodName = this.MethodName;
				this.MethodComment = this.MethodComment;
			}
		}

		private bool IsEnabledOkButton(object obj)
		{
			if (string.IsNullOrEmpty(this.SelectedPackage) ||
				string.IsNullOrEmpty(this.MethodName) ||
				string.IsNullOrEmpty(this.SelectedIdentityKeyedName))
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
			if (!projectConfigurationManager.CurrentProjectConfiguraiton.LastSavedSearch.TryGetValue(itemTypeName, out predefinedSearchItems))
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
				dynamic item = authManager.InnovatorInstance.newItem(itemTypeName, "get");
				item.setID(result.ItemId);
				item.setAttribute("select", "keyed_name");
				item = item.apply();

				this.SelectedIdentityKeyedName = item.getProperty("keyed_name");
				this.selectedIdentityId = result.ItemId;

				if (projectConfigurationManager.CurrentProjectConfiguraiton.LastSavedSearch.ContainsKey(result.ItemType))
				{
					projectConfigurationManager.CurrentProjectConfiguraiton.LastSavedSearch[result.ItemType] = result.LastSavedSearch;
				}
				else
				{
					projectConfigurationManager.CurrentProjectConfiguraiton.LastSavedSearch.Add(result.ItemType, result.LastSavedSearch);
				}
			}
		}
	}
}
