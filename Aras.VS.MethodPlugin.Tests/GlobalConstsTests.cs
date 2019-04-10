using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests
{
	[TestFixture]
	public class GlobalConstsTests
	{
		[Test]
		public void CSharp_ShouldReturnExpectedValue()
		{
			Assert.AreEqual(GlobalConsts.CSharp, "C#");
		}

		[Test]
		public void PartialPath_ShouldReturnExpectedValue()
		{
			Assert.AreEqual(GlobalConsts.PartialPath, "PartialPath");
		}

		[Test]
		public void PartialPathAttribute_ShouldReturnExpectedValue()
		{
			Assert.AreEqual(GlobalConsts.PartialPathAttribute, "PartialPathAttribute");
		}

		[Test]
		public void ExternalPath_ShouldReturnExpectedValue()
		{
			Assert.AreEqual(GlobalConsts.ExternalPath, "ExternalPath");
		}

		[Test]
		public void ExternalPathAttribute_ShouldReturnExpectedValue()
		{
			Assert.AreEqual(GlobalConsts.ExternalPathAttribute, "ExternalPathAttribute");
		}
	}
}
