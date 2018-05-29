using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="IValue"/> for <see cref="NodeType.Base"/> on conversion paths.
    /// </summary>
    public class ConvertedBaseValue : IValue
    {
        private readonly PathDefinition _path;

        public ConvertedBaseValue(PathDefinition path)
        {
            _path = path;
        }

        public NodeValue? Calculate(IValueCalculationContext valueCalculationContext)
        {
            var path = new PathDefinition(_path.ModifierSource, _path.ConversionStats.Skip(1).ToArray());
            return valueCalculationContext.GetValue(_path.ConversionStats[0], NodeType.Base, path);
        }
    }
}