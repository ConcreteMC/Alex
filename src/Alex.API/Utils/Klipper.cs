using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace Klipper.DBus
{
    [DBusInterface("org.kde.JobViewServer")]
    interface IJobViewServer : IDBusObject
    {
        Task<ObjectPath> requestViewAsync(string AppName, string AppIconName, int Capabilities);
    }

    [DBusInterface("org.kde.libkonq.FileUndoManager")]
    interface IFileUndoManager : IDBusObject
    {
        Task<byte[]> getAsync();
        Task<IDisposable> WatchlockAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchpopAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchpushAsync(Action<byte[]> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchunlockAsync(Action handler, Action<Exception> onError = null);
    }

    [DBusInterface("org.qtproject.Qt.QApplication")]
    interface IQApplication : IDBusObject
    {
        Task setStyleSheetAsync(string Sheet);
        Task setAutoSipEnabledAsync(bool Enabled);
        Task<bool> autoSipEnabledAsync();
        Task closeAllWindowsAsync();
        Task aboutQtAsync();
        Task<T> GetAsync<T>(string prop);
        Task<QApplicationProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class QApplicationProperties
    {
        private int _cursorFlashTime = default(int);
        public int CursorFlashTime
        {
            get
            {
                return _cursorFlashTime;
            }

            set
            {
                _cursorFlashTime = (value);
            }
        }

        private int _doubleClickInterval = default(int);
        public int DoubleClickInterval
        {
            get
            {
                return _doubleClickInterval;
            }

            set
            {
                _doubleClickInterval = (value);
            }
        }

        private int _keyboardInputInterval = default(int);
        public int KeyboardInputInterval
        {
            get
            {
                return _keyboardInputInterval;
            }

            set
            {
                _keyboardInputInterval = (value);
            }
        }

        private int _wheelScrollLines = default(int);
        public int WheelScrollLines
        {
            get
            {
                return _wheelScrollLines;
            }

            set
            {
                _wheelScrollLines = (value);
            }
        }

        private (int, int) _globalStrut = default((int, int));
        public (int, int) GlobalStrut
        {
            get
            {
                return _globalStrut;
            }

            set
            {
                _globalStrut = (value);
            }
        }

        private int _startDragTime = default(int);
        public int StartDragTime
        {
            get
            {
                return _startDragTime;
            }

            set
            {
                _startDragTime = (value);
            }
        }

        private int _startDragDistance = default(int);
        public int StartDragDistance
        {
            get
            {
                return _startDragDistance;
            }

            set
            {
                _startDragDistance = (value);
            }
        }

        private string _styleSheet = default(string);
        public string StyleSheet
        {
            get
            {
                return _styleSheet;
            }

            set
            {
                _styleSheet = (value);
            }
        }

        private bool _autoSipEnabled = default(bool);
        public bool AutoSipEnabled
        {
            get
            {
                return _autoSipEnabled;
            }

            set
            {
                _autoSipEnabled = (value);
            }
        }
    }

    static class QApplicationExtensions
    {
        public static Task<int> GetCursorFlashTimeAsync(this IQApplication o) => o.GetAsync<int>("cursorFlashTime");
        public static Task<int> GetDoubleClickIntervalAsync(this IQApplication o) => o.GetAsync<int>("doubleClickInterval");
        public static Task<int> GetKeyboardInputIntervalAsync(this IQApplication o) => o.GetAsync<int>("keyboardInputInterval");
        public static Task<int> GetWheelScrollLinesAsync(this IQApplication o) => o.GetAsync<int>("wheelScrollLines");
        public static Task<(int, int)> GetGlobalStrutAsync(this IQApplication o) => o.GetAsync<(int, int)>("globalStrut");
        public static Task<int> GetStartDragTimeAsync(this IQApplication o) => o.GetAsync<int>("startDragTime");
        public static Task<int> GetStartDragDistanceAsync(this IQApplication o) => o.GetAsync<int>("startDragDistance");
        public static Task<string> GetStyleSheetAsync(this IQApplication o) => o.GetAsync<string>("styleSheet");
        public static Task<bool> GetAutoSipEnabledAsync(this IQApplication o) => o.GetAsync<bool>("autoSipEnabled");
        public static Task SetCursorFlashTimeAsync(this IQApplication o, int val) => o.SetAsync("cursorFlashTime", val);
        public static Task SetDoubleClickIntervalAsync(this IQApplication o, int val) => o.SetAsync("doubleClickInterval", val);
        public static Task SetKeyboardInputIntervalAsync(this IQApplication o, int val) => o.SetAsync("keyboardInputInterval", val);
        public static Task SetWheelScrollLinesAsync(this IQApplication o, int val) => o.SetAsync("wheelScrollLines", val);
        public static Task SetGlobalStrutAsync(this IQApplication o, (int, int) val) => o.SetAsync("globalStrut", val);
        public static Task SetStartDragTimeAsync(this IQApplication o, int val) => o.SetAsync("startDragTime", val);
        public static Task SetStartDragDistanceAsync(this IQApplication o, int val) => o.SetAsync("startDragDistance", val);
        public static Task SetStyleSheetAsync(this IQApplication o, string val) => o.SetAsync("styleSheet", val);
        public static Task SetAutoSipEnabledAsync(this IQApplication o, bool val) => o.SetAsync("autoSipEnabled", val);
    }

    [DBusInterface("org.qtproject.Qt.QGuiApplication")]
    interface IQGuiApplication : IDBusObject
    {
        Task<T> GetAsync<T>(string prop);
        Task<QGuiApplicationProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class QGuiApplicationProperties
    {
        private string _applicationDisplayName = default(string);
        public string ApplicationDisplayName
        {
            get
            {
                return _applicationDisplayName;
            }

            set
            {
                _applicationDisplayName = (value);
            }
        }

        private string _desktopFileName = default(string);
        public string DesktopFileName
        {
            get
            {
                return _desktopFileName;
            }

            set
            {
                _desktopFileName = (value);
            }
        }

        private int _layoutDirection = default(int);
        public int LayoutDirection
        {
            get
            {
                return _layoutDirection;
            }

            set
            {
                _layoutDirection = (value);
            }
        }

        private string _platformName = default(string);
        public string PlatformName
        {
            get
            {
                return _platformName;
            }

            set
            {
                _platformName = (value);
            }
        }

        private bool _quitOnLastWindowClosed = default(bool);
        public bool QuitOnLastWindowClosed
        {
            get
            {
                return _quitOnLastWindowClosed;
            }

            set
            {
                _quitOnLastWindowClosed = (value);
            }
        }
    }

    static class QGuiApplicationExtensions
    {
        public static Task<string> GetApplicationDisplayNameAsync(this IQGuiApplication o) => o.GetAsync<string>("applicationDisplayName");
        public static Task<string> GetDesktopFileNameAsync(this IQGuiApplication o) => o.GetAsync<string>("desktopFileName");
        public static Task<int> GetLayoutDirectionAsync(this IQGuiApplication o) => o.GetAsync<int>("layoutDirection");
        public static Task<string> GetPlatformNameAsync(this IQGuiApplication o) => o.GetAsync<string>("platformName");
        public static Task<bool> GetQuitOnLastWindowClosedAsync(this IQGuiApplication o) => o.GetAsync<bool>("quitOnLastWindowClosed");
        public static Task SetApplicationDisplayNameAsync(this IQGuiApplication o, string val) => o.SetAsync("applicationDisplayName", val);
        public static Task SetDesktopFileNameAsync(this IQGuiApplication o, string val) => o.SetAsync("desktopFileName", val);
        public static Task SetLayoutDirectionAsync(this IQGuiApplication o, int val) => o.SetAsync("layoutDirection", val);
        public static Task SetQuitOnLastWindowClosedAsync(this IQGuiApplication o, bool val) => o.SetAsync("quitOnLastWindowClosed", val);
    }

    [DBusInterface("org.qtproject.Qt.QCoreApplication")]
    interface IQCoreApplication : IDBusObject
    {
        Task quitAsync();
        Task<T> GetAsync<T>(string prop);
        Task<QCoreApplicationProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    class QCoreApplicationProperties
    {
        private string _applicationName = default(string);
        public string ApplicationName
        {
            get
            {
                return _applicationName;
            }

            set
            {
                _applicationName = (value);
            }
        }

        private string _applicationVersion = default(string);
        public string ApplicationVersion
        {
            get
            {
                return _applicationVersion;
            }

            set
            {
                _applicationVersion = (value);
            }
        }

        private string _organizationName = default(string);
        public string OrganizationName
        {
            get
            {
                return _organizationName;
            }

            set
            {
                _organizationName = (value);
            }
        }

        private string _organizationDomain = default(string);
        public string OrganizationDomain
        {
            get
            {
                return _organizationDomain;
            }

            set
            {
                _organizationDomain = (value);
            }
        }

        private bool _quitLockEnabled = default(bool);
        public bool QuitLockEnabled
        {
            get
            {
                return _quitLockEnabled;
            }

            set
            {
                _quitLockEnabled = (value);
            }
        }
    }

    static class QCoreApplicationExtensions
    {
        public static Task<string> GetApplicationNameAsync(this IQCoreApplication o) => o.GetAsync<string>("applicationName");
        public static Task<string> GetApplicationVersionAsync(this IQCoreApplication o) => o.GetAsync<string>("applicationVersion");
        public static Task<string> GetOrganizationNameAsync(this IQCoreApplication o) => o.GetAsync<string>("organizationName");
        public static Task<string> GetOrganizationDomainAsync(this IQCoreApplication o) => o.GetAsync<string>("organizationDomain");
        public static Task<bool> GetQuitLockEnabledAsync(this IQCoreApplication o) => o.GetAsync<bool>("quitLockEnabled");
        public static Task SetApplicationNameAsync(this IQCoreApplication o, string val) => o.SetAsync("applicationName", val);
        public static Task SetApplicationVersionAsync(this IQCoreApplication o, string val) => o.SetAsync("applicationVersion", val);
        public static Task SetOrganizationNameAsync(this IQCoreApplication o, string val) => o.SetAsync("organizationName", val);
        public static Task SetOrganizationDomainAsync(this IQCoreApplication o, string val) => o.SetAsync("organizationDomain", val);
        public static Task SetQuitLockEnabledAsync(this IQCoreApplication o, bool val) => o.SetAsync("quitLockEnabled", val);
    }

    [DBusInterface("org.kde.PlasmaShell")]
    interface IPlasmaShell : IDBusObject
    {
        Task toggleDashboardAsync();
        Task toggleActivityManagerAsync();
        Task toggleWidgetExplorerAsync();
        Task setDashboardShownAsync(bool Show);
        Task showInteractiveConsoleAsync();
        Task loadScriptInInteractiveConsoleAsync(string Script);
        Task showInteractiveKWinConsoleAsync();
        Task loadKWinScriptInInteractiveConsoleAsync(string Script);
        Task evaluateScriptAsync(string Script);
        Task<byte[]> dumpCurrentLayoutJSAsync();
        Task loadLookAndFeelDefaultLayoutAsync(string Layout);
        Task activateLauncherMenuAsync();
    }

    [DBusInterface("org.kde.klipper.klipper")]
    interface IKlipper : IDBusObject
    {
        Task<string> getClipboardContentsAsync();
        Task setClipboardContentsAsync(string S);
        Task clearClipboardContentsAsync();
        Task clearClipboardHistoryAsync();
        Task saveClipboardHistoryAsync();
        Task<string[]> getClipboardHistoryMenuAsync();
        Task<string> getClipboardHistoryItemAsync(int I);
        Task showKlipperPopupMenuAsync();
        Task showKlipperManuallyInvokeActionMenuAsync();
    }

    [DBusInterface("org.freedesktop.Notifications")]
    interface INotifications : IDBusObject
    {
        Task<uint> NotifyAsync(string AppName, uint ReplacesId, string AppIcon, string Summary, string Body, string[] Actions, IDictionary<string, object> Hints, int Timeout);
        Task CloseNotificationAsync(uint Id);
        Task<string[]> GetCapabilitiesAsync();
        Task<(string name, string vendor, string version, string specVersion)> GetServerInformationAsync();
        Task<IDisposable> WatchNotificationClosedAsync(Action<(uint id, uint reason)> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchActionInvokedAsync(Action<(uint id, string actionKey)> handler, Action<Exception> onError = null);
    }

    [DBusInterface("org.kde.osdService")]
    interface IOsdService : IDBusObject
    {
        Task brightnessChangedAsync(int Percent);
        Task keyboardBrightnessChangedAsync(int Percent);
        Task volumeChangedAsync(int Percent);
        Task microphoneVolumeChangedAsync(int Percent);
        Task mediaPlayerVolumeChangedAsync(int Percent, string PlayerName, string PlayerIconName);
        Task kbdLayoutChangedAsync(string LayoutName);
        Task virtualDesktopChangedAsync(string CurrentVirtualDesktopName);
        Task touchpadEnabledChangedAsync(bool TouchpadEnabled);
        Task wifiEnabledChangedAsync(bool WifiEnabled);
        Task bluetoothEnabledChangedAsync(bool BluetoothEnabled);
        Task wwanEnabledChangedAsync(bool WwanEnabled);
        Task virtualKeyboardEnabledChangedAsync(bool VirtualKeyboardEnabled);
        Task showTextAsync(string Icon, string Text);
        Task<IDisposable> WatchosdProgressAsync(Action<(string icon, int percent, string additionalText)> handler, Action<Exception> onError = null);
        Task<IDisposable> WatchosdTextAsync(Action<(string icon, string text)> handler, Action<Exception> onError = null);
    }

    [DBusInterface("org.freedesktop.Application")]
    interface IApplication : IDBusObject
    {
        Task ActivateAsync(IDictionary<string, object> PlatformData);
        Task OpenAsync(string[] Uris, IDictionary<string, object> PlatformData);
        Task ActivateActionAsync(string ActionName, object[] Parameter, IDictionary<string, object> PlatformData);
    }

    [DBusInterface("org.kde.KDBusService")]
    interface IKDBusService : IDBusObject
    {
        Task<int> CommandLineAsync(string[] Arguments, string WorkingDir, IDictionary<string, object> PlatformData);
    }
}