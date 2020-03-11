using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;

namespace PoESkillTree.Model.Items
{
    public class SkillEnabler
    {
        private readonly Dictionary<DictKey, bool> _enabledDict = new Dictionary<DictKey, bool>();

        public Skill Apply(Skill skill)
        {
            if (_enabledDict.TryGetValue(new DictKey(skill), out var value) && value != skill.IsEnabled)
            {
                return skill.WithIsEnabled(value);
            }

            return skill;
        }

        public void SetIsEnabled(Gem gem, int skillIndex, bool isEnabled)
        {
            var key = new DictKey(gem, skillIndex);
            if (_enabledDict.TryGetValue(key, out var value) && value == isEnabled)
                return;
            _enabledDict[key] = isEnabled;
            EnabledChangedForSlots?.Invoke(this, new[] {key.Tuple.itemSlot});
        }

        public void FromJson(JToken json)
        {
            var slots = _enabledDict.Keys.Select(k => k.Tuple.itemSlot).ToHashSet();
            _enabledDict.Clear();
            foreach (var representation in json.ToObject<JsonRepresentation[]>()!)
            {
                slots.Add(representation.ItemSlot);
                _enabledDict[new DictKey(representation)] = representation.IsEnabled;
            }
            EnabledChangedForSlots?.Invoke(this, slots);
        }

        public event EventHandler<IReadOnlyCollection<ItemSlot>>? EnabledChangedForSlots;

        public JToken ToJson()
        {
            var representations = _enabledDict.Select(p => new JsonRepresentation
            {
                Id = p.Key.Tuple.id,
                ItemSlot = p.Key.Tuple.itemSlot,
                SocketIndex = p.Key.Tuple.socketIndex,
                SkillIndex = p.Key.Tuple.skillIndex,
                IsEnabled = p.Value
            });
            return JArray.FromObject(representations);
        }

        private class DictKey : ValueObject
        {
            public DictKey(Skill skill)
            {
                Tuple = (skill.Gem?.SkillId ?? skill.Id, skill.ItemSlot, skill.SocketIndex, skill.SkillIndex);
            }

            public DictKey(Gem gem, int skillIndex)
            {
                Tuple = (gem.SkillId, gem.ItemSlot, gem.SocketIndex, skillIndex);
            }

            public DictKey(JsonRepresentation jsonRepresentation)
            {
                Tuple = (jsonRepresentation.Id, jsonRepresentation.ItemSlot, jsonRepresentation.SocketIndex, jsonRepresentation.SkillIndex);
            }

            public (string id, ItemSlot itemSlot, int socketIndex, int skillIndex) Tuple { get; }

            protected override object ToTuple() => Tuple;
        }

        private class JsonRepresentation
        {
            public string Id { get; set; } = "";
            public ItemSlot ItemSlot { get; set; }
            public int SocketIndex { get; set; }
            public int SkillIndex { get; set; }

            public bool IsEnabled { get; set; }
        }
    }
}