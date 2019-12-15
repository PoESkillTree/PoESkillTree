using System;
using System.Linq;
using System.Threading.Tasks;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Model
{
    public class AttributesInJewelRadiusCalculator
    {
        private readonly PassiveTreeDefinition _tree;
        private readonly IBuilderFactories _builders;
        private readonly ObservableCalculator _calculator;

        private readonly Lazy<IStatBuilder> _strengthStatBuilder;
        private readonly Lazy<IStatBuilder> _dexterityStatBuilder;
        private readonly Lazy<IStatBuilder> _intelligenceStatBuilder;

        public AttributesInJewelRadiusCalculator(PassiveTreeDefinition tree, IBuilderFactories builders, ObservableCalculator calculator)
        {
            _tree = tree;
            _builders = builders;
            _calculator = calculator;

            _strengthStatBuilder = new Lazy<IStatBuilder>(() => _builders.StatBuilders.Attribute.Strength);
            _dexterityStatBuilder = new Lazy<IStatBuilder>(() => _builders.StatBuilders.Attribute.Dexterity);
            _intelligenceStatBuilder = new Lazy<IStatBuilder>(() => _builders.StatBuilders.Attribute.Intelligence);
        }

        public async Task<AttributeValues> GetAttributesInRadiusAsync(ushort node, JewelRadius radius)
        {
            var values = _tree.GetNodesInRadius(node, radius)
                .Select(d => d.Id)
                .Select(GetAttributeValuesAsync);
            return (await Task.WhenAll(values))
                .Aggregate((l, r) => l + r);
        }

        private async Task<AttributeValues> GetAttributeValuesAsync(ushort node)
        {
            var nodeSkilled = await GetNodeValueAsync(_builders.PassiveTreeBuilders.NodeSkilled(node));
            if (nodeSkilled.IsTrue())
            {
                return new AttributeValues(
                    await GetSingleValueOrDefaultAsync(_strengthStatBuilder.Value.AsPassiveNodePropertyFor(node)),
                    await GetSingleValueOrDefaultAsync(_dexterityStatBuilder.Value.AsPassiveNodePropertyFor(node)),
                    await GetSingleValueOrDefaultAsync(_intelligenceStatBuilder.Value.AsPassiveNodePropertyFor(node)));
            }
            else
            {
                return new AttributeValues();
            }
        }

        private async Task<double> GetSingleValueOrDefaultAsync(IStatBuilder stat)
            => (await GetNodeValueAsync(stat)).SingleOrNull() ?? 0;

        private Task<NodeValue?> GetNodeValueAsync(IStatBuilder stat)
            => _calculator.GetNodeValueAsync(stat.BuildToStats(Entity.Character).Single());

        public readonly struct AttributeValues
        {
            public AttributeValues(double strength, double dexterity, double intelligence)
                => (Strength, Dexterity, Intelligence) = (strength, dexterity, intelligence);

            public double Strength { get; }
            public double Dexterity { get; }
            public double Intelligence { get; }

            public void Deconstruct(out double strength, out double dexterity, out double intelligence)
                => (strength, dexterity, intelligence) = (Strength, Dexterity, Intelligence);

            public static AttributeValues operator +(AttributeValues left, AttributeValues right)
                => new AttributeValues(
                    left.Strength + right.Strength,
                    left.Dexterity + right.Dexterity,
                    left.Intelligence + right.Intelligence);
        }
    }
}