using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Console.Builders;
using PoESkillTree.Computation.Data;
using PoESkillTree.Computation.Data.Steps;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.Data;

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
            var parser = CreateParser();

            System.Console.WriteLine("Enter a stat line to be parsed (or 'benchmark' to time stat parsing)");
            System.Console.Write("> ");
            string statLine;
            while ((statLine = System.Console.ReadLine()) != "")
            {
                switch (statLine)
                {
                    case "benchmark":
                        Benchmark(parser);
                        break;
                    case "profile":
                        Profile(parser);
                        break;
                    default:
                        Parse(parser, statLine);
                        break;
                }
                System.Console.Write("> ");
            }
        }

        public static IParser CreateParser()
        {
            return new Parser<ParsingStep>(CreateParsingData(), new BuilderFactories());
        }

        public static IParsingData<ParsingStep> CreateParsingData()
        {
            return new ParsingData(new BuilderFactories(), new MatchContextsStub(), new SkillMatchers());
        }

        /// <summary>
        /// Parses the given stat using the given parser and writes results to the console.
        /// </summary>
        private static void Parse(IParser parser, string statLine)
        {
            try
            {
                var (success, remaining, result) = parser.Parse(statLine);
                if (!success)
                {
                    System.Console.WriteLine($"Not recognized: '{remaining}' could not be parsed.");
                }
                System.Console.WriteLine(result == null ? "null" : string.Join("\n", result));
            }
            catch (ParseException e)
            {
                System.Console.WriteLine("Parsing failed: " + e.Message);
            }
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
        {
            return File.ReadAllLines("Data/AllSkillTreeStatLines.txt")
                .Where(s => !s.StartsWith("//"));
        }
    }
}
