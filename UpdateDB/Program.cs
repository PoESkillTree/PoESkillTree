using System;
using log4net;
using log4net.Core;

namespace UpdateDB
{
    public static class Program
    {
        // Main entry point.
        public static int Main(string[] arguments)
        {
            var args = new Arguments
            {
                CreateBackup = true,
                ActivatedLoader = LoaderCategories.Any
            };

            // Get options.
            foreach (var arg in arguments)
            {
                switch (arg.ToLowerInvariant())
                {
                    case "/?":
                        Console.WriteLine("Updates item database.\r\n");
                        Console.WriteLine("Flags:\r\n");
                        Console.WriteLine("/VersionControlledOnly    Only download version controlled files (gem, affix and base item lists).");
                        Console.WriteLine("/NotVersionControlledOnly Only download not version controlled files (item images and skill tree assets).");
                        Console.WriteLine("/SourceCodeDir            Save into the WPFSKillTree source code directory instead of the AppData directory.");
                        Console.WriteLine("/CurrentDir               Save into the current directory instead of the AppData directory.");
                        Console.WriteLine("/NoBackup                 Do not create backup of files being updated before writing changes.");
                        Console.WriteLine("/Quiet                    Do not display any output.");
                        Console.WriteLine("/Verbose                  Enable verbose output.");
                        return 1;

                    case "/versioncontrolledonly":
                        args.ActivatedLoader = LoaderCategories.VersionControlled;
                        break;
                    case "/notversioncontrolledonly":
                        args.ActivatedLoader = LoaderCategories.NotVersionControlled;
                        break;

                    case "/sourcecodedir":
                        args.OutputDirectory = OutputDirectory.SourceCode;
                        break;
                    case "/currentdir":
                        args.OutputDirectory = OutputDirectory.Current;
                        break;

                    case "/nobackup":
                        args.CreateBackup = false;
                        break;

                    case "/quiet":
                        var repo = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
                        repo.Root.Level = Level.Off;
                        repo.RaiseConfigurationChanged(EventArgs.Empty);
                        break;
                    case "/verbose":
                        repo = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
                        repo.Root.Level = Level.Debug;
                        repo.RaiseConfigurationChanged(EventArgs.Empty);
                        break;

                    default:
                        Console.WriteLine("Invalid switch - \"" + (arg.Length > 1 ? arg[1].ToString() : "") + "\"");
                        return 1;
                }
            }

            var exec = new DataLoaderExecutor(args);
            exec.LoadAllAsync().Wait();

            return 0;
        }


        private class Arguments : IArguments
        {
            public LoaderCategories ActivatedLoader { get; set; }
            public OutputDirectory OutputDirectory { get; set; }
            public bool CreateBackup { get; set; }
        }
    }
}
