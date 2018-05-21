using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
	public class PartialPathAttribute : Attribute
	{
		private string path;
		public PartialPathAttribute(string path)
		{
			this.path = path;
		}
	}
}
