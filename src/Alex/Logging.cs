using System;
using System.Drawing;

namespace Alex
{
    public static class Logging
    {
        private static void Log(ErrorState state, string text, LoggingOptions options)
        {
            var time = DateTime.Now.ToString("dd-MM-yy HH:mm:ss");
            var color = state != ErrorState.INFO && options.Colored
                ? (state == ErrorState.ERROR ? Color.Red : Color.Orange)
                : Color.Black;
            var finalstring = string.Format(options.UseTimeAndStatePrefix ? "[{0}][{1}][{2}] {3}" : "{3}",
                LoggingConfiguration.ProductName, state, time,
                text);
            Console.WriteLine(finalstring);
        }

        public static void Info(string message, LoggingOptions options)
        {
            Log(ErrorState.INFO, message, options);
        }

        public static void Info(string message)
        {
            Log(ErrorState.INFO, message, new LoggingOptions
            {
                Colored = false
            });
        }

        public static void Error(string message, LoggingOptions options)
        {
            Log(ErrorState.ERROR, message, options);
        }

        public static void Error(string message)
        {
            Log(ErrorState.ERROR, message, new LoggingOptions
            {
                Colored = true,
                UseTimeAndStatePrefix = true
            });
        }

        public static void Warning(string message, LoggingOptions options)
        {
            Log(ErrorState.WARNING, message, options);
        }

        public static void Warning(string message)
        {
            Log(ErrorState.WARNING, message, new LoggingOptions
            {
                Colored = true,
                UseTimeAndStatePrefix = true
            });
        }

        private enum ErrorState
        {
            WARNING,
            ERROR,
            INFO
        }
    }

    public class LoggingOptions
    {
        public bool Colored;
        public bool UseTimeAndStatePrefix = true;
    }

    public static class LoggingConfiguration
    {
        public static dynamic LoggingBox;
        public static string ProductName = "Alex";
    }
}
