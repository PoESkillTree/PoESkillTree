using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface IModifierCollection
    {
        void AddModifier(IStat stat, Modifier modifier);
        bool RemoveModifier(IStat stat, Modifier modifier);
    }
}