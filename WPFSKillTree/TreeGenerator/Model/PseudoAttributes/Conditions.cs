using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.TreeGenerator.Model.PseudoAttributes
{
    /// <summary>
    /// Interface for conditions that can be evalated.
    /// </summary>
    public interface ICondition
    {
        /// <summary>
        /// Return whether the conditions evaluates to true under the given
        /// ConditionSettings and with placeholders of the format '{number}'
        /// replaced by the given replacement strings.
        /// </summary>
        /// <param name="settings">Settings to evaluate the condition (not null)</param>
        /// <param name="replacements">Strings to replace placeholders with (not null)</param>
        /// <returns></returns>
        bool Evaluate(ConditionSettings settings, params string[] replacements);
    }

    #region Logical composition conditions

    /// <summary>
    /// Describes a condition that evaluates to true iff all contained conditions evaluate to true.
    /// </summary>
    internal class AndComposition : ICondition
    {
        public List<ICondition> Conditions { get; private set; }

        public AndComposition()
        {
            Conditions = new List<ICondition>();
        }

        public bool Evaluate(ConditionSettings settings, params string[] replacements)
        {
            return Conditions.All(c => c.Evaluate(settings, replacements));
        }
    }

    /// <summary>
    /// Describes a condition that evaluates to true iff at least one of the contained conditions
    /// evaluates to true or there are no conditions.
    /// </summary>
    public class OrComposition : ICondition
    {
        public List<ICondition> Conditions { get; private set; }

        internal OrComposition()
        {
            Conditions = new List<ICondition>();
        }

        public bool Evaluate(ConditionSettings settings, params string[] replacements)
        {
            return Conditions.Count <= 0 || Conditions.Any(c => c.Evaluate(settings, replacements));
        }
    }

    /// <summary>
    /// Describes a condition that evaluates to true iff the contained conditions evaluates to false.
    /// </summary>
    internal class NotCondition : ICondition
    {
        private readonly ICondition _condition;

        public NotCondition(ICondition condition)
        {
            _condition = condition;
        }

        public bool Evaluate(ConditionSettings settings, params string[] replacements)
        {
            return !_condition.Evaluate(settings, replacements);
        }
    }

    #endregion

    #region Base conditions

    /// <summary>
    /// Describes a condition that evaluates to true if the specified keystone is set.
    /// </summary>
    internal class KeystoneCondition : ICondition
    {
        private readonly string _keystone;

        public KeystoneCondition(string keystone)
        {
            _keystone = keystone;
        }

        /// <summary>
        /// Returns true iff <see cref="ConditionSettings.Keystones"/> contains <see cref="_keystone"/>.
        /// </summary>
        public bool Evaluate(ConditionSettings settings, params string[] replacements)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            return settings.Keystones.Any(k => k == _keystone);
        }
    }

    /// <summary>
    /// Describes a condition that evaluates to true if the OffHand type has the
    /// specified alias.
    /// </summary>
    internal class OffHandCondition : ICondition
    {
        private readonly string _alias;

        public OffHandCondition(string alias)
        {
            _alias = alias;
        }

        /// <summary>
        /// Returns true iff <see cref="ConditionSettings.OffHand"/> has the specified
        /// alias with placeholders replaced by the given replacements.
        /// </summary>
        public bool Evaluate(ConditionSettings settings, params string[] replacements)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            // ReSharper disable once CoVariantArrayConversion
            // Passing replacements as multiple strings cast to objects is exactly what we want.
            return settings.OffHand.HasAlias(string.Format(_alias, replacements));
        }
    }

    /// <summary>
    /// Describes a condition that evaluates to true if at least one of the Tags has the
    /// specified alias.
    /// </summary>
    internal class TagCondition : ICondition
    {
        private readonly string _alias;

        public TagCondition(string alias)
        {
            _alias = alias;
        }

        /// <summary>
        /// Returns true iff <see cref="ConditionSettings.Tags"/> has the specified
        /// alias with placeholders replaced by the given replacements.
        /// </summary>
        public bool Evaluate(ConditionSettings settings, params string[] replacements)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            // ReSharper disable once CoVariantArrayConversion
            // Passing replacements as multiple strings cast to objects is exactly what we want.
            return settings.Tags.HasAlias(string.Format(_alias, replacements));
        }
    }

    /// <summary>
    /// Describes a condition that evaluates to true if the WeaponClass type has the
    /// specified alias.
    /// </summary>
    internal class WeaponClassCondition : ICondition
    {
        private readonly string _alias;

        public WeaponClassCondition(string alias)
        {
            _alias = alias;
        }

        /// <summary>
        /// Returns true iff <see cref="ConditionSettings.WeaponClass"/> has the specified
        /// alias with placeholders replaced by the given replacements.
        /// </summary>
        public bool Evaluate(ConditionSettings settings, params string[] replacements)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            // ReSharper disable once CoVariantArrayConversion
            // Passing replacements as multiple strings cast to objects is exactly what we want.
            return settings.WeaponClass.HasAlias(string.Format(_alias, replacements));
        }
    }

    #endregion
}