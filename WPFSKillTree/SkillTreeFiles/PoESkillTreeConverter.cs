using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.SkillTreeFiles
{
    public class PoESkillTreeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PoESkillTree);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            serializer.Converters.Add(new SkillTreeGroupConverter());

            var jObject = JObject.Load(reader);
            var skillTree = new PoESkillTree();

            if (jObject.GetValue("nodes") is JToken nodes && nodes.Type == JTokenType.Array)
            {
                skillTree.Nodes = nodes.ToObject<List<SkillNode>>().ToDictionary(i => i.Id, i => i);
                jObject.Remove("nodes");
            }

            if (jObject.GetValue("skillSprites") is JToken spritesToken)
            {
                var sprites = spritesToken.ToObject<Dictionary<string, List<OldSkillSprite>>>()!;
                if (sprites.Count == 2)
                {
                    skillTree.SkillSprites = new Dictionary<string, List<SkillSprite>>();
                    foreach (var i in sprites)
                    {
                        //i.Key == "active", "inactive"
                        var key = $"{i.Key.First().ToString().ToUpper()}{i.Key.Substring(1)}";
                        foreach (var j in i.Value)
                        {
                            if (!skillTree.SkillSprites.ContainsKey($"normal{key}"))
                            {
                                skillTree.SkillSprites.Add($"normal{key}", new List<SkillSprite>());
                            }
                            skillTree.SkillSprites[$"normal{key}"].Add(new SkillSprite() { FileName = j.FileName, Coords = j.Coords ?? new Dictionary<string, Art2D>() });

                            if (!skillTree.SkillSprites.ContainsKey($"notable{key}"))
                            {
                                skillTree.SkillSprites.Add($"notable{key}", new List<SkillSprite>());
                            }
                            skillTree.SkillSprites[$"notable{key}"].Add(new SkillSprite() { FileName = j.FileName, Coords = j.NotableCoords ?? new Dictionary<string, Art2D>() });

                            if (!skillTree.SkillSprites.ContainsKey($"keystone{key}"))
                            {
                                skillTree.SkillSprites.Add($"keystone{key}", new List<SkillSprite>());
                            }
                            skillTree.SkillSprites[$"keystone{key}"].Add(new SkillSprite() { FileName = j.FileName, Coords = j.KeystoneCoords ?? new Dictionary<string, Art2D>() });
                        }
                    }
                    jObject.Remove("skillSprites");
                }
            }

            serializer.Populate(jObject.CreateReader(), skillTree);
            return skillTree;
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException($"Unnecessary because {nameof(CanWrite)} is false. The type will skip the converter.");
        }
    }

    public class SkillTreeGroupConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SkillNodeGroup);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var group = new SkillNodeGroup();

            if (jObject.GetValue("oo") is JToken oo && oo.Type == JTokenType.Array)
            {
                group.OccupiedOrbits = oo.ToObject<List<bool>>().Select((value, index) => new { Index = index, Value = value }).ToDictionary(i => i.Index, i => i.Value);
                jObject.Remove("oo");
            }

            serializer.Populate(jObject.CreateReader(), group);
            return group;
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException($"Unnecessary because {nameof(CanWrite)} is false. The type will skip the converter.");
        }
    }
}
