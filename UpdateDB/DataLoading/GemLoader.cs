using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json.Linq;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils.Extensions;

namespace UpdateDB.DataLoading
{
    public class GemLoader : DataLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GemLoader));

        private const string RepoeGemsUrl = "https://raw.githubusercontent.com/brather1ng/RePoE/master/data/gems.min.json";
        private const string RepoeGemTooltipsUrl = "https://raw.githubusercontent.com/brather1ng/RePoE/master/data/gem_tooltips.min.json";

        public override bool SavePathIsFolder => false;

        private JObject _gemsJson;
        private JObject _gemTooltipsJson;

        protected override async Task LoadAsync()
        {
            await LoadRePoEAsync();

            foreach (var gemId in _gemsJson.Properties().Select(p => p.Name))
            {
                var gemObj = _gemsJson.Value<JObject>(gemId);
                if (gemObj["base_item"].Type == JTokenType.Null)
                {
                    Log.Info($"Skipping gem without base item with id {gemId}m");
                    continue;
                }
                var baseItem = gemObj.Value<JObject>("base_item");
                if (baseItem.Value<string>("release_state") == "unreleased")
                {
                    Log.Info($"Skipping unreleased gem with id {gemId}");
                    continue;
                }

                var tooltipObj = _gemTooltipsJson.Value<JObject>(gemId);
                var tooltipStatic = tooltipObj.Value<JObject>("static");
                if (tooltipStatic.Count == 0)
                {
                    Log.Warn($"Skipping gem with id {gemId} because 'static' is empty");
                    continue;
                }

                var name = tooltipStatic.Value<string>("name");
                var tags = tooltipStatic.Value<JArray>("properties").Value<string>(0);
                var attributes = new List<ItemDB.Attribute>();
                var levelProps = tooltipObj.Value<JObject>("per_level").Properties().ToList();
                var levelInts = levelProps.Select(p => p.Name.ParseInt()).ToList();
                var levelObjects = levelProps.Select(p => p.Value).Cast<JObject>().ToList();
                // properties
                // skip line with gem tags and line with gem level
                attributes.AddRange(ParseAttributes(levelInts, levelObjects, "properties", 2));
                // quality stats
                attributes.AddRange(ParseAttributes(levelInts, levelObjects, "quality_stats", atQuality: 1));
                // stats
                attributes.AddRange(ParseAttributes(levelInts, levelObjects, "stats"));
                var gem = new ItemDB.Gem
                {
                    Name = name,
                    Tags = tags,
                    Attributes = attributes
                    // todo IncludeForm, ExcludeForm, ExcludeFormSource, ExcludeSupport?
                };

                var activeSkill = gemObj["active_skill"];
                if (activeSkill != null)
                {
                    var types = activeSkill.Value<JArray>("types").Values<string>().ToHashSet();
                    if (types.Contains("shield_only"))
                    {
                        gem.RequiresEquippedShield = true;
                    }
                    if (types.Contains("dual_wield_only"))
                    {
                        gem.RequiredHand = Compute.WeaponHand.DualWielded;
                    }
                    else if (types.Contains("uses_main_hand_when_dual_wielding"))
                    {
                        gem.RequiredHand = Compute.WeaponHand.Main;
                    }
                    // Reposte doesn't have the the active skill type
                    else if (types.Contains("uses_both_at_once_when_dual_wielding") || gem.Name == "Riposte")
                    {
                        gem.StrikesWithBothWeapons = true;
                    }
                    if (types.Contains("attack"))
                    {
                        var weaponRestrictions = activeSkill.Value<JArray>("weapon_restrictions").Values<string>().ToList();
                        foreach (var restriction in weaponRestrictions)
                        {
                            var w = restriction.Replace(" Hand ", " Handed ");
                            if (w == "Thrusting One Handed Sword")
                            {
                                w = "One Handed Sword";
                            }
                            else if (w == "Sceptre")
                            {
                                w = "One Handed Mace";
                            }
                            w = Regex.Replace(w, @"([a-z]) ([A-Z])", "$1$2");
                            Compute.WeaponType weaponType;
                            if (Enum.TryParse(w, out weaponType))
                            {
                                gem.RequiredWeapon |= weaponType;
                            }
                            else
                            {
                                Log.Error("Unknown weapon type: " + w);
                            }
                        }
                    }
                }

                ItemDB.Add(gem);
            }
        }

        private async Task LoadRePoEAsync()
        {
            var gemsJsonTask = HttpClient.GetStringAsync(RepoeGemsUrl);
            var gemTooltipsJsonTask = HttpClient.GetStringAsync(RepoeGemTooltipsUrl);
            _gemsJson = JObject.Parse(await gemsJsonTask);
            MergeStaticWithPerLevel(_gemsJson);
            _gemTooltipsJson = JObject.Parse(await gemTooltipsJsonTask);
            MergeStaticWithPerLevel(_gemTooltipsJson);
        }

        private static void MergeStaticWithPerLevel(JObject json)
        {
            foreach (var token in json.PropertyValues())
            {
                var staticObject = token.Value<JObject>("static");
                foreach (var perLevelProp in token["per_level"].Cast<JProperty>())
                {
                    MergeObject(staticObject, (JObject) perLevelProp.Value);
                }
            }
        }

        private static void MergeObject(JObject staticObject, JObject perLevelObject)
        {
            foreach (var staticProp in staticObject.Properties())
            {
                var staticValue = staticProp.Value;
                var staticType = staticValue.Type;
                var propName = staticProp.Name;
                if (staticType == JTokenType.Null)
                {
                    continue;
                }
                JToken perLevelValue;
                if (perLevelObject.TryGetValue(propName, out perLevelValue))
                {
                    if (staticType == JTokenType.Object)
                    {
                        MergeObject((JObject) staticValue, (JObject) perLevelValue);
                    }
                    else if (staticType == JTokenType.Array)
                    {
                        MergeArray((JArray) staticValue, (JArray) perLevelValue);
                    }
                    // for primitives, keep the one from perLevelObject
                }
                else
                {
                    perLevelObject[propName] = staticValue;
                }
            }
        }

        private static void MergeArray(JArray staticArray, JArray perLevelArray)
        {
            if (staticArray.Count != perLevelArray.Count)
            {
                throw new ArgumentException("both JArrays must be of the same size");
            }
            for (int i = 0; i < staticArray.Count; i++)
            {
                var staticValue = staticArray[i];
                var perLevelValue = perLevelArray[i];
                if (staticValue.Type == JTokenType.Null)
                {
                    continue;
                }
                switch (perLevelValue.Type)
                {
                    case JTokenType.Null:
                        perLevelArray[i] = staticValue;
                        break;
                    case JTokenType.Object:
                        MergeObject((JObject) staticValue, (JObject) perLevelValue);
                        break;
                    case JTokenType.Array:
                        MergeArray((JArray) staticValue, (JArray) perLevelValue);
                        break;
                    // for primitives, keep the one from perLevelObject
                }
            }
        }

        private static IEnumerable<ItemDB.Attribute> ParseAttributes(IReadOnlyList<int> levels,
            IReadOnlyList<JObject> levelObjects, string propertyName, int skip = 0, int atQuality = 0)
        {
            if (levelObjects.Any(o => o[propertyName] == null))
            {
                Log.Warn($"At least one level object doesn't have the property {propertyName}");
                return Enumerable.Empty<ItemDB.Attribute>();
            }
            Func<JObject, List<JObject>> selector = 
                o => o.Value<JArray>(propertyName).Skip(skip).Cast<JObject>().ToList();
            var levelArrays = levelObjects.Select(selector).ToList();
            if (!levelArrays.Any())
                return Enumerable.Empty<ItemDB.Attribute>();
            var attributes = new List<ItemDB.Attribute>();
            for (var i = 0; i < levelArrays[0].Count; i++)
            {
                var attrs = ParseAttribute(levels, levelArrays.Select(a => a[i]).ToList(), atQuality);
                attributes.AddRange(attrs);
            }
            return attributes;
        }

        private static IEnumerable<ItemDB.Attribute> ParseAttribute(IReadOnlyList<int> levels, 
            IReadOnlyList<JObject> attributeObjects, int atQuality = 0)
        {
            var nameToValues = new Dictionary<string, List<ItemDB.Value>>();
            for (var i = 0; i < levels.Count; i++)
            {
                var level = levels[i];
                var attrObj = attributeObjects[i];
                var name = Regex.Replace(attrObj.Value<string>("text"), @"{\d}", "#");

                if (!nameToValues.ContainsKey(name))
                {
                    nameToValues[name] = new List<ItemDB.Value>();
                }
                var valuesForName = nameToValues[name];

                JToken value;
                JToken values;
                if (attrObj.TryGetValue("value", out value))
                {
                    valuesForName.Add(new ItemDB.ValueAt
                    {
                        Level = level,
                        Quality = atQuality,
                        Text = ParseValue(value)
                    });
                }
                else if (attrObj.TryGetValue("values", out values))
                {
                    // XML deserializer reads whitespace only as null, we don't want that.
                    var text = values.Any() ? string.Join(" ", values.Select(ParseValue)) : "_";
                    valuesForName.Add(new ItemDB.ValueAt
                    {
                        Level = level,
                        Quality = atQuality,
                        Text = text
                    });
                }
            }
            foreach (var pair in nameToValues)
            {
                yield return new ItemDB.Attribute
                {
                    Name = pair.Key,
                    Values = pair.Value
                };
            }
        }

        private static string ParseValue(JToken value)
        {
            switch (value.Type)
            {
                case JTokenType.Float:
                    return ((float)value).ToString(CultureInfo.InvariantCulture);
                default:
                    return value.ToString();
            }
        }

        protected override Task CompleteSavingAsync()
        {
            ItemDB.WriteToCompletePath(SavePath);
            return Task.WhenAll();
        }
    }
}