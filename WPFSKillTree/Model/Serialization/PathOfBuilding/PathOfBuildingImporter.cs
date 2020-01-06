using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;
using PoESkillTree.Model.Builds;
using PoESkillTree.SkillTreeFiles;
using Item = PoESkillTree.Model.Items.Item;

namespace PoESkillTree.Model.Serialization.PathOfBuilding
{
    public class PathOfBuildingImporter
    {
        private readonly HttpClient _httpClient;

        public PathOfBuildingImporter(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

        private static IBuild ConvertXmlBuild(XmlPathOfBuilding xmlBuild)
        {
            var items = ConvertItems(xmlBuild.Items.Items);
            var skills = ConvertSkills(xmlBuild.Skills.Skills);
            var specs = xmlBuild.Tree.Specs;
            if (specs.IsEmpty())
            {
                return ConvertXmlBuild(xmlBuild, new XmlPathOfBuildingTreeSpec {Url = Constants.DefaultTree}, items, skills);
            }
            else if (specs.Count == 1)
            {
                return ConvertXmlBuild(xmlBuild, specs.Single(), items, skills);
            }
            else
            {
                var hasDifferentTreeVersions = specs.Select(s => s.TreeVersion).Distinct().Count() > 1;
                var folder = new BuildFolder {Name = "PoB Import"};
                foreach (var spec in specs)
                {
                    folder.Builds.Add(ConvertXmlBuild(xmlBuild, spec, items, skills, hasDifferentTreeVersions));
                }
                return folder;
            }
        }

        private static IReadOnlyDictionary<int, Item> ConvertItems(IEnumerable<XmlPathOfBuildingItem> xmlItems)
        {
            // TODO
            return new Dictionary<int, Item>();
        }

        private static IReadOnlyList<Skill> ConvertSkills(IEnumerable<XmlPathOfBuildingSkill> xmlSkills)
        {
            // TODO
            return new Skill[0];
        }

        private static PoEBuild ConvertXmlBuild(
            XmlPathOfBuilding xmlBuild, XmlPathOfBuildingTreeSpec treeSpec, IReadOnlyDictionary<int, Item> items, IEnumerable<Skill> skills,
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
                ItemData = ConvertItemData(xmlBuild, items, skills, treeSpec.Sockets),
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
            var slot = xmlSkill.Slot is null ? ItemSlot.Flask1 : ConvertItemSlot(xmlSkill.Slot);
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

        private static string? ConvertItemData(
            XmlPathOfBuilding xmlBuild, IReadOnlyDictionary<int, Item> items, IEnumerable<Skill> skills, IEnumerable<XmlPathOfBuildingTreeSocket> sockets)
        {
            // TODO
            return null;
        }

        private static ItemSlot ConvertItemSlot(string slot) =>
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
                _ => ItemSlot.Unequipable,
            };
    }
}