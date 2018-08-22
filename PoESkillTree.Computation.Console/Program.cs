using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EnumsNET;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using MoreLinq;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Parsing;

namespace PoESkillTree.Computation.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SetupLogger();
            var program = new Program();
            program.Loop();
        }

        private readonly CompositionRoot _compositionRoot;
        private readonly ICoreParser _parser;
        private readonly ICalculator _calculator;

        private Program()
        {
            _compositionRoot = new CompositionRoot();
            _parser = _compositionRoot.CoreParser;
            _calculator = Calculator.CreateCalculator();
            _calculator.ExplicitlyRegisteredStats.CollectionChanged += ExplicitlyRegisteredStatsOnCollectionChanged;
        }

        private void Loop()
        {
            System.Console.WriteLine("Enter a stat line to be parsed and added to the Calculator");
            System.Console.WriteLine("- 'exit' quits");
            System.Console.WriteLine("- 'benchmark' runs the parsing benchmark");
            System.Console.WriteLine("- 'add given' adds the given stats to the Calculator");
            System.Console.WriteLine("- 'listen <stat>', 'query <stat>', 'query mods <stats>' allow querying stat " +
                                     "values and listening to stat value changes");
            System.Console.WriteLine("- 'add <value> <stat>' to BaseAdd to stats");
            System.Console.Write("> ");
            string statLine;
            while ((statLine = System.Console.ReadLine()) != null)
            {
                switch (statLine)
                {
                    case "exit":
                        return;
                    case "benchmark":
                        Benchmark(_parser);
                        break;
                    case "profile":
                        Profile(_parser);
                        break;
                    case "add given":
                        AddGivenStats();
                        break;
                    case "update SkillTreeStatLines":
                        StatLinesUpdater.UpdateSkillTreeStatLines();
                        break;
                    default:
                        if (statLine.StartsWith("listen"))
                            Listen(statLine.Substring("listen".Length));
                        else if (statLine.StartsWith("query mods"))
                            QueryMods(statLine.Substring("query mods".Length));
                        else if (statLine.StartsWith("query"))
                            QueryStat(statLine.Substring("query".Length));
                        else if (statLine.StartsWith("add "))
                            SetStat(statLine.Substring("add ".Length));
                        else if (TryParse(statLine, out var mods, verbose: true))
                            AddMods(mods);
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

        private void Listen(string statLine)
        {
            foreach (var stat in ParseStats(statLine))
            {
                var node = _calculator.NodeRepository.GetNode(stat);
                node.ValueChanged += (sender, args) => OnValueChanged(stat, node);
                System.Console.WriteLine($"Started listening to {stat} (current value: {node.Value})");
            }

            void OnValueChanged(IStat stat, ICalculationNode node)
            {
                System.Console.WriteLine($"Stat changed: {stat}");
                System.Console.WriteLine($"  Current value: {node.Value}");
            }
        }

        private void QueryStat(string statLine)
        {
            foreach (var stat in ParseStats(statLine))
            {
                System.Console.WriteLine($"Stat: {stat}");
                var node = _calculator.NodeRepository.GetNode(stat);
                System.Console.WriteLine($"  Current value: {node.Value}");
            }
        }

        private void QueryMods(string statLine)
        {
            foreach (var stat in ParseStats(statLine))
            {
                System.Console.WriteLine($"Stat: {stat}");
                var statNode = _calculator.NodeRepository.GetNode(stat);
                System.Console.WriteLine($"  Current value: {statNode.Value}");
                foreach (var form in Enums.GetValues<Form>())
                {
                    System.Console.WriteLine($"  Modifiers of form {form}:");
                    var nodeCollection = _calculator.NodeRepository.GetFormNodeCollection(stat, form);
                    foreach (var (_, modifier) in nodeCollection)
                    {
                        System.Console.WriteLine($"    {modifier.Value} (source: {modifier.Source})");
                    }
                }
            }
        }

        private void SetStat(string statLine)
        {
            statLine = statLine.Trim();
            var valuePart = statLine.Split(' ')[0];
            var statPart = statLine.Substring(valuePart.Length);
            var stats = ParseStats(statPart).ToList();
            if (!double.TryParse(valuePart, out var value))
            {
                System.Console.WriteLine($"Could not parse {valuePart}");
                return;
            }
            var mod = new Modifier(stats, Form.BaseAdd, new Constant(value), new ModifierSource.Global());
            System.Console.WriteLine(mod);
            AddMods(new[] { mod });
        }

        private IEnumerable<IStat> ParseStats(string stat)
        {
            if (TryParseMetaStat(stat, out var stats))
            {
                return stats;
            }
            if (TryParse("1% increased" + stat, out var mods))
            {
                return mods.SelectMany(m => m.Stats);
            }
            return new[] { new Stat(stat.Trim()), };
        }

        private bool TryParseMetaStat(string stat, out IEnumerable<IStat> parsedStats)
        {
            var metaStats = _compositionRoot.MetaStats;
            switch (stat.ToLowerInvariant().Trim())
            {
                case "level":
                    parsedStats = Build(_compositionRoot.BuilderFactories.StatBuilders.Level);
                    return true;
                case "skill hit dps":
                    parsedStats = Build(metaStats.SkillDpsWithHits);
                    return true;
                case "skill dot dps":
                    parsedStats = Build(metaStats.SkillDpsWithDoTs);
                    return true;
                case "ignite dps":
                    parsedStats = Build(metaStats.AilmentDps(Ailment.Ignite));
                    return true;
                case "bleed dps":
                    parsedStats = Build(metaStats.AilmentDps(Ailment.Bleed));
                    return true;
                case "poison dps":
                    parsedStats = Build(metaStats.AilmentDps(Ailment.Poison));
                    return true;
                case "cast rate":
                    parsedStats = Build(metaStats.CastRate);
                    return true;
                case "chance to hit":
                    parsedStats = Build(_compositionRoot.BuilderFactories.StatBuilders.ChanceToHit);
                    return true;
                case "hit damage source":
                    parsedStats = Build(metaStats.SkillHitDamageSource);
                    return true;
                case "uses main hand":
                    parsedStats = Build(metaStats.SkillUsesHand(AttackDamageHand.MainHand));
                    return true;
                case "uses off hand":
                    parsedStats = Build(metaStats.SkillUsesHand(AttackDamageHand.OffHand));
                    return true;
                default:
                    parsedStats = null;
                    return false;
            }

            IEnumerable<IStat> Build(IStatBuilder builder)
                => builder.Build(default).SelectMany(r => r.Stats);
        }

        /// <summary>
        /// Parses the given stat using the given parser and writes results to the console.
        /// </summary>
        private bool TryParse(string statLine, out IReadOnlyList<Modifier> mods, bool verbose = false)
        {
            var result = _parser.Parse(statLine);
            if (!result.SuccessfullyParsed)
            {
                System.Console.WriteLine($"Not recognized: '{result.RemainingSubstrings[0]}' could not be parsed.");
                mods = null;
                return false;
            }
            if (verbose)
            {
                System.Console.WriteLine(result.Modifiers.ToDelimitedString("\n") ?? "null");
            }
            mods = result.Modifiers;
            return true;
        }

        private void AddGivenStats()
        {
            var mods = GivenStatsParser.Parse(_parser, _compositionRoot.GivenStats.Result);
            AddMods(mods);
        }

        private void AddMods(IEnumerable<Modifier> mods)
        {
            _calculator.NewBatchUpdate().AddModifiers(mods).DoUpdate();
        }

        /// <summary>
        /// Reads stat lines from a file, runs them through the parser, times the parsing and writes the timing
        /// results to the console..
        /// </summary>
        private static void Benchmark(ICoreParser parser)
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
                    var parseable = parser.Parse(line).SuccessfullyParsed;
                    stopwatch.Stop();
                    batchSize++;
                    if (parseable)
                    {
                        successCounter++;
                    }
                    if (distinct.Add(line))
                    {
                        newLines++;
                        if (parseable)
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
        private static void Profile(ICoreParser parser)
        {
            foreach (var line in ReadStatLines())
            {
                parser.Parse(line);
            }
        }

        private static IEnumerable<string> ReadStatLines()
            => File.ReadAllLines("Data/SkillTreeStatLines.txt").Where(s => !s.StartsWith("//"));

        private static void SetupLogger()
        {
            var appender = new ConsoleAppender();
            var patternLayout = new PatternLayout { ConversionPattern = "%m%n" };
            patternLayout.ActivateOptions();
            appender.Layout = patternLayout;

            var hierarchy = (Hierarchy) LogManager.GetRepository();
            hierarchy.Root.AddAppender(appender);
            hierarchy.Configured = true;
        }
    }


    public static class ParserExtensions
    {
        public static ParseResult Parse(this ICoreParser @this, string stat) =>
            @this.Parse(stat, new ModifierSource.Global(), Entity.Character);
    }
}