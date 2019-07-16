using System;
using System.Diagnostics;

namespace Alex.API.Utils
{
    public static class CrossPlatformUtils
    {
        public static void OpenFolder(string directory)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Process.Start("explorer.exe", $"\"{directory}\"");
            }
            else if (Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                Process.Start("open", $"\"{directory}\"");
            }
            else if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                Process.Start("xdg-open", $"\"{directory}\"");
            }
        }
    }
}