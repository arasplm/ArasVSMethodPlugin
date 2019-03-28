using System.Collections.Generic;
using System.IO;
using Aras.VS.MethodPlugin.Dialogs;
using Aras.VS.MethodPlugin.Dialogs.Views;
using Aras.VS.MethodPlugin.Templates;
using Microsoft.VisualStudio.Shell.Interop;
using NSubstitute;
using NUnit.Framework;

namespace Aras.VS.MethodPlugin.Tests.Templates
{
	[TestFixture]
	public class TemplateLoaderTest
	{
		private TemplateLoader templateLoader;
		private IDialogFactory dialogFactory;
		private IVsUIShell iVsUIShell;
		private IMessageManager messageManager;

		[SetUp]
		public void SetUp()
		{
			this.dialogFactory = Substitute.For<IDialogFactory>();
			this.iVsUIShell = Substitute.For<IVsUIShell>();
			this.messageManager = Substitute.For<IMessageManager>();
			this.templateLoader = new TemplateLoader(dialogFactory, messageManager);
		}

		[Test]
		public void Load_ShoultLoadExpected()
		{
			//Arange
			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var methodConfigPath = Path.Combine(currentPath, @"Templates\TestData\method-config.xml");
			List<TemplateInfo> expectedtemplates = LoadExpectedtemplates();

			//Act
			templateLoader.Load(methodConfigPath);

			//Assert
			Assert.AreEqual(expectedtemplates.Count, templateLoader.Templates.Count);
			for (int i = 0; i < templateLoader.Templates.Count; i++)
			{
				Assert.IsTrue(IsSameTemplates(expectedtemplates[i], templateLoader.Templates[i]));
			}

		}

		[Test]
		public void GetTemplateFromCodeString_ShouldReturnExpected()
		{
			//Arange
			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var methodConfigPath = Path.Combine(currentPath, @"Templates\TestData\method-config.xml");
			templateLoader.Load(methodConfigPath);
			string testCode = "testCode";

			TemplateInfo expectedtemplate = LoadExpectedtemplates()[2];

			//Act
			TemplateInfo actualTemplate = templateLoader.GetTemplateFromCodeString(testCode, "C#", null);

			//Assert
			Assert.IsTrue(IsSameTemplates(expectedtemplate, actualTemplate));
		}

		[Test]
		public void GetTemplateFromCodeString_ShouldShowMessageAndReturnNull()
		{
			//Arange
			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var methodConfigPath = Path.Combine(currentPath, @"Templates\TestData\method-config.xml");
			templateLoader.Load(methodConfigPath);

			string testCode = "//MethodTemplateName=templateName";

			IMessageBoxWindow messageBoxWindow = Substitute.For<IMessageBoxWindow>();
			this.dialogFactory.GetMessageBoxWindow().Returns(messageBoxWindow);

			//Act
			TemplateInfo actualTemplate = templateLoader.GetTemplateFromCodeString(testCode, "testLanguage", "operationName");

			//Assert
			messageBoxWindow.Received().ShowDialog(Arg.Any<string>(), "operationName", MessageButtons.OK, MessageIcon.Information);
			Assert.IsNull(actualTemplate);
		}

		[Test]
		public void GetTemplateFromCodeString_ShouldShowMessageAndReturnExpected()
		{
			//Arange
			var currentPath = System.AppDomain.CurrentDomain.BaseDirectory;
			var methodConfigPath = Path.Combine(currentPath, @"Templates\TestData\method-config.xml");
			templateLoader.Load(methodConfigPath);
			string testCode = "//MethodTemplateName=templateName";
			IMessageBoxWindow messageBoxWindow = Substitute.For<IMessageBoxWindow>();

			this.dialogFactory.GetMessageBoxWindow().Returns(messageBoxWindow);
			TemplateInfo expectedtemplate = LoadExpectedtemplates()[2];

			//Act
			TemplateInfo actualTemplate = templateLoader.GetTemplateFromCodeString(testCode, "C#", "operationName");

			//Assert
			messageBoxWindow.Received().ShowDialog(Arg.Any<string>(), "operationName", MessageButtons.OK, MessageIcon.Information);
			Assert.IsTrue(IsSameTemplates(expectedtemplate, actualTemplate));
		}

		private List<TemplateInfo> LoadExpectedtemplates()
		{
			List<TemplateInfo> templateInfos = new List<TemplateInfo>();
			TemplateInfo newTemplateInfo = new TemplateInfo()
			{
				TemplateName = "VBScript",
				IsSuccessfullySupported = true,
				IsSupported = true,
				TemplateCode = "\r\nImports Microsoft.VisualBasic\r\nImports System\r\nImports System.IO\r\nImports System.Text\r\nImports System.Xml\r\nImports System.Collections\r\nImports System.Collections.Generic\r\nImports System.Data\r\nImports System.Linq\r\nImports System.Net\r\nImports System.Web\r\nImports System.Web.SessionState\r\nImports System.Globalization\r\nImports Aras.IOM\r\n\r\nNamespace $(pkgname)\r\n\r\n  Class ItemMethod\r\n    Inherits Item\r\n    \r\n    Public Sub New(InnovatorObject_arg As IServerConnection)\r\n      MyBase.New(InnovatorObject_arg)\r\n    End Sub\r\n\r\n#If EventDataIsAvailable Then\r\n\t\tPublic Function methodCode( _\r\n\t\t\t) As Item\r\n\t\t\tReturn methodCode( Nothing )\r\n\t\tEnd Function\r\n\r\n\t\tPublic Function methodCode( _\r\n\t\t\tByVal eventData As $(EventDataClass) _\r\n\t\t\t) As Item\r\n#Else\r\n\t\tPublic Function methodCode( _\r\n\t\t\t) As Item\r\n#End If\r\n\t\tDim CCO As Aras.Server.Core.CallContext = CType(serverConnection,Aras.Server.Core.IOMConnection).CCO\r\n\t\tDim RequestState As Aras.Server.Core.IContextState = CCO.RequestState\r\n\t\t$(MethodCode)\r\n    End Function\r\n  End Class\r\n\r\n  Class $(clsname)\r\n    Implements $(interfacename)\r\n#If EventDataIsAvailable Then\r\n\t\tPublic Sub $(fncname)( _\r\n\t\t\tByVal InnovatorObject_arg As IServerConnection, _\r\n\t\t\tByVal inDom As XmlDocument, _\r\n\t\t\tByVal eventData As $(EventDataClass), _\r\n\t\t\tByVal outDom As XmlDocument _\r\n\t\t\t) Implements $(interfacename).$(fncname)\r\n#Else\r\n\t\tPublic Sub $(fncname)( _\r\n\t\t\tInnovatorObject_arg As IServerConnection, _\r\n\t\t\tinDom As XmlDocument, _\r\n\t\t\toutDom As XmlDocument _\r\n\t\t\t) Implements $(interfacename).$(fncname)\r\n#End If\r\n      Dim inItem As ItemMethod\r\n      Dim outItem As Item\r\n\r\n      inItem = new ItemMethod(InnovatorObject_arg)\r\n      inItem.dom = inDom\r\n\r\n      Dim nodes As XmlNodeList = inDom.SelectNodes(\"//Item[not(ancestor::node()[local-name()='Item'])]\")\r\n      If (nodes.Count = 1) Then\r\n        inItem.node = CType(nodes(0), XmlElement)\r\n      Else\r\n        inItem.node = Nothing\r\n        inItem.nodeList = nodes\r\n      End If\r\n        \r\n#If EventDataIsAvailable Then\r\n\t\t\toutItem = inItem.methodCode( _\r\n\t\t\t\teventData _\r\n\t\t\t\t)\r\n#Else\r\n\t\t\toutItem = inItem.methodCode( _\r\n\t\t\t\t)\r\n#End If\r\n      If (Not outItem Is Nothing) Then\r\n        outDom.LoadXml(outItem.dom.OuterXml)\r\n      End If\r\n    End Sub\r\n  End Class\r\nEnd Namespace\r\n",
				TemplateLanguage = "VB"
			};

			templateInfos.Add(newTemplateInfo);
			newTemplateInfo = new TemplateInfo()
			{
				TemplateName = "VBMain",
				IsSuccessfullySupported = true,
				IsSupported = false,
				TemplateCode = "\r\nImports Microsoft.VisualBasic\r\nImports System\r\nImports System.IO\r\nImports System.Text\r\nImports System.Xml\r\nImports System.Collections\r\nImports System.Collections.Generic\r\nImports System.Data\r\nImports System.Linq\r\nImports System.Net\r\nImports System.Web\r\nImports System.Web.SessionState\r\nImports System.Globalization\r\nImports Aras.IOM\r\n\r\nNamespace $(pkgname)\r\n\r\n  Class ItemMethod\r\n    Inherits Item\r\n\r\n\tDim CCO As Aras.Server.Core.CallContext\r\n\tDim RequestState As Aras.Server.Core.IContextState\r\n\r\n    $(MethodCode)\r\n\r\n    Public Sub New(InnovatorObject_arg As IServerConnection)\r\n      MyBase.New(InnovatorObject_arg)\r\n    End Sub\r\n\r\n#If EventDataIsAvailable Then\r\n\t\tPrivate eventData As $(EventDataClass)\r\n\r\n\t\tPublic Function methodCode( _\r\n\t\t\t) As Item\r\n\t\t\tReturn methodCode( Nothing )\r\n\t\tEnd Function\r\n\r\n\t\tPublic Function methodCode( _\r\n\t\t\tByVal eventData As $(EventDataClass) _\r\n\t\t\t) As Item\r\n\t\t\tMe.eventData = eventData\r\n#Else\r\n\t\tPublic Function methodCode( _\r\n\t\t\t) As Item\r\n#End If\r\n\t\tCCO = CType(serverConnection,Aras.Server.Core.IOMConnection).CCO\r\n\t\tRequestState = CCO.RequestState\r\n\t\tReturn Main()\r\n    End Function\r\n  End Class\r\n\r\n  Class $(clsname)\r\n    Implements $(interfacename)\r\n#If EventDataIsAvailable Then\r\n    Public Sub $(fncname)( _\r\n\t\t\tByVal InnovatorObject_arg As IServerConnection, _\r\n\t\t\tByVal inDom As XmlDocument, _\r\n\t\t\tByVal eventData As $(EventDataClass), _\r\n\t\t\tByVal outDom As XmlDocument _\r\n\t\t\t) Implements $(interfacename).$(fncname)\r\n#Else\r\n    Public Sub $(fncname)( _\r\n\t\t\tByVal InnovatorObject_arg As IServerConnection, _\r\n\t\t\tByVal inDom As XmlDocument, _\r\n\t\t\tByVal outDom As XmlDocument _\r\n\t\t\t) Implements $(interfacename).$(fncname)\r\n#End If\r\n      Dim inItem As ItemMethod\r\n      Dim outItem As Item\r\n\r\n      inItem = new ItemMethod(InnovatorObject_arg)\r\n      inItem.dom = inDom\r\n      Dim nodes As XmlNodeList = inDom.SelectNodes(\"//Item[not(ancestor::node()[local-name()='Item'])]\")\r\n      If (nodes.Count = 1) Then\r\n        inItem.node = CType(nodes(0), XmlElement)\r\n      Else\r\n        inItem.node = Nothing\r\n        inItem.nodeList = nodes\r\n      End If\r\n\r\n#If EventDataIsAvailable Then\r\n      outItem = inItem.methodCode( _\r\n\t\t\t\teventData _\r\n\t\t\t\t)\r\n#Else\r\n      outItem = inItem.methodCode( _\r\n\t\t\t\t)\r\n#End If\r\n      If (Not outItem Is Nothing) Then\r\n        outDom.LoadXml(outItem.dom.OuterXml)\r\n      End If\r\n    End Sub\r\n  End Class\r\nEnd Namespace\r\n",
				TemplateLanguage = "VB"
			};

			templateInfos.Add(newTemplateInfo);
			newTemplateInfo = new TemplateInfo()
			{
				TemplateName = "CSharp",
				IsSuccessfullySupported = true,
				IsSupported = true,
				TemplateCode = "using Aras.IOM;\r\nusing System;\r\nusing System.Collections;\r\nusing System.Collections.Generic;\r\nusing System.Data;\r\nusing System.Globalization;\r\nusing System.IO;\r\nusing System.Linq;\r\nusing System.Net;\r\nusing System.Text;\r\nusing System.Web;\r\nusing System.Web.SessionState;\r\nusing System.Xml;\r\n\r\nnamespace $(pkgname)\r\n{\r\n    public class ItemMethod : Item\r\n    {\r\n      public ItemMethod(IServerConnection arg) : base(arg)\r\n        {\r\n        }\r\n#if EventDataIsAvailable\r\n\t\tpublic Item methodCode()\r\n\t\t{\r\n\t\t\treturn methodCode( null );\r\n\t\t}\r\n\r\n\t\tpublic Item methodCode($(EventDataClass) eventData)\r\n#else\r\n\t\tpublic Item methodCode()\r\n#endif\r\n        {\r\n\t\tAras.Server.Core.CallContext CCO = ((Aras.Server.Core.IOMConnection) serverConnection).CCO;\r\n\t\tAras.Server.Core.IContextState RequestState = CCO.RequestState;\r\n\t\t   $(MethodCode)\r\n        }\r\n    }\r\n  \r\n    public class $(clsname): $(interfacename)\r\n    {\r\n        [System.Diagnostics.CodeAnalysis.SuppressMessage(\"Microsoft.Naming\", \"CA1725:ParameterNamesShouldMatchBaseDeclaration\", MessageId = \"0#\")]\r\n#if EventDataIsAvailable\r\n        public void $(fncname)(IServerConnection InnovatorServerASP, XmlDocument inDom, $(EventDataClass) eventData, XmlDocument outDom)\r\n#else\r\n        public void $(fncname)(IServerConnection InnovatorServerASP, XmlDocument inDom, XmlDocument outDom)\r\n#endif\r\n        {\r\n        ItemMethod inItem = null;\r\n        Item outItem = null;\r\n        inItem = new ItemMethod(InnovatorServerASP);\r\n        if (inDom != null)\r\n        {\r\n            inItem.dom = inDom;\r\n            XmlNodeList nodes = inDom.SelectNodes(\"//Item[not(ancestor::node()[local-name()='Item'])]\");\r\n            if (nodes.Count == 1)\r\n            {\r\n                inItem.node = (XmlElement)nodes[0];\r\n            }\r\n            else\r\n            {\r\n                inItem.node = null;\r\n                inItem.nodeList = nodes;\r\n            }\r\n        }\r\n\r\n#if EventDataIsAvailable\r\n      outItem = inItem.methodCode(eventData);\r\n#else\r\n      outItem = inItem.methodCode();\r\n#endif\r\n      if (outItem != null && outDom != null)\r\n      {\r\n          outDom.ReplaceChild(outDom.ImportNode(outItem.dom.DocumentElement, true), outDom.FirstChild);\r\n      }\r\n    }\r\n  }\r\n}\r\n",
				TemplateLanguage = "C#"
			};

			templateInfos.Add(newTemplateInfo);
			return templateInfos;
		}

		private bool IsSameTemplates(TemplateInfo firstTemplate, TemplateInfo secondTemplate)
		{
			return firstTemplate.IsSuccessfullySupported == secondTemplate.IsSuccessfullySupported
				&& firstTemplate.IsSupported == secondTemplate.IsSupported
				&& firstTemplate.TemplateLabel == secondTemplate.TemplateLabel
				&& firstTemplate.TemplateLanguage == secondTemplate.TemplateLanguage
				&& firstTemplate.TemplateName == secondTemplate.TemplateName
				&& firstTemplate.Message == secondTemplate.Message
				&& firstTemplate.TemplateCode == secondTemplate.TemplateCode;
		}
	}
}
