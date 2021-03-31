//------------------------------------------------------------------------------
// <copyright file="SelectPathDialogResult.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs;

namespace OfficeConnector.Dialogs
{
	public class SelectPathDialogResult : ViewResult
	{
		public string SelectedFullPath { get; set; }
	}
}

