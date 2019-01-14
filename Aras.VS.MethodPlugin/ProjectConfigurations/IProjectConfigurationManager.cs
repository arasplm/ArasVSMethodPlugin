using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.VS.MethodPlugin.ProjectConfigurations
{
    public interface IProjectConfigurationManager
    {
        IProjectConfiguraiton Load(string configFilePath);

        void Save(string configFilePath, IProjectConfiguraiton configuration);
    }
}
