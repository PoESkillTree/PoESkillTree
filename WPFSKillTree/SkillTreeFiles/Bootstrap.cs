using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using POESKillTree.Utils;
using POESKillTree.Views;

namespace POESKillTree.SkillTreeFiles
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
    using Newtonsoft.Json;    
    // Application entry class.
    class Bootstrap : MarshalByRefObject
    {
        // Bootstrap mode.
        private enum Mode
        {
            DEFAULT,     // Initial launch.
            RESTART      // Restart application.
        }
        // The name of environment variable containing boostrap mode.
        private static readonly string ModeEnvironmentVariableName = "BOOTSTRAP_MODE";
        // The name of environment variable containing PID of original process.
        private static readonly string PIDEnvironmentVariableName = "BOOTSTRAP_PID";
        // The exit code of application restart process.
        private const int RESTART_EXIT_CODE = 42;

        // Entry point method.
        [STAThread]
        public static void Main(string[] arguments)
        {
#if (PoESkillTree_ForceGlobalJSONConverter)
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new System.Collections.Generic.List<JsonConverter> { new CustomJSONConverter() }
            };
#endif
#if (DEBUG)
            Console.SetBufferSize(Console.BufferWidth, 32766);
            Console.SetWindowSize(150, 75);
#endif
            // If executed from JumpTask, do nothing.
            if (TaskbarHelper.IsJumpTask(arguments))
                return;

            // Parse boostrap mode.
            Mode mode;
            Enum.TryParse<Mode>(Environment.GetEnvironmentVariable(ModeEnvironmentVariableName), false, out mode);
            // Parse original PID.
            int pid;
            int.TryParse(Environment.GetEnvironmentVariable(PIDEnvironmentVariableName), out pid);

            switch (mode)
            {
                case Mode.RESTART:
                    // Wait for original process to exit.
                    try
                    {
                        Process wait = Process.GetProcessById(pid);
                        if (wait != null)
                        {
                            // If process didn't exit after 10s, bail out.
                            if (!wait.WaitForExit(10000))
                                return;
                        }
                    }
                    catch (ArgumentException) { } // Process doesn't exist, continue.
                    goto default; // Fall-through.

                default:
                    Run();
                    break;
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
                Spawn(Mode.RESTART);
        }

        // Run WPF application.
        private static void Run()
        {
            App app = new App();
            app.Exit += new ExitEventHandler(RestartExitHandler);
            app.InitializeComponent();
            app.Run();
        }

        // Spawns process.
        private static void Spawn(Mode mode)
        {
            if (mode == Mode.DEFAULT)
                throw new ArgumentException("Cannot spawn process with default mode");

            // Get location of executable and its working directory.
            string location = Assembly.GetExecutingAssembly().Location;
            string workingDir = Path.GetDirectoryName(location);

            Process spawn = new Process();
            spawn.StartInfo.FileName = location;
            spawn.StartInfo.WorkingDirectory = workingDir;
            spawn.StartInfo.CreateNoWindow = true;
            spawn.StartInfo.UseShellExecute = false;
            spawn.StartInfo.EnvironmentVariables[ModeEnvironmentVariableName] = mode.ToString();
            spawn.StartInfo.EnvironmentVariables[PIDEnvironmentVariableName] = Process.GetCurrentProcess().Id.ToString();

            spawn.Start();
        }
    }
}
