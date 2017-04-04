using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.Model.Items.Affixes;

namespace POESKillTree.Model
{
    using CSharpGlobalCode.GlobalCode_ExperimentalCode;
#if (PoESkillTree_UseSmallDec_ForAttributes)
        public class AttributeSet : Dictionary<string, List<SmallDec>>
#else
    public class AttributeSet : Dictionary<string, List<float>>
#endif
    {
        public AttributeSet() { }

        // Initialize from dictionary instance.
#if (PoESkillTree_UseSmallDec_ForAttributes)
        public AttributeSet(Dictionary<string, List<SmallDec>> dict)
        {
            foreach (var attr in dict)
                Add(attr.Key, new List<SmallDec>(attr.Value));
        }
#else
        public AttributeSet(Dictionary<string, List<float>> dict)
        {
            foreach (var attr in dict)
                Add(attr.Key, new List<float>(attr.Value));
        }
#endif

        // Adds attributes.
        // Existing attributes have value increased by value of attribute being added.
        public void Add(AttributeSet add)
        {
            foreach (var attr in add) Add(attr);
        }

        // Adds attribute.
        // Existing attribute has value increased by value of attribute being added.
#if (PoESkillTree_UseSmallDec_ForAttributes)
        public void Add(KeyValuePair<string, List<SmallDec>> attr)
#else
        public void Add(KeyValuePair<string, List<float>> attr)
#endif
        {
            if (ContainsKey(attr.Key))
            {
                if (attr.Value.Count > 0)
                    for (int i = 0; i < attr.Value.Count; ++i)
                        this[attr.Key][i] += attr.Value[i];
            }
            else
#if (PoESkillTree_UseSmallDec_ForAttributes)
                Add(attr.Key, new List<SmallDec>(attr.Value));
#else
                Add(attr.Key, new List<float>(attr.Value));
#endif
        }

#if (PoESkillTree_UseSmallDec_ForAttributes)
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
            {
                List<SmallDec> NewList = new List<SmallDec>();
                foreach(var value in attr.Value)
                {
                    NewList.Add((SmallDec)value);
                }
                Add(attr.Key, NewList);
            }
        }
#endif

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
            {
#if (PoESkillTree_UseSmallDec_ForAttributes && PoESkillTree_UseSmallDec_ForGeneratorBars)
                Add(itemMod.Attribute, new List<SmallDec>(itemMod.Value));
#elif (PoESkillTree_UseSmallDec_ForAttributes)
                List<SmallDec> NewList = new List<SmallDec>();
                foreach (var value in itemMod.Value)
                {
                    NewList.Add((SmallDec)value);
                }
                Add(itemMod.Attribute,	NewList);
#else
                Add(itemMod.Attribute, new List<float>(itemMod.Value));
#endif
            }

        }

        // Returns new copy of this attribute set.
        public AttributeSet Copy()
        {
            AttributeSet copy = new AttributeSet();

            // Values must be instantiated with new.
            foreach (var attr in this)
#if (PoESkillTree_UseSmallDec_ForAttributes)
                copy.Add(attr.Key, new List<SmallDec>(attr.Value));
#else
                copy.Add(attr.Key, new List<float>(attr.Value));
#endif
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
#if (PoESkillTree_UseSmallDec_ForAttributes)
        public void Remove(KeyValuePair<string, List<SmallDec>> attr)
#else
        public void Remove(KeyValuePair<string, List<float>> attr)
#endif
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

#if (PoESkillTree_UseSmallDec_ForAttributes)
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

        // Replaces attribute values with values from specified set.
        public void Replace(AttributeSet attrs)
        {
            foreach (var attr in attrs)
                if (ContainsKey(attr.Key))
                    this[attr.Key] = attr.Value;
                else
                    Add(attr);
        }
    }
}
