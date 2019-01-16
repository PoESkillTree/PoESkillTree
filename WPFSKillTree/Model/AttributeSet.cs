using System.Collections.Generic;

namespace POESKillTree.Model
{
    public class AttributeSet : Dictionary<string, List<float>>
    {
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
    }
}
