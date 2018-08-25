using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Equipment;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing
{
    public class ActiveSkillParser : IParser<Skill>
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;
        private readonly IModifierBuilder _modifierBuilder = new ModifierBuilder();

        public ActiveSkillParser(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_skillDefinitions, _builderFactories, _metaStatBuilders) =
                (skillDefinitions, builderFactories, metaStatBuilders);

        public ParseResult Parse(Skill parameter)
        {
            var definition = _skillDefinitions.GetSkillById(parameter.Id);
            var activeSkill = definition.ActiveSkill;
            var level = definition.Levels[parameter.Level];

            var displayName = definition.BaseItem?.DisplayName ??
                              (definition.IsSupport ? parameter.Id : definition.ActiveSkill.DisplayName);
            var localSource = new ModifierSource.Local.Skill(displayName);
            var globalSource = new ModifierSource.Global(localSource);
            var isMainSkill = _metaStatBuilders.MainSkillSocket(parameter.ItemSlot, parameter.SocketIndex).IsSet;
            var modifiers = new List<Modifier>();

            void AddLocal(IIntermediateModifier m) => modifiers.AddRange(BuildLocal(m));
            IReadOnlyList<Modifier> BuildLocal(IIntermediateModifier m) => m.Build(localSource, Entity.Character);
            void AddGlobal(IIntermediateModifier m) => modifiers.AddRange(BuildGlobal(m));
            IReadOnlyList<Modifier> BuildGlobal(IIntermediateModifier m) => m.Build(globalSource, Entity.Character);

            AddGlobal(_modifierBuilder
                .WithStat(_metaStatBuilders.SkillHitDamageSource)
                .WithForm(Forms.TotalOverride)
                .WithValue(CreateValue((int) DamageSource.Attack))
                .WithCondition(isMainSkill).Build());
            AddGlobal(_modifierBuilder
                .WithStat(_metaStatBuilders.SkillUsesHand(AttackDamageHand.MainHand))
                .WithForm(Forms.TotalOverride)
                .WithValue(CreateValue(1))
                .WithCondition(isMainSkill).Build());
            AddGlobal(_modifierBuilder
                .WithStat(_metaStatBuilders.SkillUsesHand(AttackDamageHand.OffHand))
                .WithForm(Forms.TotalOverride)
                .WithValue(CreateValue(1))
                .WithCondition(isMainSkill.And(Equipment[ItemSlot.OffHand].HasItem)).Build());
            AddGlobal(_modifierBuilder
                .WithStat(_metaStatBuilders.MainSkillId)
                .WithForm(Forms.TotalOverride)
                .WithValue(CreateValue(definition.NumericId))
                .WithCondition(isMainSkill).Build());

            void AddKeywordModifiers(Func<Keyword, IStatBuilder> statFactory)
                => modifiers.AddRange(KeywordModifiers(activeSkill.Keywords, statFactory, isMainSkill)
                    .SelectMany(BuildGlobal));

            AddKeywordModifiers(_metaStatBuilders.MainSkillHasKeyword);
            AddKeywordModifiers(_metaStatBuilders.MainSkillPartHasKeyword);
            AddKeywordModifiers(_metaStatBuilders.MainSkillPartCastRateHasKeyword);
            AddKeywordModifiers(k => _metaStatBuilders.MainSkillPartDamageHasKeyword(k, DamageSource.Attack));

            if (level.DamageEffectiveness.HasValue)
            {
                AddGlobal(_modifierBuilder
                    .WithStat(_metaStatBuilders.DamageBaseAddEffectiveness)
                    .WithForm(Forms.TotalOverride)
                    .WithValue(CreateValue(level.DamageEffectiveness.Value))
                    .WithCondition(isMainSkill).Build());
            }
            if (level.DamageMultiplier.HasValue)
            {
                AddGlobal(_modifierBuilder
                    .WithStat(_metaStatBuilders.DamageBaseSetEffectiveness)
                    .WithForm(Forms.TotalOverride)
                    .WithValue(CreateValue(level.DamageMultiplier.Value))
                    .WithCondition(isMainSkill).Build());
            }
            if (level.ManaCost.HasValue)
            {
                AddGlobal(_modifierBuilder
                    .WithStat(_builderFactories.StatBuilders.Pool.From(Pool.Mana).Cost)
                    .WithForm(Forms.BaseSet)
                    .WithValue(CreateValue(level.ManaCost.Value))
                    .WithCondition(isMainSkill).Build());
            }

            AddLocal(_modifierBuilder
                .WithStat(_builderFactories.StatBuilders.Requirements.Level)
                .WithForm(Forms.BaseSet)
                .WithValue(CreateValue(level.RequiredLevel)).Build());
            AddLocal(_modifierBuilder
                .WithStat(_builderFactories.StatBuilders.Requirements.Dexterity)
                .WithForm(Forms.BaseSet)
                .WithValue(CreateValue(level.RequiredDexterity)).Build());

            if (level.QualityStats.Any())
            {
                AddGlobal(_modifierBuilder
                    .WithStat(_builderFactories.StatBuilders.CastRate.With(DamageSource.Attack))
                    .WithForm(Forms.PercentIncrease)
                    .WithValue(CreateValue(level.QualityStats[0].Value * parameter.Quality))
                    .WithCondition(isMainSkill).Build());
            }

            if (level.Stats.Any())
            {
                AddGlobal(_modifierBuilder
                    .WithStat(_builderFactories.DamageTypeBuilders.Physical.Damage)
                    .WithForm(Forms.PercentIncrease)
                    .WithValue(level.Stats[0].Value * _builderFactories.ChargeTypeBuilders.Frenzy.Amount.Value)
                    .WithCondition(isMainSkill).Build());
                AddGlobal(_modifierBuilder
                    .WithStat(_builderFactories.StatBuilders.CastRate.With(DamageSource.Attack))
                    .WithForm(Forms.PercentIncrease)
                    .WithValue(level.Stats[0].Value * _builderFactories.ChargeTypeBuilders.Frenzy.Amount.Value)
                    .WithCondition(isMainSkill).Build());
            }

            return ParseResult.Success(modifiers);
        }

        private IEnumerable<IIntermediateModifier> KeywordModifiers(
            IEnumerable<Keyword> keywords, Func<Keyword, IStatBuilder> statFactory, IConditionBuilder isMainSkill)
        {
            foreach (var keyword in keywords)
            {
                var condition = isMainSkill;
                var mainHandIsRanged = Equipment[ItemSlot.MainHand].Has(Tags.Ranged);
                switch (keyword)
                {
                    case Keyword.Melee:
                        condition = condition.And(mainHandIsRanged.Not);
                        break;
                    case Keyword.Projectile:
                        condition = condition.And(mainHandIsRanged);
                        break;
                }
                yield return _modifierBuilder
                    .WithStat(statFactory(keyword))
                    .WithForm(Forms.TotalOverride)
                    .WithValue(CreateValue(1))
                    .WithCondition(condition)
                    .Build();
            }
        }

        private IFormBuilders Forms => _builderFactories.FormBuilders;
        private IValueBuilder CreateValue(double value) => _builderFactories.ValueBuilders.Create(value);
        private IEquipmentBuilderCollection Equipment => _builderFactories.EquipmentBuilders.Equipment;
    }

    public struct Skill
    {
        public Skill(string id, int level, int quality, ItemSlot itemSlot, int socketIndex, int? gemGroup)
            => (Id, Level, Quality, ItemSlot, SocketIndex, GemGroup) =
                (id, level, quality, itemSlot, socketIndex, gemGroup);

        public string Id { get; }
        public int Level { get; }
        public int Quality { get; }

        public ItemSlot ItemSlot { get; }

        // Null: item inherent skill
        public int? GemGroup { get; }

        public int SocketIndex { get; }
    }
}