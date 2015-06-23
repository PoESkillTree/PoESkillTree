using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using POESKillTree.Localization;
using POESKillTree.Model;
using POESKillTree.Utils;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Single instance of persistent data.
        private static readonly PersistentData PrivatePersistentData = new PersistentData();
        // Expose persistent data.
        public static PersistentData PersistentData { get { return PrivatePersistentData; } }
        // The Mutex for detecting running application instance.
        private Mutex RunningInstanceMutex;
        // The name of RunningInstanceMutex.
        private static string RunningInstanceMutexName { get { return POESKillTree.Properties.Version.AppId; } }

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
                RunningInstanceMutex = new Mutex(true, RunningInstanceMutexName, out created);
                if (!created) throw new Exception("Unable to create application mutex");
            }

            // Load persistent data.
            PrivatePersistentData.LoadPersistentDataFromFile();

            // Initialize localization.
            L10n.Initialize(PrivatePersistentData);
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
                MessageBox.Show("The program crashed.  A stack trace can be found at:\n" + theErrorPath);
                e.Handled = true;
                Application.Current.Shutdown();
            }
        }
    }
}
