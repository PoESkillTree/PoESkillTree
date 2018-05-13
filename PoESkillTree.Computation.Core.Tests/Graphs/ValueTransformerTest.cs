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
        private IReadOnlyList<Mock<IValueTransformable>> _transformableMocks;
        private IStat _stat;
        private NodeType _nodeType;
        private IReadOnlyList<NodeSelector> _selectors;
        private IReadOnlyList<IValueTransformation> _transformations;
        private IEnumerable<IBehavior> _behaviors;
        private ValueTransformer _sut;

        [SetUp]
        public void SetUp()
        {
            _transformableMocks = new[] { new Mock<IValueTransformable>(), new Mock<IValueTransformable>() };
            _stat = new StatStub();
            _nodeType = NodeType.Base;
            _selectors = new[]
            {
                new NodeSelector(_nodeType, PathDefinition.MainPath),
                new NodeSelector(_nodeType, new PathDefinition(new ModifierSourceStub(), new StatStub())),
            };
            _transformations = Helper.MockMany<IValueTransformation>(3);
            _behaviors = new[]
            {
                // _behaviors[0]/_transformations[0] affects both _selectors/_transformableMocks
                Mock.Of<IBehavior>(b =>
                    b.AffectedStats == new[] { _stat } && b.AffectedNodeTypes == new[] { _nodeType } &&
                    b.AffectedPaths == BehaviorPathInteraction.AllPaths && b.Transformation == _transformations[0]),
                // _behaviors[1]/_transformations[1] only affects _selectors[0]/_transformableMocks[0]
                Mock.Of<IBehavior>(b =>
                    b.AffectedStats == new[] { _stat } && b.AffectedNodeTypes == new[] { _nodeType } &&
                    b.AffectedPaths == BehaviorPathInteraction.MainPathOnly && b.Transformation == _transformations[1]),
                // _behaviors[2]/_transformations[2] only affects _selectors[1]/_transformableMocks[1]
                Mock.Of<IBehavior>(b =>
                    b.AffectedStats == new[] { _stat } && b.AffectedNodeTypes == new[] { _nodeType } &&
                    b.AffectedPaths == BehaviorPathInteraction.ConversionPathsOnly && 
                    b.Transformation == _transformations[2]),
            };
            _sut = new ValueTransformer();
        }

        [Test]
        public void AddBehaviorsAddsToStoredTransformables()
        {
            AddTransformables();

            _sut.AddBehaviors(_behaviors);

            VerifyTransformationsWereAdded();
        }

        [Test]
        public void AddTransformableAddsStoredTransformations()
        {
            _sut.AddBehaviors(_behaviors);

            AddTransformables();
            
            VerifyTransformationsWereAdded();
        }

        private void VerifyTransformationsWereAdded()
        {
            _transformableMocks[0].Verify(t => t.Add(_transformations[0]));
            _transformableMocks[1].Verify(t => t.Add(_transformations[0]));
            _transformableMocks[0].Verify(t => t.Add(_transformations[1]));
            _transformableMocks[1].Verify(t => t.Add(_transformations[1]), Times.Never);
            _transformableMocks[0].Verify(t => t.Add(_transformations[2]), Times.Never);
            _transformableMocks[1].Verify(t => t.Add(_transformations[2]));
        }

        [Test]
        public void RemoveBehaviorsRemovesFromStoredTransformables()
        {
            _sut.AddBehaviors(_behaviors);
            AddTransformables();

            _sut.RemoveBehaviors(_behaviors);

            _transformableMocks[0].Verify(t => t.Remove(_transformations[0]));
            _transformableMocks[1].Verify(t => t.Remove(_transformations[0]));
            _transformableMocks[0].Verify(t => t.Remove(_transformations[1]));
            _transformableMocks[1].Verify(t => t.Remove(_transformations[1]), Times.Never);
        }

        [Test]
        public void AddTransformableDoesNotAddRemovedTransformations()
        {
            _sut.AddBehaviors(_behaviors);

            _sut.RemoveBehaviors(_behaviors);
            AddTransformables();

            _transformableMocks[0].Verify(t => t.Remove(_transformations[0]), Times.Never);
        }

        [Test]
        public void RemoveTransformableCallsRemoveAll()
        {
            _sut.AddBehaviors(_behaviors);
            AddTransformables();

            RemoveTransformables();

            _transformableMocks[0].Verify(t => t.RemoveAll());
        }

        [Test]
        public void AddBehaviorsDoesNotAddToRemovedTransformables()
        {
            AddTransformables();

            RemoveTransformables();
            _sut.AddBehaviors(_behaviors);

            _transformableMocks[0].Verify(t => t.Add(_transformations[0]), Times.Never);
        }

        private void AddTransformables()
        {
            _sut.AddTransformable(_stat, _selectors[0], _transformableMocks[0].Object);
            _sut.AddTransformable(_stat, _selectors[1], _transformableMocks[1].Object);
        }

        private void RemoveTransformables()
        {
            _sut.RemoveTransformable(_stat, _selectors[0]);
            _sut.RemoveTransformable(_stat, _selectors[1]);
        }
    }
}