using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    [TestFixture]
    public class ValueTransformerTest
    {
        private Mock<IValueTransformable> _transformableMock;
        private IStat _stat;
        private NodeType _nodeType;
        private IValueTransformation _transformation;
        private IEnumerable<IBehavior> _behaviors;

        [SetUp]
        public void SetUp()
        {
            _transformableMock = new Mock<IValueTransformable>();
            _stat = new StatStub();
            _nodeType = NodeType.Base;
            _transformation = Mock.Of<IValueTransformation>();
            var behavior = Mock.Of<IBehavior>(b =>
                b.AffectedStats == new[] { _stat } && b.AffectedNodeTypes == new[] { _nodeType } &&
                b.Transformation == _transformation);
            _behaviors = new[] { behavior };
        }

        [Test]
        public void AddBehaviorsAddsToStoredTransformables()
        {
            var sut = new ValueTransformer();
            sut.AddTransformable(_stat, _nodeType, _transformableMock.Object);

            sut.AddBehaviors(_behaviors);

            _transformableMock.Verify(t => t.Add(_transformation));
        }

        [Test]
        public void AddTransformableAddsStoredTransformations()
        {
            var sut = new ValueTransformer();
            sut.AddBehaviors(_behaviors);
            
            sut.AddTransformable(_stat, _nodeType, _transformableMock.Object);

            _transformableMock.Verify(t => t.Add(_transformation));
        }

        [Test]
        public void RemoveBehaviorsRemovesFromStoredTransformables()
        {
            var sut = new ValueTransformer();
            sut.AddBehaviors(_behaviors);
            sut.AddTransformable(_stat, _nodeType, _transformableMock.Object);

            sut.RemoveBehaviors(_behaviors);

            _transformableMock.Verify(t => t.Remove(_transformation));
        }

        [Test]
        public void AddTransformableDoesNotAddRemovedTransformations()
        {
            var sut = new ValueTransformer();
            sut.AddBehaviors(_behaviors);

            sut.RemoveBehaviors(_behaviors);
            sut.AddTransformable(_stat, _nodeType, _transformableMock.Object);

            _transformableMock.Verify(t => t.Add(_transformation), Times.Never);
        }

        [Test]
        public void RemoveTransformableCallsRemoveAll()
        {
            var sut = new ValueTransformer();
            sut.AddBehaviors(_behaviors);
            sut.AddTransformable(_stat, _nodeType, _transformableMock.Object);

            sut.RemoveTransformable(_stat, _nodeType);

            _transformableMock.Verify(t => t.RemoveAll());
        }

        [Test]
        public void AddBehaviorsDoesNotAddToRemovedTransformables()
        {
            var sut = new ValueTransformer();
            sut.AddTransformable(_stat, _nodeType, _transformableMock.Object);

            sut.RemoveTransformable(_stat, _nodeType);
            sut.AddBehaviors(_behaviors);

            _transformableMock.Verify(t => t.Add(_transformation), Times.Never);
        }
    }
}