using System;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
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
            var expected = Mock.Of<IDisposableNodeViewProvider>();
            var value = Mock.Of<IValue>();
            var sut = CreateSut(new TransformableValue(value), expected);

            var actual = sut.Create(value, null);

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
            var key = Mock.Of<IDisposableNodeViewProvider>();
            var transformableValue = new TransformableValue(null);
            var sut = CreateSut(transformableValue, key);

            sut.Create(null, null);

            Assert.IsTrue(sut.TransformableDictionary.ContainsKey(key));
            Assert.AreSame(transformableValue, sut.TransformableDictionary[key]);
        }

        [Test]
        public void DisposingProviderRemovesIt()
        {
            var providerMock = new Mock<IDisposableNodeViewProvider>();
            var sut = CreateSut(provider: providerMock.Object);
            sut.Create(null, null);

            providerMock.Raise(p => p.Disposed += null, EventArgs.Empty);

            CollectionAssert.IsEmpty(sut.TransformableDictionary);
        }

        [Test]
        public void RaisingTransformableValuesValueChangedCallsProvidersRaiseValueChanged()
        {
            var transformableValue = new TransformableValue(null);
            var providerMock = new Mock<IDisposableNodeViewProvider>();
            var sut = CreateSut(transformableValue, providerMock.Object);
            sut.Create(null, null);

            transformableValue.RemoveAll();

            providerMock.Verify(p => p.RaiseValueChanged());
        }

        [Test]
        public void RaisingTransformableValuesValueChangedDoesNotCallProvidersRaiseValueChangedAfterDisposal()
        {
            var transformableValue = new TransformableValue(null);
            var providerMock = new Mock<IDisposableNodeViewProvider>();
            var sut = CreateSut(transformableValue, providerMock.Object);
            sut.Create(null, null);
            
            providerMock.Raise(p => p.Disposed += null, EventArgs.Empty);
            transformableValue.RemoveAll();

            providerMock.Verify(p => p.RaiseValueChanged(), Times.Never);
        }

        private static TransformableNodeFactory CreateSut(
            TransformableValue transformableValue = null, IDisposableNodeViewProvider provider = null)
        {
            transformableValue = transformableValue ?? new TransformableValue(null);
            var injectedFactory = Mock.Of<INodeFactory>(f => f.Create(transformableValue, null) == provider);
            return new TransformableNodeFactory(injectedFactory, _ => transformableValue);
        }
    }
}