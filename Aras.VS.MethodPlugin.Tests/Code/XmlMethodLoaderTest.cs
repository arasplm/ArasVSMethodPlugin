using System.Collections.Generic;
using System.IO;
using Aras.VS.MethodPlugin.Code;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Code
{
	[TestFixture]
	public class XmlMethodLoaderTest
	{
		[Test]
		public void LoadMethod_ShouldReturnNull()
		{
			//Arange
			XmlMethodLoader xmlMethodLoader = new XmlMethodLoader();

			//Act
			XmlMethodInfo loadMethodResult = xmlMethodLoader.LoadMethod("");

			//Assert
			Assert.IsNull(loadMethodResult);
		}

		[Test]
		public void LoadMethod_ShouldReturnExpected()
		{
			//Arange
			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var methodAmlPath = Path.Combine(currentPath, @"Code\TestData\MethodAml\ReturnNullMethodAml.xml");
			XmlMethodLoader xmlMethodLoader = new XmlMethodLoader();

			//Act
			XmlMethodInfo loadMethodResult = xmlMethodLoader.LoadMethod(methodAmlPath);

			//Assert
			Assert.IsNotNull(loadMethodResult);
			Assert.AreEqual("\r\nreturn null;", loadMethodResult.Code);
			Assert.AreEqual("ReturnNullMethodAml", loadMethodResult.MethodName);
			Assert.AreEqual("C#", loadMethodResult.MethodType);
			Assert.AreEqual(methodAmlPath, loadMethodResult.Path);
		}

		[Test]
		public void LoadMethods_ShouldReturnExpected()
		{
			//Arange
			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var methodAmlPath = Path.Combine(currentPath, @"Code\TestData\MethodAml\ReturnNullMethodAml.xml");
			XmlMethodLoader xmlMethodLoader = new XmlMethodLoader();
			List<string> methodPaths = new List<string>() { methodAmlPath };

			//Act
			List<XmlMethodInfo> loadMethodResult = xmlMethodLoader.LoadMethods(methodPaths);

			//Assert
			Assert.IsNotNull(loadMethodResult);
			Assert.AreEqual(1, loadMethodResult.Count);
		}
	}
}
