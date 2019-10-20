using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace UpdateDB
{
    /// <summary>
    /// Updates the data necessary for releases. This item images and tree assets.
    /// What is updated and where it is saved can be set through arguments, see the console output below.
    /// </summary>
    /// <remarks>
    /// Base item images and skill tree assets are not version controlled. They are
    /// packaged into releases via release.xml.
    /// The skill tree assets can be updated through the main program menu. The base item images
    /// can be updated by running dist-updateItemImages.bat. They are written into the main program's Debug directory.
    /// If you are in Release mode, copy them from Debug to Release (Debug/Data/Equipment/Assets/).
    /// </remarks>
    public static class Program
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        // Main entry point.
        public static async Task<int> Main(string[] arguments)
        {
            var args = new Arguments
            {
                LoaderFlags = new List<string>(),
                OutputDirectory = OutputDirectory.AppData
            };

            // Get options.
            var unrecognizedSwitches = new List<string>();
            foreach (var arg in arguments)
            {
                if (!arg.StartsWith("/"))
                    continue;

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
                        Console.WriteLine("/CurrentDir               Save into the current directory instead of the AppData directory.");
                        Console.WriteLine("/SpecifiedDir:dirPath     Save into the specified directory instead of the AppData directory.");
                        Console.WriteLine("/ItemImages, /TreeAssets");
                        Console.WriteLine("If at least one is specified, only the specified files are downloaded.\r\n");
                        return 1;

                    case "/currentdir":
                        args.OutputDirectory = OutputDirectory.Current;
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
                args.LoaderFlags = unrecognizedSwitches;
            }

            await exec.LoadAllAsync();
            return 0;
        }


        private class Arguments : IArguments
        {
            public OutputDirectory OutputDirectory { get; set; }
            public string SpecifiedOutputDirectory { get; set; }
            public IEnumerable<string> LoaderFlags { get; set; }
        }
    }
}
