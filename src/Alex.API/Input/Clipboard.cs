using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Alex.API.Input
{
    public static class Clipboard
    {
        private static IClipboardImplementation Implementation { get; set; } = null;

        static Clipboard()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Implementation = new WindowsClipboard();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (XClipClipboard.IsXClipAvailable())
                {
                    Implementation = new XClipClipboard();
                }
                else
                {
                    var desktopEnvironment = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP");
                    if (desktopEnvironment != null)
                    {
                        if (desktopEnvironment.ToLower().Contains("kde"))
                        {
                            Implementation = new LinuxKdeClipboard();
                        }
                    }
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Implementation = new MacOSClipboard();
            }

            if (Implementation == null)
                Implementation = new MockClipboard();
        }

        public static bool IsClipboardAvailable()
        {
            return Implementation != null && !(Implementation is MockClipboard);
        }
        
        public static void SetText(string text)
        {
            Implementation.SetText(text);
        }

        public static string GetText()
        {
            return Implementation.GetText();
        }
    }

    public class MockClipboard : IClipboardImplementation
    {
        public void SetText(string value)
        {
            
        }

        public string GetText()
        {
            return String.Empty;
        }
    }

    public class MacOSClipboard : IClipboardImplementation
    {
        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr objc_getClass(string className);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, string arg1);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        [DllImport("/System/Library/Frameworks/AppKit.framework/AppKit")]
        static extern IntPtr sel_registerName(string selectorName);
        
        IntPtr nsString = objc_getClass("NSString");
        IntPtr nsPasteboard = objc_getClass("NSPasteboard");
        IntPtr nsStringPboardType;
        IntPtr utfTextType;
        IntPtr generalPasteboard;
        IntPtr initWithUtf8Register = sel_registerName("initWithUTF8String:");
        IntPtr allocRegister = sel_registerName("alloc");
        IntPtr setStringRegister = sel_registerName("setString:forType:");
        IntPtr stringForTypeRegister = sel_registerName("stringForType:");
        IntPtr utf8Register = sel_registerName("UTF8String");
        IntPtr generalPasteboardRegister = sel_registerName("generalPasteboard");
        IntPtr clearContentsRegister = sel_registerName("clearContents");

        public MacOSClipboard()
        {
            utfTextType = objc_msgSend(objc_msgSend(nsString, allocRegister), initWithUtf8Register, "public.utf8-plain-text");
            nsStringPboardType = objc_msgSend(objc_msgSend(nsString, allocRegister), initWithUtf8Register, "NSStringPboardType");

            generalPasteboard = objc_msgSend(nsPasteboard, generalPasteboardRegister);
        }
        
        public void SetText(string value)
        {
            IntPtr str = default;
            try
            {
                str = objc_msgSend(objc_msgSend(nsString, allocRegister), initWithUtf8Register, value);
                objc_msgSend(generalPasteboard, clearContentsRegister);
                objc_msgSend(generalPasteboard, setStringRegister, str, utfTextType);
            }
            finally
            {
                if (str != default)
                {
                    objc_msgSend(str, sel_registerName("release"));
                }
            }
        }

        public string GetText()
        {
            var ptr = objc_msgSend(generalPasteboard, stringForTypeRegister, nsStringPboardType);
            var charArray = objc_msgSend(ptr, utf8Register);
            return Marshal.PtrToStringAnsi(charArray);
        }
    }

    public class XClipClipboard : IClipboardImplementation
    {
        private static string Run(string commandLine, bool waitForExit = true)
        {
            var errorBuilder = new StringBuilder();
            var outputBuilder = new StringBuilder();
            var arguments = $"-c \"{commandLine}\"";
            using (var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            })
            {
                process.Start();
                process.OutputDataReceived += (sender, args) => { outputBuilder.AppendLine(args.Data); };
                process.BeginOutputReadLine();
                process.ErrorDataReceived += (sender, args) => { errorBuilder.AppendLine(args.Data); };
                process.BeginErrorReadLine();
                if (waitForExit)
                {
                    if (!DoubleWaitForExit(process))
                    {
                        var timeoutError = $@"Process timed out. Command line: bash {arguments}.
Output: {outputBuilder}
Error: {errorBuilder}";
                        throw new Exception(timeoutError);
                    }
                }
                else
                {
                    process.WaitForExit(500);
                }

                if (process.ExitCode == 0)
                {
                    return outputBuilder.ToString();
                }

                var error = $@"Could not execute process. Command line: bash {arguments}.
Output: {outputBuilder}
Error: {errorBuilder}";
                throw new Exception(error);
            }
        }

        //To work around https://github.com/dotnet/runtime/issues/27128
        static bool DoubleWaitForExit(Process process)
        {
            var result = process.WaitForExit(500);
            if (result)
            {
                process.WaitForExit();
            }
            return result;
        }

        public static bool IsXClipAvailable()
        {
            try
            {
                new XClipClipboard().SetText("Alex was here, sorry for replacing your keyboard contents!");
                string content = Run("xclip -o");

                return !string.IsNullOrWhiteSpace(content) && !content.Contains("but can be installed with");
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        
        public void SetText(string value)
        {
            var tempFileName = Path.GetTempFileName();
            File.WriteAllText(tempFileName, value);
            try
            {
                Run($"cat {tempFileName} | xclip -i -selection clipboard", false);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        public string GetText()
        {
            var tempFileName = Path.GetTempFileName();
            try
            {
                Run($"xclip -o -selection clipboard > {tempFileName}");
                return File.ReadAllText(tempFileName);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }
    }
    
    public class LinuxKdeClipboard : IClipboardImplementation
    {
        public void SetText(string value)
        {
            Tmds.DBus.ObjectPath objectPath = new Tmds.DBus.ObjectPath("/klipper");
            string service = "org.kde.klipper";

            using (Tmds.DBus.Connection connection = new Tmds.DBus.Connection(Tmds.DBus.Address.Session))
            {
                connection.ConnectAsync().Wait();

                Klipper.DBus.IKlipper klipper = connection.CreateProxy<Klipper.DBus.IKlipper>(service, objectPath);
                klipper.setClipboardContentsAsync(value).Wait();
            }
        }

        public string GetText()
        {
            string clipboardContents = null;

            Tmds.DBus.ObjectPath objectPath = new Tmds.DBus.ObjectPath("/klipper");
            string service = "org.kde.klipper";

            using (Tmds.DBus.Connection connection = new Tmds.DBus.Connection(Tmds.DBus.Address.Session))
            {
                connection.ConnectAsync().Wait();

                Klipper.DBus.IKlipper klipper = connection.CreateProxy<Klipper.DBus.IKlipper>(service, objectPath);

                clipboardContents = klipper.getClipboardContentsAsync().Result;
            } // End Using connection 

            return clipboardContents;
        }
    }

    public class WindowsClipboard : IClipboardImplementation
    {
        [DllImport("user32.dll")]
        internal static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        internal static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        internal static extern bool SetClipboardData(uint uFormat, IntPtr data);
        
        [DllImport("user32.dll")]
        static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("kernel32.dll")]
        static extern IntPtr GlobalLock(IntPtr hMem);
        
        [DllImport("kernel32.dll")]
        static extern bool GlobalUnlock(IntPtr hMem);
        
        [DllImport("kernel32.dll")]
        private static extern UIntPtr GlobalSize(IntPtr hMem);
        
        [DllImport("user32.dll")]
        static extern bool IsClipboardFormatAvailable(uint format);
        
        const uint CF_UNICODETEXT = 13;
        
        public void SetText(string value)
        {
            OpenClipboard(IntPtr.Zero);

            SetClipboardData(CF_UNICODETEXT, Marshal.StringToHGlobalUni(value));
            
            CloseClipboard();
        }

        public string GetText()
        {
            if (!IsClipboardFormatAvailable(CF_UNICODETEXT))
                return null;
            
            if (!OpenClipboard(IntPtr.Zero))
                return null;

            string data = null;
            var hGlobal = GetClipboardData(CF_UNICODETEXT);
            if (hGlobal != IntPtr.Zero)
            {
                var lpwcstr = GlobalLock(hGlobal);
                if (lpwcstr != IntPtr.Zero)
                {
                    data = Marshal.PtrToStringUni(lpwcstr);
                    GlobalUnlock(lpwcstr);
                }
            }
            CloseClipboard();

            return data;
        }
    }

    public interface IClipboardImplementation
    {
        void SetText(string value);
        string GetText();
    }
}