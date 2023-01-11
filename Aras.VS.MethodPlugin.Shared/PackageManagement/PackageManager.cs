//------------------------------------------------------------------------------
// <copyright file="PackageManager.cs" company="Aras Corporation">
//     © 2017-2023 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Aras.Method.Libs;
using Aras.Method.Libs.Aras.Package;
using Aras.VS.MethodPlugin.Authentication;

namespace Aras.VS.MethodPlugin.PackageManagement
{
	public class PackageManager
	{
		private readonly IAuthenticationManager authenticationManager;
		private readonly MessageManager messageManager;

		private dynamic innovatorInst { get { return authenticationManager.InnovatorInstance; } }

		private const string allPackageDefinition = "<AML><Item action=\"get\" type=\"PackageDefinition\" select=\"name\"></Item></AML>";
		private const string getPachageByNameTemplate = "<AML><Item action=\"get\" type=\"PackageDefinition\" select=\"name\" levels=\"2\"><name>{0}</name></Item></AML>";
		private const string createPackageDefinitionTemplate = "<AML><Item action=\"add\" type=\"PackageDefinition\"><name>{0}</name></Item></AML>";
		private const string packageDefinitionByElementIdTemplate = "<AML><Item action=\"get\" type=\"PackageDefinition\"><Relationships><Item action = \"get\" type=\"PackageGroup\"><Relationships><Item action = \"get\" type=\"PackageElement\" select=\"element_id,element_type,name\"><element_type>Method</element_type><element_id>{0}</element_id></Item></Relationships></Item></Relationships></Item></AML>";
		private const string deleteElementByIdTemplate = "<AML><Item action =\"delete\" type=\"PackageElement\" id=\"{0}\"></Item></AML>";
		private const string addElementToPackageTemplate = "<AML><Item action=\"get\" type=\"PackageDefinition\"><Relationships><Item action = \"get\" type=\"PackageGroup\"><Relationships><Item action = \"add\" isNew=\"1\" type=\"PackageElement\" ><element_type>Method</element_type><element_id>{0}</element_id><name>{1}</name></Item></Relationships></Item></Relationships></Item></AML>";

		public PackageManager(IAuthenticationManager authenticationManager, MessageManager messageManager)
		{
			this.authenticationManager = authenticationManager ?? throw new ArgumentNullException(nameof(authenticationManager));
			this.messageManager = messageManager ?? throw new ArgumentNullException(nameof(messageManager));
		}

		public List<PackageInfo> GetPackageDefinitionList()
		{
			List<PackageInfo> resultList = new List<PackageInfo>();

			string amlQuery = allPackageDefinition;
			var reusltItem = innovatorInst.applyAML(amlQuery);
			for (int i = 0; i < reusltItem.getItemCount(); i++)
			{
				string name = reusltItem.getItemByIndex(i).getProperty("name");

				PackageInfo packageInfo = new PackageInfo(name);
				resultList.Add(packageInfo);
			}

			return resultList;
		}

		public PackageInfo GetPackageDefinitionByElementId(string configId)
		{
			string amlQuery = string.Format(packageDefinitionByElementIdTemplate, configId);
			var reusltItem = innovatorInst.applyAML(amlQuery);
			if (reusltItem.getItemCount() > 1)
			{
				throw new Exception(messageManager.GetMessage("MoreThenOneItemFound"));
			}
			if (reusltItem.getItemCount() == 0)
			{
				return new PackageInfo(string.Empty);
			}

			return new PackageInfo(reusltItem.getItemByIndex(0).getProperty("name"));
		}

		public bool DeletePackageByElementIdFromPackageDefinition(string configId)
		{
			string amlQuery = string.Format(packageDefinitionByElementIdTemplate, configId);
			var reusltItem = innovatorInst.applyAML(amlQuery);
			var packageElementId = reusltItem.getItemsByXPath(@"//Item/Relationships/Item/Relationships/Item").getID();

			var result = innovatorInst.applyAML(string.Format(deleteElementByIdTemplate, packageElementId));

			return result.isError();
		}

		public void AddPackageElementToPackageDefinition(string configId, string name, string packageName)
		{
			//package exist
			var packageQuery = string.Format(getPachageByNameTemplate, packageName);
			var packageDefinition = innovatorInst.applyAML(packageQuery);
			dynamic packageGroup = null;
			dynamic packageElement = null;
			if (packageDefinition.isError() || packageDefinition.getItemCount() == 0)
			{
				var newPackageQuery = string.Format(createPackageDefinitionTemplate, packageName);
				packageDefinition = innovatorInst.applyAML(newPackageQuery);
				packageGroup = packageDefinition.createRelationship("PackageGroup", "add");
				packageGroup.setProperty("name", "Method");
				packageElement = packageGroup.createRelationship("PackageElement", "add");
				packageElement.setProperty("element_id", configId);
				packageElement.setProperty("element_type", "Method");
				packageElement.setProperty("name", name);
				packageDefinition.apply();
				return;
			}

			var packageGroups = packageDefinition.getRelationships("PackageGroup");
			for (int i = 0; i < packageGroups.getItemCount(); i++)
			{
				var pckgGr = packageGroups.getItemByIndex(i);
				if (pckgGr.getProperty("name") == "Method")
				{
					packageGroup = pckgGr;
				}
			}

			if (packageGroup == null)
			{
				packageGroup = packageDefinition.createRelationship("PackageGroup", "add");
				packageGroup.setProperty("name", "Method");
				packageElement = packageGroup.createRelationship("PackageElement", "add");
				packageElement.setProperty("element_id", configId);
				packageElement.setProperty("element_type", "Method");
				packageElement.setProperty("name", name);
				packageDefinition.apply();
				return;
			}

			packageElement = packageGroup.createRelationship("PackageElement", "add");
			packageElement.setProperty("element_id", configId);
			packageElement.setProperty("element_type", "Method");
			packageElement.setProperty("name", name);
			packageDefinition.apply();
		}
	}
}
