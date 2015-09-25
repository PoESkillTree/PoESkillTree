using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Model.PseudoAttributes
{
    public interface ICondition
    {
        bool Eval(ConditionSettings settings, params string[] placeholder);
    }

    #region Logical composition conditions

    internal class AndComposition : ICondition
    {
        public List<ICondition> Conditions { get; private set; }

        public AndComposition()
        {
            Conditions = new List<ICondition>();
        }

        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return Conditions.All(c => c.Eval(settings, placeholder));
        }
    }

    public class OrComposition : ICondition
    {
        public List<ICondition> Conditions { get; private set; }

        internal OrComposition()
        {
            Conditions = new List<ICondition>();
        }

        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return Conditions.Count <= 0 || Conditions.Any(c => c.Eval(settings, placeholder));
        }
    }

    internal class NotCondition : ICondition
    {
        private readonly ICondition _condition;

        public NotCondition(ICondition condition)
        {
            _condition = condition;
        }

        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return !_condition.Eval(settings, placeholder);
        }
    }

    #endregion

    #region Base conditions

    internal class KeystoneCondition : ICondition
    {
        private readonly string _keystone;

        public KeystoneCondition(string keystone)
        {
            _keystone = keystone;
        }

        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return settings.Keystones.Any(k => k == _keystone);
        }
    }

    internal class OffHandCondition : ICondition
    {
        private readonly string _alias;

        public OffHandCondition(string alias)
        {
            _alias = alias;
        }

        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return settings.OffHand.HasAlias(string.Format(_alias, placeholder));
        }
    }

    internal class TagCondition : ICondition
    {
        private readonly string _alias;

        public TagCondition(string alias)
        {
            _alias = alias;
        }

        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return settings.Tags.HasAlias(string.Format(_alias, placeholder));
        }
    }

    internal class WeaponClassCondition : ICondition
    {
        private readonly string _alias;

        public WeaponClassCondition(string alias)
        {
            _alias = alias;
        }

        public bool Eval(ConditionSettings settings, params string[] placeholder)
        {
            return settings.WeaponClass.HasAlias(string.Format(_alias, placeholder));
        }
    }

    #endregion
}