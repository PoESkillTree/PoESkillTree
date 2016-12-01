using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.Model.Items.Affixes;
using System.Linq;
using POESKillTree.Utils.Extensions;
using System;

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
                Add(attr.Key, attr.Value);
        }

        // Adds attributes.
        // Existing attributes have value increased by value of attribute being added.
        public void Add(string key, IEnumerable<float> values)
        {
            Add(key, values.ToList());
        }

        public new void Add(string key, List<float> values)
        {
            if (!ContainsKey(key))
                this[key] = values;
            else
            {
                for (int i = 0; i < values.Count; ++i)
                    this[key][i] += values[i];
            }
        }

        public void Add(string key, float val)
        {
            Add(key, new[] { val }.ToList());
        }

        // Adds item mod.
        // Existing attribute has value increased by value of attribute being added.
        public void Add(ItemMod itemMod)
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

        public AttributeSet Matches(string patt)
        {
            return Matches(new Regex(patt));
        }
        // Returns attribute set of attributes whose key matches regular expression.
        public AttributeSet Matches(Regex re)
        {
            AttributeSet matches = new AttributeSet();

            foreach (var attr in this)
                if (re.IsMatch(attr.Key))
                    matches.Add(attr.Key, attr.Value);

            return matches;
        }

        // Returns attribute set of attributes whose key matches any of regular expressions passed.
        public AttributeSet MatchesAny(Regex[] rea)
        {
            AttributeSet matches = new AttributeSet();

            foreach (var attr in this)
                foreach (Regex re in rea)
                    if (re.IsMatch(attr.Key))
                        matches.Add(attr.Key, attr.Value);

            return matches;
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

        // Replaces attribute values with values from specified set.
        public void Replace(AttributeSet attrs)
        {
            foreach (var attr in attrs)
                if (ContainsKey(attr.Key))
                    this[attr.Key] = attr.Value;
                else
                    Add(attr.Key, attr.Value);
        }

        public float GetOrDefault(string key, int index = 0, float def = 0)
        {
            if (this.ContainsKey(key))
                return this[key][index];
            return def;
        }

        public void AddAsSum(string key, float value)
        {
            if (ContainsKey(key))
                this[key][0] += value;
            else
                this[key] = new[] { value }.ToList();
        }
    }
}
