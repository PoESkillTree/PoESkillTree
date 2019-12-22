using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;
using PoESkillTree.Computation.Model;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.ViewModels
{
    public class AttributesInJewelRadiusViewModel
    {
        private static readonly NotifyingTask<double> CompletedTask = new NotifyingTask<double>(Task.FromResult(0D), _ => {});

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private const string UsesAttributesInJewelRadiusRegex =
            "(per|for every) # {0} (allocated|(from|on) allocated passives) in radius";

        private readonly PassiveTreeDefinition _tree;
        private readonly IBuilderFactories _builders;
        private readonly ObservableCalculator _calculator;

        private readonly Lazy<IStatBuilder> _strengthStatBuilder;
        private readonly Lazy<IStatBuilder> _dexterityStatBuilder;
        private readonly Lazy<IStatBuilder> _intelligenceStatBuilder;

        public AttributesInJewelRadiusViewModel(PassiveTreeDefinition tree, IBuilderFactories builders, ObservableCalculator calculator)
        {
            _tree = tree;
            _builders = builders;
            _calculator = calculator;

            _strengthStatBuilder = new Lazy<IStatBuilder>(() => _builders.StatBuilders.Attribute.Strength);
            _dexterityStatBuilder = new Lazy<IStatBuilder>(() => _builders.StatBuilders.Attribute.Dexterity);
            _intelligenceStatBuilder = new Lazy<IStatBuilder>(() => _builders.StatBuilders.Attribute.Intelligence);
        }

        public bool DisplayAttributes => DisplayStrength || DisplayDexterity || DisplayIntelligence;

        public bool DisplayStrength { get; private set; }
        public NotifyingTask<double> Strength { get; private set; } = CompletedTask;
        public bool DisplayDexterity { get; private set; }
        public NotifyingTask<double> Dexterity { get; private set; } = CompletedTask;
        public bool DisplayIntelligence { get; private set; }
        public NotifyingTask<double> Intelligence { get; private set; } = CompletedTask;

        public void Calculate(ushort node, JewelRadius radius, IReadOnlyCollection<string> modifiers)
        {
            DisplayStrength = AnyModifierUsesAttributesInJewelRadius(modifiers, "Strength");
            DisplayDexterity = AnyModifierUsesAttributesInJewelRadius(modifiers, "Dexterity");
            DisplayIntelligence = AnyModifierUsesAttributesInJewelRadius(modifiers, "Intelligence");
            if (!DisplayAttributes)
                return;

            var task = GetAttributesInRadiusAsync(node, radius);
            if (DisplayStrength)
            {
                Strength = new NotifyingTask<double>(GetStrengthAsync(task),
                    e => Log.Error(e, "Calculating Strength in radius failed"));
            }
            if (DisplayDexterity)
            {
                Dexterity = new NotifyingTask<double>(GetDexterityAsync(task),
                    e => Log.Error(e, "Calculating Dexterity in radius failed"));
            }
            if (DisplayIntelligence)
            {
                Intelligence = new NotifyingTask<double>(GetIntelligenceAsync(task),
                    e => Log.Error(e, "Calculating Intelligence in radius failed"));
            }

            static async Task<double> GetStrengthAsync(Task<AttributeValues> t) => (await t).Strength;
            static async Task<double> GetDexterityAsync(Task<AttributeValues> t) => (await t).Dexterity;
            static async Task<double> GetIntelligenceAsync(Task<AttributeValues> t) => (await t).Intelligence;
        }

        private static bool AnyModifierUsesAttributesInJewelRadius(IEnumerable<string> modifiers, string attribute)
            => modifiers.Any(m => Regex.IsMatch(m, string.Format(UsesAttributesInJewelRadiusRegex, attribute), RegexOptions.IgnoreCase));

        private async Task<AttributeValues> GetAttributesInRadiusAsync(ushort node, JewelRadius radius)
        {
            var values = _tree.GetNodesInRadius(node, radius)
                .Select(d => d.Id)
                .Select(GetAttributeValuesAsync);
            return (await Task.WhenAll(values))
                .DefaultIfEmpty()
                .Aggregate((l, r) => l + r);
        }

        private async Task<AttributeValues> GetAttributeValuesAsync(ushort node)
        {
            var nodeSkilled = await GetNodeValueAsync(_builders.PassiveTreeBuilders.NodeAllocated(node));
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

        private readonly struct AttributeValues
        {
            public AttributeValues(double strength, double dexterity, double intelligence)
                => (Strength, Dexterity, Intelligence) = (strength, dexterity, intelligence);

            public double Strength { get; }
            public double Dexterity { get; }
            public double Intelligence { get; }

            public static AttributeValues operator +(AttributeValues left, AttributeValues right)
                => new AttributeValues(
                    left.Strength + right.Strength,
                    left.Dexterity + right.Dexterity,
                    left.Intelligence + right.Intelligence);
        }
    }
}