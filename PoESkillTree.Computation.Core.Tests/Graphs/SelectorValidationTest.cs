using System;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    [TestFixture]
    public class SelectorValidationTest
    {
        [TestCase(NodeType.Total)]
        [TestCase(NodeType.Subtotal)]
        [TestCase(NodeType.UncappedSubtotal)]
        [TestCase(NodeType.TotalOverride)]
        public void NodeSelectorThrows(NodeType nodeType)
        {
            Assert.Throws<ArgumentException>(() => new NodeSelector(nodeType, NotMainPath));
        }

        [TestCase(NodeType.Total, true)]
        [TestCase(NodeType.More, false)]
        public void NodeSelectorDoesNotThrow(NodeType nodeType, bool mainPath)
        {
            var path = mainPath ? PathDefinition.MainPath : NotMainPath;

            Assert.DoesNotThrow(() => new NodeSelector(nodeType, path));
        }

        [TestCase(Form.TotalOverride)]
        public void FormNodeSelectorThrows(Form form)
        {
            Assert.Throws<ArgumentException>(() => new FormNodeSelector(form, NotMainPath));
        }

        [TestCase(Form.TotalOverride, true)]
        [TestCase(Form.More, false)]
        public void FormNodeSelectorDoesNotThrow(Form form, bool mainPath)
        {
            var path = mainPath ? PathDefinition.MainPath : NotMainPath;

            Assert.DoesNotThrow(() => new FormNodeSelector(form, path));
        }

        private static readonly PathDefinition NotMainPath = NodeHelper.NotMainPath;
    }
}