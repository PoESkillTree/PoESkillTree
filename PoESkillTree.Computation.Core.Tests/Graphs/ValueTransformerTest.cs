using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.Nodes;
using PoESkillTree.Utils.Extensions;

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
        private IReadOnlyList<Behavior> _behaviors;
        private ValueTransformer _sut;

        [SetUp]
        public void SetUp()
        {
            _transformableMocks = new[]
                { new Mock<IValueTransformable>(), new Mock<IValueTransformable>(), new Mock<IValueTransformable>() };
            _stat = new StatStub();
            _nodeType = NodeType.Base;
            _selectors = new[]
            {
                new NodeSelector(_nodeType, PathDefinition.MainPath),
                new NodeSelector(_nodeType, new PathDefinition(new ModifierSource.Local.Given(), new StatStub())),
                new NodeSelector(_nodeType, new PathDefinition(new ModifierSource.Local.Given())),
            };
            _transformations = Helper.MockMany<IValueTransformation>(4);
            _behaviors = new[]
            {
                // _behaviors[0]/_transformations[0] affects all _selectors/_transformableMocks
                new Behavior(new[] { _stat }, new[] { _nodeType },
                    new FunctionalPathRule(_ => true), 
                    _transformations[0]),
                // _behaviors[1]/_transformations[1] affects _selectors/_transformableMocks 0 and 1
                new Behavior(new[] { _stat }, new[] { _nodeType },
                    new FunctionalPathRule(p => p.IsMainPath || p.ConversionStats.Any()),
                    _transformations[1]),
                // _behaviors[2]/_transformations[2] only affects _selectors[1]/_transformableMocks[1]
                new Behavior(new[] { _stat }, new[] { _nodeType },
                    new FunctionalPathRule(p => p.ConversionStats.Any()),
                    _transformations[2]),
                // _behaviors[3]/_transformations[3] affects _selectors/_transformableMocks 0 and 2
                new Behavior(new[] { _stat }, new[] { _nodeType },
                    new FunctionalPathRule(p => p.ConversionStats.IsEmpty()),
                    _transformations[3]),
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
            _transformableMocks[2].Verify(t => t.Add(_transformations[0]));

            _transformableMocks[0].Verify(t => t.Add(_transformations[1]));
            _transformableMocks[1].Verify(t => t.Add(_transformations[1]));
            _transformableMocks[2].Verify(t => t.Add(_transformations[1]), Times.Never);

            _transformableMocks[0].Verify(t => t.Add(_transformations[2]), Times.Never);
            _transformableMocks[1].Verify(t => t.Add(_transformations[2]));
            _transformableMocks[2].Verify(t => t.Add(_transformations[2]), Times.Never);

            _transformableMocks[0].Verify(t => t.Add(_transformations[3]));
            _transformableMocks[1].Verify(t => t.Add(_transformations[3]), Times.Never);
            _transformableMocks[2].Verify(t => t.Add(_transformations[3]));
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
            _transformableMocks[2].Verify(t => t.Remove(_transformations[1]), Times.Never);
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

        [Test]
        public void RemoveBehaviorDoesNotRemoveIfRemovedLessThanAdded()
        {
            AddTransformables();

            _sut.AddBehaviors(_behaviors);
            _sut.AddBehaviors(new[] { _behaviors[0] });
            _sut.RemoveBehaviors(new[] { _behaviors[0] });
            
            _transformableMocks[0].Verify(t => t.Remove(_transformations[0]), Times.Never);
        }

        [Test]
        public void RemoveBehaviorDoesNothingIfNeverAdded()
        {
            _sut.RemoveBehaviors(_behaviors);
        }

        private void AddTransformables()
        {
            _sut.AddTransformable(_stat, _selectors[0], _transformableMocks[0].Object);
            _sut.AddTransformable(_stat, _selectors[1], _transformableMocks[1].Object);
            _sut.AddTransformable(_stat, _selectors[2], _transformableMocks[2].Object);
        }

        private void RemoveTransformables()
        {
            _sut.RemoveTransformable(_stat, _selectors[0]);
            _sut.RemoveTransformable(_stat, _selectors[1]);
            _sut.RemoveTransformable(_stat, _selectors[2]);
        }

        private class FunctionalPathRule : IBehaviorPathRule
        {
            private readonly Predicate<PathDefinition> _rule;

            public FunctionalPathRule(Predicate<PathDefinition> rule) => _rule = rule;

            public bool AffectsPath(PathDefinition path) => _rule(path);
        }
    }
}