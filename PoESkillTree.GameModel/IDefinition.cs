namespace PoESkillTree.GameModel
{
    public interface IDefinition<out T>
    {
        T Id { get; }
    }
}