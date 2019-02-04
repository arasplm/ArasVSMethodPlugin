using System;
using System.IO;
using System.Xml;

namespace Aras.VS.MethodPlugin.Dialogs.ViewModels
{
	public class ShortMethodInfoViewModel : BaseViewModel
	{
		private readonly string fullName;

		private XmlDocument methodFile;
		private string methodType;
		private string methodCode;

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
		}

		public string FullName
		{
			get
			{
				return this.fullName;
			}
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
		}

		public string MethodCode
		{
			get
			{
				if (string.IsNullOrEmpty(this.methodCode))
				{
					this.methodCode = LoadMethodCode();
				}

				return this.methodCode;
			}
		}

		private string LoadMethodType()
		{
			if (this.methodFile == null)
			{
				this.methodFile = new XmlDocument();
				this.methodFile.Load(this.fullName);
			}

			XmlNode methodTypeNode = this.methodFile.SelectSingleNode("//method_type");
			return methodTypeNode.InnerText;
		}

		private string LoadMethodCode()
		{
			if (this.methodFile == null)
			{
				this.methodFile = new XmlDocument();
				this.methodFile.Load(this.fullName);
			}

			XmlNode methodCodeNode = this.methodFile.SelectSingleNode("//method_code");
			return methodCodeNode.InnerText;
		}
	}
}
