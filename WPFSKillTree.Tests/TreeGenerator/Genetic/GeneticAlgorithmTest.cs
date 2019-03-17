using System;
using NUnit.Framework;
using PoESkillTree.TreeGenerator.Genetic;

namespace PoESkillTree.Tests.TreeGenerator.Genetic
{
    [TestFixture]
    public class GeneticAlgorithmTest
    {
        [Test]
        public void TestEasyCases()
        {
            // To test the interaction with the skill tree, just test it manually.

            var bitFitness = new[]
            {
                1, 0, 5, 2, 3, 2, 4, 4
            };
            var dnaLength = bitFitness.Length;
            const int perfectBitCount = 3;
            var bestSolution = new BitArray(new[] { false, false, true, false, false, false, true, true });

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
                for (int i = Math.Abs(bitsSet - perfectBitCount); i < dnaLength; i++)
                {
                    multiplier *= 4;
                }
                return multiplier;
            });

            ga.InitializeEvolution(new GeneticAlgorithmParameters(50, dnaLength));
            while (ga.GenerationCount < 50)
                ga.NewGeneration();
            var gaBest = ga.GetBestDNA();

            Assert.AreEqual(bestSolution, gaBest);
        }
    }
}