using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.GameModel.Skills
{
    public class SkillDefinition
    {
        [Obsolete("Temporary constructor to support PoESkillTree.Computation.Console.SkillDefinition")]
        public SkillDefinition(string skillName, int numericId, IReadOnlyList<Keyword> keywords, bool providesBuff)
        {
            Id = skillName;
            NumericId = numericId;
            IsSupport = false;
            ActiveSkill = new ActiveSkillDefinition(skillName, new string[0], keywords, providesBuff);
            Levels = new Dictionary<int, SkillLevelDefinition>();
        }

        private SkillDefinition(
            string id, int numericId, bool isSupport, ActiveSkillDefinition activeSkill,
            IReadOnlyDictionary<int, SkillLevelDefinition> levels)
            => (Id, NumericId, IsSupport, ActiveSkill, Levels) = (id, numericId, isSupport, activeSkill, levels);

        public static SkillDefinition CreateActive(
            string id, int numericId, ActiveSkillDefinition activeSkill, IReadOnlyDictionary<int, SkillLevelDefinition> levels)
            => new SkillDefinition(id, numericId, false, activeSkill, levels);

        public string Id { get; }
        public int NumericId { get; }
        public bool IsSupport { get; }
        public ActiveSkillDefinition ActiveSkill { get; }
        public IReadOnlyDictionary<int, SkillLevelDefinition> Levels { get; }
    }

    public class ActiveSkillDefinition
    {
        public ActiveSkillDefinition(
            string displayName, IEnumerable<string> activeSkillTypes, IReadOnlyList<Keyword> keywords,
            bool providesBuff)
            => (DisplayName, ActiveSkillTypes, Keywords, ProvidesBuff) =
                (displayName, activeSkillTypes, keywords, providesBuff);

        public string DisplayName { get; }
        public IEnumerable<string> ActiveSkillTypes { get; }
        public IReadOnlyList<Keyword> Keywords { get; }
        public bool ProvidesBuff { get; }
    }

    public class SkillLevelDefinition
    {
        public SkillLevelDefinition(
            double? damageEffectiveness, double? damageMultiplier, int manaCost,
            int requiredLevel, int requiredDexterity, int requiredIntelligence, int requiredStrength,
            IReadOnlyList<UntranslatedStat> qualityStats, IReadOnlyList<UntranslatedStat> stats,
            SkillTooltipDefinition tooltip)
        {
            DamageEffectiveness = damageEffectiveness;
            DamageMultiplier = damageMultiplier;
            ManaCost = manaCost;
            RequiredLevel = requiredLevel;
            RequiredDexterity = requiredDexterity;
            RequiredIntelligence = requiredIntelligence;
            RequiredStrength = requiredStrength;
            QualityStats = qualityStats;
            Stats = stats;
            Tooltip = tooltip;
        }

        public double? DamageEffectiveness { get; }
        public double? DamageMultiplier { get; }
        public int ManaCost { get; }
        public int RequiredLevel { get; }
        public int RequiredDexterity { get; }
        public int RequiredIntelligence { get; }
        public int RequiredStrength { get; }

        public IReadOnlyList<UntranslatedStat> QualityStats { get; }
        public IReadOnlyList<UntranslatedStat> Stats { get; }

        public SkillTooltipDefinition Tooltip { get; }
    }

    public struct UntranslatedStat
    {
        public UntranslatedStat(string statId, int value) => (StatId, Value) = (statId, value);

        public string StatId { get; }
        public int Value { get; }
    }

    public class SkillTooltipDefinition
    {
        public SkillTooltipDefinition(
            string name, IReadOnlyList<TranslatedStat> properties, IReadOnlyList<TranslatedStat> requirements,
            IReadOnlyList<string> description,
            IReadOnlyList<TranslatedStat> qualityStats, IReadOnlyList<TranslatedStat> stats)
        {
            Name = name;
            Properties = properties;
            Requirements = requirements;
            Description = description;
            QualityStats = qualityStats;
            Stats = stats;
        }

        public string Name { get; }
        public IReadOnlyList<TranslatedStat> Properties { get; }
        public IReadOnlyList<TranslatedStat> Requirements { get; }
        public IReadOnlyList<string> Description { get; }
        public IReadOnlyList<TranslatedStat> QualityStats { get; }
        public IReadOnlyList<TranslatedStat> Stats { get; }
    }

    public class TranslatedStat
    {
        public TranslatedStat(string formatText, params double[] values)
            => (FormatText, Values) = (formatText, values);

        public string FormatText { get; }
        public IReadOnlyList<double> Values { get; }

        public override bool Equals(object obj)
            => this == obj || (obj is TranslatedStat other && Equals(other));

        private bool Equals(TranslatedStat other)
            => FormatText == other.FormatText && Values.SequenceEqual(other.Values);

        public override int GetHashCode()
            => (FormatText, Values.SequenceHash()).GetHashCode();

        public override string ToString()
            => string.Format(FormatText, Values.Cast<object>().ToArray());
    }
}