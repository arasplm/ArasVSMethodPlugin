//------------------------------------------------------------------------------
// <copyright file="MessageBoxWindow.xaml.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	/// <summary>
	/// Interaction logic for ConnectionInfoView.xaml
	/// </summary>
	public partial class MessageBoxWindow : Window
	{
		private MessageDialogResult dialogResult = MessageDialogResult.Cancel;

		public MessageBoxWindow()
		{
			InitializeComponent();

			var viewModel = new MessageBoxViewModel();
			this.DataContext = viewModel;
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			this.dialogResult = MessageDialogResult.OK;
			this.Close();
		}

		private void YesButton_Click(object sender, RoutedEventArgs e)
		{
			this.dialogResult = MessageDialogResult.Yes;
			this.Close();
		}

		private void NoButton_Click(object sender, RoutedEventArgs e)
		{
			this.dialogResult = MessageDialogResult.No;
			this.Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			this.dialogResult = MessageDialogResult.Cancel;
			this.Close();
		}

		public MessageDialogResult ShowDialog(Window owner, string message, string title, MessageButtons buttons, MessageIcon icon)
		{
			if (owner == null)
			{
				var windowInteropHelper = new WindowInteropHelper(this);
				windowInteropHelper.Owner = Process.GetCurrentProcess().MainWindowHandle;
			}
			else
			{
				this.Owner = owner;
			}

			return ShowDialog(message, title, buttons, icon);
		}

		public MessageDialogResult ShowDialog(string message, string title, MessageButtons buttons, MessageIcon icon)
		{
			this.Title = title;
			this.MessageTextBlock.Text = message;

			if (buttons == MessageButtons.OK)
			{
				this.YesButton.Visibility = Visibility.Collapsed;
				this.NoButton.Visibility = Visibility.Collapsed;
				this.CancelButton.Visibility = Visibility.Collapsed;
			}
			else if (buttons == MessageButtons.OKCancel)
			{
				this.YesButton.Visibility = Visibility.Collapsed;
				this.NoButton.Visibility = Visibility.Collapsed;
			}
			else if (buttons == MessageButtons.YesNoCancel)
			{
				this.OKButton.Visibility = Visibility.Collapsed;
			}
			else if (buttons == MessageButtons.YesNo)
			{
				this.OKButton.Visibility = Visibility.Collapsed;
				this.CancelButton.Visibility = Visibility.Collapsed;
			}

			if (icon == MessageIcon.Error)
			{
				this.MessageImage.Source = iconToImageSource(SystemIcons.Error);
			}
			else if (icon == MessageIcon.Warning)
			{
				this.MessageImage.Source = iconToImageSource(SystemIcons.Warning);
			}
			else if (icon == MessageIcon.Information)
			{
				this.MessageImage.Source = iconToImageSource(SystemIcons.Information);
			}
			else
			{
				this.MessageImage.Visibility = Visibility.Collapsed;
			}

			this.ShowDialog();
			return dialogResult;
		}

		private ImageSource iconToImageSource(Icon icon)
		{
			ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
				icon.Handle,
				Int32Rect.Empty,
				BitmapSizeOptions.FromEmptyOptions());

			return imageSource;
		}
	}
}
