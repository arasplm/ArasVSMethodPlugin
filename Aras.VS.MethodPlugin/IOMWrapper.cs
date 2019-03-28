//------------------------------------------------------------------------------
// <copyright file="IOMWrapper.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;

namespace Aras.VS.MethodPlugin
{
	internal class IOMWrapper : IIOMWrapper
	{
		private readonly IMessageManager messageManager;

		private const string IOMnamespace = "Aras.IOM";
		private const string localIOMPath = "ArasLibs\\IOM.dll";

		private string projectFullName;
		private Assembly IOMAssembly;

		public IOMWrapper(IMessageManager messageManager, string projectFullName)
		{
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));

			this.projectFullName = projectFullName;

			string projectPath = Path.GetDirectoryName(projectFullName);
			string IOMPath = Path.Combine(projectPath, localIOMPath);

			if (!File.Exists(IOMPath))
			{
				throw new FileNotFoundException(this.messageManager.GetMessage("IOMDllInTheCurrentProjectIsNotFound"));
			}

			this.IOMAssembly = Assembly.LoadFrom(IOMPath);
		}

		#region Properties

		public string ProjectFullName
		{
			get { return projectFullName; }
		}

		#endregion

		public dynamic Innovator_ScalcMD5(string password)
		{
			Type innovator = IOMAssembly.GetType(string.Format("{0}.{1}", IOMnamespace, "Innovator"));
			object result = innovator.GetMethod("ScalcMD5").Invoke(null, new object[] { password });
			return result.ToString();
		}

		public dynamic IomFactory_CreateHttpServerConnection(string serverUrl, string databaseName, string login, string passwordHash)
		{
			Type iomFactory = IOMAssembly.GetType(string.Format("{0}.{1}", IOMnamespace, "IomFactory"));
			object result = iomFactory.GetMethod("CreateHttpServerConnection", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) }).Invoke(null, new object[] { serverUrl, databaseName, login, passwordHash });
			return result;
		}

		public dynamic Innovator_Ctor(dynamic serverConnection)
		{
			Type innovator = IOMAssembly.GetType(string.Format("{0}.{1}", IOMnamespace, "Innovator"));
			ConstructorInfo constructor = innovator.GetConstructor(new Type[] { typeof(string) });
			dynamic innovatorInstance = Activator.CreateInstance(innovator, new object[] { serverConnection });
			return innovatorInstance;
		}

		public dynamic IomFactory_CreateHttpServerConnection(string innovatorURL)
		{
			Type iomFactory = IOMAssembly.GetType(string.Format("{0}.{1}", IOMnamespace, "IomFactory"));
			object result = iomFactory.GetMethod("CreateHttpServerConnection", new Type[] { typeof(string)}).Invoke(null, new object[] { innovatorURL });
			return result;
		}

		public dynamic IomFactory_CreateWinAuthHttpServerConnection(string innovatorURL, string databaseName)
		{
			Type iomFactory = IOMAssembly.GetType(string.Format("{0}.{1}", IOMnamespace, "IomFactory"));
			object result = iomFactory.GetMethod("CreateWinAuthHttpServerConnection", new Type[] { typeof(string), typeof(string) }).Invoke(null, new object[] { innovatorURL, databaseName });
			return result;
		}
	}
}
