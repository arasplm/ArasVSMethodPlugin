//------------------------------------------------------------------------------
// <copyright file="SelectPathViewModel.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Aras.VS.MethodPlugin.Dialogs.Directory;
using Aras.VS.MethodPlugin.Dialogs.Directory.Data;
using Aras.VS.MethodPlugin.Dialogs.Views;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class SelectPathViewModel : BaseViewModel
	{
		private ObservableCollection<DirectoryItemViewModel> directoryItems;
		private DirectoryItemViewModel selectDirectoryItem;
		private DirectoryItemType searchToLevel;

		public event Action<string> SelectionChanged;

		private ICommand newFolderCommand;
		private ICommand renameFolderCommand;
		private ICommand deleteFolderCommand;
		private ICommand okCommand;
		private ICommand closeCommand;

		public SelectPathViewModel(DirectoryItemType searchToLevel, string startPath = "")
		{
			this.searchToLevel = searchToLevel;

			this.newFolderCommand = new RelayCommand<object>(OnNewFolderClick);
			this.renameFolderCommand = new RelayCommand<object>(OnRenameFolderClick);
			this.deleteFolderCommand = new RelayCommand<object>(OnDeleteFolderClick);
			this.okCommand = new RelayCommand<object>(OkCommandClick);
			this.closeCommand = new RelayCommand<object>(OnCloseCliked);

			this.DirectoryItems = new ObservableCollection<DirectoryItemViewModel>();
			var children = DirectoryStructure.GetLogicalDrives();


			foreach (DirectoryItem drive in children)
			{
				var childViewModel = new DirectoryItemViewModel(drive.FullPath, drive.Type, searchToLevel);
				childViewModel.SelectDirectoryItem += OnSelectDirectoryItem;
				this.DirectoryItems.Add(childViewModel);
			}

			if (!string.IsNullOrEmpty(startPath))
			{
				var pathList = startPath.Split(Path.DirectorySeparatorChar).ToList();
				Navigate(this.DirectoryItems, pathList);
			}
			//TODO: navigate to path
		}

		public void Navigate(IList<DirectoryItemViewModel> currentItems, List<string> pathList)
		{
			var currentPath = pathList.FirstOrDefault();
			if (!string.IsNullOrEmpty(currentPath))
			{

				var currentDirectory = currentItems.FirstOrDefault(di => di.Name.Replace(Path.DirectorySeparatorChar.ToString(),string.Empty) == currentPath);
				if (currentDirectory != null)
				{
					currentDirectory.Expand();
					currentDirectory.IsExpanded = true;
					currentDirectory.IsSelected = true;
					pathList.Remove(currentPath);
					Navigate(currentDirectory.Children, pathList);
				}
			}
		}

		#region Properties

		public ObservableCollection<DirectoryItemViewModel> DirectoryItems
		{
			get { return directoryItems; }
			set { directoryItems = value; }
		}

		public string SelectedPath
		{
			get { return selectDirectoryItem.FullPath; }
			set { selectDirectoryItem.FullPath = value; RaisePropertyChanged(nameof(SelectedPath)); }
		}

		#endregion

		public ICommand NewFolderCommand { get { return newFolderCommand; } }

		public ICommand RenameFolderCommand { get { return renameFolderCommand; } }

		public ICommand DeleteFolderCommand { get { return deleteFolderCommand; } }

		public ICommand OkCommand { get { return okCommand; } }

		public ICommand CloseCommand { get { return closeCommand; } }

		private void OnCloseCliked(object window)
		{
			var wnd = window as Window;
			wnd.Close();
		}

		private void OkCommandClick(object window)
		{
			var wnd = window as Window;
			wnd.DialogResult = true;
			wnd.Close();
		}

		private void OnSelectDirectoryItem(DirectoryItemViewModel selectDirectoryItem)
		{
			this.selectDirectoryItem = selectDirectoryItem;
			var selectionChanged = SelectionChanged;
			if (selectionChanged != null)
			{
				selectionChanged(this.SelectedPath);
			}
		}

		private void OnNewFolderClick(object window)
		{
			var viewModel = new FolderNameViewModel();
			var view = new FolderNameDialog();
			view.DataContext = viewModel;
			view.Owner = window as Window;

			if (view.ShowDialog() == true)
			{
				string folderName = viewModel.FolderName;

				string newFolderPath = Path.Combine(selectDirectoryItem.FullPath, folderName);
				int index = 1;

				while (System.IO.Directory.Exists(newFolderPath))
				{
					newFolderPath = Path.Combine(selectDirectoryItem.FullPath, $"{folderName} {index}");
					index++;
				}

				System.IO.Directory.CreateDirectory(newFolderPath);

				if (selectDirectoryItem.IsExpanded)
				{
					var childViewModel = new DirectoryItemViewModel(newFolderPath, DirectoryItemType.Folder, searchToLevel);
					childViewModel.Parent = selectDirectoryItem;
					childViewModel.SelectDirectoryItem += OnSelectDirectoryItem;

					selectDirectoryItem.Children.Add(childViewModel);
					selectDirectoryItem.Children = new ObservableCollection<DirectoryItemViewModel>(selectDirectoryItem.Children.OrderBy(x => x.Name));
					selectDirectoryItem.IsSelected = false;
					childViewModel.IsSelected = true;
				}
				else
				{
					selectDirectoryItem.IsExpanded = true;

					var childViewModel = selectDirectoryItem.Children.FirstOrDefault(x => x.FullPath == newFolderPath);
					if (childViewModel != null)
					{
						selectDirectoryItem.IsSelected = false;
						childViewModel.IsSelected = true;
					}
				}
			}
		}

		private void OnRenameFolderClick(object window)
		{
			var viewModel = new FolderNameViewModel();
			var view = new FolderNameDialog();
			view.DataContext = viewModel;
			view.Owner = window as Window;

			if (view.ShowDialog() == true)
			{
				string newFolderName = viewModel.FolderName;
				string newFullPath = Path.Combine(Path.GetDirectoryName(selectDirectoryItem.FullPath), newFolderName);

				if (string.Equals(selectDirectoryItem.FullPath, newFullPath))
				{
					return;
				}

				try
				{
					System.IO.Directory.Move(selectDirectoryItem.FullPath, newFullPath);
					selectDirectoryItem.FullPath = newFullPath;
				}
				catch (Exception ex)
				{
					var messageWindow = new MessageBoxWindow();
					messageWindow.ShowDialog(window as Window,
						ex.Message,
						"Aras VS method plugin",
						MessageButtons.OK,
						MessageIcon.Error);
				}
			}
		}

		private void OnDeleteFolderClick(object window)
		{
			string message = $"Are you sure you want to delete the {selectDirectoryItem.Name} folder?";
			var messageWindow = new MessageBoxWindow();
			var dialogResult = messageWindow.ShowDialog(window as Window,
				message,
				"Aras VS method plugin",
				MessageButtons.OKCancel,
				MessageIcon.Warning);

			if (dialogResult == MessageDialogResult.OK)
			{
				try
				{
					System.IO.Directory.Delete(selectDirectoryItem.FullPath, true);

					if (selectDirectoryItem.Parent != null)
					{
						selectDirectoryItem.Parent.Update();
					}
				}
				catch (Exception ex)
				{
					var errorWindow = new MessageBoxWindow();
					errorWindow.ShowDialog(window as Window,
						ex.Message,
						"Aras VS method plugin",
						MessageButtons.OK,
						MessageIcon.Error);
				}
			}
		}
	}
}
