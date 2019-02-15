namespace PoESkillTree.Computation.Core.Graphs
{
    public class CompositeCalculationGraphPruner : ICalculationGraphPruner
    {
        private readonly ICalculationGraphPruner[] _elements;

        public CompositeCalculationGraphPruner(params ICalculationGraphPruner[] elements)
            => _elements = elements;

        public void RemoveUnusedNodes()
        {
            foreach (var element in _elements)
            {
                element.RemoveUnusedNodes();
            }
        }
    }
}