using System;
using System.Diagnostics;
using MB.Algodat;

namespace POESKillTree.Model.Items.Mods
{
    public interface IStat
    {
        string Id { get; }
        Range<int> Range { get; }
    }

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