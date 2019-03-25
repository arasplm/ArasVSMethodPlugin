using Aras.VS.MethodPlugin.Dialogs.ViewModels;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	class FolderNameDialogAdapter : ViewAdaper<FolderNameDialog, FolderNameDialogResult>
	{
		public FolderNameDialogAdapter(FolderNameDialog view) : base(view)
		{
		}

		public override FolderNameDialogResult ShowDialog()
		{
			FolderNameViewModel viewModel = view.DataContext as FolderNameViewModel;
			bool? result = view.ShowDialog();

			return new FolderNameDialogResult()
			{
				DialogOperationResult = result,
				FolderName = viewModel.FolderName
			};
		}
	}
}
