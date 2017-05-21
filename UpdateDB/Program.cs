using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using log4net.Core;

namespace UpdateDB
{
    /// <summary>
    /// Updates the item database. This includes affixes, base items, gems, item images and tree assets.
    /// What is updated and where it is saved can be set through arguments, see the console output below.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For updating the lists that are version controlled (affixes, base items and gems):
    /// <code>UpdateDB /VersionControlledOnly /SourceCodeDir</code> or run dist-update.bat.
    /// If you only want to update some of these three lists, you can specify them explicitly
    /// (skip the arguments that you don't want):
    /// <code>UpdateDB /Items /Gems /RePoE /SourceCodeDir</code>
    /// </para>
    /// <para>
    /// The other files (base item images and skill tree assets) are not version controlled. They are
    /// packaged into releases via release.xml.
    /// The skill tree assets can be updated through the main program menu. The base item images
    /// can be updated by running dist-updateItemImages.bat. They are written into the main program's Debug directory.
    /// If you are in Release mode, copy them from Debug to Release (Debug/Data/Equipment/Assets/).
    /// </para>
    /// </remarks>
    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        // Main entry point.
        public static int Main(string[] arguments)
        {
            var args = new Arguments
            {
                ActivatedLoaders = LoaderCategories.Any,
                LoaderFlags = new List<string>(),
                OutputDirectory = OutputDirectory.AppData
            };
            var loaderArguments = new List<string>();

            // Get options.
            var unrecognizedSwitches = new List<string>();
            foreach (var arg in arguments)
            {
                if (!arg.StartsWith("/"))
                    continue;

                if (arg.Contains("."))
                {
                    loaderArguments.Add(arg.Substring(1));
                    continue;
                }

                if (arg.StartsWith("/specifieddir:", StringComparison.InvariantCultureIgnoreCase))
                {
                    args.OutputDirectory = OutputDirectory.Specified;
                    args.SpecifiedOutputDirectory = arg.Substring("/specifieddir:".Length);
                    continue;
                }
                switch (arg.ToLowerInvariant())
                {
                    case "/?":
                        Console.WriteLine("Updates item database.\r\n");
                        Console.WriteLine("Flags:\r\n");
                        Console.WriteLine("/VersionControlledOnly    Only download version controlled files (gem, base item and unique lists and RePoE data).");
                        Console.WriteLine("/NotVersionControlledOnly Only download not version controlled files (item images and skill tree assets).");
                        Console.WriteLine("/SourceCodeDir            Save into the WPFSKillTree source code directory instead of the AppData directory.");
                        Console.WriteLine("/CurrentDir               Save into the current directory instead of the AppData directory.");
                        Console.WriteLine("/SpecifiedDir:dirPath     Save into the specified directory instead of the AppData directory.");
                        Console.WriteLine("/Quiet                    Do not display any output.");
                        Console.WriteLine("/Verbose                  Enable verbose output.");
                        Console.WriteLine("/Items, /ItemImages, /TreeAssets, /Gems, /Uniques, /RePoE");
                        Console.WriteLine("If at least one is specified, only the specified files are downloaded.\r\n");
                        return 1;

                    case "/versioncontrolledonly":
                        args.ActivatedLoaders = LoaderCategories.VersionControlled;
                        break;
                    case "/notversioncontrolledonly":
                        args.ActivatedLoaders = LoaderCategories.NotVersionControlled;
                        break;

                    case "/sourcecodedir":
                        args.OutputDirectory = OutputDirectory.SourceCode;
                        break;
                    case "/currentdir":
                        args.OutputDirectory = OutputDirectory.Current;
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
                        unrecognizedSwitches.Add(arg.Substring(1));
                        break;
                }
            }

            var exec = new DataLoaderExecutor(args);

            var nonFlags = unrecognizedSwitches.Where(s => !exec.IsLoaderFlagRecognized(s)).ToList();
            if (nonFlags.Any())
            {
                Log.Error("Invalid switches - \"" + string.Join("\", \"", nonFlags) + "\"");
                return 1;
            }
            if (unrecognizedSwitches.Any())
            {
                args.ActivatedLoaders = LoaderCategories.None;
                args.LoaderFlags = unrecognizedSwitches;
            }

            foreach (var loaderArgument in loaderArguments)
            {
                var split = loaderArgument.Split(new[] {'.', ':'}, 3);
                if (split.Length < 2 || !exec.IsArgumentSupported(split[0], split[1]))
                {
                    Log.Error("Invalid argument - \"" + loaderArgument + "\"");
                    return 1;
                }
                if (split.Length == 2)
                    exec.AddArgument(split[0], split[1]);
                else
                    exec.AddArgument(split[0], split[1], split[2]);
            }

            exec.LoadAllAsync().Wait();
            return 0;
        }


        private class Arguments : IArguments
        {
            public LoaderCategories ActivatedLoaders { get; set; }
            public OutputDirectory OutputDirectory { get; set; }
            public string SpecifiedOutputDirectory { get; set; }
            public IEnumerable<string> LoaderFlags { get; set; }
        }
    }
}
