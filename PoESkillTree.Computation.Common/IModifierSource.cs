namespace PoESkillTree.Computation.Common
{
    public interface IModifierSource
    {
        // First level: Global, Local
        // Second level: Given, Tree, Skill or Item (maybe more).
        // Third level: item slot for items
        // Also contains information about e.g. tree node names, "Dexterity", item names, ...

        // Only Local modifiers can receive special handling in calculation.
        // Everything else is just to allow more detailed breakdowns.
    }
}