//------------------------------------------------------------------------------
// <copyright file="MessageMenager.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Aras.Method.Libs
{
	public abstract class MessageManager
	{
		private static Dictionary<string, string> messages = new Dictionary<string, string>
		{
			{ "CurrentProjectTypeIsNotSupported", "Current project type is not supported" },
			{ "NoPartialClassesFound", "No partial classes found." },
			{ "MethodNameCaNotBeEmpty", "Method name can not be empty." },
			{ "TemplateNotFound", "Template not found." },
			{ "NoAttributeFound", "No attribute found." },
			{ "CurrentCodeTypeIsNotSupported", "Current code type is not supported." },
			{ "CurrentCodeElementTypeIsNotSupported", "Current code element type is not supported." },
			{ "startYourCodeInsideRegionMethodCodeDoNotChangeCodeAbove", "// start your code inside region MethodCode - DO NOT CHANGE CODE ABOVE" },
			{ "endyourCodeInsideRegionMethodCodeDoNotChangeCodeBelow", "// end your code inside region MethodCode - DO NOT CHANGE CODE BELOW" },
			{ "errorWhileLoadingProjectConfigFile", "Error while loading project config file." },
			{ "errorWhileSavingProjectConfigFile", "Error while saving project config file." }
		};

		public virtual string GetMessage(string key)
		{
			string message = string.Empty;
			messages.TryGetValue(key, out message);
			return message;
		}

		public virtual string GetMessage(string key, params string[] args)
		{
			var message = string.Empty;
			if (messages.TryGetValue(key, out message))
			{
				message = string.Format(message, args);
			}

			return message;
		}
	}
}
