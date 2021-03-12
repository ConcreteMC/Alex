using System.Net.Mime;
using System.Reflection;

namespace Alex.Utils
{
	internal static class VersionUtils
	{
		#if RELEASE || DirectX
		
		public const bool IsReleaseBuild = true;
		
		#else
		
		public const bool IsReleaseBuild = false;
		
		#endif
		
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