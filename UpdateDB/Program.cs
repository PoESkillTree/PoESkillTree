using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POESKillTree.SkillTreeFiles;
using Gem = POESKillTree.SkillTreeFiles.ItemDB.Gem;

namespace UpdateDB
{
    class Program
    {
        // Levels of verbosity.
        enum VerbosityLevel { Normal, Verbose, Quiet };
        // The current level of verbosity.
        static VerbosityLevel Verbosity = VerbosityLevel.Normal;

        // Main entry point.
        static int Main(string[] arguments)
        {
            bool exit = false;
            int argFile = -1;
            int argGem = -1;
            int argMerge = -1;
            string optFileName = null;
            string optGemName = null;
            string optMergeName = null;
            bool assetsDownload = false;
            bool noBackup = false;

            // Get options.
            if (arguments.Length > 0)
            {
                List<string> args = new List<string>(arguments);
                int pos = 0;

                for (var i = 0; i < args.Count && !exit;)
                {
                    string arg = args[i];

                    if (arg.Length == 0) args.RemoveAt(0); // Ignore empty argument.
                    else if (arg[0] == '/') // Decode switch.
                    {
                        switch (arg.ToUpperInvariant())
                        {
                            case "/?":
                                Console.WriteLine("Updates item database.\r\n");
                                Console.WriteLine("UPDATEDB [/F file] [[/G string] | [/M file]] [/N] [/Q | /V]\r\n");
                                Console.WriteLine("UPDATEDB /A\r\n");
                                Console.WriteLine("/A\tDownloads skill tree assets.");
                                Console.WriteLine("/F\tUpdate specified file instead of default file \"Items.xml\".");
                                Console.WriteLine("/G\tUpdate single gem specified by string.");
                                Console.WriteLine("/M\tMerge data of specified file instead of update.");
                                Console.WriteLine("/N\tDoes not create backup of file being updated before writing changes.");
                                Console.WriteLine("/Q\tDoes not display any output.");
                                Console.WriteLine("/V\tEnables verbose output.");
                                exit = true;
                                break;

                            case "/A":
                                assetsDownload = true;
                                break;

                            case "/F":
                                argFile = pos++;
                                break;

                            case "/G":
                                argGem = pos++;
                                break;

                            case "/M":
                                argMerge = pos++;
                                break;

                            case "/N":
                                noBackup = true;
                                break;

                            case "/Q":
                                Verbosity = VerbosityLevel.Quiet;
                                break;

                            case "/V":
                                Verbosity = VerbosityLevel.Verbose;
                                break;

                            default:
                                Console.WriteLine("Invalid switch - \"" + (arg.Length > 1 ? arg[1].ToString() : "") + "\"");
                                exit = true;
                                break;
                        }

                        args.RemoveAt(i);
                    }
                    else ++i; // Skip non-switch argument.
                }

                // Download assets if requested.
                if (assetsDownload)
                {
                    try
                    {
                        Info("Downloading skill tree assests...");
                        SkillTree.CreateSkillTree();
                        Info("Done.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message);

                        return 1;
                    }

                    return 0;
                }

                // Consume non-switch arguments in order of their switch appearance.
                if (argFile >= 0)
                {
                    if (args.Count < argFile + 1)
                    {
                        Console.WriteLine("Missing name of file to update");
                        exit = true;
                    }
                    else
                        optFileName = args[argFile];
                }
                if (argGem >= 0)
                {
                    if (args.Count < argGem + 1)
                    {
                        Console.WriteLine("Missing gem name");
                        exit = true;
                    }
                    else
                        optGemName = args[argGem];
                }
                if (argMerge >= 0)
                {
                    if (args.Count < argMerge + 1)
                    {
                        Console.WriteLine("Missing name of file to merge");
                        exit = true;
                    }
                    else
                        optMergeName = args[argMerge];
                }
            }
            if (exit) return 1;

            string updateFileName = optFileName == null ? "Items.xml" : optFileName;
            if (!File.Exists(updateFileName))
            {
                Console.WriteLine("File not found: " + updateFileName);

                return 1;
            }
            if (optMergeName != null & !File.Exists(optMergeName))
            {
                Console.WriteLine("File not found: " + optMergeName);

                return 1;
            }

            ItemDB.Load(updateFileName);

            bool modified = false;
            Reader reader = new GamepediaReader();

            if (optMergeName != null)
            {
                ItemDB.Merge(optMergeName);

                modified = true;
            }
            else if (optGemName != null)
            {
                Gem fetched = reader.FetchGem(optGemName);
                if (fetched != null)
                {
                    Gem gem = ItemDB.GetGem(optGemName);
                    if (gem == null)
                        ItemDB.Add(fetched);
                    else
                        gem.Merge(fetched);

                    modified = true;
                }
            }
            else
            {
                foreach (Gem gem in ItemDB.GetAllGems())
                {
                    Gem fetched = reader.FetchGem(gem.Name);
                    if (fetched != null)
                    {
                        gem.Merge(fetched);
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                if (!noBackup)
                    File.Copy(updateFileName, updateFileName + ".bak", true);
                ItemDB.WriteTo(updateFileName);
            }

            return 0;
        }

        // Prints normal message suppressed by /Q option.
        public static void Info(string message)
        {
            if (Verbosity != VerbosityLevel.Quiet)
                Console.WriteLine(message);
        }

        // Prints verbose message if enabled with /V option.
        public static void Verbose(string message)
        {
            if (Verbosity == VerbosityLevel.Verbose)
                Console.WriteLine(message);
        }

        // Prints warning suppressed by /Q option.
        public static void Warning(string message)
        {
            if (Verbosity != VerbosityLevel.Quiet)
                Console.WriteLine("Warning: " + message);
        }
    }
}
