namespace PoESkillTree.Computation.Common
{
    public interface IBehaviorPathRule
    {        
        /// <summary>
        /// True if the associated behavior affects the given <see cref="PathDefinition"/>.
        /// </summary>
        bool AffectsPath(PathDefinition path);
    }
}