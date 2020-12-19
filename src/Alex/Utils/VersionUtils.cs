using System.Net.Mime;
using System.Reflection;

namespace Alex.Utils
{
	internal static class VersionUtils
	{
		public static string GetVersion()
		{
			try
			{
				 var assembly = Assembly.GetExecutingAssembly();
				 var informationVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

				 return informationVersion;
			}
			catch
			{
				return "Unknown";
			}
		}
	}
}