using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using POESKillTree.Computation.Model;
using POESKillTree.Computation.ViewModels;

namespace PoESkillTree.Tests.Computation.ViewModels
{
    [TestFixture]
    public class ConfigurationNodeViewModelTest
    {
        [Test]
        public void SubscribeCalculatorGeneratesCorrectValues()
        {
            var stat = new Stat("");
            var calculatorMock = new Mock<IObservingCalculator>();
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat.Minimum, NodeType.Total, PathDefinition.MainPath) == new NodeValue(1) &&
                c.GetValue(stat.Maximum, NodeType.Total, PathDefinition.MainPath) == new NodeValue(3));
            var expectedValues = new (NodeValue? added, NodeValue? removed)[]
            {
                ((NodeValue?) true, null),
                (new NodeValue(3), (NodeValue?) true),
                (null, new NodeValue(3)),
                (new NodeValue(1), null),
            };
            using (var sut = CreateSut(stat))
            {
                var actualUpdates = new List<CalculatorUpdate>();
                calculatorMock.Setup(
                        c => c.SubscribeTo(It.IsAny<IObservable<CalculatorUpdate>>(), It.IsAny<Action<Exception>>()))
                    .Callback<IObservable<CalculatorUpdate>, Action<Exception>>(
                        (observable, _) => observable.Subscribe(actualUpdates.Add))
                    .Returns(Disposable.Empty);

                sut.SubscribeCalculator(calculatorMock.Object);
                sut.BoolValue = true;
                sut.NumericValue = 4;
                sut.Value = null;
                sut.NumericValue = -2;
                var actualValues = actualUpdates.Select(u =>
                    (u.AddedModifiers.Single()?.Value.Calculate(context),
                        u.RemovedModifiers.SingleOrDefault()?.Value.Calculate(context)));

                Assert.AreEqual(expectedValues, actualValues);
            }
        }

        private static ConfigurationNodeViewModel CreateSut<T>(NodeValue? value)
        {
            var stat = new Stat("", dataType: typeof(T));
            return new ConfigurationNodeViewModel(stat) { Value = value };
        }

        private static ConfigurationNodeViewModel CreateSut(IStat stat)
            => new ConfigurationNodeViewModel(stat);
    }
}