using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using PoESkillTree.GameModel;

namespace PoESkillTree.Computation.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            SetupLogger();
            var compositionRoot = new CompositionRoot();
            var program = new Program(compositionRoot);
            await program.LoopAsync();
        }

        private readonly CompositionRoot _compositionRoot;
        private readonly ICalculator _calculator;

        private Program(CompositionRoot compositionRoot)
        {
            _compositionRoot = compositionRoot;
            _calculator = Calculator.Create();
            _calculator.ExplicitlyRegisteredStats.CollectionChanged += ExplicitlyRegisteredStatsOnCollectionChanged;
        }

        private async Task LoopAsync()
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
                        await Benchmark();
                        break;
                    case "profile":
                        await Profile();
                        break;
                    case "add given":
                        await AddGivenStatsAsync();
                        break;
                    case "update SkillTreeStatLines":
                        TestDataUpdater.UpdateSkillTreeStatLines();
                        break;
                    case "update ParseableBaseItems":
                        TestDataUpdater.UpdateParseableBaseItems(await _compositionRoot.GameData.BaseItems);
                        break;
                    default:
                        await HandleParseCommandAsync(statLine);
                        break;
                }
                System.Console.Write("> ");
            }
        }

        private async Task HandleParseCommandAsync(string statLine)
        {
            if (statLine.StartsWith("listen"))
            {
                await ListenAsync(statLine.Substring("listen".Length));
            }
            else if (statLine.StartsWith("query mods"))
            {
                await QueryModsAsync(statLine.Substring("query mods".Length));
            }
            else if (statLine.StartsWith("query"))
            {
                await QueryStatAsync(statLine.Substring("query".Length));
            }
            else if (statLine.StartsWith("add "))
            {
                await SetStatAsync(statLine.Substring("add ".Length));
            }
            else 
            {
                var parser = await _compositionRoot.Parser;
                if (TryParse(parser, statLine, out var mods, verbose: true))
                {
                    AddMods(mods);
                }
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

        private async Task ListenAsync(string statLine)
        {
            foreach (var stat in await ParseStatsAsync(statLine))
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

        private async Task QueryStatAsync(string statLine)
        {
            foreach (var stat in await ParseStatsAsync(statLine))
            {
                System.Console.WriteLine($"Stat: {stat}");
                var node = _calculator.NodeRepository.GetNode(stat);
                System.Console.WriteLine($"  Current value: {node.Value}");
            }
        }

        private async Task QueryModsAsync(string statLine)
        {
            foreach (var stat in await ParseStatsAsync(statLine))
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

        private async Task SetStatAsync(string statLine)
        {
            statLine = statLine.Trim();
            var valuePart = statLine.Split(' ')[0];
            var statPart = statLine.Substring(valuePart.Length);
            var stats = (await ParseStatsAsync(statPart)).ToList();
            if (!double.TryParse(valuePart, out var value))
            {
                System.Console.WriteLine($"Could not parse {valuePart}");
                return;
            }
            var mod = new Modifier(stats, Form.BaseAdd, new Constant(value), new ModifierSource.Global());
            System.Console.WriteLine(mod);
            AddMods(new[] { mod });
        }

        private async Task<IEnumerable<IStat>> ParseStatsAsync(string stat)
        {
            var (isMetaStat, metaStats) = await TryParseMetaStatAsync(stat);
            if (isMetaStat)
            {
                return metaStats;
            }
            var parser = await _compositionRoot.Parser;
            if (TryParse(parser, "1% increased" + stat, out var mods))
            {
                return mods.SelectMany(m => m.Stats);
            }
            return new[] { new Stat(stat.Trim()), };
        }

        private async Task<(bool, IEnumerable<IStat>)> TryParseMetaStatAsync(string stat)
        {
            var builderFactories = await _compositionRoot.BuilderFactories;
            var metaStats = builderFactories.MetaStatBuilders;
            switch (stat.ToLowerInvariant().Trim())
            {
                case "level":
                    return (true, Build(builderFactories.StatBuilders.Level));
                case "skill hit dps":
                    return (true, Build(metaStats.SkillDpsWithHits));
                case "skill dot dps":
                    return (true, Build(metaStats.SkillDpsWithDoTs));
                case "ignite dps":
                    return (true, Build(metaStats.AilmentDps(Ailment.Ignite)));
                case "bleed dps":
                    return (true, Build(metaStats.AilmentDps(Ailment.Bleed)));
                case "poison dps":
                    return (true, Build(metaStats.AilmentDps(Ailment.Poison)));
                case "cast rate":
                    return (true, Build(metaStats.CastRate));
                case "chance to hit":
                    return (true, Build(builderFactories.StatBuilders.ChanceToHit));
                case "hit damage source":
                    return (true, Build(metaStats.SkillHitDamageSource));
                case "uses main hand":
                    return (true, Build(metaStats.SkillUsesHand(AttackDamageHand.MainHand)));
                case "uses off hand":
                    return (true, Build(metaStats.SkillUsesHand(AttackDamageHand.OffHand)));
                default:
                    return (false, null);
            }

            IEnumerable<IStat> Build(IStatBuilder builder)
                => builder.BuildToStats(Entity.Character);
        }

        /// <summary>
        /// Parses the given stat using the given parser and writes results to the console.
        /// </summary>
        private static bool TryParse(
            IParser parser, string statLine, out IReadOnlyList<Modifier> mods, bool verbose = false)
        {
            var result = parser.Parse(statLine);
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

        private async Task AddGivenStatsAsync()
        {
            var parser = await _compositionRoot.Parser;
            AddMods(parser.ParseGivenModifiers());
        }

        private void AddMods(IEnumerable<Modifier> mods)
        {
            _calculator.NewBatchUpdate().AddModifiers(mods).DoUpdate();
        }

        /// <summary>
        /// Reads stat lines from a file, runs them through the parser, times the parsing and writes the timing
        /// results to the console..
        /// </summary>
        private async Task Benchmark()
        {
            var stopwatch = Stopwatch.StartNew();
            var parser = await _compositionRoot.Parser;
            stopwatch.Stop();
            System.Console.WriteLine($"Async parser initialization:\n  {stopwatch.ElapsedMilliseconds} ms");

            stopwatch.Restart();
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
        private async Task Profile()
        {
            var parser = await _compositionRoot.Parser;
            foreach (var line in ReadStatLines())
            {
                parser.Parse(line);
            }
        }

        private static IEnumerable<string> ReadStatLines()
            => File.ReadAllLines("Data/SkillTreeStatLines.txt").Where(s => !s.StartsWith("//"));

        public static void SetupLogger()
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
        public static ParseResult Parse(this IParser @this, string stat) =>
            @this.ParseRawModifier(stat, new ModifierSource.Global(), Entity.Character);
    }
}