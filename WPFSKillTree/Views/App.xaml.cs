using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using NLog;
using PoESkillTree.Utils;
using PoESkillTree.Localization;
using PoESkillTree.Model;
using PoESkillTree.Model.Serialization;

namespace PoESkillTree.Views
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        // The flag whether application exit is in progress.
        private bool _isExiting;

        // The flag whether it is safe to exit application.
        private bool _isSafeToExit;

#pragma warning disable CS8618 // Initialized in OnStartup
        // Single instance of persistent data.
        public static IPersistentData PersistentData { get; private set; }

        // The Mutex for detecting running application instance.
        private Mutex _runningInstanceMutex;
#pragma warning restore

        private static string RunningInstanceMutexName => AppData.ProductName;

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
                Log.Error(ex, "Exception while exiting");
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

        // Compared to AppDomain.CurrentDomain.UnhandledException, this allows for a more graceful shutdown for
        // unhandled exceptions that occur in the main/UI thread
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached) return;

            OnUnhandledException(e.Exception);
            e.Handled = true;
            Shutdown();
        }

        private static void OnUnhandledException(Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");

            if (!Debugger.IsAttached)
            {
                var filePath = AppData.GetFolder() + "/logs/crash.log";
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