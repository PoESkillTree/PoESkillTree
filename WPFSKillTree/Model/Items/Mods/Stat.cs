using System;
using System.Diagnostics;
using MB.Algodat;

namespace POESKillTree.Model.Items.Mods
{
    /// <summary>
    /// A single stat of a <see cref="IMod"/>
    /// </summary>
    public interface IStat
    {
        /// <summary>
        /// Gets the id of this stat (used for translating stats into strings)
        /// </summary>
        string Id { get; }
        /// <summary>
        /// Gets the range of values this stat can have
        /// </summary>
        Range<int> Range { get; }
    }

    /// <summary>
    /// Implementation of <see cref="IStat"/> based on an encapsulated <see cref="JsonStat"/>
    /// </summary>
    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public class Stat : IStat
    {
        public string Id { get; }
        public Range<int> Range { get; }

        public Stat(JsonStat jsonStat)
        {
            Id = jsonStat.Id;
            var min = jsonStat.Min;
            var max = jsonStat.Max;
            Range = new Range<int>(Math.Min(min, max), Math.Max(min, max));
        }
    }
}