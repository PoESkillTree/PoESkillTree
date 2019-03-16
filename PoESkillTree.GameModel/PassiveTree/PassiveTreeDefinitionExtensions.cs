using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel.PassiveTree
{
    public static class PassiveTreeDefinitionExtensions
    {
        public static IEnumerable<PassiveNodeDefinition> GetNodesInRadius(this PassiveTreeDefinition @this,
            PassiveNodeDefinition node, uint radius)
            => @this.Nodes.Where(n => Distance(n.Position, node.Position) <= radius);

        private static double Distance(NodePosition a, NodePosition b)
        {
            var xDistance = a.X - b.X;
            var yDistance = a.Y - b.Y;
            return Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
        }
    }
}