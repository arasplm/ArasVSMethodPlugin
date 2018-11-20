using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.VS.MethodPlugin.Tests.Stubs
{
    public class MethodItemStub
    {
        public bool isError()
        {
            return false;
        }
        public void setProperty(string propertyName, string value)
        {
        }
        public int getLockStatus()
        {
            return 0;
        }

        public dynamic apply(dynamic query)
        {
            return this;
        }

        public string getID()
        {
            return string.Empty;
        }

        public string getProperty(string property)
        {
            return string.Empty;
        }

        public int getItemCount()
        {
            return 0;
        }
        public dynamic createRelationship(string element, string action)
        {
            return this;
        }

        public dynamic apply()
        {
            return this;
        }

        public dynamic getItemsByXPath(string xPath)
        {
            return this;
        }
    }
}
