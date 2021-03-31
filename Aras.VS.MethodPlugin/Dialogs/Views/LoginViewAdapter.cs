//------------------------------------------------------------------------------
// <copyright file="LoginViewAdapter.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class LoginViewAdapter : ViewAdaper<LoginView,ViewResult>
	{
		public LoginViewAdapter(LoginView view) : base(view)
		{
		}

		public override ViewResult ShowDialog()
		{
			var result = view.ShowDialog();
			return new ViewResult() { DialogOperationResult = result };
		}
	}
}
