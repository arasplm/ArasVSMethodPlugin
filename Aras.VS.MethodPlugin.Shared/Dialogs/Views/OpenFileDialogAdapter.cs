using System.Windows.Forms;

namespace Aras.VS.MethodPlugin.Dialogs.Views
{
	class OpenFileDialogAdapter : IViewAdaper<OpenFileDialog, OpenFileDialogResult>
	{
		private readonly OpenFileDialog view;

		public OpenFileDialogAdapter(OpenFileDialog view)
		{
			this.view = view;
		}

		public OpenFileDialogResult ShowDialog()
		{
			DialogResult dialogResult = this.view.ShowDialog();
			return new OpenFileDialogResult()
			{
				DialogResult = dialogResult,
				FileName = view.FileName
			};
		}
	}
}
