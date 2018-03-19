using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    public interface IValueTransformable
    {
        void Add(IValueTransformation transformation);

        void Remove(IValueTransformation transformation);

        void RemoveAll();
    }
}