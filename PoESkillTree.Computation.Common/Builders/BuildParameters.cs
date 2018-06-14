namespace PoESkillTree.Computation.Common.Builders
{
    /// <summary>
    /// Common parameters for stat builders and all builders producible from stat builders (e.g. value and condition).
    /// Contains the information required to build them.
    /// </summary>
    /// <remarks>
    /// This is a struct to allow simply using default instead of object creations in tests where the parameters don't
    /// matter. That is only sensible as long as the struct only contains fields of value types.
    /// </remarks>
    public struct BuildParameters
    {
        public BuildParameters(Entity modifierSourceEntity, Form modifierForm)
        {
            ModifierSourceEntity = modifierSourceEntity;
            ModifierForm = modifierForm;
        }

        /// <summary>
        /// The entity the modifier comes from.
        /// </summary>
        public Entity ModifierSourceEntity { get; }

        /// <summary>
        /// The form of the built <see cref="Modifier"/>.
        /// </summary>
        public Form ModifierForm { get; }

        public override bool Equals(object obj) =>
            obj is BuildParameters other && Equals(other);

        private bool Equals(BuildParameters other) =>
            ModifierSourceEntity == other.ModifierSourceEntity && ModifierForm == other.ModifierForm;

        public override int GetHashCode() =>
            (ModifierSourceEntity, ModifierForm).GetHashCode();
    }
}