using System;
using System.Linq;
using NUnit.Framework;
using POESKillTree.TreeGenerator.Genetic;

namespace PoESkillTree.Tests.TreeGenerator.Genetic
{
    [TestFixture]
    public class WeightedSamplerTest
    {
        private const int TotalWeight = 38;

        private static readonly int[] Weights =
        {
            1, 0, 5, 10, 3, 2, 1, 7, 4, 5
        };

        private static readonly int[] WeightsAdded =
        {
            1,1,6,16,19,21,22,29,33,38
        };

        private static readonly int[] Sequence =
        {
            21, 19, 7, 10, 26, 21, 30, 29, 35, 35, 0, 38
        };

        private class FixedRandom : Random
        {
            private int _i;

            public override double NextDouble()
            {
                var randomNumber = Sequence[_i++];
                return (WeightsAdded.First(n => n >= randomNumber) - 0.5) / TotalWeight;
            }
        }

        [Test]
        public void RandomSampleTest()
        {
            var sampler = new WeightedSampler<int>(new FixedRandom());
            for (var i = 0; i < Weights.Length; i++)
            {
                sampler.AddEntry(i, Weights[i]);
            }
            foreach (var t in Sequence)
            {
                Assert.AreEqual(WeightsAdded.First(n => n >= t), WeightsAdded[sampler.RandomSample()]);
            }
        }
    }
}