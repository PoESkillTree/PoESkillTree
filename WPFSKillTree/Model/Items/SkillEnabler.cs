using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;

namespace PoESkillTree.Model.Items
{
    public class SkillEnabler
    {
        private readonly Dictionary<DictKey, bool> _enabledDict = new Dictionary<DictKey, bool>();

        public Skill Apply(Skill skill)
        {
            if (_enabledDict.TryGetValue(new DictKey(skill), out var value) && value != skill.IsEnabled)
            {
                if (skill.Gem is null)
                {
                    return Skill.FromItem(skill.Id, skill.Level, skill.Quality, skill.ItemSlot, skill.SkillIndex, value);
                }
                else if (skill.SkillIndex > 0)
                {
                    return Skill.SecondaryFromGem(skill.Id, skill.Gem, value);
                }
                else
                {
                    return Skill.FromGem(skill.Gem, value);
                }
            }

            return skill;
        }

        public void Store(IReadOnlyCollection<IReadOnlyList<Skill>> skillsToAdd, IReadOnlyCollection<IReadOnlyList<Skill>> skillsToRemove)
        {
            var storedKeys = new HashSet<DictKey>();
            foreach (var skill in skillsToAdd.Flatten())
            {
                var key = new DictKey(skill);
                _enabledDict[key] = skill.IsEnabled;
                storedKeys.Add(key);
            }
            foreach (var skill in skillsToRemove.Flatten())
            {
                var key = new DictKey(skill);
                if (!storedKeys.Contains(key))
                {
                    _enabledDict.Remove(key);
                }
            }
            JsonRepresentationChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetIsEnabled(Gem gem, int skillIndex, bool isEnabled)
        {
            _enabledDict[new DictKey(gem, skillIndex)] = isEnabled;
        }

        public void FromJson(JToken json)
        {
            _enabledDict.Clear();
            foreach (var representation in json.ToObject<JsonRepresentation[]>()!)
            {
                _enabledDict[new DictKey(representation)] = representation.IsEnabled;
            }
            JsonRepresentationChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? JsonRepresentationChanged;

        public string ToJsonString()
        {
            var representations = _enabledDict.Select(p => new JsonRepresentation
            {
                Id = p.Key.Tuple.id,
                ItemSlot = p.Key.Tuple.itemSlot,
                SocketIndex = p.Key.Tuple.socketIndex,
                SkillIndex = p.Key.Tuple.skillIndex,
                IsEnabled = p.Value
            });
            return JsonConvert.SerializeObject(representations);
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