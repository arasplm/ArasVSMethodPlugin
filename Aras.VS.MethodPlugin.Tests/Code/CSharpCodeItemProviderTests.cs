using System;
using System.Collections.Generic;
using System.IO;
using Aras.Method.Libs;
using Aras.Method.Libs.Code;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Code
{
	[TestFixture]
	public class CSharpCodeItemProviderTests
	{
		private MessageManager messageManager;
		private CSharpCodeItemProvider provider;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			messageManager = Substitute.For<MessageManager>();
			messageManager.GetMessage("CurrentCodeElementTypeIsNotSupported").Returns("Current code element type is not supported");

			provider = new CSharpCodeItemProvider(messageManager);
		}

		[Test]
		public void GetSupportedCodeElementTypes_Partial_ShouldReturnExpectedList()
		{
			// Arrange
			List<CodeElementType> expected = new List<CodeElementType>();
			expected.Add(CodeElementType.Interface);
			expected.Add(CodeElementType.Class);
			expected.Add(CodeElementType.Struct);
			expected.Add(CodeElementType.Method);
			expected.Add(CodeElementType.Enum);
			expected.Add(CodeElementType.Custom);

			// Act
			List<CodeElementType> actual = provider.GetSupportedCodeElementTypes(CodeType.Partial);

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GetSupportedCodeElementTypes_External_ShouldReturnExpectedList()
		{
			// Arrange
			List<CodeElementType> expected = new List<CodeElementType>();
			expected.Add(CodeElementType.Interface);
			expected.Add(CodeElementType.Class);
			expected.Add(CodeElementType.Struct);
			expected.Add(CodeElementType.Enum);
			expected.Add(CodeElementType.Custom);

			// Act
			List<CodeElementType> actual = provider.GetSupportedCodeElementTypes(CodeType.External);

			// Assert
			Assert.AreEqual(expected, actual);
		}

		[TestCase(CodeType.External, CodeElementType.Method)]
		public void GetCodeElementTypeTemplate_NotSupportedCodeElement_ShouldThrowsException(CodeType codeType, CodeElementType elementType)
		{
			// Assert
			Exception exception = Assert.Throws<NotSupportedException>(() =>
				{
					// Act
					provider.GetCodeElementTypeTemplate(codeType, elementType);
				});

			Assert.AreEqual(this.messageManager.GetMessage("CurrentCodeElementTypeIsNotSupported"), exception.Message);
		}

		[TestCase(CodeType.Partial, CodeElementType.Interface, @"Code\TestData\CodeElementTypeTemplates\PartialInterface.txt")]
		[TestCase(CodeType.Partial, CodeElementType.Class, @"Code\TestData\CodeElementTypeTemplates\PartialClass.txt")]
		[TestCase(CodeType.Partial, CodeElementType.Struct, @"Code\TestData\CodeElementTypeTemplates\PartialStruct.txt")]
		[TestCase(CodeType.Partial, CodeElementType.Method, @"Code\TestData\CodeElementTypeTemplates\PartialMethod.txt")]
		[TestCase(CodeType.Partial, CodeElementType.Enum, @"Code\TestData\CodeElementTypeTemplates\PartialEnum.txt")]
		[TestCase(CodeType.Partial, CodeElementType.Custom, @"Code\TestData\CodeElementTypeTemplates\PartialCustom.txt")]
		[TestCase(CodeType.External, CodeElementType.Interface, @"Code\TestData\CodeElementTypeTemplates\ExternalInterface.txt")]
		[TestCase(CodeType.External, CodeElementType.Class, @"Code\TestData\CodeElementTypeTemplates\ExternalClass.txt")]
		[TestCase(CodeType.External, CodeElementType.Struct, @"Code\TestData\CodeElementTypeTemplates\ExternalStruct.txt")]
		[TestCase(CodeType.External, CodeElementType.Enum, @"Code\TestData\CodeElementTypeTemplates\ExternalEnum.txt")]
		[TestCase(CodeType.External, CodeElementType.Custom, @"Code\TestData\CodeElementTypeTemplates\ExternalCustom.txt")]
		public void GetCodeElementTypeTemplate_ShouldReturnExpectedTemplate(CodeType codeType, CodeElementType elementType, string path)
		{
			// Act
			string actual = provider.GetCodeElementTypeTemplate(codeType, elementType);

			// Assert
			Assert.AreEqual(File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)), actual);
		}
	}
}
