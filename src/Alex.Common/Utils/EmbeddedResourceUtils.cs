using System;
using System.IO;
using System.Reflection;
using NLog;

namespace Alex.Common.Utils
{
	public class EmbeddedResourceUtils
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(EmbeddedResourceUtils));
		
		public static byte[] GetApiRequestFile(string namespaceAndFileName)
		{
			try
			{
				using (MemoryStream ms = new MemoryStream())
				{
					using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(namespaceAndFileName))
					{
						int read = 0;

						do
						{
							byte[] buffer = new byte[256];
							read = stream.Read(buffer, 0, buffer.Length);

							ms.Write(buffer, 0, read);
						} while (read > 0);
					}

					return ms.ToArray();
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception, $"Failed to read Embedded Resource {namespaceAndFileName}");

				return null;
			}
		}
	}
}