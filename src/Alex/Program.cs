using System;
using log4net;
using Microsoft.Xna.Framework;

namespace Alex
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
#if FNA
            FNALoggerEXT.LogError = s => Log.Error(s);
            FNALoggerEXT.LogWarn = s => Log.Warn(s);
            FNALoggerEXT.LogInfo = s => Log.Info(s);
#endif
            using (var game = new Alex())
                game.Run();
        }
    }
#endif
}
