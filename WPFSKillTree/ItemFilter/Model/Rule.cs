using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace POESKillTree.ItemFilter.Model
{
    public class Rule : Block
    {
        public class RuleState
        {
            [XmlAttribute]
            public string Background;

            [XmlAttribute]
            public string Border;

            [XmlAttribute]
            public bool Checked;

            [XmlAttribute]
            public string Id;

            [XmlAttribute]
            public string Text;

            public RuleState() { }

            public RuleState(Rule rule)
            {
                Id = rule.UniqueId;

                if (rule.BackgroundColor != null)
                    Background = rule.BackgroundColor.ToString();
                if (rule.BorderColor != null)
                    Border = rule.BorderColor.ToString();
                if (rule.TextColor != null)
                    Text = rule.TextColor.ToString();

                Checked = rule.IsChecked;
            }
        }

        public List<Match> AnyMatches
        {
            get
            {
                if (IsSet)
                {
                    List<Match> set = new List<Match>();
                    foreach (List<Match> element in Set) set.AddRange(element);

                    return set;
                }

                return Matches;
            }
        }

        public string Description { get; set; }

        public RuleGroup Group { get; set; }

        public int GroupIndex { get { return Group.Index; } }

        public string GroupName { get { return Group.Name; } }

        public bool HasAnyMatches { get { return HasMatches || IsSet; } }

        public bool HasDescription { get { return !string.IsNullOrEmpty(Description); } }

        public string Id { get; set; }

        public int Index { get; set; }

        public bool IsChecked
        {
            get { return Show; }
            set { Show = value; }
        }

        public bool IsEnabled { get; set; }

        public bool IsSet
        {
            get { return Set != null; }
        }

        public string Name { get; set; }

        public List<Match>[] Set;

        public string UniqueId { get { return Group.Id + "." + Id;  } }

        public Rule()
        {
            IsEnabled = true;
            Show = true;
        }

        public void Restore(RuleState state)
        {
            if (state != null)
            {
                BackgroundColor = state.Background;
                BorderColor = state.Border;
                IsChecked = IsEnabled ? state.Checked : true; // If rule is disabled, it's always checked.
                TextColor = state.Text;
            }
        }

        public RuleState Store()
        {
            return new RuleState(this);
        }

        public Block ToBlock()
        {
            // Emit block only if it has matches defined and its group is either not hidden or it doesn't have matches.
            if (HasMatches && (!Group.IsHidden || !Group.HasMatches))
            {
                // Rule doesn't have colors defined, but its group does. Inherit them.
                if (!HasColors && Group.HasColors)
                    return new Block(this)
                    {
                        DebugOrigin = "#" + Group.Id + "." + Id,
                        OfGroup = Group.HasMatches ? Group : null, // XXX: Don't set OfGroup if Group doesn't have matches.
                        BackgroundColor = Group.BackgroundColor,
                        BorderColor = Group.BorderColor,
                        TextColor = Group.TextColor
                    };
                else
                    return new Block(this)
                    {
                        DebugOrigin = "#" + Group.Id + "." + Id,
                        OfGroup = Group.HasMatches ? Group : null, // XXX: Don't set OfGroup if Group doesn't have matches.
                    };
            }

            // No output.
            return null;
        }

        public List<Block> ToBlocks()
        {
            // Emit blocks only if it has set defined and its group is either not hidden or it doesn't have matches.
            if (IsSet && (!Group.IsHidden || !Group.HasMatches))
            {
                List<Block> blocks = new List<Block>();

                // Emit block for each list of matches in set.
                for (int i = 0; i < Set.Length; ++i)
                {
                    // Match-less block.
                    Block block;

                    // Rule doesn't have colors defined, but its group does. Inherit them.
                    if (!HasColors && Group.HasColors)
                        block = new Block(this)
                        {
                            DebugOrigin = "#" + Group.Id + "." + Id + "[" + i + "]",
                            OfGroup = Group.HasMatches ? Group : null, // XXX: Don't set OfGroup if Group doesn't have matches.
                            BackgroundColor = Group.BackgroundColor,
                            BorderColor = Group.BorderColor,
                            TextColor = Group.TextColor
                        };
                    else
                        block = new Block(this)
                        {
                            DebugOrigin = "#" + Group.Id + "." + Id + "[" + i + "]",
                            OfGroup = Group.HasMatches ? Group : null, // XXX: Don't set OfGroup if Group doesn't have matches.
                        };

                    // Copy matches.
                    block.Matches = new List<Match>(Set[i]);

                    blocks.Add(block);
                }

                return blocks;
            }

            // No output.
            return null;
        }
    }
}
