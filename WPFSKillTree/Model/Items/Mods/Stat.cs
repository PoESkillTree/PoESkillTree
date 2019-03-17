using System;
using System.Diagnostics;
using MB.Algodat;

namespace PoESkillTree.Model.Items.Mods
{
    /// <summary>
    /// A single stat of an <see cref="IMod"/>.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public class Stat
    {
        public string Id { get; }
        public Range<int> Range { get; }

        public Stat(JsonStat jsonStat)
            : this(jsonStat.Id, jsonStat.Min, jsonStat.Max)
        {
        }

        public Stat(string id, int from, int to)
        {
            Id = id;
            Range = new Range<int>(Math.Min(from, to), Math.Max(from, to));
        }
    }
}