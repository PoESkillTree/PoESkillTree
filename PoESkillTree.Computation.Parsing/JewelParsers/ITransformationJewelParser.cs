using System.Collections.Generic;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    public interface ITransformationJewelParser
    {
        bool IsTransformationJewelModifier(string jewelModifier);

        IEnumerable<TransformedNodeModifier> ApplyTransformation(
            string jewelModifier, IEnumerable<PassiveNodeDefinition> nodesInRadius);
    }
}