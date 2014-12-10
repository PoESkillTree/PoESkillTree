using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using POESKillTree.Views;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Taskbar;

namespace POESKillTree.SkillTreeFiles
{
    // Application entry class.
    class Bootstrap : MarshalByRefObject
    {
        // The GUID of "Program Files" known folder not distinguishable from "Program Files (x86)" for 32-bit application.
        private static readonly Guid FOLDERID_ProgramFiles = new Guid("905e63b6-c1bf-494e-b29c-65b732d3d21a");
        // The GUID of "Program Files\Common Files" known folder not distinguishable from "Program Files (x86)\Common Files" for 32-bit application.
        private static readonly Guid FOLDERID_ProgramFilesCommon = new Guid("f7f1ed05-9f6d-47a2-aaae-29d317c6f066");
        // Bootstrap mode.
        private enum Mode
        {
            DEFAULT,     // Default mode.
            SHADOW_COPY, // Spawn executable from shadow copy.
            RESTART      // Spawn original executable.
        }
        // The name of environment variable containing boostrap mode.
        private static readonly string ModeEnvironmentVariableName = "BOOTSTRAP_MODE";
        // The name of environment variable containing PID of original process.
        private static readonly string PIDEnvironmentVariableName = "BOOTSTRAP_PID";
        // The exit code of application restart process.
        private const int RESTART_EXIT_CODE = 42;

        // Returns system-generated application identifier used to group taskbar items.
        private static string GetAppUserModelId(string location)
        {
            string path = Path.GetDirectoryName(location);

            // Iterate through all filesystem-based known folders.
            foreach (IKnownFolder knownFolder in KnownFolders.All)
            {
                if (knownFolder is FileSystemKnownFolder)
                {
                    // Skip non-distinguishable folders for 32-bit application.
                    if (knownFolder.FolderId.Equals(FOLDERID_ProgramFiles)
                        || knownFolder.FolderId.Equals(FOLDERID_ProgramFilesCommon))
                        continue;

                    // If location is inside of known folder, replace its path with GUID.
                    if (path.StartsWith(knownFolder.ParsingName))
                        return Path.Combine("{" + knownFolder.FolderId.ToString().ToUpperInvariant() + "}", Path.GetFileName(location));
                }
            }

            return location;
        }

        // Returns location of original executable when invoked from shadow copy enabled application domain.
        private static string GetOriginalLocation(bool executable = false)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string dir = Path.GetDirectoryName(new Uri(assembly.GetName().EscapedCodeBase).LocalPath);

            // Filename part must be from assembly location's filename (assembly CodeBase has mangled filename extension).
            return executable ? Path.Combine(dir, Path.GetFileName(assembly.Location)) : dir ;
        }

        // Entry point method.
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        public static void Main(string[] arguments)
        {
            // Don't do shadow copying when being debugged in VS.
            if (Debugger.IsAttached)
            {
                App app = new App();
                app.InitializeComponent();
                app.Run();

                return;
            }

            // Parse boostrap mode.
            Mode mode;
            Enum.TryParse<Mode>(Environment.GetEnvironmentVariable(ModeEnvironmentVariableName), false, out mode);
            // Parse original PID.
            int pid;
            int.TryParse(Environment.GetEnvironmentVariable(PIDEnvironmentVariableName), out pid);

            Bootstrap boot;
            AppDomain domain;
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationName = AppDomain.CurrentDomain.FriendlyName;
            setup.ShadowCopyFiles = "true";

            switch (mode)
            {
                case Mode.SHADOW_COPY:
                    // Use current working directory as application base.
                    setup.ApplicationBase = Environment.CurrentDirectory;
                    domain = AppDomain.CreateDomain(AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Evidence, setup);

                    boot = (Bootstrap)domain.CreateInstanceFromAndUnwrap(Path.Combine(Environment.CurrentDirectory, AppDomain.CurrentDomain.FriendlyName), typeof(Bootstrap).FullName);
                    boot.Run();
                    break;

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
                    // Use current domain base directory as application base.
                    setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                    domain = AppDomain.CreateDomain(AppDomain.CurrentDomain.FriendlyName, AppDomain.CurrentDomain.Evidence, setup);

                    boot = (Bootstrap)domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(Bootstrap).FullName);
                    boot.Spawn(Mode.SHADOW_COPY);
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
        private void RestartExitHandler(object sender, ExitEventArgs e)
        {
            if (e.ApplicationExitCode == RESTART_EXIT_CODE)
                Spawn(Mode.RESTART);
        }

        // Run WPF application.
        private void Run()
        {
            // TaskbarManager is supported only on Windows Vista and later.
            if (TaskbarManager.IsPlatformSupported)
            {
                // Set AppUserModelId of process based on location of original executable.
                TaskbarManager.Instance.ApplicationId = GetAppUserModelId(GetOriginalLocation(true));
            }

            App app = new App();
            app.Exit += new ExitEventHandler(RestartExitHandler);
            app.InitializeComponent();
            app.Run();
        }

        // Spawns process.
        private void Spawn(Mode mode)
        {
            if (mode == Mode.DEFAULT)
                throw new ArgumentException("Cannot spawn process with default mode");

            // If spawning shadow copy use shadow copy location, otherwise use original executable location.
            string location = mode == Mode.SHADOW_COPY ? Assembly.GetExecutingAssembly().Location : GetOriginalLocation(true);
            // Get working directory from original location.
            string workingDir = mode == Mode.SHADOW_COPY ? GetOriginalLocation() : Path.GetDirectoryName(location);

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
