using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class StatFactory : IStatFactory
    {
        public IStat ChanceToDouble(IStat stat) =>
            CopyWithSuffix(stat, nameof(ChanceToDouble), dataType: typeof(int));

        // TODO behaviors
        public IEnumerable<IStat> ConvertTo(IStat sourceStat, IEnumerable<IStat> targetStats)
        {
            foreach (var targetStat in targetStats)
            {
                yield return CopyWithSuffix(sourceStat, $"{nameof(ConvertTo)}({targetStat})",
                    dataType: typeof(int));
            }
            yield return CopyWithSuffix(sourceStat, "Conversion", dataType: typeof(int));
            yield return CopyWithSuffix(sourceStat, "SkillConversion", dataType: typeof(int));
        }
        
        // TODO behaviors
        public IEnumerable<IStat> GainAs(IStat sourceStat, IEnumerable<IStat> targetStats)
        {
            foreach (var targetStat in targetStats)
            {
                yield return CopyWithSuffix(sourceStat, $"{nameof(GainAs)}({targetStat})", dataType: typeof(int));
            }
        }

        private static IStat CopyWithSuffix(
            IStat source, string identitySuffix, bool isRegisteredExplicitly = false, Type dataType = null,
            IReadOnlyCollection<Behavior> behaviors = null)
        {
            return new Stat(source.Identity + "." + identitySuffix, source.Entity, isRegisteredExplicitly,
                dataType ?? source.DataType, behaviors);
        }
    }
}