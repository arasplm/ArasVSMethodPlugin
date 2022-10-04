﻿//------------------------------------------------------------------------------
// <copyright file="CommandIds.cs" company="Aras Corporation">
//     © 2017-2022 Aras Corporation. All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Aras.VS.MethodPlugin.Commands
{
	public struct CommandIds
	{
		public static Guid CreateMethod = new Guid("B69B1AC9-3D7E-4553-9786-A852B873DF01");
		public static Guid CreateCodeItemElement = new Guid("714c822b-ebc4-4413-89b5-c93eaed863fc");
		public static Guid OpenFromAras = new Guid("AEA8535B-C666-4112-9BDD-5ECFA4934B47");
		public static Guid OpenFromPackage = new Guid("AEA8535B-C666-4112-9BDD-5ECFA4934B47");
		public static Guid SaveToAras = new Guid("694F6136-7CF1-46E1-B9E2-24296488AE96");
		public static Guid SaveToPackage = new Guid("694F6136-7CF1-46E1-B9E2-24296488AE96");
		public static Guid UpdateMethod = new Guid("CF767190-3696-4365-9857-3600622B097D");
		public static Guid RefreshConfig = new Guid("DB77AE9E-9CB5-4C13-9EB3-ED388DC94B66");
		public static Guid ConnectionInfo = new Guid("E15DDF0A-1B6E-46A8-8B78-AEC2A7BB4922");
		public static Guid DebugMethod = new Guid("020DC4DF-2FC3-493E-97D3-4012DE93BCAB");
		public static Guid MoveTo = new Guid("e9768243-2e32-45cc-9fc6-76d28ccb244a");
		public static Guid ImportOpenInVSAction = new Guid("9FA04931-AC92-4234-92AA-0EBBD0F9A798");
	}
}
