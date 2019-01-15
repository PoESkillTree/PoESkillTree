using System;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using POESKillTree.Computation.Model;
using POESKillTree.Computation.ViewModels;

namespace PoESkillTree.Tests.Computation.ViewModels
{
    [TestFixture]
    public class CalculationNodeViewModelTest
    {
        [TestCase("s1", Entity.Character, NodeType.Total, ExpectedResult = "s1")]
        [TestCase("s2", Entity.Character, NodeType.Base, ExpectedResult = "s2 (Base)")]
        [TestCase("s3", Entity.Enemy, NodeType.Total, ExpectedResult = "Enemy s3")]
        [TestCase("s4", Entity.Enemy, NodeType.Base, ExpectedResult = "Enemy s4 (Base)")]
        public string ToStringReturnsCorrectResult(string statIdentity, Entity statEntity, NodeType nodeType)
        {
            var stat = new Stat(statIdentity, statEntity);
            var sut = CreateSut(stat, nodeType);

            return sut.ToString();
        }

        [TestCase(false, ExpectedResult = "False")]
        [TestCase(true, ExpectedResult = "True")]
        public string StringValueOfBoolReturnsCorrectResult(bool value)
        {
            var sut = CreateSut<bool>((NodeValue?) value);

            return sut.StringValue;
        }

        [TestCase(null, ExpectedResult = "None")]
        [TestCase(TestEnum.A, ExpectedResult = "A")]
        [TestCase(TestEnum.B, ExpectedResult = "B")]
        public string StringValueReturnsCorrectResult(TestEnum? value)
        {
            var sut = CreateSut<TestEnum>((NodeValue?) (int?) value);

            return sut.StringValue;
        }

        [TestCase(null, ExpectedResult = "None")]
        [TestCase(1.2, ExpectedResult = "1.2")]
        public string StringValueOfDoubleReturnsCorrectResult(double? value)
        {
            var sut = CreateSut<double>((NodeValue?) value);

            return sut.StringValue;
        }

        [Test]
        public void ObserveSetsValueCorrectly()
        {
            var stat = new Stat("a");
            var subject = new Subject<NodeValue?>();
            var repository = Mock.Of<IObservableNodeRepository>(r =>
                r.ObserveNode(stat, NodeType.More) == subject);
            var sut = CreateSut(stat, NodeType.More);

            using (sut.Observe(repository, Scheduler.Immediate))
            {
                Assert.AreEqual(null, sut.NumericValue);
                subject.OnNext(new NodeValue(2));
                Assert.AreEqual(2, sut.NumericValue);
                subject.OnNext(new NodeValue(1));
                Assert.AreEqual(true, sut.BoolValue);
            }
        }

        private static CalculationNodeViewModel CreateSut<T>(NodeValue? value)
        {
            var stat = new Stat("", dataType: typeof(T));
            return new CalculationNodeViewModel(stat) { Value = value };
        }

        private static CalculationNodeViewModel CreateSut(IStat stat, NodeType nodeType)
            => new CalculationNodeViewModel(stat, nodeType);

        public enum TestEnum
        {
            A,
            B
        }
    }
}