//------------------------------------------------------------------------------
// <copyright file="OpenFromPackageTreeViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class OpenFromPackageTreeViewModel : BaseViewModel
	{
		private const string importFileName = ".mf";
		private const string searchMethodFilePatern = "*.xml";

		private const string searchByContentKey = "MethodContent";
		private const string searchByFileNameKey = "MethodName";

		private readonly IDialogFactory dialogFactory;
		private readonly IIOWrapper iOWrapper;
		private readonly MessageManager messageManager;

		private Dictionary<string, string> packages;
		private List<ShortMethodInfoViewModel> methods;

		private List<ShortMethodInfoViewModel> allPackageMethods;

		private string selectedPackageName;
		private ShortMethodInfoViewModel selectedMethodValue;
		private string rootFolderPath;
		private SelectPathViewModel selectPathViewModel;
		private string searchPattern;
		private ICommand okCommand;
		private ICommand closeCommand;
		private ICommand pathChangeCommand;
		private ICommand cancelCommand;

		private DispatcherTimer dispatcherTimer;
		private Dictionary<string, SearchType> searchTypes;

		public OpenFromPackageTreeViewModel(
			IDialogFactory dialogFactory,
			IIOWrapper iOWrapper,
			MessageManager messageManager,
			string lastSelectedManifestFilePath,
			string lastSelectedPackage,
			string lastSelectedMethod,
			string lastUsedSearchType)
		{
			if (dialogFactory == null) throw new ArgumentNullException(nameof(dialogFactory));
			if (iOWrapper == null) throw new ArgumentNullException(nameof(iOWrapper));
			if (messageManager == null) throw new ArgumentNullException(nameof(messageManager));

			this.dialogFactory = dialogFactory;
			this.iOWrapper = iOWrapper;
			this.messageManager = messageManager;

			SelectPathViewModel = new SelectPathViewModel(dialogFactory, DirectoryItemType.File, iOWrapper, this.messageManager, startPath:lastSelectedManifestFilePath, fileExtantion: importFileName);
			SelectPathViewModel.SelectionChanged += OnSelectDirectoryItem;
			this.okCommand = new RelayCommand<object>(OkCommandClick);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
			this.pathChangeCommand = new RelayCommand<object>(OnPathChange);
			this.cancelCommand = new RelayCommand<object>(OnCloseCliked);

			this.dispatcherTimer = new DispatcherTimer();
			this.dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 750);
			this.dispatcherTimer.Tick += DispatcherTimer_Tick;

			if (!string.IsNullOrEmpty(lastSelectedManifestFilePath))
			{
				OnSelectDirectoryItem(lastSelectedManifestFilePath);
				this.SelectedPackageName = lastSelectedPackage;
				this.SelectedMethod = this.Methods.FirstOrDefault(x => x.Name == lastSelectedMethod);
			}

			this.searchTypes = new Dictionary<string, SearchType>
			{
				{ searchByContentKey, new SearchType() { Icon = Properties.Resources.searchByContent, TypeName = "Search by method content" } },
				{ searchByFileNameKey, new SearchType() { Icon = Properties.Resources.searchFile, TypeName = "Search by method name" } }
			};

			if (!string.IsNullOrEmpty(lastUsedSearchType) && this.searchTypes.TryGetValue(lastUsedSearchType, out SearchType searchType))
			{
				this.selectedSearchType = lastUsedSearchType;
			}
			else
			{
				this.selectedSearchType = searchByContentKey;
			}
		}

		#region Properties

		public SelectPathViewModel SelectPathViewModel
		{
			get { return selectPathViewModel; }
			set
			{
				selectPathViewModel = value;
				RaisePropertyChanged(nameof(SelectPathViewModel));
			}
		}

		public Dictionary<string, string> Packages
		{
			get { return packages; }
			set
			{
				this.packages = value;
				RaisePropertyChanged(nameof(Packages));
			}
		}

		public string SelectedPackageName
		{
			get { return selectedPackageName; }
			set
			{
				selectedPackageName = value;
				LoadMethods();
				RunSearchByCriteria();
				RaisePropertyChanged(nameof(this.SelectedPackageName));
			}
		}

		public PackageInfo SelectedPakckageInfo { get { return new PackageInfo(selectedPackageName); } }

		public List<ShortMethodInfoViewModel> Methods
		{
			get
			{
				return this.methods;
			}
			set
			{
				this.methods = value;
				RaisePropertyChanged(nameof(Methods));
			}
		}

		public ShortMethodInfoViewModel SelectedMethod
		{
			get { return selectedMethodValue; }
			set
			{
				selectedMethodValue = value;
				RaisePropertyChanged(nameof(SelectedMethod));
			}
		}


		public string SearchPattern
		{
			get { return this.searchPattern; }
			set
			{
				this.searchPattern = value;
				this.dispatcherTimer.Stop();
				this.dispatcherTimer.Start();
				RaisePropertyChanged(nameof(this.SearchPattern));
			}
		}

		public Dictionary<string, SearchType> SearchTypes
		{
			get { return this.searchTypes; }
		}

		private string selectedSearchType;
		public string SelectedSearchType
		{
			get { return this.selectedSearchType; }
			set
			{
				this.selectedSearchType = value;
				if (!string.IsNullOrEmpty(this.searchPattern))
				{
					RunSearchByCriteria();
				}
			}
		}

		#endregion

		#region Commands
		public ICommand OkCommand
		{ get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		public ICommand PathChangeCommand { get { return pathChangeCommand; } }

		public ICommand CancelCommand { get { return cancelCommand; } }
		#endregion

		private void OnSelectDirectoryItem(string fullPath)
		{
			this.rootFolderPath = Path.GetDirectoryName(fullPath);

			var localPackages = new Dictionary<string, string>();
			var extension = Path.GetExtension(fullPath);
			if (string.Equals(extension, importFileName))
			{
				var xmlDocument = new XmlDocument();
				xmlDocument.Load(fullPath);
				XmlNodeList packageXmlNodes = xmlDocument.SelectNodes("imports/package");
				foreach (XmlNode packageXmlNode in packageXmlNodes)
				{
					string name = packageXmlNode.Attributes["name"].InnerText;
					string path = packageXmlNode.Attributes["path"].InnerText;
					localPackages.Add(name, path);
				}
			}

			this.Packages = localPackages;
		}

		private void OkCommandClick(object window)
		{
			var wnd = window as Window;

			if (SelectedMethod == null)
			{
				var messageWindow = this.dialogFactory.GetMessageBoxWindow();
				messageWindow.ShowDialog(messageManager.GetMessage("MethodIsNotSelected"),
					messageManager.GetMessage("OpenMethodFromAMLPackage"),
					MessageButtons.OK,
					MessageIcon.None);
			}
			else
			{
				wnd.DialogResult = true;
				wnd.Close();
			}
		}

		private void OnCloseCliked(object view)
		{
			(view as Window).Close();
		}

		private void OnPathChange(object window)
		{
			var path = SelectPathViewModel.SelectedPath;
			if (this.iOWrapper.FileExists(path) || this.iOWrapper.DirectoryExists(path))
			{
				selectPathViewModel.Navigate(selectPathViewModel.DirectoryItems, path.Split(Path.DirectorySeparatorChar).ToList());
			}
			else
			{
				var messageWindow = this.dialogFactory.GetMessageBoxWindow();
				messageWindow.ShowDialog(messageManager.GetMessage("FileOrFolderWereNotFound"),
					messageManager.GetMessage("OpenMethodFromAMLPackage"),
					MessageButtons.OK,
					MessageIcon.None);
			}
		}

		private void LoadMethods()
		{
			this.allPackageMethods = new List<ShortMethodInfoViewModel>();
			if (!string.IsNullOrEmpty(selectedPackageName))
			{
				string methodsPath = Path.Combine(rootFolderPath, Packages[selectedPackageName], "Method");
				if (this.iOWrapper.DirectoryExists(methodsPath))
				{
					this.allPackageMethods = this.iOWrapper.DirectoryGetFiles(methodsPath, searchMethodFilePatern).Select(x => new ShortMethodInfoViewModel(x)).ToList();
				}
			}
		}

		private void RunSearchByCriteria()
		{
			List<ShortMethodInfoViewModel> localMethods = new List<ShortMethodInfoViewModel>();
			if (!string.IsNullOrEmpty(this.SearchPattern))
			{
				if (this.SelectedSearchType == searchByContentKey)
				{
					this.Methods = this.allPackageMethods.Where(x => x.MethodCode.IndexOf(this.SearchPattern, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
				}
				else
				{
					this.Methods = this.allPackageMethods.Where(x => x.Name.IndexOf(this.SearchPattern, StringComparison.InvariantCultureIgnoreCase) != -1).ToList();
				}
			}
			else
			{
				this.Methods = this.allPackageMethods;
			}
		}

		private void DispatcherTimer_Tick(object sender, EventArgs e)
		{
			this.dispatcherTimer.Stop();
			RunSearchByCriteria();
		}
	}

	public class SearchType
	{
		public Bitmap Icon { get; set; }
		public string TypeName { get; set; }
	}
}
