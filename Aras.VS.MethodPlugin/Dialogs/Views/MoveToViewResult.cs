//------------------------------------------------------------------------------
// <copyright file="MoveToViewResult.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.Method.Libs.Code;
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

