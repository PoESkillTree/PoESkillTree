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
    /// <typeparam name="T"></typeparam>
    class WeightedSampling<T>
    {
        /// I would've liked to use something like a Java TreeMap data structure,
        /// but the C# equivalent (SortedDictionary) 1. doesn't accept duplicate
        /// keys and 2. doesn't expose any binary search in the underlying binary
        /// tree.
        List<KeyValuePair<T, double>> entries;
        bool isSorted;
        double totalWeight;

        Random random;

        public WeightedSampling(Random r = null)
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
            entries.Add(new KeyValuePair<T, double>(entry, totalWeight));
            isSorted = false;
        }

        public T Sample()
        {
            double r = random.NextDouble() * totalWeight;

            if (!isSorted)
            {
                entries.Sort(kvpComparer);
                isSorted = true;
            }

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
