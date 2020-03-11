using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using NLog;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;

namespace PoESkillTree.Computation.Model
{
    public class AdditionalSkillStatApplier
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private readonly ObservableCalculator _calculator;
        private readonly IGemStatBuilders _gemStatBuilders;

        private readonly Dictionary<DictKey, int> _levels = new Dictionary<DictKey, int>();
        private readonly Dictionary<DictKey, IDisposable> _levelSubscriptions = new Dictionary<DictKey, IDisposable>();

        private readonly Dictionary<DictKey, int> _qualities = new Dictionary<DictKey, int>();
        private readonly Dictionary<DictKey, IDisposable> _qualitySubscriptions = new Dictionary<DictKey, IDisposable>();

        public AdditionalSkillStatApplier(ObservableCalculator calculator, IGemStatBuilders gemStatBuilders)
        {
            _calculator = calculator;
            _gemStatBuilders = gemStatBuilders;
        }

        public Skill Apply(Skill skill)
        {
            if (skill.Gem is null)
                return skill;

            var key = new DictKey(skill);
            EnsureIsObserved(skill, key);

            if (_levels.TryGetValue(key, out var additionalLevels))
            {
                skill = skill.WithLevel(skill.Gem.Level + additionalLevels);
            }

            if (_qualities.TryGetValue(key, out var additionalQuality))
            {
                skill = skill.WithQuality(skill.Gem!.Quality + additionalQuality);
            }

            return skill;
        }

        private void EnsureIsObserved(Skill skill, DictKey key)
        {
            if (!_levelSubscriptions.ContainsKey(key))
            {
                ObserveLevel(skill, key);
            }

            if (!_qualitySubscriptions.ContainsKey(key))
            {
                ObserveQuality(skill, key);
            }
        }

        private void ObserveLevel(Skill skill, DictKey key)
        {
            var levelStat = _gemStatBuilders.AdditionalLevels(skill).BuildToStats(Entity.Character).Single();
            _levelSubscriptions[key] = _calculator.ObserveNode(levelStat)
                .ObserveOnDispatcher()
                .Subscribe(v =>
                {
                    var level = (int) (v.SingleOrNull() ?? 0);
                    if (_levels.TryGetValue(key, out var oldLevel) && oldLevel == level)
                        return;
                    _levels[key] = level;
                    StatChangedForSlot?.Invoke(this, skill.ItemSlot);
                }, e => Log.Error(e, $"ObserveLevel({skill}, {key}) failed"));
        }

        private void ObserveQuality(Skill skill, DictKey key)
        {
            var qualityStat = _gemStatBuilders.AdditionalQuality(skill).BuildToStats(Entity.Character).Single();
            _qualitySubscriptions[key] = _calculator.ObserveNode(qualityStat)
                .ObserveOnDispatcher()
                .Subscribe(v =>
                {
                    var quality = (int) (v.SingleOrNull() ?? 0);
                    if (_qualities.TryGetValue(key, out var oldLevel) && oldLevel == quality)
                        return;
                    _qualities[key] = quality;
                    StatChangedForSlot?.Invoke(this, skill.ItemSlot);
                }, e => Log.Error(e, $"ObserveQuality({skill}, {key}) failed"));
        }

        public event EventHandler<ItemSlot>? StatChangedForSlot;

        public void CleanSubscriptions(IReadOnlyCollection<IReadOnlyList<Skill>> removedItems, IReadOnlyCollection<IReadOnlyList<Skill>> addedItems)
        {
            var removedItemKeys = removedItems.Flatten().Select(s => new DictKey(s));
            var addedItemKeys = addedItems.Flatten().Select(s => new DictKey(s));
            var removableKeys = removedItemKeys.Except(addedItemKeys).ToHashSet();
            foreach (var key in removableKeys)
            {
                if (_levelSubscriptions.TryGetValue(key, out var levelSubscription))
                {
                    levelSubscription.Dispose();
                }

                if (_qualitySubscriptions.TryGetValue(key, out var qualitySubscription))
                {
                    qualitySubscription.Dispose();
                }

                _levels.Remove(key);
                _levelSubscriptions.Remove(key);
                _qualities.Remove(key);
                _qualitySubscriptions.Remove(key);
            }
        }

        private class DictKey : ValueObject
        {
            private readonly object _tuple;

            public DictKey(Skill skill)
            {
                _tuple = (skill.ItemSlot, skill.SocketIndex, skill.SkillIndex);
            }

            protected override object ToTuple() => _tuple;
        }
    }
}