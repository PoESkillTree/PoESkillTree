using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Events;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
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

        public IDictionary<ISuspendableEventViewProvider<ICalculationNode>, IValueTransformable>
            TransformableDictionary { get; } =
            new Dictionary<ISuspendableEventViewProvider<ICalculationNode>, IValueTransformable>();

        public IDisposableNodeViewProvider Create(IValue value)
        {
            var transformableValue = _transformableValueFactory(value);
            var result = _decoratedFactory.Create(transformableValue);
            TransformableDictionary[result] = transformableValue;
            result.Disposed += (_, args) => TransformableDictionary.Remove(result);
            return result;
        }
    }
}