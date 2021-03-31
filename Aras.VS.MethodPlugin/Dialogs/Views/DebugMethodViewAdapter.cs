// <copyright file="DebugMethodViewAdapter.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	public class DebugMethodViewAdapter : ViewAdaper<DebugMethodView, DebugMethodViewResult>
	{
		public DebugMethodViewAdapter(DebugMethodView view) : base(view)
		{
		}

		public override DebugMethodViewResult ShowDialog()
		{
			var viewModel = view.DataContext as DebugMethodViewModel;
			var result = view.ShowDialog();

			return new DebugMethodViewResult()
			{
				DialogOperationResult = result,
				MethodContext = viewModel.MethodContext
			};
		}
	}
}
