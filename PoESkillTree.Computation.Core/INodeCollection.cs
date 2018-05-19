namespace PoESkillTree.Computation.Core
{
    /// <summary>
    /// An <see cref="IObservableCollection{T}"/> of tuples of <see cref="ICalculationNode"/> and
    /// <see cref="TProperty"/>. This interface's purpose is to make the type more readable in signatures.
    /// </summary>
    /// <typeparam name="TProperty"></typeparam>
    public interface INodeCollection<TProperty> : IObservableCollection<(ICalculationNode node, TProperty property)>
    {
    }
}