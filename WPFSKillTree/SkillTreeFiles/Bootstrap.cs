using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using POESKillTree.Views;

namespace POESKillTree.SkillTreeFiles
{
    /* Application entry class.
     * Creates copy of main executable and executes its assembly in separate application domain with shadow copying enabled.
     * This allows update process to simply overwrite all files during runtime.
     */
    class Bootstrap
    {
        // The name of shadow copy enabled domain.
        private static readonly string ShadowCopyDomainName = "POESkillTreeShadowCopy";
        // The name of environment variable to check for restart process.
        private static readonly string RestartEnvironmentVariableName = "RESTART";
        // The exit code of application restart process.
        private const int RESTART_EXIT_CODE = 42;

        // Entry point method.
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        public static void Main(string[] arguments)
        {
            // Don't do shadow copying when being debugged in VS.
            if (Debugger.IsAttached)
            {
                Run();

                return;
            }

            // Get current assembly location.
            string location = Assembly.GetExecutingAssembly().Location;

            // Check whether copy was executed.
            if (location.EndsWith(".run"))
            {
                // Check whether shadow copy enabled domain was executed.
                if (AppDomain.CurrentDomain.FriendlyName == ShadowCopyDomainName)
                {
                    Run();
                }
                else
                {
                    // Create shadow copy enabled application domain.
                    AppDomainSetup setup = new AppDomainSetup();
                    setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                    setup.ApplicationName = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".run");
                    setup.ShadowCopyFiles = "true";
                    AppDomain domain = AppDomain.CreateDomain(ShadowCopyDomainName, AppDomain.CurrentDomain.Evidence, setup);

                    // Execute assembly.
                    domain.ExecuteAssembly(location);
                }
            }
            else // Main executable was executed.
            {
                // Get process name of run copy.
                string runProcessName = AppDomain.CurrentDomain.FriendlyName.Replace(".exe", ".run");

                    // Check whether application restart is being performed.
                bool isRestarting = Environment.GetEnvironmentVariable(RestartEnvironmentVariableName) == RestartEnvironmentVariableName;

                // Check whether copy isn't already running.
                Process[] list = Process.GetProcessesByName(runProcessName);
                if (list.Length > 0)
                {
                    // Too many processes running.
                    if (list.Length > 1) return;

                    // Check whether application restart is being performed.
                    if (isRestarting)
                    {
                        // Wait for 10s at most, then bail out.
                        list[0].WaitForExit(10000);
                        if (Process.GetProcessesByName(runProcessName).Length > 0) return;
                    }
                    else
                    {
                        // TODO: Bring application window to front.
                        return;
                    }
                }

                // Create copy of main executable and execute it.
                string runLocation = location.Replace(".exe", ".run");
                File.Copy(location, runLocation, true);

                Process runProcess = new Process();
                runProcess.StartInfo.FileName = runLocation;
                runProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(runLocation);
                runProcess.StartInfo.CreateNoWindow = true;
                runProcess.StartInfo.UseShellExecute = false;
                if (isRestarting)
                    runProcess.StartInfo.EnvironmentVariables.Remove(RestartEnvironmentVariableName);
                runProcess.Start();
            }
        }

        // Restarts WPF application.
        public static void Restart()
        {
            // Shutdown application with restart specific exit code.
            Application.Current.Shutdown(RESTART_EXIT_CODE);
        }

        // Invoked on application exit.
        private static void RestartExitHandler(object sender, ExitEventArgs e)
        {
            if (e.ApplicationExitCode == RESTART_EXIT_CODE)
            {
                // Get original location of assembly (not shadow copy location).
                string location = new Uri(Assembly.GetExecutingAssembly().GetName().EscapedCodeBase).LocalPath;

                // Start main executable with restart environment variable set to indicate restart process.
                Process exeProcess = new Process();
                exeProcess.StartInfo.FileName = location.Replace(".run", ".exe");
                exeProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(location);
                exeProcess.StartInfo.CreateNoWindow = true;
                exeProcess.StartInfo.UseShellExecute = false;
                exeProcess.StartInfo.EnvironmentVariables.Add(RestartEnvironmentVariableName, RestartEnvironmentVariableName);
                exeProcess.Start();
            }
        }

        // Run WPF application.
        private static void Run()
        {
            App app = new App();
            app.Exit += new ExitEventHandler(RestartExitHandler);
            app.InitializeComponent();
            app.Run();
        }
    }
}
