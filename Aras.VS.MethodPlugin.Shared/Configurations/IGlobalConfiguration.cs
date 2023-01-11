//------------------------------------------------------------------------------
// <copyright file="IGlobalConfiguration.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.Configurations
{
	public interface IGlobalConfiguration
	{
		void Load();
		void Save();
		List<string> GetUserCodeTemplatesPaths();
		void AddUserCodeTemplatePath(string path);

		void RemoveUserCodeTemplatePath(string path);
	}
}
