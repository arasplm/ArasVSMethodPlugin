//------------------------------------------------------------------------------
// <copyright file="CreatePartialElementViewResult.cs" company="Aras Corporation">
//     Copyright © 2018 Aras Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class CreatePartialElementViewResult : ViewResult
	{
		public string FileName { get; set; }

		public bool IsUseVSFormattingCode { get; set; }
	}
}
