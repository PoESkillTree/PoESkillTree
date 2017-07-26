using System;
using PoESkillTree.Common.Model.Items.Enums;

namespace PoESkillTree.Computation.Providers
{
    public interface IConditionProvider
    {

    }


    public interface IProviderCollection<out T>
    {
        T this[int index] { get; }

        T First { get; }

        T Last { get; }

        // returns the only element or throws
        T Single { get; }

        ValueProvider Count(Func<T, IConditionProvider> predicate = null);

        IConditionProvider Any(Func<T, IConditionProvider> predicate = null);
    }


    public static class ConditionProviders
    {
        public static readonly IConditionProvider LocalIsMelee =
            And(EquipmentProviders.LocalHand.Has(Tags.Weapon), Not(EquipmentProviders.LocalHand.Has(Tags.Ranged)));
        public static readonly IConditionProvider Unarmed = Not(EquipmentProviders.MainHand.HasItem);

        public static readonly IConditionProvider WhileLeeching;

        public static IConditionProvider With(ISkillProviderCollection skills)
        {
            throw new NotImplementedException();
        }

        public static IConditionProvider With(ISkillProvider skill)
        {
            throw new NotImplementedException();
        }

        // Minions have their own offensive and defensive stats.
        // stats only apply to minions when they have this condition (probably with some exceptions)
        // Totems have their own defensive stats.
        // defensive stats only apply to totems when they have this condition
        // Can also be used to apply stats to Enemy instead of Self.
        public static IConditionProvider For(params ITargetProvider[] targets)
        {
            throw new NotImplementedException();
        }

        // increases with this condition only affect base values coming from the specified equipment
        public static IConditionProvider BaseValueComesFrom(IEquipmentProvider equipment)
        {
            throw new NotImplementedException();
        }

        // These need to be set by the user in a checkbox (will probably also need a section name or something)
        // or are displayed as chances to gain something (as tooltip or something on the "Do you have X?" checkbox)
        // Name may be a regex replacement.
        public static IConditionProvider UniqueCondition(string name)
        {
            throw new NotImplementedException();
        }

        public static IConditionProvider And(params IConditionProvider[] conditions)
        {
            throw new NotImplementedException();
        }

        public static IConditionProvider Or(params IConditionProvider[] conditions)
        {
            throw new NotImplementedException();
        }

        public static IConditionProvider Not(IConditionProvider condition)
        {
            throw new NotImplementedException();
        }
    }
}