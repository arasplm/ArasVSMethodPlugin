using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MethodLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
				var pathToDll = args[0];
				var className = args[1];
				var selectedMethodName = args[2];
				var pathToPdb = pathToDll.Replace(".dll", ".pdb");
				var as1 = Assembly.LoadFrom(pathToDll);
				Type type = as1.GetType(className);
				string IOMDirectoryPath = Path.GetDirectoryName(pathToDll);
				string IOMPath = Path.Combine(IOMDirectoryPath, "IOM.dll");
				var asmIom = Assembly.LoadFrom(IOMPath);
				Type IomFactoryType = asmIom.GetType("Aras.IOM.IomFactory");
				MethodInfo CreateHttpServerConnectionMethodInfo = IomFactoryType.GetMethod(
					"CreateHttpServerConnection",
					System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public,
					null,
					new Type[]{ typeof(string),typeof(string),typeof(string),typeof(string)},
					null);
				var serverConnection = CreateHttpServerConnectionMethodInfo.Invoke(null, new object[] { "http://localhost/InnovatorServersp12", "InnovatorSolutionssp12new", "admin", "607920b64fe136f9ab2389e371852af2" });
				Type httpServerConnectionType = asmIom.GetType("Aras.IOM.HttpServerConnection");
				MethodInfo loginMethodInfo  =httpServerConnectionType.GetMethod("Login");
				loginMethodInfo.Invoke(serverConnection,null);
				while (!System.Diagnostics.Debugger.IsAttached)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("waiting debugger");
                }

                if (type != null)
                {
                    MethodInfo methodInfo = type.GetMethod(selectedMethodName);
					var constructor = type.GetConstructors().First();
					var arg= constructor.GetParameters().First().ParameterType;
					//var serverConnection = Substitute.For(new Type[] { arg }, new object[] { });
					
					var classInstance = Activator.CreateInstance(type, serverConnection);
                    methodInfo.Invoke(classInstance, new object[] { null,null});
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
