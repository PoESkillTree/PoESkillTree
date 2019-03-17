using System;

namespace PoESkillTree.ItemFilter.Model
{
    public abstract class Match : IComparable<Match>, IEquatable<Match>
    {
        public const char MultilineMatchDelimiter = '\n';

        [Flags]
        public enum Type
        {
            Class = 1,
            BaseType = 2,
            Rarity = 4,
            Quality = 8,
            ItemLevel = 16,
            DropLevel = 32,
            SocketGroup = 64,
            Sockets = 128,
            LinkedSockets = 256
        }

        public string Keyword;

        public Type Priority;

        protected Match() { }

        protected Match(Match copy)
        {
            Keyword = copy.Keyword;
            Priority = copy.Priority;
        }

        public virtual bool CanMerge(Match match)
        {
            return match is IMergeableMatch && Priority == match.Priority;
        }

        public abstract Match Clone();

        public virtual int CompareTo(Match match)
        {
            if (match == null) return 1;

            return match.Priority.CompareTo(Priority);
        }

        public virtual bool Equals(Match match)
        {
            return match != null && (match as Match).Priority == Priority;
        }

        /// <summary>
        /// Determines whether match is implicit.
        /// Implicit matches are not output to filter (e.g. Quality >= 0).
        /// </summary>
        /// <returns>true if match is implicit, false otherwise.</returns>
        public virtual bool IsImplicit()
        {
            return false;
        }
    }
}
