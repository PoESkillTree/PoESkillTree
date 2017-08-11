using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using POESKillTree.Utils.Converter;

namespace POESKillTree.ItemFilter.Model
{
    public class Block : IComparable<Block>
    {
        public string BackgroundColor { get; set; }

        public string BorderColor { get; set; }

        public string DebugOrigin;

        /// <summary>
        /// The bitwise sum of all contained match types excluding implicit ones.
        /// </summary>
        public int ExplicitPriority
        {
            get
            {
                int pri = 0;

                foreach (Match match in Matches)
                    if (! match.IsImplicit())
                        pri |= (int)match.Priority;

                return pri;
            }
        }

        public bool HasColors { get { return BackgroundColor != null || BorderColor != null || TextColor != null; } }

        public bool HasMatches { get { return Matches != null;  } }
        
        public List<Match> Matches;

        /// <summary>
        /// A RuleGroup from which this block was emitted.
        /// </summary>
        public RuleGroup OfGroup;

        /// <summary>
        /// The bitwise sum of all contained match types.
        /// </summary>
        public int Priority
        {
            get
            {
                int pri = 0;

                foreach (Match match in Matches)
                    pri |= (int)match.Priority;

                return pri;
            }
        }

        public bool Show { get; set; }

        public string TextColor { get; set; }

        public Block() { }

        public Block(Block block)
        {
            DebugOrigin = "#" + block.DebugOrigin;
            Show = block.Show;
            BackgroundColor = block.BackgroundColor;
            TextColor = block.TextColor;
            BorderColor = block.BorderColor;

            if (block.Matches != null)
            {
                Matches = new List<Match>(block.Matches.Count);
                foreach (Match match in block.Matches)
                    Matches.Add(match.Clone());
            }
        }

        public bool CanMerge(Block block)
        {
            // Block can be merged if it has same visual, same number of matches and same priority.
            if (VisualEquals(block)
                && Matches.Count == block.Matches.Count // TODO: ItemLevel >= X && ItemLevel < Y merge with ItemLevel >= Y
                && Priority == block.Priority)
            {
                // All non-mergeable matches must be equal.
                foreach (Match match in Matches.FindAll(m => !(m is IMergeableMatch)))
                {
                    // Find same type matches.
                    List<Match> sameTypes = block.Matches.FindAll(m => m.Priority == match.Priority);
                    if (sameTypes.Count != 1) return false;

                    // Test value equality.
                    if (!match.Equals(sameTypes[0])) return false;
                }

                // Merging is allowed only within one mergeable match type.
                // i.e. There must be only one mergeable match, while rest of mergeable matches must be equal.
                List<Match> mergeables = Matches.FindAll(m => m is IMergeableMatch);
                if (mergeables.Count > 0)
                {
                    foreach (Match match in mergeables)
                    {
                        // Find other corresponding matches.
                        List<Match> sameTypes = block.Matches.FindAll(m => m.Priority == match.Priority);
                        if (sameTypes.Count != 1) return false;

                        // Determine wheter our match can be merged with other corresponding match.
                        if (match.CanMerge(sameTypes[0]))
                        {
                            // Each our leftover mergeable match must be equal to other corresponding leftover match.
                            bool allEquals = true;
                            foreach (Match leftover in mergeables.Except(new List<Match>() { match }))
                            {
                                // Find other leftover corresponding matches.
                                List<Match> others = block.Matches.FindAll(m => m.Priority == leftover.Priority && m != sameTypes[0]);
                                if (others.Count != 1) return false;

                                if (!leftover.Equals(others[0]))
                                {
                                    allEquals = false;
                                    break;
                                }
                            }

                            // All leftover matches are equal to other corresponding leftover matches.
                            if (allEquals) return true;
                        }
                    }

                    // No combination of mergeable matches satisfy condition for mergeability.
                    return false;
                }

                // No mergeable matches and non-mergeable matches are equal.
                return true;
            }

            // Differs in visual, number of matches or type of matches (priority).
            return false;
        }

        // Blocks are sorted from highest priority to lowest.
        // TODO: Fully transitive ordering.
        public int CompareTo(Block block)
        {
            if (block == null) return 1;

            // Any Show block is always in front of any Hide block, unless both blocks are from same rule group.
            // FIXME: Not every block combination is compared. @Flasks and #Flasks.* blocks may never be compared against each other, while they may require specific ordering due to difference in visibility!
            if (Show != block.Show)
                return OfGroup == block.OfGroup && OfGroup != null
                    ? (Show ? 1 : -1)
                    : (Show ? -1 : 1);

            // Comparison by explicit priority.
            int result = block.ExplicitPriority.CompareTo(ExplicitPriority);
            if (result != 0) return result;

            // Compare mergeable matches.
            foreach (Match match in Matches.FindAll(m => m is IMergeableMatch))
            {
                // Find same type matches.
                List<Match> sameTypes = block.Matches.FindAll(m => m.Priority == match.Priority);
                // No matches of same type in other block, thus treat this block as higher priority one.
                if (sameTypes.Count == 0) return -1;
                // Multiple matches of same type in other block, thus treat this block as lower priority one. XXX: Can this actualy happen?
                if (sameTypes.Count > 1) return 1;

                result = match.CompareTo(sameTypes[0]);
                if (result != 0) return result;
            }

            return 0;
        }



        public List<string> GetLines()
        {
            List<string> lines = new List<string> { Show ? "Show" : "Hide" };

            // XXX: Write block comment.
            lines.Insert(0, String.Format("# {0}", DebugOrigin));

            foreach (Match match in Matches)
                if (!match.IsImplicit())
                    foreach (string line in match.ToString().Split(Match.MultilineMatchDelimiter))
                        lines.Add("\t" + line);

            if (Show)
            {
                if (BackgroundColor != null)
                    lines.Add("\tSetBackgroundColor " + ToBlockColor(BackgroundColor));
                if (BorderColor != null)
                    lines.Add("\tSetBorderColor " + ToBlockColor(BorderColor));
                if (TextColor != null)
                    lines.Add("\tSetTextColor " + ToBlockColor(TextColor));
            }

            return lines;
        }

        public void Merge(Block block)
        {
            DebugOrigin = DebugOrigin + " + " + block.DebugOrigin;

            foreach (Match target in Matches.FindAll(m => m is IMergeableMatch))
                foreach (Match mergeable in block.Matches.FindAll(m => target.CanMerge(m)))
                    (target as IMergeableMatch).Merge(mergeable);

            // Once this block is merged with other block of different rule group, remove this block's rule group association.
            if (OfGroup != block.OfGroup) OfGroup = null;
        }

        public bool Subsets(Block block)
        {
            // This priority is "less specific" than the other block's priority, thus this block cannot be subset of the other block.
            if (Priority < block.Priority) return false;

            // All non-mergeable matches must be equal.
            foreach (Match match in Matches.FindAll(m => !(m is IMergeableMatch)))
            {
                // Find same type matches.
                List<Match> sameTypes = block.Matches.FindAll(m => m.Priority == match.Priority);
                if (sameTypes.Count != 1) return false;

                // Test value equality.
                if (!match.Equals(sameTypes[0])) return false;
            }

            // All mergeable matches must subset block mergeable matches.
            foreach (Match match in Matches.FindAll(m => m is IMergeableMatch))
            {
                if (!block.Matches.Exists(m => m is IMergeableMatch && (match as IMergeableMatch).Subsets(m)))
                    return false;
            }

            return true;
        }

        public bool SubsetsExplicitly(Block block)
        {
            // This explicit priority is "less specific" than the other block's explicit priority, thus this block cannot be subset of the other block.
            if (ExplicitPriority < block.ExplicitPriority) return false;

            // All non-mergeable explicit matches must be equal.
            foreach (Match match in Matches.FindAll(m => !(m is IMergeableMatch) && !m.IsImplicit()))
            {
                // Find same type matches.
                List<Match> sameTypes = block.Matches.FindAll(m => m.Priority == match.Priority);
                if (sameTypes.Count != 1) return false;

                // Test value equality.
                if (!match.Equals(sameTypes[0])) return false;
            }

            // All mergeable matches must subset block mergeable matches.
            foreach (Match match in Matches.FindAll(m => m is IMergeableMatch && !m.IsImplicit()))
            {
                if (!block.Matches.Exists(m => m is IMergeableMatch && !m.IsImplicit() && (match as IMergeableMatch).Subsets(m)))
                    return false;
            }

            return true;
        }

        private string ToBlockColor(string color)
        {
            Color c = ColorUtils.FromRgbString(color);

            return c.R + " " + c.G + " " + c.B + (c.A < 255 ? " " + c.A : "");
        }

        public override string ToString()
        {
            return (Show ? "Show " : "Hide ") + (DebugOrigin == null ? "(original)" : DebugOrigin);
        }

        public bool VisualEquals(Block block)
        {
            // Both are either hidden, or define same colors.
            return !Show && !block.Show
                || Show && block.Show && BackgroundColor == block.BackgroundColor && BorderColor == block.BorderColor && TextColor == block.TextColor;
        }
    }
}
