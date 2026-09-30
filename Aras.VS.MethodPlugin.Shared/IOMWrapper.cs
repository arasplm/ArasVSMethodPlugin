//------------------------------------------------------------------------------
// <copyright file="IOMWrapper.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Reflection;
using Aras.Method.Libs;

namespace Aras.VS.MethodPlugin
{
	internal class IOMWrapper : IIOMWrapper
	{
		private readonly MessageManager messageManager;

		private Assembly IOMAssembly;

		public IOMWrapper(MessageManager messageManager, string projectFullName, string IOMDllFilePath)
		{
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));

			ProjectFullName = projectFullName;

			if (!File.Exists(IOMDllFilePath))
			{
				throw new FileNotFoundException(this.messageManager.GetMessage("IOMDllInTheCurrentProjectIsNotFound"));
			}

			this.IOMAssembly = Assembly.LoadFrom(IOMDllFilePath);
		}

		#region Properties

		public string ProjectFullName { get; }

		#endregion

		public dynamic IomFactory_CreateHttpServerConnection(string serverUrl, string databaseName, string login, string password)
		{
			Type iomFactory = IOMAssembly.GetType(string.Format("{0}.{1}", GlobalConsts.IOMnamespace, "IomFactory"));
			return iomFactory.GetMethod("CreateHttpServerConnection", new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) }).Invoke(null, new object[] { serverUrl, databaseName, login, password });
		}

		public dynamic Innovator_Ctor(dynamic serverConnection)
		{
			Type innovator = IOMAssembly.GetType(string.Format("{0}.{1}", GlobalConsts.IOMnamespace, "Innovator"));
			ConstructorInfo constructor = innovator.GetConstructor(new Type[] { typeof(string) });
			return Activator.CreateInstance(innovator, new object[] { serverConnection });
		}

		public dynamic IomFactory_CreateHttpServerConnection(string innovatorURL)
		{
			Type iomFactory = IOMAssembly.GetType(string.Format("{0}.{1}", GlobalConsts.IOMnamespace, "IomFactory"));
			return iomFactory.GetMethod("CreateHttpServerConnection", new Type[] { typeof(string)}).Invoke(null, new object[] { innovatorURL });
		}

		public dynamic IomFactory_CreateWinAuthHttpServerConnection(string innovatorURL, string databaseName)
		{
			Type iomFactory = IOMAssembly.GetType(string.Format("{0}.{1}", GlobalConsts.IOMnamespace, "IomFactory"));
			return iomFactory.GetMethod("CreateWinAuthHttpServerConnection", new Type[] { typeof(string), typeof(string) }).Invoke(null, new object[] { innovatorURL, databaseName });
		}
	}
}
