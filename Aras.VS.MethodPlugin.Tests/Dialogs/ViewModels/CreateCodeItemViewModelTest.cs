using System;
using Aras.Method.Libs.Code;
using Aras.VS.MethodPlugin.Code;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.ViewModels
{
	class CreateCodeItemViewModelTest
	{
		[Test]
		public void Ctor_ShouldThrowCodeItemProviderArgumentNullException()
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				CreateCodeItemViewModel viewModel = new CreateCodeItemViewModel(null, false);
			});
		}

		[Test]
		public void Ctor_ShouldInitExpectedProperty()
		{
			//Arange
			ICodeItemProvider codeItemProvider = Substitute.For<ICodeItemProvider>();
			bool usedVSFormatting = true;

			//Act
			CreateCodeItemViewModel viewModel = new CreateCodeItemViewModel(codeItemProvider, usedVSFormatting);

			//Assert
			Assert.IsNotNull(viewModel.OKCommand);
			Assert.IsNotNull(viewModel.CancelCommand);
			Assert.IsNotNull(viewModel.CloseCommand);
			Assert.AreEqual(usedVSFormatting, viewModel.IsUseVSFormattingCode);
			Assert.AreEqual("File1", viewModel.FileName);
		}

		[Test]
		public void OkCommand_CanExecuteReturnTrue()
		{
			//Arange
			ICodeItemProvider codeItemProvider = Substitute.For<ICodeItemProvider>();
			bool usedVSFormatting = true;
			CreateCodeItemViewModel viewModel = new CreateCodeItemViewModel(codeItemProvider, usedVSFormatting);

			//Act
			bool canExecute = viewModel.OKCommand.CanExecute(null);

			//Assert
			Assert.IsTrue(canExecute);
		}

		[Test]
		public void OkCommand_CanExecuteReturnFalse()
		{
			//Arange
			ICodeItemProvider codeItemProvider = Substitute.For<ICodeItemProvider>();
			bool usedVSFormatting = true;

			CreateCodeItemViewModel viewModel = new CreateCodeItemViewModel(codeItemProvider, usedVSFormatting);
			viewModel.FileName = string.Empty;

			//Act
			bool canExecute = viewModel.OKCommand.CanExecute(null);

			//Assert
			Assert.IsFalse(canExecute);
		}
	}
}
