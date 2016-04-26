using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.TreeGenerator.Genetic;

namespace UnitTests
{
    [TestClass]
    public class TestGeneticAlgorithmEasy
    {
        [TestMethod]
        public void TestEasyCases()
        {
            // To test the interaction with the skill tree, just test it manually.

            var bitFitness = new[]
            {
                1, 0, 5, 2, 1, 3, 2, 4, 4, 1
            };
            const int perfectBitCount = 3;
            var bestSolution = new BitArray(new []{false, false, true, false, false, false, false, true, true, false});

            var ga = new GeneticAlgorithm(dna =>
            {
                var multiplier = 1.0;
                var bitsSet = 0;
                for (int i = 0; i < dna.Length; i++)
                {
                    if (dna[i])
                    {
                        multiplier *= bitFitness[i];
                        bitsSet++;
                    }
                }
                for (int i = Math.Abs(bitsSet - perfectBitCount); i < 10; i++)
                {
                    multiplier *= 4;
                }
                return multiplier;
            });
            ga.InitializeEvolution(new GeneticAlgorithmParameters(100, 100, 10));
            while (ga.GenerationCount < ga.MaxGeneration)
                ga.NewGeneration();
            var gaBest = ga.GetBestDNA();
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(bestSolution[i], gaBest[i]);
            }
        }
    }
}
