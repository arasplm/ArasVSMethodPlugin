using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace MethodLauncher
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				string configFilePath = args[0];
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.Load(configFilePath);
				File.Delete(configFilePath);

				string pathToDll = xmlDocument.SelectSingleNode("/LauncherConfig/dllFullPath").InnerText;
				string className = xmlDocument.SelectSingleNode("/LauncherConfig/className").InnerText;
				string selectedMethodName = xmlDocument.SelectSingleNode("/LauncherConfig/methodName").InnerText;
				string methodContext = xmlDocument.SelectSingleNode("/LauncherConfig/сontext").InnerText;
				string url = xmlDocument.SelectSingleNode("/LauncherConfig/url").InnerText;
				string database = xmlDocument.SelectSingleNode("/LauncherConfig/database").InnerText;
				string userName = xmlDocument.SelectSingleNode("/LauncherConfig/userName").InnerText;
				string password = args[1];

				var pathToPdb = pathToDll.Replace(".dll", ".pdb");
				var as1 = Assembly.LoadFrom(pathToDll);
				Type type = as1.GetType(className);
				string IOMDirectoryPath = Path.GetDirectoryName(pathToDll);
				string IOMPath = Path.Combine(IOMDirectoryPath, "IOM.dll");
				var asmIom = Assembly.LoadFrom(IOMPath);
				Type IomFactoryType = asmIom.GetType("Aras.IOM.IomFactory");

				MethodInfo CreateHttpServerConnectionMethodInfo = IomFactoryType.GetMethod(
					"CreateHttpServerConnection",
					BindingFlags.Static | BindingFlags.Public,
					null,
					new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) },
					null);

				var serverConnection = CreateHttpServerConnectionMethodInfo.Invoke(null, new object[] { url, database, userName, password });

				Type httpServerConnectionType = asmIom.GetType("Aras.IOM.HttpServerConnection");
				MethodInfo loginMethodInfo = httpServerConnectionType.GetMethod("Login");
				loginMethodInfo.Invoke(serverConnection, null);
				while (!System.Diagnostics.Debugger.IsAttached)
				{
					Thread.Sleep(1000);
					Console.WriteLine("waiting debugger");
				}

				if (type != null)
				{
					MethodInfo methodInfo = type.GetMethod(selectedMethodName);
					var constructor = type.GetConstructors().First();
					var arg = constructor.GetParameters().First().ParameterType;
					//var serverConnection = Substitute.For(new Type[] { arg }, new object[] { });

					var classInstance = Activator.CreateInstance(type, serverConnection);
					MethodInfo loadAMLInfo = type.GetMethod("loadAML");
					loadAMLInfo.Invoke(classInstance, new object[] { methodContext });

					methodInfo.Invoke(classInstance, new object[] { null, null });
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}
