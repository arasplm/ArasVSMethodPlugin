using System.Windows.Forms;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.SubAdapters
{

	class OpenFileDialogTestAdapter : IViewAdaper<OpenFileDialog, OpenFileDialogResult>
	{
		private DialogResult dialogResult;
		private string fileName;

		public OpenFileDialogTestAdapter(DialogResult dialogResult, string fileName)
		{
			this.dialogResult = dialogResult;
			this.fileName = fileName;
		}

		public OpenFileDialogResult ShowDialog()
		{
			return new OpenFileDialogResult()
			{
				DialogResult = this.dialogResult,
				FileName = this.fileName
			};
		}
	}
}
