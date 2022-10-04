using System;
using System.Text;
using Aras.Method.Libs;

namespace Aras.VS.MethodPlugin.SolutionManagement
{
	internal class ProjectUpdater
	{
		private readonly IIOWrapper iOWrapper;

		public ProjectUpdater(IIOWrapper iOWrapper)
		{
			this.iOWrapper = iOWrapper ?? throw new ArgumentNullException(nameof(iOWrapper));
		}

		public void UpdateProjectAttributes(string projectFolderPath)
		{
			if (string.IsNullOrEmpty(projectFolderPath))
			{
				return;
			}

			string partialPathAttributePath = iOWrapper.PathCombine(projectFolderPath, "Attributes", "PartialPathAttribute.cs");
			string partialPathAttributeContent = iOWrapper.ReadAllText(partialPathAttributePath, Encoding.UTF8);
			if (partialPathAttributeContent.IndexOf("public int Index") == -1)
			{
				iOWrapper.WriteAllTextIntoFile(partialPathAttributePath, @"using System;

namespace Common.Attributes
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class PartialPathAttribute : Attribute
	{
		private readonly string path;
		private readonly int index;

		public string Path
		{
			get { return path; }
		}

		public int Index
		{
			get { return index; }
		}

		public PartialPathAttribute(string path, int index = 0)
		{
			this.path = path;
			this.index = index;
		}
	}
}
", Encoding.UTF8);

				string externalPathAttributePath = iOWrapper.PathCombine(projectFolderPath, "Attributes", "ExternalPathAttribute.cs");
				iOWrapper.WriteAllTextIntoFile(externalPathAttributePath, @"using System;

namespace Common.Attributes
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
	public sealed class ExternalPathAttribute : Attribute
	{
		private readonly string path;
		private readonly int index;

		public string Path
		{
			get { return path; }
		}

		public int Index
		{
			get { return index; }
		}

		public ExternalPathAttribute(string path, int index = 0)
		{
			this.path = path;
			this.index = index;
		}
	}
}
", Encoding.UTF8);
			}
		}
	}
}
