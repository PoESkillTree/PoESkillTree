using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    /// <summary>
    ///  A generic class storing entries that can be randomly sampled from. The
    ///  probability of a particular entry being picked as a sample is directly
    ///  proportional to its weight.
    /// </summary>
    /// <typeparam name="T">The type of the stored objects.</typeparam>
    class WeightedSampler<T>
    {
        /// The basic idea is to generate the (discrete) cumulative distribution
        /// function (CDF) and then randomly sample from its value range (which
        /// equals the sum of all weights).
        /// 
        /// Note that removing elements is not implemented!
        
        private SortedDictionary<double, T> entries;
        private double totalWeight;

        private Random random;

        /// <summary>
        ///  Indicates whether any entries are present to sample from.
        /// </summary>
        public bool CanSample
        { get { return totalWeight > 0; } }

        /// <summary>
        ///  The number of entries in the sampler.
        /// </summary>
        public int EntryCount
        { get { return entries.Count; } }

        /// <summary>
        ///  A new instance of the WeigtedSampler class.
        /// </summary>
        /// <param name="random">A random number generator. If nothing is passed, a
        /// new one is created.</param>
        public WeightedSampler(Random random = null)
        {
            entries = new SortedDictionary<double, T>();
            totalWeight = 0;

            if (random == null) this.random = new Random();
            else this.random = random;
        }

        /// <summary>
        ///  Adds a new entry with a specified weight to the sampler.
        /// </summary>
        /// <param name="entry">The object that should be randomly selected.</param>
        /// <param name="weight">A value proportional to the object's chance to be
        /// selected.</param>
        public void AddEntry(T entry, double weight)
        {
            if (double.IsInfinity(weight))
                throw new ArgumentException("Infinite weights are not allowed!", "weight");
            if (weight < 0)
                throw new ArgumentException("Negative weights are not allowed!", "weight");

            // No need to sample 0 weight individuals;
            if (weight == 0) return;

            totalWeight += weight;

            // This is where the CDF comes from.
            entries.Add(totalWeight, entry);
        }

        /// <summary>
        ///  Draw a random element from the stored entries. Each element's chance to
        ///  be drawn here is proportional to the weight it was inserted with.
        /// </summary>
        /// <returns>The randomly drawn element.</returns>
        public T RandomSample()
        {
            // Randomly sample the CDF.
            double r = random.NextDouble() * totalWeight;
            var entry = entries.First(kvp => kvp.Key >= r);

            return entry.Value;
        }

        /// <summary>
        ///  Resets the sampler.
        /// </summary>
        public void Clear()
        {
            entries.Clear();
            totalWeight = 0;
        }
    }
}
