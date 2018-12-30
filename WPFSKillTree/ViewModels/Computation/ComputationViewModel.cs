using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;

namespace POESKillTree.ViewModels.Computation
{
    public class ComputationViewModel
    {
        public ObservableCollection<ConfigurationStatViewModel> ConfigurationStats { get; }

        public ComputationViewModel()
        {
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
                    { BoolValue = true},
            };
        }

        private class StatStub : IStat
        {
            public StatStub(string identity, Entity entity, Type dataType)
            {
                Identity = identity;
                Entity = entity;
                DataType = dataType;
            }

            public bool Equals(IStat other) => Equals((object) other);

            public string Identity { get; }
            public Entity Entity { get; }
            public IStat Minimum { get; } = null;
            public IStat Maximum { get; } = null;
            public ExplicitRegistrationType ExplicitRegistrationType { get; } = null;
            public Type DataType { get; }
            public IReadOnlyList<Behavior> Behaviors { get; } = new Behavior[0];
        }
    }
}