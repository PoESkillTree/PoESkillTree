using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MoreLinq;
using PoESkillTree.Computation.Parsing;

namespace PoESkillTree.Computation.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var compositionRoot = new CompositionRoot();
            var parser = compositionRoot.Parser;

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

        private static void Parse(IParser parser, string statLine)
        {
            try
            {
                if (!parser.TryParse(statLine, out var remaining, out var result))
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

        private static void Benchmark(IParser parser)
        {
            var stopwatch = Stopwatch.StartNew();
            parser.TryParse("Made-up", out var _, out var _);
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
                    var parsable = parser.TryParse(line, out var _, out var _);
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

        // For CPU profiling without the output overhead of Benchmark()
        private static void Profile(IParser parser)
        {
            foreach (var line in ReadStatLines())
            {
                parser.TryParse(line, out var _, out var _);
            }
        }

        private static IEnumerable<string> ReadStatLines()
        {
            return File.ReadAllLines("Data/AllSkillTreeStatLines.txt")
                .Where(s => !s.StartsWith("//"));
        }
    }
}
