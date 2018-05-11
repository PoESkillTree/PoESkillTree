using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Graphs;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    [TestFixture]
    public class SelectorValidatingStatGraphTest
    {
        [Test]
        public void SutIsStatGraph()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<IStatGraph>(sut);
        }
        
        [TestCase(NodeType.Total)]
        [TestCase(NodeType.Subtotal)]
        [TestCase(NodeType.UncappedSubtotal)]
        [TestCase(NodeType.TotalOverride)]
        public void GetNodeThrows(NodeType nodeType)
        {
            var sut = CreateSut();
            var selector = new NodeSelector(nodeType, NotMainPath);

            Assert.Throws<ArgumentException>(() => sut.GetNode(selector));
        }

        [TestCase(NodeType.Total, true)]
        [TestCase(NodeType.More, false)]
        public void GetNodeDoesNotThrow(NodeType nodeType, bool mainPath)
        {
            var sut = CreateSut();
            var path = mainPath ? PathDefinition.MainPath : NotMainPath;
            var selector = new NodeSelector(nodeType, path);

            Assert.DoesNotThrow(() => sut.GetNode(selector));
        }

        [TestCase(Form.TotalOverride)]
        public void GetFormNodeCollectionThrows(Form form)
        {
            var sut = CreateSut();
            var selector = new FormNodeSelector(form, NotMainPath);

            Assert.Throws<ArgumentException>(() => sut.GetFormNodeCollection(selector));
        }

        [TestCase(Form.TotalOverride, true)]
        [TestCase(Form.More, false)]
        public void GetFormNodeCollectionDoesNotThrow(Form form, bool mainPath)
        {
            var sut = CreateSut();
            var path = mainPath ? PathDefinition.MainPath : NotMainPath;
            var selector = new FormNodeSelector(form, path);

            Assert.DoesNotThrow(() => sut.GetFormNodeCollection(selector));
        }

        private static SelectorValidatingStatGraph CreateSut() => 
            new SelectorValidatingStatGraph(Mock.Of<IStatGraph>());

        private static readonly PathDefinition NotMainPath =
            new PathDefinition(new GlobalModifierSource(), new StatStub());
    }
}