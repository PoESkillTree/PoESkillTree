using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace PoESkillTree.GameModel.PassiveTree
{
    [TestFixture]
    public class PassiveTreeDefinitionExtensionsTest
    {
        [TestCase(0, 0, 0)]
        [TestCase(0, 1, 1)]
        [TestCase(0, 1, 3)]
        [TestCase(0, 2, 2)]
        [TestCase(0, 2, 4)]
        [TestCase(2, 1, 2)]
        public void GetMNodesInRadiusReturnsNodeInsideRadius(int nodeId, int radius, int expectedNodeId)
        {
            var tree = CreateTree();
            var node = tree.GetNodeById((ushort) nodeId);
            var expected = tree.GetNodeById((ushort) expectedNodeId);

            var actual = tree.GetNodesInRadius(node, (uint) radius);

            actual.Should().Contain(expected);
        }

        [TestCase(0, 1, 2)]
        [TestCase(0, 1, 4)]
        public void GetNodesInRadiusDoesNotReturnNodeOutsideRadius(int nodeId, int radius, int notExpectedNodeId)
        {
            var tree = CreateTree();
            var node = tree.GetNodeById((ushort) nodeId);
            var notExpected = tree.GetNodeById((ushort) notExpectedNodeId);

            var actual = tree.GetNodesInRadius(node, (uint) radius);

            actual.Should().NotContain(notExpected);
        }

        private static PassiveTreeDefinition CreateTree()
            => new PassiveTreeDefinition(CreateNodes().ToList());

        private static IEnumerable<PassiveNodeDefinition> CreateNodes()
        {
            for (ushort x = 0; x < 3; x++)
            {
                for (ushort y = 0; y < 2; y++)
                {
                    yield return CreateNode((ushort) (x + y * 3), new NodePosition(x, y));
                }
            }
        }

        private static PassiveNodeDefinition CreateNode(ushort id, NodePosition position = default)
            => new PassiveNodeDefinition(id, default, "", false, false,
                0, position, new string[0]);
    }
}