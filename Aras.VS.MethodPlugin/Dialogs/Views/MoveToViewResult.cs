//------------------------------------------------------------------------------
// <copyright file="MoveToViewResult.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs;

namespace OfficeConnector.Dialogs
{
	public class MoveToViewResult : ViewResult
	{
		public string SelectedFullPath { get; set; }

		public string FileName { get; set; }

		public CodeType SelectedCodeType { get; set; }
	}
}

