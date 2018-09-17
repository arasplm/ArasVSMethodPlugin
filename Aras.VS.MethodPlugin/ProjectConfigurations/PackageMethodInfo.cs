using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.VS.MethodPlugin.ProjectConfigurations
{
	class PackageMethodInfo : MethodInfo
	{
		public PackageMethodInfo()
		{

		}

		public PackageMethodInfo(PackageMethodInfo packageMethodInfo) : base(packageMethodInfo)
		{
			this.ManifestFileName = packageMethodInfo.ManifestFileName;
		}

		public string ManifestFileName{ get; set; }
	}
}
