using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Effects;

namespace PoESkillTree.Computation.Builders.Effects
{
    public class AilmentBuilders : IAilmentBuilders
    {
        private readonly AilmentBuilderCollection _allAilments;

        public AilmentBuilders(IStatFactory statFactory)
        {
            _allAilments = new AilmentBuilderCollection(statFactory, Enums.GetValues<Ailment>().ToList());
            Elemental = new AilmentBuilderCollection(statFactory,
                new[] { Ailment.Ignite, Ailment.Shock, Ailment.Chill, Ailment.Freeze });
        }

        public IAilmentBuilder Ignite => _allAilments[Ailment.Ignite];
        public IAilmentBuilder Shock => _allAilments[Ailment.Shock];
        public IAilmentBuilder Chill => _allAilments[Ailment.Chill];
        public IAilmentBuilder Freeze => _allAilments[Ailment.Freeze];
        public IAilmentBuilder Bleed => _allAilments[Ailment.Bleed];
        public IAilmentBuilder Poison => _allAilments[Ailment.Poison];
        public IAilmentBuilder From(Ailment ailment) => _allAilments[ailment];
        public IAilmentBuilderCollection Elemental { get; }
    }

    internal class AilmentBuilderCollection
        : FixedBuilderCollection<Ailment, IAilmentBuilder>, IAilmentBuilderCollection
    {
        public AilmentBuilderCollection(IStatFactory statFactory, IReadOnlyList<Ailment> keys)
            : base(keys, a => new AilmentBuilder(statFactory, CoreBuilder.Create(a)))
        {
        }
    }
}