using System.Collections.Generic;
using Mod = POESKillTree.ViewModels.ItemAttributes.Item.Mod;

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
            foreach (var attr in add) Add(attr);
        }

        // Adds attribute.
        // Existing attribute has value increased by value of attribute being added.
        public void Add(KeyValuePair<string, List<float>> attr)
        {
            if (ContainsKey(attr.Key))
            {
                if (attr.Value.Count > 0)
                    for (int i = 0; i < attr.Value.Count; ++i)
                        this[attr.Key][i] += attr.Value[i];
            }
            else
                Add(attr.Key, new List<float>(attr.Value));
        }

        // Adds item mod.
        // Existing attribute has value increased by value of attribute being added.
        public void Add(Mod itemMod)
        {
            if (ContainsKey(itemMod.Attribute))
            {
                if (itemMod.Value.Count > 0)
                    for (int i = 0; i < itemMod.Value.Count; ++i)
                        this[itemMod.Attribute][i] += itemMod.Value[i];
            }
            else
                Add(itemMod.Attribute, new List<float>(itemMod.Value));
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

        // Removes attribute.
        // Attribute has value decreased by value of attribute being removed.
        // If attribute in set has all values zeroes or it has no value at all it will be removed from set.
        public void Remove(KeyValuePair<string, List<float>> attr)
        {
            if (ContainsKey(attr.Key))
            {
                if (attr.Value.Count > 0)
                {
                    for (int i = 0; i < attr.Value.Count; ++i)
                        this[attr.Key][i] -= attr.Value[i];

                    // Remove attribute from set if all values are zeroes.
                    for (int i = 0; i < attr.Value.Count; ++i)
                        if (this[attr.Key][i] != 0) return;
                    Remove(attr.Key);
                }
                else // Remove from set if it has no values.
                    Remove(attr.Key);
            }
        }
    }
}
