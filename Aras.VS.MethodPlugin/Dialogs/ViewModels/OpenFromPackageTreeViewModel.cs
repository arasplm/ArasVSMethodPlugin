//------------------------------------------------------------------------------
// <copyright file="OpenFromPackageTreeViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;
using Aras.VS.MethodPlugin.Dialogs.Views;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class OpenFromPackageTreeViewModel : BaseViewModel
	{
		private const string importFileName = ".mf";

		private Dictionary<string, string> packages;
		private List<ShortMethodInfoViewModel> methods;

		private string selectedPackageName;
		private ShortMethodInfoViewModel selectedMethodValue;
		private string rootFolderPath;
		private SelectPathViewModel selectPathViewModel;
		private ICommand okCommand;
		private ICommand closeCommand;
		private ICommand pathChangeCommand;

		public OpenFromPackageTreeViewModel(string lastSelectedManifestFilePath, string lastSelectedPackage, string lastSelectedMethod)
		{

			SelectPathViewModel = new SelectPathViewModel(DirectoryItemType.File, lastSelectedManifestFilePath, importFileName);
			SelectPathViewModel.SelectionChanged += OnSelectDirectoryItem;
			this.okCommand = new RelayCommand<object>(OkCommandClick);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
			this.pathChangeCommand = new RelayCommand<object>(OnPathChange);

			if (!string.IsNullOrEmpty(lastSelectedManifestFilePath))
			{
				OnSelectDirectoryItem(lastSelectedManifestFilePath);
				this.SelectedPackageName = lastSelectedPackage;
				this.SelectedMethod = this.Methods.FirstOrDefault(x => x.Name == lastSelectedMethod);
			}
		}

		#region Properties

		public SelectPathViewModel SelectPathViewModel
		{
			get { return selectPathViewModel; }
			set { selectPathViewModel = value; RaisePropertyChanged(nameof(SelectPathViewModel)); }
		}

		public Dictionary<string, string> Packages
		{
			get { return packages; }
			set
			{
				this.packages = value;
				this.Methods = new List<ShortMethodInfoViewModel>();
				RaisePropertyChanged(nameof(Packages));
			}
		}

		public string SelectedPackageName
		{
			get { return selectedPackageName; }
			set
			{
				selectedPackageName = value;

				var localMethods = new List<ShortMethodInfoViewModel>();
				if (!string.IsNullOrEmpty(selectedPackageName))
				{
					string methodsPath = Path.Combine(rootFolderPath, Packages[selectedPackageName], "Method");
					if (System.IO.Directory.Exists(methodsPath))
					{
						localMethods = new DirectoryInfo(methodsPath).GetFiles("*.xml").Select(x => new ShortMethodInfoViewModel(x.FullName)).ToList();
					}
				}

				this.Methods = localMethods;
			}
		}

		public List<ShortMethodInfoViewModel> Methods
		{
			get { return methods; }
			set
			{
				this.methods = value;
				RaisePropertyChanged(nameof(Methods));
			}
		}

		public ShortMethodInfoViewModel SelectedMethod
		{
			get { return selectedMethodValue; }
			set { selectedMethodValue = value; RaisePropertyChanged(nameof(SelectedMethod)); }
		}

		#endregion

		#region Commands

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		public ICommand PathChangeCommand { get { return pathChangeCommand; } }
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
				var messageWindow = new MessageBoxWindow();
				messageWindow.ShowDialog(wnd,
					"Method is not selected.",
					"Open method from AML package",
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
			var wnd = window as Window;
			var path = SelectPathViewModel.SelectedPath;
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path))
			{
				selectPathViewModel.Navigate(selectPathViewModel.DirectoryItems, path.Split(Path.DirectorySeparatorChar).ToList());
			}
			else
			{
				var messageWindow = new MessageBoxWindow();
				messageWindow.ShowDialog(wnd,
					"File or Folder were not found",
					"Open method from AML package",
					MessageButtons.OK,
					MessageIcon.None);
			}
		}
	}
}
