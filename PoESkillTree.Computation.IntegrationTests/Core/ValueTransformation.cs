using System;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.IntegrationTests.Core
{
    internal class ValueTransformation : IValueTransformation
    {
        private readonly Func<IValue, IValue> _transformation;

        public ValueTransformation(Func<IValue, IValue> transformation)
        {
            _transformation = transformation;
        }

        public IValue Transform(IValue value) => _transformation(value);
    }
}