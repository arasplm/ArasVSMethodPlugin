//------------------------------------------------------------------------------
// <copyright file="IView.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Windows.Forms;

namespace OfficeConnector.Dialogs
{
	public interface IView
	{
		void Show();

		void ShowAsDialog(IWin32Window parentWindow = null);

		void Close();

		DialogResult DialogResult { get; set; }
	}
}
