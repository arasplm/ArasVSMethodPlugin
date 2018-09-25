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
		private Dictionary<string, string> methods;

		private string selectedPackageValue;
		private string selectedMethodValue;
		private string rootFolderPath;
		private SelectPathViewModel selectPathViewModel;
		private ICommand okCommand;
		private ICommand closeCommand;
	    private ICommand pathChangeCommand;

        public OpenFromPackageTreeViewModel(string lastSelectedDir)
		{

			SelectPathViewModel = new SelectPathViewModel(DirectoryItemType.File, lastSelectedDir, importFileName);
			SelectPathViewModel.SelectionChanged += OnSelectDirectoryItem;
			this.okCommand = new RelayCommand<object>(OkCommandClick);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);
		    this.pathChangeCommand = new RelayCommand<object>(OnPathChange);
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
				this.Methods = new Dictionary<string, string>();
				RaisePropertyChanged(nameof(Packages));
			}
		}

		public string SelectedPackageValue
		{
			get { return selectedPackageValue; }
			set
			{
				selectedPackageValue = value;

				var localMethods = new Dictionary<string, string>();
				if (!string.IsNullOrEmpty(selectedPackageValue))
				{
					string methodsPath = Path.Combine(rootFolderPath, selectedPackageValue, "Method");
					if (System.IO.Directory.Exists(methodsPath))
					{
						localMethods = new DirectoryInfo(methodsPath).GetFiles("*.xml").ToDictionary(x => x.Name, x => x.FullName);
					}
				}

				this.Methods = localMethods;
			}
		}

		public Dictionary<string, string> Methods
		{
			get { return methods; }
			set
			{
				this.methods = value;
				RaisePropertyChanged(nameof(Methods));
			}
		}

		public string SelectedMethodValue
		{
			get { return selectedMethodValue; }
			set { selectedMethodValue = value; }
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
			var extension =  Path.GetExtension(fullPath);
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

			if (string.IsNullOrEmpty(SelectedMethodValue))
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
