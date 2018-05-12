using System.Collections.Generic;
using System.Linq;
using Moq;

namespace PoESkillTree.Computation.Common.Tests
{
    public static class Helper
    {
        public static T[] MockMany<T>(int count = 3) where T : class =>
            Enumerable.Range(0, count).Select(_ => Mock.Of<T>()).ToArray();


        public static Modifier[] MockManyModifiers() => new[] { MockModifier(), MockModifier(), MockModifier() };

        public static Modifier MockModifier(
            IStat stat, Form form = Form.BaseAdd, IValue value = null, IModifierSource source = null) =>
            MockModifier(new[] { stat }, form, value, source);

        public static Modifier MockModifier(
            IReadOnlyList<IStat> stats = null, Form form = Form.BaseAdd, IValue value = null, IModifierSource source = null) => 
            new Modifier(stats ?? new IStat[0], form, value ?? Mock.Of<IValue>(), source ?? new ModifierSourceStub());
    }
}