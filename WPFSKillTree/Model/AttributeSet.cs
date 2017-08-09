using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.Model.Items.Mods;


namespace POESKillTree.Model
{
#if (PoESkillTree_UseSmallDec_ForAttributes)
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
#endif
    using SmallDigit =
#if (PoESkillTree_UseSmallDec_ForAttributes)
    SmallDec;
#else
    System.Single;
#endif
    public class AttributeSet : Dictionary<string, List<SmallDigit>>
    {
        public AttributeSet() { }

        // Initialize from dictionary instance.
        public AttributeSet(Dictionary<string, List<SmallDigit>> dict)
        {
            foreach (var attr in dict)
                Add(attr.Key, new List<SmallDigit>(attr.Value));
        }

        // Adds attributes.
        // Existing attributes have value increased by value of attribute being added.
        public void Add(AttributeSet add)
        {
            foreach (var attr in add) Add(attr);
        }

        // Adds attribute.
        // Existing attribute has value increased by value of attribute being added.
        public void Add(KeyValuePair<string, List<SmallDigit>> attr)
        {
            if (ContainsKey(attr.Key))
            {
                if (attr.Value.Count > 0)
                    for (int i = 0; i < attr.Value.Count; ++i)
                        this[attr.Key][i] += attr.Value[i];
            }
            else
                Add(attr.Key, new List<SmallDigit>(attr.Value));
        }

        // Adds item mod.
        // Existing attribute has value increased by value of attribute being added.
        public void Add(ItemMod itemMod)
        {
            if (ContainsKey(itemMod.Attribute))
            {
                if (itemMod.Values.Count > 0)
                    for (int i = 0; i < itemMod.Values.Count; ++i)
                        this[itemMod.Attribute][i] += itemMod.Values[i];
            }
            else
                Add(itemMod.Attribute, new List<SmallDigit>(itemMod.Values));
        }

        // Returns new copy of this attribute set.
        public AttributeSet Copy()
        {
            AttributeSet copy = new AttributeSet();

            // Values must be instantiated with new.
            foreach (var attr in this)
                copy.Add(attr.Key, new List<SmallDigit>(attr.Value));

            return copy;
        }

        // Returns attribute set of attributes whose key matches regular expression.
        public AttributeSet Matches(Regex re)
        {
            AttributeSet matches = new AttributeSet();

            foreach (var attr in this)
                if (re.IsMatch(attr.Key))
                    matches.Add(attr);

            return matches;
        }

        // Returns attribute set of attributes whose key matches any of regular expressions passed.
        public AttributeSet MatchesAny(Regex[] rea)
        {
            AttributeSet matches = new AttributeSet();

            foreach (var attr in this)
                foreach (Regex re in rea)
                    if (re.IsMatch(attr.Key))
                        matches.Add(attr);

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
        // If attribute in set has all values zeros or it has no value at all it will be removed from set.
        public void Remove(KeyValuePair<string, List<SmallDigit>> attr)
        {
            if (ContainsKey(attr.Key))
            {
                if (attr.Value.Count > 0)
                {
                    for (int i = 0; i < attr.Value.Count; ++i)
                        this[attr.Key][i] -= attr.Value[i];

                    // Remove attribute from set if all values are zeros.
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
                    Add(attr);
        }
//Support both List<SmallDec> and List<float> at same time 
//(enables having ItemAttributes as stored as float while having player attributes stored as SmallDec)
#if (PoESkillTree_UseSmallDec_ForAttributes)
        // Adds attribute.
        // Existing attribute has value increased by value of attribute being added.
        public void Add(KeyValuePair<string, List<float>> attr)
        {
            if (ContainsKey(attr.Key))
            {
                if (attr.Value.Count > 0)
                    for (int i = 0; i < attr.Value.Count; ++i)
                        this[attr.Key][i] += (SmallDec)attr.Value[i];
            }
            else
            {
                List<SmallDec> NewList = new List<SmallDec>();
                foreach(var value in attr.Value)
                {
                    NewList.Add((SmallDec)value);
                }
                Add(attr.Key, NewList);
            }
        }
        public void Remove(KeyValuePair<string, List<float>> attr)
        {
            if (ContainsKey(attr.Key))
            {
                if (attr.Value.Count > 0)
                {
                    for (int i = 0; i < attr.Value.Count; ++i)
                        this[attr.Key][i] -= attr.Value[i];

                    // Remove attribute from set if all values are zeros.
                    for (int i = 0; i < attr.Value.Count; ++i)
                        if (this[attr.Key][i] != 0) return;
                    Remove(attr.Key);
                }
                else // Remove from set if it has no values.
                    Remove(attr.Key);
            }
        }
#endif
    }
}
