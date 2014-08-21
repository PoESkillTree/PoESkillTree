using System;
using System.Diagnostics;
using System.Windows;

namespace POESKillTree.Views
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {

            if (!Debugger.IsAttached)
            {
            
            Exception theException = e.Exception;
            string theErrorPath = "debug.txt";
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
