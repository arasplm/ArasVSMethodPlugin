//------------------------------------------------------------------------------
// <copyright file="CSharpCodeElementTypeProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.Code
{
	public class CSharpCodeElementTypeProvider : ICodeElementTypeProvider
	{
		private Dictionary<CodeElementType, string> templates = new Dictionary<CodeElementType, string>
		{
			{ CodeElementType.Interface , "{0}using Common;\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal interface {4}\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}" },
			{ CodeElementType.Class , "{0}using Common;\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal class {4}\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}" },
			{ CodeElementType.Struct , "{0}using Common;\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal struct {4}\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}" },
			{ CodeElementType.Method , "{0}using Common;\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal void {4}()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}" },
			{ CodeElementType.Enum , "{0}using Common;\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal enum {4}\r\n        {{\r\n            None\r\n        }}\r\n    }}\r\n}}" },
			{ CodeElementType.Custom , "{0}using Common;\r\nnamespace {3}\r\n{{\r\n    internal partial class {1}\r\n    {{\r\n        //[PartialPath(\"{2}\")]\r\n    }}\r\n}}" }
		};

		public string GetCodeElementTypeTemplate(CodeElementType type)
		{
			string template;
			if (!templates.TryGetValue(type, out template))
			{
				throw new NotSupportedException("Current code element type is not supported");
			}

			return template;
		}
	}
}
