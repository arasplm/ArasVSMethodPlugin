using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aras.VS.MethodPlugin.Dialogs.ViewModels;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Dialogs.ViewModels
{
	[TestFixture]
	class ShortMethodInfoViewModelTest
	{
		[Test]
		[TestCase(null)]
		[TestCase("")]
		public void Ctor_ShouldThrowFullNameArgumentNullException(string fullName)
		{
			//Assert
			Assert.Throws<ArgumentNullException>(() =>
			{
				//Act
				ShortMethodInfoViewModel shortMethodInfoView = new ShortMethodInfoViewModel(fullName);
			});
		}

		[Test]
		public void MethodType_ShouldHaveExpectedValue()
		{
			//Arange
			string currentPath = AppDomain.CurrentDomain.BaseDirectory;
			string testFileFullPath = Path.Combine(currentPath, @"Dialogs\ViewModels\TestData\TestMethodItem.xml");

			//Act
			ShortMethodInfoViewModel shortMethodInfoView = new ShortMethodInfoViewModel(testFileFullPath);

			//Assert
			Assert.AreEqual("C#", shortMethodInfoView.MethodType);
		}

		[Test]
		public void MethodCode_ShouldHaveExpectedValue()
		{
			//Arange
			string currentPath = AppDomain.CurrentDomain.BaseDirectory;
			string testFileFullPath = Path.Combine(currentPath, @"Dialogs\ViewModels\TestData\TestMethodItem.xml");

			//Act
			ShortMethodInfoViewModel shortMethodInfoView = new ShortMethodInfoViewModel(testFileFullPath);

			//Assert
			Assert.AreEqual("return null;", shortMethodInfoView.MethodCode);
		}
	}
}
