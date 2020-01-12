using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Modifiers;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Model.Items;
using PoESkillTree.Model.Items.Mods;
using PoESkillTree.Utils.Extensions;
using Item = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.Model.Serialization.PathOfBuilding
{
    public class ItemConverter
    {
        private readonly EquipmentData _equipmentData;

        public ItemConverter(EquipmentData equipmentData)
        {
            _equipmentData = equipmentData;
        }

        public Item Convert(XmlPathOfBuildingItem xmlItem)
        {
            var lines = xmlItem.Data.Trim().Split('\n')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            var item = CreateItem(lines);

            var i = (item.Frame == FrameType.White || item.Frame == FrameType.Magic) ? 2 : 3;

            var quality = 0;
            var levelRequirement = 0;
            var implicitCount = 0;
            for (; i < lines.Count; i++)
            {
                var line = lines[i];
                var colonPos = line.IndexOf(':');
                if (colonPos < 0 || colonPos + 1 >= line.Length)
                    continue;

                var key = line.Substring(0, colonPos).Trim();
                var value = line.Substring(colonPos + 1).Trim();

                if (key == "Quality")
                {
                    quality = int.Parse(value);
                }
                else if (key == "LevelReq")
                {
                    levelRequirement = int.Parse(value);
                }
                else if (key == "Implicits")
                {
                    implicitCount = int.Parse(value);
                    break;
                }
            }

            ConvertMods(xmlItem, lines, i + 1, item, implicitCount);

            item.UpdateProperties(quality);
            item.UpdateRequirements(levelRequirement);

            return item;
        }

        private Item CreateItem(IReadOnlyList<string> lines)
        {
            var frameType = GetFrameType(lines[0]);
            string nameLine;
            string typeLine;
            if (frameType == FrameType.White || frameType == FrameType.Magic)
            {
                nameLine = "";
                typeLine = lines[1];
            }
            else
            {
                nameLine = lines[1];
                typeLine = lines[2];
            }

            var itemBase = GetItemBase(frameType, nameLine, typeLine);
            return new Item(itemBase)
            {
                Frame = frameType,
                NameLine = nameLine,
                TypeLine = typeLine,
            };
        }

        private static FrameType GetFrameType(string rarityLine) =>
            rarityLine.Replace("Rarity: ", "").ToLowerInvariant() switch
            {
                "relic" => FrameType.Foil,
                "unique" => FrameType.Unique,
                "rare" => FrameType.Rare,
                "magic" => FrameType.Magic,
                "normal" => FrameType.White,
                _ => FrameType.Rare,
            };

        private IItemBase GetItemBase(FrameType frameType, string nameLine, string typeLine)
        {
            if (frameType == FrameType.Magic && _equipmentData.ItemBaseFromTypeline(typeLine) is ItemBase mBase)
            {
                return mBase;
            }
            else if ((frameType == FrameType.Unique || frameType == FrameType.Foil)
                && _equipmentData.UniqueBaseDictionary.TryGetValue(nameLine, out var uBase))
            {
                return uBase;
            }
            else if (_equipmentData.ItemBaseDictionary.TryGetValue(typeLine, out var iBase))
            {
                return iBase;
            }
            else
            {
                return new ItemBase(_equipmentData.ItemImageService, ItemSlot.Unequipable, typeLine, frameType);
            }
        }

        private static void ConvertMods(XmlPathOfBuildingItem xmlItem, IReadOnlyList<string> lines, int startIndex, Item item, int implicitCount)
        {
            for (var i = startIndex; i < lines.Count; i++)
            {
                var line = lines[i];

                if (!IsCorrectVariant(xmlItem, ref line))
                    continue;
                var isCrafted = IsCrafted(ref line);
                var valueRange = GetModifierValueRange(ref line);

                var itemMod = CreateItemMod(item.Tags, line, valueRange);

                if (i < implicitCount)
                {
                    item.ImplicitMods.Add(itemMod);
                }
                else if (isCrafted)
                {
                    item.CraftedMods.Add(itemMod);
                }
                else
                {
                    item.ExplicitMods.Add(itemMod);
                }
            }
        }

        private static bool IsCorrectVariant(XmlPathOfBuildingItem xmlItem, ref string modifierLine)
        {
            var match = Regex.Match(modifierLine, @"{variant:(\d+(,\d+)*)}");
            if (!match.Success)
                return true;

            modifierLine = modifierLine.Replace(match.Groups[0].Value, "");
            var variants = match.Groups[1].Value.Split(',').Select(int.Parse);
            return variants.Any(v => v == xmlItem.Variant || v == xmlItem.VariantAlt);
        }

        private static bool IsCrafted(ref string modifierLine)
        {
            var modIsCrafted = modifierLine.Contains("{crafted}");
            if (modIsCrafted)
            {
                modifierLine = modifierLine.Replace("{crafted}", "");
            }

            return modIsCrafted;
        }

        private static float GetModifierValueRange(ref string modifierLine)
        {
            var match = Regex.Match(modifierLine, @"{range:(\d*\.?\d+)}");
            if (!match.Success)
                return 1;
            
            modifierLine = modifierLine.Replace(match.Groups[0].Value, "");
            return match.Groups[1].Value.ParseFloat();
        }

        private static ItemMod CreateItemMod(Tags itemTags, string modifierLine, float valueRange)
        {
            var regex = new Regex(@"(-?\d*\.?\d+)|\((-?\d*\.?\d+)-(-?\d*\.?\d+)\)");

            var modifier = regex.Replace(modifierLine, "#");
            var isLocal = ModifierLocalityTester.IsLocal(modifier, itemTags);
            
            var matches = regex.Matches(modifierLine);
            var values = new List<float>();
            foreach (var match in matches.WhereNotNull())
            {
                if (match.Groups[1].Success)
                {
                    values.Add(match.Groups[1].Value.ParseFloat());
                }
                else
                {
                    var min = match.Groups[2].Value.ParseFloat();
                    var max = match.Groups[3].Value.ParseFloat();
                    values.Add(min + (max - min) * valueRange);
                }
            }

            return new ItemMod(modifier, isLocal, values, values.Select(_ => ValueColoring.White));
        }
    }
}