using System;
using System.Text.RegularExpressions;

namespace Aras.Method.Libs.Aras.Package
{
	public class PackageInfo
	{
		public PackageInfo(string name)
		{
			Name = name;

			if (IsStandardPackage(name))
			{
				Path = $"{name.Replace(".", @"\")}\\Import";
			}
			else
			{
				Regex regLabel = new Regex(@"([^\.]+)$", RegexOptions.IgnoreCase);
				Match matchLab = regLabel.Match(name);
				string solNameSuf = matchLab.Groups[1].ToString();
				Path = $"{solNameSuf}\\Import";
			}

			MethodFolderPath = $"{Path}\\Method\\";
		}

		public PackageInfo(string name, string path)
		{
			Name = name;
			Path = path;

			if (Path == @".\")
			{
				path = name.Replace(".", "\\");
			}

			MethodFolderPath = $"{path}\\Method\\";
		}

		public string Name { get; private set; }

		public string Path { get; private set; }

		public string MethodFolderPath { get; private set; }

		public override string ToString()
		{
			return Name;
		}

		#region Private methods

		private bool IsStandardPackage(string packageName)
		{
			return IsDefaultPackage(packageName) || IsCorePackage(packageName);
		}

		private bool IsDefaultPackage(string packageName)
		{
			return packageName.StartsWith("com.aras.defaults.", StringComparison.OrdinalIgnoreCase);
		}

		private bool IsCorePackage(string packageName)
		{
			return packageName.StartsWith("com.aras.innovator.") && !packageName.StartsWith("com.aras.innovator.solution.");
		}

		#endregion
	}
}
