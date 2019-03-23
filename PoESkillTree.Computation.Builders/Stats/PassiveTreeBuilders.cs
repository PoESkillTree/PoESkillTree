using System.Linq;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Builders.Stats
{
    public class PassiveTreeBuilders : StatBuildersBase, IPassiveTreeBuilders
    {
        private readonly PassiveTreeDefinition _tree;

        public PassiveTreeBuilders(IStatFactory statFactory, PassiveTreeDefinition tree) : base(statFactory)
        {
            _tree = tree;
        }

        public IStatBuilder NodeSkilled(ushort nodeId) => FromIdentity($"{nodeId}.Skilled", typeof(bool));

        public ValueBuilder TotalInModifierSourceJewelRadius(IStatBuilder stat)
            => new ValueBuilder(new ValueBuilderImpl(
                ps => BuildTotalInModifierSourceJewelRadiusValue(ps, stat),
                c => TotalInModifierSourceJewelRadius(stat.Resolve(c))));

        private IValue BuildTotalInModifierSourceJewelRadiusValue(BuildParameters parameters, IStatBuilder stat)
        {
            var modifierSource = GetJewelSource(parameters);
            return _tree.GetNodesInRadius(modifierSource.PassiveNodeId, modifierSource.Radius)
                .Select(d => stat.AsPassiveNodePropertyFor(d.Id))
                .Select(b => b.Value)
                .Aggregate((l, r) => l + r)
                .Build(parameters);
        }

        private static ModifierSource.Local.Jewel GetJewelSource(BuildParameters parameters)
        {
            var modifierSource = parameters.ModifierSource;
            if (modifierSource is ModifierSource.Global globalSource)
                modifierSource = globalSource.LocalSource;
            if (modifierSource is ModifierSource.Local.Jewel jewelSource)
                return jewelSource;
            throw new ParseException(
                "IPassiveTreeBuilders.TotalInModifierSourceJewelRadius can only be used with a source of type ModifierSource.Local.Jewel");
        }
    }
}