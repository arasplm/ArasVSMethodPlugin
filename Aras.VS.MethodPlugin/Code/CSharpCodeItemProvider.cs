//------------------------------------------------------------------------------
// <copyright file="CSharpCodeItemProvider.cs" company="Aras Corporation">
//     © 2017-2018 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Aras.VS.MethodPlugin.Code
{
	public class CSharpCodeItemProvider : ICodeItemProvider
	{
		private readonly IMessageManager messageManager;

		public CSharpCodeItemProvider(IMessageManager messageManager)
		{
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
		}

		private Dictionary<CodeType, Dictionary<CodeElementType, string>> codeTemplates = new Dictionary<CodeType, Dictionary<CodeElementType, string>>
		{
			{
				CodeType.Partial, new Dictionary<CodeElementType, string>
				{
					{ CodeElementType.Interface , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal interface {4}\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}" },
					{ CodeElementType.Class , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal class {4}\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}" },
					{ CodeElementType.Struct , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal struct {4}\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}" },
					{ CodeElementType.Method , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal void {4}()\r\n        {{\r\n\r\n        }}\r\n    }}\r\n}}" },
					{ CodeElementType.Enum , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    public partial class {1}\r\n    {{\r\n        [PartialPath(\"{2}\")]\r\n        internal enum {4}\r\n        {{\r\n            None\r\n        }}\r\n    }}\r\n}}" },
					{ CodeElementType.Custom , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    internal partial class {1}\r\n    {{\r\n        //[PartialPath(\"{2}\")]\r\n    }}\r\n}}" }
				}
			},
			{
				CodeType.External, new Dictionary<CodeElementType, string>
				{
					{ CodeElementType.Interface , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    [ExternalPath(\"{2}\")]\r\n    internal interface {4}\r\n    {{\r\n\r\n    }}\r\n}}" },
					{ CodeElementType.Class , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    [ExternalPath(\"{2}\")]\r\n    internal class {4}\r\n    {{\r\n\r\n    }}\r\n}}" },
					{ CodeElementType.Struct , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    [ExternalPath(\"{2}\")]\r\n    internal struct {4}\r\n    {{\r\n\r\n    }}\r\n}}" },
					{ CodeElementType.Enum , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    [ExternalPath(\"{2}\")]\r\n    internal enum {4}\r\n    {{\r\n        None\r\n    }}\r\n}}" },
					{ CodeElementType.Custom , "{0}using Common;\r\nusing Common.Attributes;\r\n\r\nnamespace {3}\r\n{{\r\n    //[ExternalPath(\"{2}\")]\r\n}}" }
				}
			},
		};

		public List<CodeElementType> GetSupportedCodeElementTypes(CodeType type)
		{
			List<CodeElementType> elementTypes = new List<CodeElementType>();
			if (type == CodeType.Partial)
			{
				elementTypes.Add(CodeElementType.Interface);
				elementTypes.Add(CodeElementType.Class);
				elementTypes.Add(CodeElementType.Struct);
				elementTypes.Add(CodeElementType.Method);
				elementTypes.Add(CodeElementType.Enum);
				elementTypes.Add(CodeElementType.Custom);
			}
			else if (type == CodeType.External)
			{
				elementTypes.Add(CodeElementType.Interface);
				elementTypes.Add(CodeElementType.Class);
				elementTypes.Add(CodeElementType.Struct);
				elementTypes.Add(CodeElementType.Enum);
				elementTypes.Add(CodeElementType.Custom);
			}

			return elementTypes;
		}

		public string GetCodeElementTypeTemplate(CodeType codeType, CodeElementType codeElementType)
		{
			Dictionary<CodeElementType, string> templates;
			if (!codeTemplates.TryGetValue(codeType, out templates))
			{
				throw new NotSupportedException(messageManager.GetMessage("CurrentCodeTypeIsNotSupported"));
			}

			string template;
			if (!templates.TryGetValue(codeElementType, out template))
			{
				throw new NotSupportedException(messageManager.GetMessage("CurrentCodeElementTypeIsNotSupported"));
			}

			return template;
		}
	}
}
