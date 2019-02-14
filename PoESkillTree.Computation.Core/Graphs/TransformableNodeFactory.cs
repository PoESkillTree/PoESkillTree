using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Decorates an <see cref="INodeFactory"/> by replacing <see cref="IValue"/>s passed to it with
    /// <see cref="TransformableValue"/>s initialized from those values.
    /// </summary>
    public class TransformableNodeFactory : INodeFactory
    {
        private readonly INodeFactory _decoratedFactory;
        private readonly Func<IValue, TransformableValue> _transformableValueFactory;

        public TransformableNodeFactory(
            INodeFactory decoratedFactory, Func<IValue, TransformableValue> transformableValueFactory)
        {
            _decoratedFactory = decoratedFactory;
            _transformableValueFactory = transformableValueFactory;
        }

        public IDictionary<IBufferingEventViewProvider<ICalculationNode>, IValueTransformable>
            TransformableDictionary { get; } =
            new Dictionary<IBufferingEventViewProvider<ICalculationNode>, IValueTransformable>();

        public IDisposableNodeViewProvider Create(IValue value, PathDefinition path)
        {
            var transformableValue = _transformableValueFactory(value);
            var result = _decoratedFactory.Create(transformableValue, path);
            TransformableDictionary[result] = transformableValue;
            transformableValue.ValueChanged += TransformableValueValueChanged;
            result.Disposed += ResultDisposed;
            return result;

            void TransformableValueValueChanged(object sender, EventArgs args) => result.RaiseValueChanged();

            void ResultDisposed(object sender, EventArgs args)
            {
                TransformableDictionary.Remove(result);
                transformableValue.ValueChanged -= TransformableValueValueChanged;
                result.Disposed -= ResultDisposed;
            }
        }
    }
}