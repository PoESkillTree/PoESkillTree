using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Graphs;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Tests.Graphs
{
    [TestFixture]
    public class TransformableNodeFactoryTest
    {
        [Test]
        public void SutIsNodeFactory()
        {
            var sut = CreateSut();

            Assert.IsInstanceOf<INodeFactory>(sut);
        }

        [Test]
        public void CreateReturnsInjectedResult()
        {
            var expected = Mock.Of<ISuspendableEventViewProvider<IDisposableNode>>();
            var value = Mock.Of<IValue>();
            var transformableValue = new TransformableValue(value);
            var injectedFactory = Mock.Of<INodeFactory>(f => f.Create(transformableValue) == expected);
            var sut = CreateSut(injectedFactory, _ => transformableValue);

            var actual = sut.Create(value);

            Assert.AreSame(expected, actual);
        }

        [Test]
        public void TransformableDictionaryIsEmptyInitially()
        {
            var sut = CreateSut();

            CollectionAssert.IsEmpty(sut.TransformableDictionary);
        }

        [Test]
        public void CreateAddsToTransformableDictionary()
        {
            var key = Mock.Of<ISuspendableEventViewProvider<IDisposableNode>>();
            var transformableValue = new TransformableValue(null);
            var injectedFactory = Mock.Of<INodeFactory>(f => f.Create(transformableValue) == key);
            var sut = CreateSut(injectedFactory, _ => transformableValue);

            sut.Create(null);

            Assert.IsTrue(sut.TransformableDictionary.ContainsKey(key));
            Assert.AreSame(transformableValue, sut.TransformableDictionary[key]);
        }

        private static TransformableNodeFactory CreateSut(
            INodeFactory injectedFactory = null, Func<IValue, TransformableValue> transformableFactory = null)
        {
            return new TransformableNodeFactory(injectedFactory, transformableFactory);
        }
    }
}