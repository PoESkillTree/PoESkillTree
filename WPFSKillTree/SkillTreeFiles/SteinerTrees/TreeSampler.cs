using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    /// <summary>
    ///  Implements a collection of objects, each associated with a score,
    ///  that can be randomly sampled. The probability of a particular object
    ///  being chosen is directly proportional to its score.
    /// </summary>
    /// <typeparam name="EntryType"></typeparam>
    class TreeSampler<EntryType>
    {
        public static int CompareKVP(KeyValuePair<EntryType, double> kvp1, KeyValuePair<EntryType, double> kvp2)
        {
            return kvp1.Value.CompareTo(kvp2.Value);
        }

        // I'd love to use a better data structure. Unfortunately,
        // unlike the java TreeMap, duplicate keys are not allowed and
        // binary search is not directly supported in SortedDictionary<>,
        // which would make the entire code a lot less elegant / more painful.
        List<KeyValuePair<EntryType, double>> entries;
        Boolean isSorted;

        double totalScore;

        Random random;

        public TreeSampler(Random random = null)
        {
            entries = new List<KeyValuePair<EntryType, double>>();
            isSorted = false;
            totalScore = 0;

            if (random == null) this.random = new Random();
            else this.random = random;
        }

        public void AddEntry(EntryType entry, double score)
        {
            if (score < 0)
                throw new Exception("Negative scores are forbidden!");

            totalScore += score;
            entries.Add(new KeyValuePair<EntryType, double>(entry, totalScore));
            isSorted = false;
        }

        public EntryType SampleEntry()
        {
            double r = random.NextDouble() * totalScore;

            if (!isSorted)
            {
                entries.Sort(CompareKVP);
                isSorted = true;
            }

            // Yeah, this is ugly.
            int index = entries.BinarySearch(new KeyValuePair<EntryType,double>(default(EntryType), r));

            if (index < 0) 
                index = ~index;

            return entries[index].Key;
        }

        public void Clear()
        {
            entries.Clear();
            isSorted = false;
            totalScore = 0;
        }
    }
}
