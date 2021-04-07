using System.IO;
using NLog;

namespace Alex.API.Utils
{
	public static class LoggerSetup
	{
		public static void ConfigureNLog(string baseDir, string defaultConfiguration)
		{
			if (!Directory.Exists(baseDir))
			{
				Directory.CreateDirectory(baseDir);
			}
			
			string loggerConfigFile = Path.Combine(baseDir, "NLog.config");
			if (!File.Exists(loggerConfigFile))
			{
				File.WriteAllText(loggerConfigFile, defaultConfiguration);
			}

			string logsDir = Path.Combine(baseDir, "logs");
			if (!Directory.Exists(logsDir))
			{
				Directory.CreateDirectory(logsDir);
			}

			LogManager.ThrowConfigExceptions = false;
			LogManager.LoadConfiguration(loggerConfigFile);
			//			LogManager.Configuration = new XmlLoggingConfiguration(loggerConfigFile);
			LogManager.Configuration.Variables["basedir"] = baseDir;

			NLogAppender.Initialize();
		}
	}
}