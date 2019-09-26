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
        private static string eventClass;
        private static string directoryPath;
        private static string userName;
        private static string templateName;
        private static object serverConnection;

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
				userName = xmlDocument.SelectSingleNode("/LauncherConfig/userName").InnerText;
                eventClass = xmlDocument.SelectSingleNode("/LauncherConfig/eventClass").InnerText;
                templateName = xmlDocument.SelectSingleNode("/LauncherConfig/templateName").InnerText;
                string password = args[1];
                
                var pathToPdb = pathToDll.Replace(".dll", ".pdb");
				var as1 = Assembly.LoadFrom(pathToDll);
				Type type = as1.GetType(className);
				directoryPath = Path.GetDirectoryName(pathToDll);
				string IOMPath = Path.Combine(directoryPath, "IOM.dll");
                var asmIom = Assembly.LoadFrom(IOMPath);
                Type IomFactoryType = asmIom.GetType("Aras.IOM.IomFactory");
                
                MethodInfo CreateHttpServerConnectionMethodInfo = IomFactoryType.GetMethod(
					"CreateHttpServerConnection",
					BindingFlags.Static | BindingFlags.Public,
					null,
					new Type[] { typeof(string), typeof(string), typeof(string), typeof(string) },
					null);

				serverConnection = CreateHttpServerConnectionMethodInfo.Invoke(null, new object[] { url, database, userName, password });

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
                    var ctorParamsInfo = constructor.GetParameters();
                    var ctorParams = GetInstanseByParameterType(ctorParamsInfo);
                    var classInstance = Activator.CreateInstance(type, ctorParams);
					MethodInfo loadAMLInfo = type.GetMethod("loadAML");
                    if (loadAMLInfo != null)
                        loadAMLInfo.Invoke(classInstance, new object[] { methodContext });

                    var methodParamsInfo = methodInfo.GetParameters();

                    var methodParams = GetInstanseByParameterType(methodParamsInfo);
                    methodInfo.Invoke(classInstance, methodParams);
                }
			}
			catch (Exception ex)
			{
                Console.WriteLine(ex);
			}
		}
        private static object[] GetInstanseByParameterType(ParameterInfo[] parameters)
        {
            if (parameters.Count() == 0)
                return null;

            string innovatorCorePath = Path.Combine(directoryPath, "InnovatorCore.dll");
            if (!File.Exists(innovatorCorePath))
            {
                innovatorCorePath = Path.Combine(directoryPath, "Aras.Server.Core.dll");
            }

            var innCoreAssembly = Assembly.LoadFrom(innovatorCorePath);

            object[] paramsInstances = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(eventClass) && parameters[i].ParameterType.Name.Contains("EventArgs"))
                {
                    Type eventType = innCoreAssembly.GetType(eventClass);
                    var eventCtor = eventType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
                    var ctorArgs = GetInstanseByParameterType(eventCtor.GetParameters());
                    var eventObject = eventCtor.Invoke(ctorArgs);
                    paramsInstances[i] = eventObject;
                }
                else if (parameters[i].ParameterType.Name == "IServerConnection")
                {
                    paramsInstances[i] = serverConnection;
                }
                else if (parameters[i].ParameterType.Name == "String" && parameters[i].Name == "loginName")
                {
                    paramsInstances[i] = userName;
                }
                else if (parameters[i].ParameterType.Name == "IContextState" && parameters[i].Name == "RequestState")
                {
                    var requestStateType = innCoreAssembly.GetType("Aras.Server.Core.RequestState");
                    if (innCoreAssembly.GetType("Aras.Server.Core.IContextState").IsAssignableFrom(requestStateType))
                    {
                        var requestStateCtor = requestStateType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
                        var requestStateObject = requestStateCtor.Invoke(null);
                        paramsInstances[i] = requestStateObject;
                    }
                    else
                    {
                        paramsInstances[i] = null;
                    }
                }
                else
                    paramsInstances[i] = null;
            }
            return paramsInstances;
        }
    }
}
