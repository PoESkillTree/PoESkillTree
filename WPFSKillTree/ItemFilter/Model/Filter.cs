using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using PoESkillTree.Utils.Converter;

namespace PoESkillTree.ItemFilter.Model
{
    public class Filter
    {
        [XmlRoot("Filter")]
        public class FilterState
        {
            [XmlElement("Group")]
            public List<RuleGroup.RuleGroupState> Groups = new List<RuleGroup.RuleGroupState>();

            [XmlElement("Rule")]
            public List<Rule.RuleState> Rules = new List<Rule.RuleState>();

            public FilterState() { }

            public FilterState(Filter filter)
            {
                foreach (RuleGroup group in filter.Groups)
                    Groups.Add(group.Store());

                foreach (Rule rule in filter.Rules)
                    Rules.Add(rule.Store());
            }
        }

        public List<RuleGroup> Groups { get; set; }

        public string Name { get; set; }

        public List<Rule> Rules { get; set; }

        public Filter()
        {
            Rules = new List<Rule>();
        }

        public List<Block> GetBlocks()
        {
            List<Block> blocks = new List<Block>();

            foreach (RuleGroup group in Groups)
            {
                Block block;

                foreach (Rule rule in group.Rules)
                {
                    if (rule.IsSet)
                    {
                        List<Block> blockSet = rule.ToBlocks();
                        if (blockSet != null) blocks.AddRange(blockSet);
                    }
                    else
                    {
                        block = rule.ToBlock();
                        if (block != null) blocks.Add(block);
                    }
                }

                block = group.ToBlock();
                if (block != null) blocks.Add(block);
            }

            return blocks;
        }

        public void Refresh()
        {
            Rules.Clear();

            int groupIndex = 0, ruleIndex = 0;
            foreach (RuleGroup group in Groups)
            {
                group.Index = groupIndex++;

                foreach (Rule rule in group.Rules)
                {
                    rule.Group = group;
                    rule.Index = ruleIndex++;

                    Rules.Add(rule);
                }
            }
        }

        public void Restore(FilterState state)
        {
            if (state != null)
            {
                foreach (RuleGroup group in Groups)
                    group.Restore(state.Groups.Find(r => r.Id == group.Id));

                foreach (Rule rule in Rules)
                    rule.Restore(state.Rules.Find(r => r.Id == rule.UniqueId));
            }
        }

        public FilterState Store()
        {
            return new FilterState(this);
        }
    }
}
