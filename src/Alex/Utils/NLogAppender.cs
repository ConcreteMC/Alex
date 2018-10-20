using System;
using System.Collections.Generic;
using System.Reflection;
using NLog;
using log4net.Appender;
using log4net.Config;
using log4net.Core;

namespace log4net
{
	public class NLogAppender : AppenderSkeleton
	{
		readonly object _syncRoot = new object();
		Dictionary<string, Logger> _cache = new Dictionary<string, Logger>();

		protected override void Append(LoggingEvent loggingEvent)
		{
			var logger = GetLoggerFromCacheSafe(loggingEvent);

			var logEvent = ConvertToNLog(loggingEvent);

			logger.Log(logEvent);
		}

		Logger GetLoggerFromCacheSafe(LoggingEvent loggingEvent)
		{
			Logger logger;
			if (_cache.TryGetValue(loggingEvent.LoggerName, out logger))
				return logger;

			lock (_syncRoot)
			{
				if (_cache.TryGetValue(loggingEvent.LoggerName, out logger))
					return logger;

				logger = NLog.LogManager.GetLogger(loggingEvent.LoggerName);
				_cache = new Dictionary<string, Logger>(_cache) { { loggingEvent.LoggerName, logger } };
			}
			return logger;
		}

		static LogEventInfo ConvertToNLog(LoggingEvent loggingEvent)
		{
			return new LogEventInfo
			{
				Exception = loggingEvent.ExceptionObject,
				FormatProvider = null,
				LoggerName = loggingEvent.LoggerName,
				Message = Convert.ToString(loggingEvent.MessageObject),
				Level = ConvertLevel(loggingEvent.Level),
				TimeStamp = loggingEvent.TimeStamp
			};
		}

		static LogLevel ConvertLevel(Level level)
		{
			if (level == Level.Info)
				return LogLevel.Info;
			if (level == Level.Debug)
				return LogLevel.Debug;
			if (level == Level.Error)
				return LogLevel.Error;
			if (level == Level.Fatal)
				return LogLevel.Fatal;
			if (level == Level.Off)
				return LogLevel.Off;
			if (level == Level.Trace)
				return LogLevel.Trace;
			if (level == Level.Warn)
				return LogLevel.Warn;

			throw new NotSupportedException("Level " + level + " is currently not supported.");
		}

		public static void Initialize()
		{
			var repo = LogManager.GetRepository(Assembly.GetEntryAssembly());
			BasicConfigurator.Configure(repo, new NLogAppender());
		}
	}
}

