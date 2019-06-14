namespace Aras.Method.Libs.Aras.Package
{
	public class PackageInfo
	{
		public PackageInfo(string name)
		{
			Name = name;
			Path = $"{name.Replace(".", "\\")}\\Import";
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
	}
}
