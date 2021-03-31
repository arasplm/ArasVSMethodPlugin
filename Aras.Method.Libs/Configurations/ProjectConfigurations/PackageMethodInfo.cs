//------------------------------------------------------------------------------
// <copyright file="PackageMethodInfo.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace Aras.Method.Libs.Configurations.ProjectConfigurations
{
	public class PackageMethodInfo : MethodInfo
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
