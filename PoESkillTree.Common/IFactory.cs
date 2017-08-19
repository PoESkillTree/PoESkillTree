namespace PoESkillTree.Common
{
    public interface IFactory<out T>
    {
        T Create();
    }
}