using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using JumpList = System.Windows.Shell.JumpList;
using JumpTask = System.Windows.Shell.JumpTask;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace POESKillTree.Utils
{
    // Taskbar helper class.
    class TaskbarHelper
    {
        #region Interop

        // Interop declaration for USER32.DLL SendMessage function.
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        #endregion // Interop

        // The argument used by JumpTask.
        const string JumpTaskArgument = "/T";
        // Window message base for JumpTasks (WM_APP + 0x100).
        const uint WM_JUMPTASK = 0x8000 + 0x100;

        // Enables JumpTask.
        public static void EnableJumpTask(Window window, string name)
        {
            JumpList jumpList = JumpList.GetJumpList(Application.Current);
            if (jumpList != null)
            {
                string exePath = Assembly.GetExecutingAssembly().Location;

                JumpTask jumpTask = new JumpTask();
                jumpTask.ApplicationPath = exePath;
                jumpTask.WorkingDirectory = Path.GetDirectoryName(exePath);
                jumpTask.IconResourcePath = exePath;
                // jumpTask.IconResourceIndex = 0;
                jumpTask.Arguments = JumpTaskArgument;
                jumpTask.Title = name;
                jumpList.JumpItems.Add(jumpTask);

                jumpList.Apply();
            }

            // Register window message processing hook for main application window.
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        // Returns true if execution was trigger from JumpTask.
        public static bool IsJumpTask(string[] arguments)
        {
            // Pinning requested (executed from JumpTask).
            if (arguments.Length == 1 && arguments[0] == JumpTaskArgument)
            {
                NotifyApplication(WM_JUMPTASK);

                return true;
            }

            return false;
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

        // Sets AppUserModelId of the application process.
        // Must be invoked before any window is created.
        public static void SetAppUserModelId()
        {
            // TaskbarManager is supported only on Windows Vista and later.
            if (TaskbarManager.IsPlatformSupported)
            {
                // Set AppUserModelId of process.
                TaskbarManager.Instance.ApplicationId = Properties.Version.AppId;
            }
        }

        // Handles window message of application.
        private static IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_JUMPTASK)
            {
                handled = true;

                // TODO: Trigger Event registered with JumpTask.
            }

            return IntPtr.Zero;
        }
    }
}
