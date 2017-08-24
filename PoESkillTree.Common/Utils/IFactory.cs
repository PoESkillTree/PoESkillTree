namespace PoESkillTree.Common.Utils
{
    public interface IFactory<out T>
    {
        T Create();
    }
}