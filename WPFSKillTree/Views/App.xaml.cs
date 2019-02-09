using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using log4net;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.Serialization;
using POESKillTree.Utils;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(App));

        // The flag whether application exit is in progress.
        private bool _isExiting;

        // The flag whether it is safe to exit application.
        private bool _isSafeToExit;

        // Single instance of persistent data.
        public static IPersistentData PersistentData { get; private set; }

        // The Mutex for detecting running application instance.
        private Mutex _runningInstanceMutex;

        private static string RunningInstanceMutexName => POESKillTree.Properties.Version.AppId;

        // Invoked when application is about to exit.
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            if (_isExiting) return;
            _isExiting = true;

            try
            {
                // Try to aquire mutex.
                if (_runningInstanceMutex.WaitOne(0))
                {
                    PersistentData.Save();
                    PersistentData.SaveFolders();

                    _isSafeToExit = true;

                    _runningInstanceMutex.ReleaseMutex();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception while exiting", ex);
            }
        }

        // Invoked when application is being started up (before MainWindow creation).
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException +=
                (_, args) => OnUnhandledException((Exception) args.ExceptionObject);

            // Create Mutex if this is first instance.
            try
            {
                _runningInstanceMutex = Mutex.OpenExisting(RunningInstanceMutexName);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                _runningInstanceMutex = new Mutex(false, RunningInstanceMutexName, out var created);
                if (!created)
                    throw new Exception("Unable to create application mutex");
            }

            // Load persistent data.
            // Take the first not-switch argument as path to the build that will be imported
            var importedBuildPath = e.Args.FirstOrDefault(s => !s.StartsWith("/"));
            PersistentData = PersistentDataSerializationService.CreatePersistentData(importedBuildPath);

            // Initialize localization.
            L10n.Initialize(PersistentData.Options.Language);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            OnUnhandledException(e.Exception);

            if (!Debugger.IsAttached)
            {
                e.Handled = true;
                Shutdown();
            }
        }

        private static void OnUnhandledException(Exception ex)
        {
            Log.Error("Unhandled exception", ex);

            if (!Debugger.IsAttached)
            {
                string filePath = AppData.GetFolder(true) + "debug.txt";
                using (TextWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"Error time: {DateTime.Now:u}");
                    while (ex != null)
                    {
                        writer.WriteLine($"Exception: {ex}");
                        ex = ex.InnerException;
                    }
                }
                MessageBox.Show("The program crashed. A stack trace can be found at:\n" + filePath);
            }
        }

        // Invoked when Windows is being shutdown or user is being logged off.
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);

            // Cancel session ending unless it's safe to exit.
            e.Cancel = !_isSafeToExit;

            // If Exit event wasn't raised yet, perform explicit shutdown (Windows 7 bug workaround).
            if (!_isExiting) Shutdown();
        }
    }
}