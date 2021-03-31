//------------------------------------------------------------------------------
// <copyright file="IProjectConfigurationManager.cs" company="Aras Corporation">
//     © 2017-2021 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------


namespace Aras.Method.Libs.Configurations.ProjectConfigurations
{
	public interface IProjectConfigurationManager
	{
		IProjectConfiguraiton CurrentProjectConfiguraiton { get; }

		void Load(string configFilePath);
		void Save(string configFilePath);
	}
}
