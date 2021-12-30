//------------------------------------------------------------------------------
// <copyright file="CreateCodeItemViewResult.cs" company="Aras Corporation">
//     Copyright © 2022 Aras Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.Method.Libs.Code;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class CreateCodeItemViewResult : ViewResult
	{
		public string FileName { get; set; }

		public CodeType SelectedCodeType { get; set; }

		public CodeElementType SelectedElementType { get; set; }

		public bool IsUseVSFormattingCode { get; set; }
	}
}
