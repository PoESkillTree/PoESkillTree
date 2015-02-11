using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    /// <summary>
    ///  A generic collection of objects that can be randomly sampled from. The
    ///  probability of a particular object being picked is directly proportional
    ///  to its weight.
    /// </summary>
    /// <typeparam name="T">The type of the stored objects.</typeparam>
    class WeightedSampler<T>
    {
        /// The basic idea is to generate the (discrete) cumulative distribution
        /// function (CDF) and then randomly sample from its value range (which
        /// equals the sum of all weights). This would be very easy with a binary
        /// search tree structure, with the CDF values as keys and the stored
        /// entries as values.
        /// 
        /// I would've liked to use something like a Java TreeMap data structure,
        /// but the C# equivalent (SortedDictionary) 1. doesn't accept duplicate
        /// keys and 2. doesn't expose any binary search in the underlying binary
        /// tree. So it's pretty much useless and I have to resort to this...
        /// well, ugliness.
        /// If you have a better idea, let me know.
        /// 
        /// 
        /// Note that removing elements is not implemented!
        
        //List<KeyValuePair<T, double>> entries;
        SortedDictionary<double, T> entries;
        bool isSorted;
        double totalWeight;

        public bool CanSample
        { get { return totalWeight > 0; } }

        Random random;

        IOrderedEnumerable<KeyValuePair<T, double>> sortedSet;

        public WeightedSampler(Random random = null)
        {
            entries = new SortedDictionary<double, T>();
            isSorted = false;
            totalWeight = 0;

            if (random == null) this.random = new Random();
            else this.random = random;
        }

        public void AddEntry(T entry, double weight)
        {
            if (weight < 0)
                throw new ArgumentException("Negative weights are not allowed!", "weight");

            // No need to sample 0 weight individuals;
            if (weight == 0) return;

            totalWeight += weight;

            // This is where the CDF comes from.
            entries.Add(totalWeight, entry);
            isSorted = false;
        }

        public T RandomSample()
        {

            // Randomly sample the CDF.
            double r = random.NextDouble() * totalWeight;
            var entry = entries.First(kvp => kvp.Key >= r);

            return entry.Value;
            return default(T);
        }

        public void Clear()
        {
            entries.Clear();
            isSorted = false;
            totalWeight = 0;
        }

        // Yes, this is ugly.
        public class KVPComparer : Comparer<KeyValuePair<T, double>>
        {
            public override int Compare(KeyValuePair<T, double> kvp1, KeyValuePair<T, double> kvp2)
            {
                return kvp1.Value.CompareTo(kvp2.Value);
            }
        }
        public static KVPComparer kvpComparer = new KVPComparer();

        /// <summary>
        ///  Performs a binary search
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /*private int binarySearch(double value)
        {

        }*/

    }
}
