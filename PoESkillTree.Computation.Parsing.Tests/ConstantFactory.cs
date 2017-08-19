using PoESkillTree.Common;

namespace PoESkillTree.Computation.Parsing.Tests
{
    internal class ConstantFactory<T> : IFactory<T>
    {
        private readonly T _obj;

        public ConstantFactory(T obj)
        {
            _obj = obj;
        }

        public T Create()
        {
            return _obj;
        }
    }
}