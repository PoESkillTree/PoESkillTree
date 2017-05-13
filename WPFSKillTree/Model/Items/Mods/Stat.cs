using System;
using System.Diagnostics;
using MB.Algodat;

namespace POESKillTree.Model.Items.Mods
{
    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public class Stat
    {
        public string Id { get; }
        public int Min { get; }
        public int Max { get; }
        public Range<int> Range { get; }

        public Stat(JsonStat jsonStat)
        {
            Id = jsonStat.Id;
            Min = jsonStat.Min;
            Max = jsonStat.Max;
            Range = new Range<int>(Math.Min(Min, Max), Math.Max(Min, Max));
        }
    }
}