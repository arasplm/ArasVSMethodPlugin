//------------------------------------------------------------------------------
// <copyright file="ConnectionInfoViewAdapter.cs" company="Aras Corporation">
//     © 2017-2019 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class ConnectionInfoViewAdapter : ViewAdaper<ConnectionInfoView, ViewResult>
	{
		public ConnectionInfoViewAdapter(ConnectionInfoView view) : base(view)
		{
		}

		public override ViewResult ShowDialog()
		{
			var result = view.ShowDialog();
			return new ViewResult() { DialogOperationResult = result };
		}
	}
}
