using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Data.Base;
using PoESkillTree.Computation.Data.Collections;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Data
{
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation that matches keystones by their name.
    /// </summary>
    public class KeystoneStatMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly IReadOnlyList<PassiveNodeDefinition> _passives;

        public KeystoneStatMatchers(
            IBuilderFactories builderFactories, IMatchContexts matchContexts, IModifierBuilder modifierBuilder,
            IReadOnlyList<PassiveNodeDefinition> passives)
            : base(builderFactories, matchContexts)
            => (_modifierBuilder, _passives) = (modifierBuilder, passives);

        protected override IEnumerable<MatcherData> CreateCollection()
        {
            var collection = new FormAndStatMatcherCollection(_modifierBuilder, ValueFactory);
            foreach (var keystone in _passives.Where(d => d.Type == PassiveNodeType.Keystone))
            {
                collection.Add(keystone.Name.ToLowerInvariant(),
                    TotalOverride, 1, Stat.PassiveNodeSkilled(keystone.Id));
            }
            return collection;
        }
    }
}