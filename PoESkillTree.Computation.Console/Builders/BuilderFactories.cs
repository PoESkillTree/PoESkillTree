using PoESkillTree.Computation.Common.Builders.Buffs;
using PoESkillTree.Computation.Common.Builders.Charges;
using PoESkillTree.Computation.Common.Builders.Skills;

namespace PoESkillTree.Computation.Console.Builders
{
    /*
     * This namespace contains stub implementations of the interfaces in PoESkillTree.Computation.Parsing.Builders.
     *
     * The implementations are really simple, they just build a string representing the called method/property chain.
     *
     * The fact that implementations must support resolving of references and values (see IResolvable<T>) and that
     * the things to resolve are generally at the start of method chains, requires all implementations to pass
     * the resolve calls up and build another string after resolving. This complicates things a little, but the
     * details are hidden in the utility functions provided by BuilderFactory. Except for some IActionBuilder creations,
     * as it has additional parameters with its Source and Target.
     *
     * MatchContextStub and ReferenceConverterStub are the classes where resolving is not just passing the call on.
     */

    public class BuilderFactories : Computation.Builders.BuilderFactories
    {
        public override IBuffBuilders BuffBuilders => new BuffBuildersStub();

        public override IChargeTypeBuilders ChargeTypeBuilders => new ChargeTypeBuildersStub();

        public override ISkillBuilders SkillBuilders => new SkillBuildersStub();
    }
}