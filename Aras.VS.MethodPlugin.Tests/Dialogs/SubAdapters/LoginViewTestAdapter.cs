using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.SubAdapters
{
	public class LoginViewTestAdapter : IViewAdaper<LoginView, ViewResult>
	{
		private bool dialogResult;
		public LoginViewTestAdapter(bool dialogResult)
		{
			this.dialogResult = dialogResult;
		}

		public ViewResult ShowDialog()
		{
			return new ViewResult()
			{
				DialogOperationResult = dialogResult
			};
		}
	}
}
