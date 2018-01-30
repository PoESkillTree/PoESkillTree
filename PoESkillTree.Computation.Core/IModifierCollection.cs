using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core
{
    public interface IModifierCollection
    {
        void AddModifier(Modifier modifier);
        void RemoveModifier(Modifier modifier);
    }
}