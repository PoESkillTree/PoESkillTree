using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Model.Builds;
using PoESkillTree.Model.Items;
using PoESkillTree.SkillTreeFiles;
using Item = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.Model.Serialization.PathOfBuilding
{
    public class PathOfBuildingImporter
    {
        private readonly HttpClient _httpClient;
        private readonly EquipmentData _equipmentData;
        private readonly ItemConverter _itemConverter;

        public PathOfBuildingImporter(HttpClient httpClient, EquipmentData equipmentData)
        {
            _httpClient = httpClient;
            _equipmentData = equipmentData;
            _itemConverter = new ItemConverter(equipmentData);
        }

        public async Task<IBuild?> FromPastebinAsync(string pastebinUrl)
        {
            var url = pastebinUrl.Replace("pastebin.com/", "pastebin.com/raw/");
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var base64 = await response.Content.ReadAsStringAsync();
            return await FromBase64Async(base64);
        }

        public async Task<IBuild?> FromBase64Async(string input)
        {
            var compressed = Convert.FromBase64String(input.Replace('-', '+').Replace('_', '/'));
            await using var ms = new MemoryStream(compressed);
            // Skip compression type header
            ms.Seek(2, SeekOrigin.Begin);
            await using var deflateStream = new DeflateStream(ms, CompressionMode.Decompress);
            var xmlBuild = await XmlSerializationUtils.DeserializeAsync<XmlPathOfBuilding>(new StreamReader(deflateStream));
            return ConvertXmlBuild(xmlBuild);
        }

        private IBuild ConvertXmlBuild(XmlPathOfBuilding xmlBuild)
        {
            var items = ConvertItems(xmlBuild.Items.Items);
            var gems = ConvertSkills(xmlBuild.Skills.Skills).ToList();
            var specs = xmlBuild.Tree.Specs;
            if (specs.IsEmpty())
            {
                return ConvertXmlBuild(xmlBuild, new XmlPathOfBuildingTreeSpec {Url = Constants.DefaultTree}, items, gems);
            }
            else if (specs.Count == 1)
            {
                return ConvertXmlBuild(xmlBuild, specs.Single(), items, gems);
            }
            else
            {
                var hasDifferentTreeVersions = specs.Select(s => s.TreeVersion).Distinct().Count() > 1;
                var folder = new BuildFolder {Name = "PoB Import"};
                foreach (var spec in specs)
                {
                    folder.Builds.Add(ConvertXmlBuild(xmlBuild, spec, items, gems, hasDifferentTreeVersions));
                }
                return folder;
            }
        }

        private IReadOnlyDictionary<int, Item> ConvertItems(IEnumerable<XmlPathOfBuildingItem> xmlItems)
            => xmlItems.ToDictionary(x => x.Id, _itemConverter.Convert);

        private static IEnumerable<PoBGem> ConvertSkills(IEnumerable<XmlPathOfBuildingSkill> xmlSkills)
        {
            var socketIndex = 0;
            var gemGroup = 0;
            foreach (var xmlSkill in xmlSkills)
            {
                if (xmlSkill.Source != null)
                {
                    // item-inherent skill
                    continue;
                }

                var slot = ConvertItemSlot(xmlSkill.Slot, ItemSlot.Amulet);
                foreach (var xmlGem in xmlSkill.Gems)
                {
                    var isEnabled = xmlSkill.Enabled && xmlGem.Enabled;
                    var gem = new Gem(xmlGem.SkillId, xmlGem.Level, xmlGem.Quality, slot, socketIndex, gemGroup, isEnabled);
                    yield return new PoBGem(gem, xmlGem.PrimarySkillEnabled, xmlGem.SecondarySkillEnabled);
                    socketIndex++;
                }

                gemGroup++;
            }
        }

        private PoEBuild ConvertXmlBuild(
            XmlPathOfBuilding xmlBuild, XmlPathOfBuildingTreeSpec treeSpec, IReadOnlyDictionary<int, Item> items, IEnumerable<PoBGem> gems,
            bool addTreeVersionToName = false)
        {
            var name = treeSpec.Title ?? "Default";
            if (addTreeVersionToName && treeSpec.TreeVersion != null)
            {
                name += $" ({treeSpec.TreeVersion})";
            }
            return new PoEBuild(
                new BanditSettings {Choice = xmlBuild.Build.Bandit},
                new string[0][],
                new ushort[0],
                new ushort[0],
                ConfigurationStatConverter.Convert(xmlBuild.Config).Concat(ConvertMainSkillConfiguration(xmlBuild)),
                null)
            {
                ItemData = ConvertItemData(xmlBuild, items, gems, treeSpec.Sockets),
                Level = xmlBuild.Build.Level,
                Name = name,
                Note = xmlBuild.Notes?.Trim(),
                TreeUrl = treeSpec.Url.Trim(),
            };
        }

        private static IEnumerable<(string, double?)> ConvertMainSkillConfiguration(XmlPathOfBuilding xmlBuild)
        {
            if (xmlBuild.Skills.Skills.Count < xmlBuild.Build.MainSocketGroup)
                yield break;

            var xmlSkill = xmlBuild.Skills.Skills[xmlBuild.Build.MainSocketGroup - 1];
            var slot = ConvertItemSlot(xmlSkill.Slot, ItemSlot.Amulet);
            yield return ("Character.MainSkillItemSlot", (int) slot);

            if (xmlSkill.Gems.Count < xmlSkill.MainActiveSkill)
                yield break;
            yield return ("Character.MainSkillSocketIndex", xmlSkill.MainActiveSkill);

            var xmlGem = xmlSkill.Gems[xmlSkill.MainActiveSkill - 1];
            if (xmlGem.SkillPart > 0)
            {
                yield return ("Character.MainSkillPart", xmlGem.SkillPart);
            }
        }

        private string ConvertItemData(
            XmlPathOfBuilding xmlBuild, IReadOnlyDictionary<int, Item> items, IEnumerable<PoBGem> gems, IEnumerable<XmlPathOfBuildingTreeSocket> sockets)
        {
            var itemSerializer = new ItemAttributes(_equipmentData, null!);

            foreach (var (slot, group) in gems.GroupBy(s => s.Gem.ItemSlot))
            {
                var groupAsList = group.ToList();
                itemSerializer.SetGemsInSlot(groupAsList.Select(g => g.Gem).ToList(), slot);
                foreach (var poBGem in groupAsList)
                {
                    itemSerializer.SkillEnabler.SetIsEnabled(poBGem.Gem, 0, poBGem.PrimarySkillIsEnabled);
                    itemSerializer.SkillEnabler.SetIsEnabled(poBGem.Gem, 0, poBGem.SecondarySkillIsEnabled);
                }
            }

            foreach (var xmlSlot in xmlBuild.Items.Slots)
            {
                var (slotString, socket) = ConvertItemSocket(xmlSlot.Name);
                var slot = ConvertItemSlot(slotString);
                if (slot == ItemSlot.Unequipable)
                    continue;
                var item = new Item(items[xmlSlot.ItemId])
                {
                    IsEnabled = xmlSlot.Active || !slot.IsFlask()
                };
                itemSerializer.SetItemInSlot(item, slot, socket);
            }

            foreach (var xmlTreeSocket in sockets)
            {
                if (items.TryGetValue(xmlTreeSocket.ItemId, out var item))
                {
                    itemSerializer.SetItemInSlot(new Item(item), ItemSlot.SkillTree, (ushort) xmlTreeSocket.NodeId);
                }
            }

            return itemSerializer.ToJsonString();
        }

        private static ItemSlot ConvertItemSlot(string? slot, ItemSlot nullSlot = ItemSlot.Unequipable) =>
            slot switch
            {
                "Body Armour" => ItemSlot.BodyArmour,
                "Weapon 1" => ItemSlot.MainHand,
                "Weapon 2" => ItemSlot.OffHand,
                "Ring 1" => ItemSlot.Ring,
                "Ring 2" => ItemSlot.Ring2,
                "Amulet" => ItemSlot.Amulet,
                "Helmet" => ItemSlot.Helm,
                "Gloves" => ItemSlot.Gloves,
                "Boots" => ItemSlot.Boots,
                "Belt" => ItemSlot.Belt,
                "Flask 1" => ItemSlot.Flask1,
                "Flask 2" => ItemSlot.Flask2,
                "Flask 3" => ItemSlot.Flask3,
                "Flask 4" => ItemSlot.Flask4,
                "Flask 5" => ItemSlot.Flask5,
                null => nullSlot,
                _ => ItemSlot.Unequipable,
            };

        private static (string slotWithoutSocket, ushort? socket) ConvertItemSocket(string slot)
        {
            var match = Regex.Match(slot, @"^(.+) Abyssal Socket (\d+)$");
            if (match.Success)
                return (match.Groups[1].Value, ushort.Parse(match.Groups[2].Value));
            return (slot, null);
        }

        private class PoBGem
        {
            public PoBGem(Gem gem, bool primarySkillIsEnabled, bool secondarySkillIsEnabled)
            {
                Gem = gem;
                PrimarySkillIsEnabled = primarySkillIsEnabled;
                SecondarySkillIsEnabled = secondarySkillIsEnabled;
            }

            public Gem Gem { get; }

            public bool PrimarySkillIsEnabled { get; }

            public bool SecondarySkillIsEnabled { get; }
        }
    }
}