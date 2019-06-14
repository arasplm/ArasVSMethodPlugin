namespace Aras.Method.Libs
{
	public class GlobalConsts
	{
		#region Languages

		public const string CSharp = "C#";

		#endregion

		#region Attributes

		public const string PartialPath = "PartialPath";
		public const string PartialPathAttribute = "PartialPathAttribute";
		public const string ExternalPath = "ExternalPath";
		public const string ExternalPathAttribute = "ExternalPathAttribute";

		#endregion

		#region Regions

		public const string RegionMethodCode = "#region MethodCode";
		public const string EndregionMethodCode = "#endregion MethodCode";

		#endregion

		#region Dlls

		public const string IOMnamespace = "Aras.IOM";

		#endregion

		#region FileNames

		public const string methodConfigFileName = "method-config.xml";
		public const string projectConfigFileName = "projectConfig.xml";
		public const string globalSuppressionsFileName = "GlobalSuppressions.cs";

		#endregion

		#region Extensions

		public static string CSExtension { get { return ".cs"; } }

		#endregion
	}
}
