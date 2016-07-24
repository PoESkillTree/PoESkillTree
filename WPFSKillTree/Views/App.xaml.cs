using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Model.Serialization;
using POESKillTree.Utils;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // The flag whether application exit is in progress.
        bool IsExiting = false;
        // The flag whether it is safe to exit application.
        bool IsSafeToExit = false;
        // Single instance of persistent data.
        public static IPersistentData PersistentData { get; private set; }
        // The Mutex for detecting running application instance.
        private Mutex RunningInstanceMutex;
        // The name of RunningInstanceMutex.
        private static string RunningInstanceMutexName { get { return POESKillTree.Properties.Version.AppId; } }

        // Invoked when application is about to exit.
        private void App_Exit(object sender, ExitEventArgs e)
        {
            if (IsExiting) return;
            IsExiting = true;

            try
            {
                // Try to aquire mutex.
                if (RunningInstanceMutex.WaitOne(0))
                {
                    PersistentData.Save();

                    IsSafeToExit = true;

                    RunningInstanceMutex.ReleaseMutex();
                }
            }
            catch
            {
                // Too late to report anything.
            }
        }

        // Invoked when application is being started up (before MainWindow creation).
        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Set main thread apartment state.
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

            // Set AppUserModelId of current process.
            TaskbarHelper.SetAppUserModelId();

            // Create Mutex if this is first instance.
            try
            {
                RunningInstanceMutex = Mutex.OpenExisting(RunningInstanceMutexName);
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                bool created = false;
                RunningInstanceMutex = new Mutex(false, RunningInstanceMutexName, out created);
                if (!created) throw new Exception("Unable to create application mutex");
            }

            // Load persistent data.
#if !DEBUG
            try
            {
#endif
                PersistentData = PersistentDataSerializationService.CreatePersistentData();
#if !DEBUG
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred during a load operation.\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
#endif

            // Initialize localization.
            L10n.Initialize(PersistentData.Options.Language);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

            if (!Debugger.IsAttached)
            {

                Exception theException = e.Exception;
                string theErrorPath = AppData.GetFolder(true) + "debug.txt";
                using (System.IO.TextWriter theTextWriter = new System.IO.StreamWriter(theErrorPath, true))
                {
                    DateTime theNow = DateTime.Now;
                    theTextWriter.WriteLine("The error time: " + theNow.ToShortDateString() + " " +
                                            theNow.ToShortTimeString());
                    while (theException != null)
                    {
                        theTextWriter.WriteLine("Exception: " + theException.ToString());
                        theException = theException.InnerException;
                    }
                }
                MessageBox.Show("The program crashed. A stack trace can be found at:\n" + theErrorPath);
                e.Handled = true;
                Application.Current.Shutdown();
            }
        }

        // Invoked when Windows is being shutdown or user is being logged off.
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);

            // Cancel session ending unless it's safe to exit.
            e.Cancel = !IsSafeToExit;

            // If Exit event wasn't raised yet, perform explicit shutdown (Windows 7 bug workaround).
            if (!IsExiting) Shutdown();
        }
    }
}
