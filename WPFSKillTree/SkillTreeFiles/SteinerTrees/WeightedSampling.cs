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
        
        List<KeyValuePair<T, double>> entries;
        bool isSorted;
        double totalWeight;

        Random random;

        public WeightedSampler(Random r = null)
        {
            entries = new List<KeyValuePair<T, double>>();
            isSorted = false;
            totalWeight = 0;

            if (r == null) random = new Random();
            else random = r;
        }

        public void AddEntry(T entry, double weight)
        {
            totalWeight += weight;

            // This is where the CDF comes from.
            entries.Add(new KeyValuePair<T, double>(entry, totalWeight));
            isSorted = false;
        }

        public T Sample()
        {
            if (!isSorted)
            {
                entries.Sort(kvpComparer);
                isSorted = true;
            }

            // Randomly sample the CDF.
            double r = random.NextDouble() * totalWeight;
            int index = entries.BinarySearch(new KeyValuePair<T, double>(default(T), r), kvpComparer);

            return entries[index].Key;
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
            public int Compare(KeyValuePair<T, double> kvp1, KeyValuePair<T, double> kvp2)
            {
                return kvp1.Value.CompareTo(kvp2.Value);
            }
        }
        public static KVPComparer kvpComparer = new KVPComparer();
    }
}
