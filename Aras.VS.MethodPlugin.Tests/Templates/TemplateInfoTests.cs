using Aras.VS.MethodPlugin.Templates;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Templates
{
	[TestFixture]
	public class TemplateInfoTests
	{
		[TestCase("", "")]
		[TestCase(null, null)]
		[TestCase("TemplateName", "TemplateName")]

		public void ToString_ShouldReturnExpectedValue(string templateName, string expected)
		{
			// Arrange
			TemplateInfo templateInfo = new TemplateInfo
			{
				TemplateName = templateName
			};

			// Act
			string actual = templateInfo.ToString();

			// Assert
			Assert.AreEqual(expected, actual);
		}
	}
}
