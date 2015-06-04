using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using POESKillTree.Localization;

namespace POESKillTree.Utils
{
    // Taskbar helper class.
    class TaskbarHelper
    {
        #region Interop

        // Interop declaration for KERNEL32.DLL FreeLibrary function.
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeLibrary(IntPtr hModule);
        // Interop declaration for KERNEL32.DLL LoadLibrary function.
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
        // Interop declaration for USER32.DLL LoadString function.
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int LoadString(IntPtr hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);
        // Interop declaration for USER32.DLL SendMessage function.
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        #endregion

        // String resource identifiers of FolderItemVerbs in Shell32 DLL.
        enum Shell32VerbID
        {
            PinToTaskbar = 5386,
            UnpinFromTaskbar = 5387
        }

        // The argument used by pinning JumpTask.
        const string JumpTaskPinningArgument = "/P";
        // Window message for pinning JumpTask (WM_APP + 1).
        const uint WM_PINNING = 0x8000 + 1;

        // The flag whether all pinning Verbs were retrieved.
        static bool HasPinningVerbs = false;
        // The flag whether Verb retrieval was attempted.
        static bool HasVerbsLoaded = false;
        // Localized "Pin to Taskbar" Verb.
        static string PinToTaskbarVerb;
        // Localized "Unpin from Taskbar" Verb.
        static string UnpinFromTaskbarVerb;

        // Enables custom pinning.
        public static void EnablePinning(Window window)
        {
            // No pinning when debugging.
            if (Debugger.IsAttached) return;

            if (IsPinningSupported && !IsPinned)
            {
                // Prevent window from getting pinned by OS.
                WindowProperties.SetWindowProperty(window, SystemProperties.System.AppUserModel.PreventPinning, "true");

                JumpList jumpList = JumpList.GetJumpList(Application.Current);
                if (jumpList != null)
                {
                    string exePath = GetOriginalExeLocation();

                    JumpTask jumpTask = new JumpTask();
                    jumpTask.ApplicationPath = exePath;
                    jumpTask.WorkingDirectory = Path.GetDirectoryName(exePath);
                    jumpTask.IconResourcePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), "imageres.dll");
                    jumpTask.IconResourceIndex = 217; // Pin
                    jumpTask.Arguments = JumpTaskPinningArgument;
                    jumpTask.Title = L10n.Message("Pin this program to taskbar");
                    jumpList.JumpItems.Add(jumpTask);

                    jumpList.Apply();
                }

                // Register window message processing hook for main application window.
                HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
                source.AddHook(new HwndSourceHook(WndProc));
            }
        }

        // Returns location of original executable.
        static string GetOriginalExeLocation()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string dir = Path.GetDirectoryName(new Uri(assembly.GetName().EscapedCodeBase).LocalPath);

            // Filename part must be from assembly location's filename (assembly CodeBase has mangled filename extension).
            return Path.Combine(dir, Path.GetFileName(assembly.Location));
        }

        // Returns true if execution was trigger from JumpTask.
        public static bool IsJumpTask(string[] arguments)
        {
            // Pinning requested (executed from JumpTask).
            if (arguments.Length == 1 && arguments[0] == JumpTaskPinningArgument)
            {
                // No pinning when debugging.
                if (Debugger.IsAttached) return true;

                NotifyApplication(WM_PINNING);

                return true;
            }

            return false;
        }

        // Returns true if executable is pinned.
        static bool IsPinned
        {
            get
            {
                if (!IsPinningSupported) return false;

                dynamic application = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

                string path = GetOriginalExeLocation();
                string dirName = Path.GetDirectoryName(path);
                string fileName = Path.GetFileName(path);

                dynamic directory = application.NameSpace(dirName);
                dynamic link = directory.ParseName(fileName);
                dynamic verbs = link.Verbs();

                for (int i = 0; i < verbs.Count(); i++)
                {
                    dynamic verb = verbs.Item(i);
                    string verbName = verb.Name;

                    if (verbName == UnpinFromTaskbarVerb)
                        return true;
                    else
                        if (verbName == PinToTaskbarVerb)
                            return false;
                }

                return false;
            }
        }

        // Returns true if OS version supports taskbar pinning.
        static bool IsPinningSupported
        {
            get
            {
                if (!HasVerbsLoaded) LoadVerbs();

                return HasPinningVerbs;
            }
        }

        // Loads localized Verbs from Shell32 DLL.
        static void LoadVerbs()
        {
            // Don't attempt to load Verbs again.
            if (HasVerbsLoaded) return;
            HasVerbsLoaded = true;

            StringBuilder sb = new StringBuilder(255);

            IntPtr handle = LoadLibrary("shell32.dll");
            if (handle == IntPtr.Zero) return;

            int loaded = LoadString(handle, (uint)Shell32VerbID.PinToTaskbar, sb, sb.Capacity + 1);
            if (loaded > 0 && loaded < sb.Capacity)
                PinToTaskbarVerb = sb.ToString();
            sb.Clear();

            LoadString(handle, (uint)Shell32VerbID.UnpinFromTaskbar, sb, sb.Capacity + 1);
            if (loaded > 0 && loaded < sb.Capacity)
                UnpinFromTaskbarVerb = sb.ToString();
            sb.Clear();

            HasPinningVerbs = !string.IsNullOrEmpty(PinToTaskbarVerb) && !string.IsNullOrEmpty(UnpinFromTaskbarVerb);

            FreeLibrary(handle);
        }

        // Notifies application.
        static void NotifyApplication(uint wm)
        {
            // Get current PID to exclude from running process.
            int pid = Process.GetCurrentProcess().Id;

            Process[] ps = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            for (int i = 0; i < ps.Length; ++i)
            {
                if (ps[i].Id == pid) continue;

                IntPtr handle = ps[i].MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    // Send message to window.
                    SendMessage(handle, wm, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }

        // Pins executable to taskbar.
        static void PinToTaskbar()
        {
            if (!IsPinningSupported) throw new NotSupportedException("Taskbar pinning not supported");

            dynamic application = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

            string path = GetOriginalExeLocation();
            string dirName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            dynamic directory = application.NameSpace(dirName);
            dynamic link = directory.ParseName(fileName);
            dynamic verbs = link.Verbs();

            for (int i = 0; i < verbs.Count(); i++)
            {
                dynamic verb = verbs.Item(i);
                string verbName = verb.Name;

                if (verbName == PinToTaskbarVerb)
                {
                    verb.DoIt();

                    return;
                }
            }
        }

        // Removes pinning JumpTask.
        static void RemoveJumpTask()
        {
            JumpList jumpList = JumpList.GetJumpList(Application.Current);
            if (jumpList != null && jumpList.JumpItems.Count == 1)
            {
                jumpList.JumpItems.Clear();
                jumpList.Apply();
            }
        }

        // Unpins executable from taskbar.
        static void UnpinFromTaskbar()
        {
            if (!IsPinningSupported) throw new NotSupportedException("Taskbar pinning not supported");

            dynamic application = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

            string path = GetOriginalExeLocation();
            string dirName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            dynamic directory = application.NameSpace(dirName);
            dynamic link = directory.ParseName(fileName);
            dynamic verbs = link.Verbs();

            for (int i = 0; i < verbs.Count(); i++)
            {
                dynamic verb = verbs.Item(i);
                string verbName = verb.Name;

                if (verbName == UnpinFromTaskbarVerb)
                {
                    verb.DoIt();

                    return;
                }
            }
        }

        // Handles window message of application.
        private static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_PINNING)
            {
                handled = true;

                if (!IsPinned) PinToTaskbar();

                RemoveJumpTask();
            }

            return IntPtr.Zero;
        }
    }
}
