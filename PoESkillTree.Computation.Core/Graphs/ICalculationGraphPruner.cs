namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Prunes an <see cref="ICalculationGraph"/>.
    /// </summary>
    public interface ICalculationGraphPruner
    {
        /// <summary>
        /// Removes all unused nodes in the <see cref="ICalculationGraph"/> of this instance.
        /// </summary>
        void RemoveUnusedNodes();
    }
}