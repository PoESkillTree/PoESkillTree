using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EnumsNET;
using MoreLinq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Parsing;

namespace PoESkillTree.Computation.Console
{
    public static class Program
    {
        /// <summary>
        /// Console program prompting the user to enter commands in a loop. Supports parsing single stat lines
        /// and timing the parsing of many stat lines.
        /// </summary>
        public static void Main(string[] args)
        {
            var compRoot = new CompositionRoot();
            var parser = compRoot.Parser;
            var calculator = Calculator.CreateCalculator();
            calculator.ExplicitlyRegisteredStats.CollectionChanged += ExplicitlyRegisteredStatsOnCollectionChanged;

            System.Console.WriteLine("Enter a stat line to be parsed and added to the Calculator");
            System.Console.WriteLine("- 'exit' quits");
            System.Console.WriteLine("- 'benchmark' runs the parsing benchmark");
            System.Console.WriteLine("- 'add given' adds the given stats to the Calculator.");
            System.Console.WriteLine("- 'listen <stat>', 'query <stat>', 'query mods <stats>' allow querying stat values " +
                                     "and listening to stat value changes.");
            System.Console.Write("> ");
            string statLine;
            while ((statLine = System.Console.ReadLine()) != null)
            {
                switch (statLine)
                {
                    case "exit":
                        return;
                    case "benchmark":
                        Benchmark(parser);
                        break;
                    case "profile":
                        Profile(parser);
                        break;
                    case "add given":
                        AddGivenStats(calculator, compRoot);
                        break;
                    default:
                        if (statLine.StartsWith("listen"))
                            Listen(calculator, parser, statLine.Substring("listen".Length));
                        else if (statLine.StartsWith("query mods"))
                            QueryMods(calculator, parser, statLine.Substring("query mods".Length));
                        else if (statLine.StartsWith("query"))
                            QueryStat(calculator, parser, statLine.Substring("query".Length));
                        else if (TryParse(parser, statLine, out var mods, verbose: true))
                            AddMods(calculator, mods);
                        break;
                }
                System.Console.Write("> ");
            }
        }

        private static void ExplicitlyRegisteredStatsOnCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            System.Console.WriteLine($"ExplicitlyRegisteredStats, {e.Action}");
            if (e.Action != CollectionChangeAction.Refresh)
            {
                var (node, stat) = ((ICalculationNode, IStat)) e.Element;
                System.Console.WriteLine($"  Stat: {stat}");
                switch (stat.ExplicitRegistrationType)
                {
                    case ExplicitRegistrationType.UserSpecifiedValue t:
                        System.Console.WriteLine($"  User specified with default value {t.DefaultValue}");
                        break;
                    case ExplicitRegistrationType.GainOnAction t:
                        System.Console.WriteLine(
                            $"  Gain stat {t.GainedStat} on action {t.Action} by entity {t.ActionEntity}");
                        break;
                }
                System.Console.WriteLine($"  Current value: {node.Value}");
            }
        }

        private static void Listen(ICalculator calculator, IParser parser, string statLine)
        {
            foreach (var stat in ParseStats(parser, statLine))
            {
                var node = calculator.NodeRepository.GetNode(stat);
                node.ValueChanged += (sender, args) => OnValueChanged(stat, node);
                System.Console.WriteLine($"Started listening to {stat} (current value: {node.Value})");
            }

            void OnValueChanged(IStat stat, ICalculationNode node)
            {
                System.Console.WriteLine($"Stat changed: {stat}");
                System.Console.WriteLine($"  Current value: {node.Value}");
            }
        }

        private static void QueryStat(ICalculator calculator, IParser parser, string statLine)
        {
            foreach (var stat in ParseStats(parser, statLine))
            {
                System.Console.WriteLine($"Stat: {stat}");
                var node = calculator.NodeRepository.GetNode(stat);
                System.Console.WriteLine($"  Current value: {node.Value}");
            }
        }

        private static void QueryMods(ICalculator calculator, IParser parser, string statLine)
        {
            foreach (var stat in ParseStats(parser, statLine))
            {
                System.Console.WriteLine($"Stat: {stat}");
                var statNode = calculator.NodeRepository.GetNode(stat);
                System.Console.WriteLine($"  Current value: {statNode.Value}");
                foreach (var form in Enums.GetValues<Form>())
                {
                    System.Console.WriteLine($"  Modifiers of form {form}:");
                    var nodeCollection = calculator.NodeRepository.GetFormNodeCollection(stat, form);
                    foreach (var (_, modifier) in nodeCollection)
                    {
                        System.Console.WriteLine($"    {modifier.Value} (source: {modifier.Source})");
                    }
                }
            }
        }

        private static IEnumerable<IStat> ParseStats(IParser parser, string stat)
        {
            if (TryParse(parser, "1% increased" + stat, out var mods))
            {
                return mods.SelectMany(m => m.Stats);
            }
            return Enumerable.Empty<IStat>();
        }

        /// <summary>
        /// Parses the given stat using the given parser and writes results to the console.
        /// </summary>
        private static bool TryParse(
            IParser parser, string statLine, out IReadOnlyList<Modifier> mods, bool verbose = false)
        {
            try
            {
                var (success, remaining, result) = parser.Parse(statLine);
                if (verbose)
                {
                    System.Console.WriteLine(result?.ToDelimitedString("\n") ?? "null");
                }
                if (success)
                {
                    mods = result;
                    return true;
                }
                System.Console.WriteLine($"Not recognized: '{remaining}' could not be parsed.");
            }
            catch (ParseException e)
            {
                System.Console.WriteLine("Parsing failed: " + e.Message);
            }
            mods = null;
            return false;
        }

        private static void AddGivenStats(ICalculator calculator, CompositionRoot compositionRoot)
        {
            var mods = GivenStatsParser.Parse(compositionRoot.Parser,
                compositionRoot.GivenStats.Append(new EnemyBaseStats()));
            AddMods(calculator, mods);
        }

        private static void AddMods(ICalculator calculator, IEnumerable<Modifier> mods)
        {
            calculator.NewBatchUpdate().AddModifiers(mods).DoUpdate();
        }

        /// <summary>
        /// Reads stat lines from a file, runs them through the parser, times the parsing and writes the timing
        /// results to the console..
        /// </summary>
        private static void Benchmark(IParser parser)
        {
            var stopwatch = Stopwatch.StartNew();
            parser.Parse("Made-up");
            stopwatch.Stop();
            System.Console.WriteLine($"Initialization (parsing 1 made-up stat):\n  {stopwatch.ElapsedMilliseconds} ms");
            stopwatch.Reset();

            var rng = new Random(1);
            var lines = ReadStatLines().OrderBy(e => rng.Next()).ToList();
            var distinct = new HashSet<string>();
            var batchCounter = 1;
            var successCounter = 0;
            var distinctSuccessCounter = 0;

            System.Console.WriteLine(
                "Batch | Total ms | ms/line | ms/distinct | Batch size | #Distinct\n" +
                "=================================================================");
            foreach (var batch in lines.Batch((int) Math.Ceiling(lines.Count / 10.0)))
            {
                var batchStart = stopwatch.ElapsedMilliseconds;
                var batchSize = 0;
                var newLines = 0;

                foreach (var line in batch)
                {
                    stopwatch.Start();
                    var (parsable, _, _) = parser.Parse(line);
                    stopwatch.Stop();
                    batchSize++;
                    if (parsable)
                    {
                        successCounter++;
                    }
                    if (distinct.Add(line))
                    {
                        newLines++;
                        if (parsable)
                        {
                            distinctSuccessCounter++;
                        }
                    }
                }

                var elapsed = stopwatch.ElapsedMilliseconds - batchStart;
                System.Console.WriteLine(
                    $"{batchCounter,5} " +
                    $"| {elapsed,8} " +
                    $"| {(elapsed / (double) batchSize),7:F} " +
                    $"| {(elapsed / (double) newLines),11:F} " +
                    $"| {batchSize,10} " +
                    $"| {newLines,9} ");
                batchCounter++;
            }
            System.Console.WriteLine(
                "Total " +
                $"| {stopwatch.ElapsedMilliseconds,8} " +
                $"| {(stopwatch.ElapsedMilliseconds / (double) lines.Count),7:F} " +
                $"| {(stopwatch.ElapsedMilliseconds / (double) distinct.Count),11:F} " +
                $"| {lines.Count,10} " +
                $"| {distinct.Count,9} ");
            System.Console.WriteLine(
                "Successfully parsed lines:\n" +
                $"  {successCounter}/{lines.Count} ({successCounter * 100.0 / lines.Count:F1}%)\n" +
                "Successfully parsed distinct lines:\n" +
                $"  {distinctSuccessCounter}/{distinct.Count} ({distinctSuccessCounter * 100.0 / distinct.Count:F1}%)");
        }

        /// <summary>
        /// Reads stat lines from a file and runs them through the parser.
        /// </summary>
        /// <remarks>
        /// For CPU profiling without the output overhead of Benchmark()
        /// </remarks>
        private static void Profile(IParser parser)
        {
            foreach (var line in ReadStatLines())
            {
                parser.Parse(line);
            }
        }

        private static IEnumerable<string> ReadStatLines()
            => File.ReadAllLines("Data/AllSkillTreeStatLines.txt").Where(s => !s.StartsWith("//"));
    }


    public static class ParserExtensions
    {
        public static ParseResult Parse(this IParser @this, string stat) =>
            @this.Parse(stat, new ModifierSource.Global(), Entity.Character);
    }
}