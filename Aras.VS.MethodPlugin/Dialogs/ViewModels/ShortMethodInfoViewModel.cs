using System;
using System.IO;
using System.Xml;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class ShortMethodInfoViewModel : BaseViewModel
	{
		private readonly string fullName;
		
		private string methodType;

		public ShortMethodInfoViewModel(string fullName)
		{
			if (string.IsNullOrEmpty(fullName))
			{
				throw new ArgumentNullException(nameof(fullName));
			}

			this.fullName = fullName;
		}

		public string Name
		{
			get
			{
				return Path.GetFileNameWithoutExtension(this.fullName);
			}
			set { }
		}

		public string FullName
		{
			get
			{
				return this.fullName;
			}
			set { }
		}

		public string MethodType
		{
			get
			{
				if (string.IsNullOrEmpty(this.methodType))
				{
					this.methodType = LoadMethodType();
				}

				return this.methodType;
			}
			set
			{ }
		}

		private string LoadMethodType()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(this.fullName);
			XmlNode xmlNode = xmlDocument.SelectSingleNode("//method_type");
			return xmlNode.InnerText;
		}
	}
}
