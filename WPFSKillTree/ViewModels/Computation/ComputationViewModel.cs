using System.Collections.ObjectModel;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.GameModel;

namespace POESKillTree.ViewModels.Computation
{
    public class ComputationViewModel
    {
        public ResultStatsViewModel OffensiveStats { get; }
        public ResultStatsViewModel DefensiveStats { get; }
        public ObservableCollection<ConfigurationStatViewModel> ConfigurationStats { get; }

        public ComputationViewModel()
        {
            OffensiveStats = new ResultStatsViewModel(
                new ObservableCollection<string> { "BaseCastTime.Attack.MainHand.Skill", "DPS.OverTime" });
            OffensiveStats.AddStat(new StatStub("DPS.Hit", Entity.Character, typeof(double)),
                new NodeValue(1234.56));
            OffensiveStats.AddStat(new StatStub("Physical.Damage.Attack.MainHand.Skill", Entity.Character, typeof(int)),
                new NodeValue(2, 8));
            OffensiveStats.AddStat(new StatStub("HitRate", Entity.Character, typeof(double)));
            OffensiveStats.AddStat(new StatStub("CriticalStrikeChanceIsLucky", Entity.Character, typeof(bool)),
                (NodeValue?) true);
            OffensiveStats.AddStat(new StatStub("FarShot", Entity.Character, typeof(bool)));
            OffensiveStats.AddStat(new StatStub("SkillHitDamageSource", Entity.Character, typeof(DamageSource)),
                new NodeValue((int) DamageSource.Attack));
            OffensiveStats.AddStat(
                new StatStub("Stun.ThresholdModifier.Attack.MainHand.Skill", Entity.Enemy, typeof(double)),
                new NodeValue(0.8));

            DefensiveStats = new ResultStatsViewModel(
                new ObservableCollection<string> { "Fire.Resistance", "Lightning.Resistance" });
            DefensiveStats.AddStat(new StatStub("Life", Entity.Character, typeof(int)),
                new NodeValue(500));
            DefensiveStats.AddStat(new StatStub("Life", Entity.Character, typeof(int)),
                new NodeValue(100), NodeType.Increase);
            DefensiveStats.AddStat(new StatStub("Cold.Resistance", Entity.Character, typeof(int)),
                new NodeValue(40));

            ConfigurationStats = new ObservableCollection<ConfigurationStatViewModel>
            {
                new ConfigurationStatViewModel(
                        new StatStub("SelectedQuestPart", Entity.Character, typeof(QuestPart)))
                    { SingleValue = (int) QuestPart.Epilogue },
                new ConfigurationStatViewModel(
                        new StatStub("Level", Entity.Enemy, typeof(int)))
                    { SingleValue = 84 },
                new ConfigurationStatViewModel(
                    new StatStub("Projectile.TravelDistance", Entity.Character, typeof(double))),
                new ConfigurationStatViewModel(
                    new StatStub("Shocked", Entity.Enemy, typeof(bool))),
                new ConfigurationStatViewModel(
                        new StatStub("Chilled", Entity.Enemy, typeof(bool)))
                    { BoolValue = true },
            };
        }
    }
}