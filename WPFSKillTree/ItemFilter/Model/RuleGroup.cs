using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace POESKillTree.ItemFilter.Model
{
    public class RuleGroup : Block
    {
        public class RuleGroupState
        {
            [XmlAttribute]
            public string Background;

            [XmlAttribute]
            public string Border;

            [XmlAttribute]
            public string Id;

            [XmlAttribute]
            public string Text;

            public RuleGroupState() { }

            public RuleGroupState(RuleGroup group)
            {
                Id = group.Id;

                if (group.BackgroundColor != null)
                    Background = group.BackgroundColor.ToString();
                if (group.BorderColor != null)
                    Border = group.BorderColor.ToString();
                if (group.TextColor != null)
                    Text = group.TextColor.ToString();
            }
        }

        public string Id { get; set; }

        public int Index { get; set; }

        public bool IsExpanded { get; set; }

        public bool IsHidden
        {
            get
            {
                // The group is hidden when there is no rule to be shown.
                return !Rules.Exists(r => r.Show);
            }
        }

        public string Name { get; set; }

        public List<Rule> Rules { get; set; }

        public RuleGroup()
        {
            Show = true;
        }

        /// <summary>
        /// Learns BaseTypes of each rule which subset group's Class match (if it has any).
        /// Teaches rules about group's implicit matches.
        /// </summary>
        public void Learn ()
        {
            if (HasMatches)
            {
                // If group has Class match, look for BaseType matches in its rules and learn them.
                Match clazz = Matches.Find(m => m is MatchClass);
                if (clazz != null)
                {
                    foreach (Rule rule in Rules.FindAll(r => r.HasAnyMatches))
                    {
                        Match baseType = rule.AnyMatches.Find(m => m is MatchBaseType);
                        if (baseType != null) (clazz as MatchClass).Learn(baseType as MatchBaseType);
                    }
                }

                // If group has implicit matches, add them to each rule if they don't have them already.
                foreach (Match match in Matches.FindAll(m => m.IsImplicit()))
                    foreach (Rule rule in Rules.FindAll(r => r.HasAnyMatches))
                        if (!rule.AnyMatches.Exists(m => m.Priority == match.Priority))
                        {
                            if (rule.IsSet)
                            {
                                foreach (List<Match> matches in rule.Set)
                                    matches.Add(match.Clone());
                            }
                            else rule.Matches.Add(match.Clone());
                        }
            }

            // Narrow Class matches with their BaseTypes within single rule (also learn BaseType containement in Class).
            foreach (Rule rule in Rules.FindAll(r => r.HasAnyMatches && r.AnyMatches.Exists(m => m is MatchClass) && r.AnyMatches.Exists(m => m is MatchBaseType)))
            {
                Match clazz = rule.AnyMatches.Find(m => m is MatchClass);
                (clazz as MatchClass).NarrowBy(rule.AnyMatches.Find(m => m is MatchBaseType) as MatchBaseType);
            }
        }

        public void Restore(RuleGroupState state)
        {
            if (state != null)
            {
                BackgroundColor = state.Background;
                BorderColor = state.Border;
                TextColor = state.Text;
            }
        }

        public RuleGroupState Store()
        {
            return new RuleGroupState(this);
        }

        // Return a Block for this group or null if there is no reason to have block for this group.
        public Block ToBlock()
        {
            // Has matches, and either defines colors or is hidden.
            return HasMatches && (HasColors || IsHidden)
                ? new Block(this)
                {
                    DebugOrigin = "@" + Id,
                    OfGroup = this,
                    Show = !IsHidden
                }
                : null;
        }
    }
}
