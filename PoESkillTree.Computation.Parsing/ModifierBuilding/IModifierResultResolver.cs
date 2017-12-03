using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Stats;

namespace PoESkillTree.Computation.Parsing.ModifierBuilding
{
    public interface IModifierResultResolver
    {
        IModifierResult Resolve(IModifierResult unresolvedResult, ResolveContext context);
        IStatBuilder ResolveToReferencedBuilder(IModifierResult unresolvedResult, ResolveContext context);
    }
}