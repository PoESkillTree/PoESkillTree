using System.Collections.Generic;

namespace POESKillTree.Model
{
    public class AttributeSet : Dictionary<string, List<float>>
    {
        public AttributeSet() { }

        // Initialize from dictionary instance.
        public AttributeSet(Dictionary<string, List<float>> dict)
        {
            foreach (var attr in dict)
                Add(attr.Key, new List<float>(attr.Value));
        }

        // Adds attributes.
        // Existing attributes have value increased by value of attribute being added.
        public void Add(AttributeSet add)
        {
            foreach (var attr in add)
                if (ContainsKey(attr.Key))
                {
                    if (attr.Value.Count > 0)
                        for (int i = 0; i < attr.Value.Count; ++i)
                            this[attr.Key][i] += attr.Value[i];
                }
                else
                    Add(attr.Key, new List<float>(attr.Value));
        }

        // Returns new copy of this attribute set.
        public AttributeSet Copy()
        {
            AttributeSet copy = new AttributeSet();

            // Values must be instantiated with new.
            foreach (var attr in this)
                copy.Add(attr.Key, new List<float>(attr.Value));

            return copy;
        }

        // Merges specified attribute set with this one returning new attribute set.
        // Existing attributes have value increased by value of attribute being merged.
        public AttributeSet Merge(AttributeSet merge)
        {
            AttributeSet merged = Copy();

            merged.Add(merge);

            return merged;
        }

    }
}
