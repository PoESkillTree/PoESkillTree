namespace PoESkillTree.Computation.Core
{
    // This interface's purpose is to make the type more readable in signatures.
    public interface INodeCollection<TProperty> : IObservableCollection<(ICalculationNode node, TProperty property)>
    {
    }
}