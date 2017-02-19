using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MoreLinq;
using POESKillTree.Model.Items.Affixes;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Utils;

namespace POESKillTree.Model.Items
{
    public class EquipmentData
    {
        private const string ResourcePath =
            "pack://application:,,,/PoESkillTree;component/Data/Equipment/";

        public IReadOnlyDictionary<ItemType, IReadOnlyList<Affix>> AffixesPerItemType { get; private set; }

        public IReadOnlyList<ItemBase> ItemBases { get; private set; }

        public IReadOnlyDictionary<string, ItemBase> ItemBaseDictionary { get; private set; }

        public IReadOnlyList<UniqueBase> UniqueBases { get; private set; }

        public IReadOnlyDictionary<string, UniqueBase> UniqueBaseDictionary { get; private set; }

        private readonly ItemImageService _itemImageService;

        // ReSharper disable once ConvertToAutoPropertyWhenPossible 
        // (private field is used to reduce ambiguity between class and property)
        public ItemImageService ItemImageService { get { return _itemImageService; } }

        private WordSetTreeNode _root;

        private EquipmentData(Options options)
        {
            _itemImageService = new ItemImageService(options);
        }

        private async Task InitializeAsync()
        {
            AffixesPerItemType =
                (from a in await LoadAffixes()
                 group a by a.ItemType into types
                 select types)
                 .ToDictionary(g => g.Key, g => (IReadOnlyList<Affix>)new List<Affix>(g));

            ItemBases = (await LoadBases()).ToList();
            UniqueBases = (await LoadUniques()).ToList();

            ItemBaseDictionary = ItemBases.DistinctBy(b => b.Name).ToDictionary(b => b.Name);
            UniqueBaseDictionary = UniqueBases.DistinctBy(b => b.UniqueName).ToDictionary(b => b.UniqueName);

            _root = new WordSetTreeNode(ItemBases.Select(m => m.Name));
        }

        public static async Task<EquipmentData> CreateAsync(Options options)
        {
            var o = new EquipmentData(options);
            await o.InitializeAsync();
            return o;
        }

        private static async Task<IEnumerable<XmlAffix>> LoadAffixFile(string fileName)
        {
            var affixList = await DeserializeResourceAsync<XmlAffixList>(fileName);
            return affixList.Affixes;
        }

        private static async Task<IEnumerable<Affix>> LoadAffixes()
        {
            return (await LoadAffixFile("Affixes.xml"))
                    .Union(await LoadAffixFile("SignatureAffixes.xml"))
                    .SelectMany(GroupToTypes)
                    .Select(x => new Affix(x));
        }

        private static IEnumerable<XmlAffix> GroupToTypes(XmlAffix affix)
        {
            if (affix.ItemGroup == ItemGroup.Unknown)
            {
                yield return affix;
            }
            else
            {
                foreach (var itemType in affix.ItemGroup.Types())
                {
                    yield return new XmlAffix
                    {
                        Global = affix.Global,
                        ItemGroup = ItemGroup.Unknown,
                        ItemType = itemType,
                        ModType = affix.ModType,
                        Name = affix.Name,
                        Tiers = affix.Tiers
                    };
                }
            }
        }

        private async Task<IEnumerable<ItemBase>> LoadBases()
        {
            var xmlList = await DeserializeResourceAsync<XmlItemList>("Items.xml");
            return xmlList.ItemBases.Select(x => new ItemBase(_itemImageService, x));
        }

        private async Task<IEnumerable<UniqueBase>> LoadUniques()
        {
            var metadataToBase = ItemBases.ToDictionary(b => b.MetadataId);
            var xmlList = await DeserializeResourceAsync<XmlUniqueList>("Uniques.xml");
            return xmlList.Uniques.Select(
                x => new UniqueBase(_itemImageService, metadataToBase[x.BaseMetadataId], x));
        }

        private static async Task<T> DeserializeResourceAsync<T>(string file)
        {
            var resource = Application.GetResourceStream(new Uri(ResourcePath + file));
            using (var stream = resource.Stream)
            using (var reader = new StreamReader(stream))
            {
                var text = await reader.ReadToEndAsync();
                return SerializationUtils.XmlDeserializeString<T>(text);
            }
        }

        public ItemBase ItemBaseFromTypeline(string typeline)
        {
            var wlist = typeline.Split(' ');
            var ms = new List<WordSetTreeNode>();

            for (int i = 0; i < wlist.Length; i++)
            {
                var m = _root.Match(wlist.Skip(i)).OrderByDescending(n => n.Level).FirstOrDefault();
                if (m != null)
                    ms.Add(m);
            }

            if (ms.Count == 0)
                return null;
            if (ms.Count > 1)
                throw new NotSupportedException("duplicate type match");
            return ItemBaseDictionary[ms[0].ToString()];
        }


        private class WordSetTreeNode
        {
            public int Level { get; private set; }

            private string _word;

            private WordSetTreeNode _parent;

            private bool _isLeaf;

            private readonly Dictionary<string, WordSetTreeNode> _children = new Dictionary<string, WordSetTreeNode>();

            private WordSetTreeNode()
            {
            }

            public WordSetTreeNode(IEnumerable<string> names)
            {
                var allbs = names.Select(m => m.Split(' ')).OrderBy(s => s[0]);
                foreach (var wl in allbs)
                    AddWordset(wl);
            }

            private void AddWordset(IEnumerable<string> words)
            {
                var enumerated = words as string[] ?? words.ToArray();

                var word = enumerated.First();

                WordSetTreeNode nod;
                if (!_children.TryGetValue(word, out nod))
                {
                    nod = new WordSetTreeNode { _word = word, _parent = this, Level = Level + 1 };
                    _children.Add(word, nod);
                }

                if (enumerated.Length > 1)
                    nod.AddWordset(enumerated.Skip(1));
                else
                    nod._isLeaf = true;
            }

            public IEnumerable<WordSetTreeNode> Match(IEnumerable<string> enumerable)
            {
                var nod = this;
                foreach (var w in enumerable)
                {
                    if (nod._isLeaf)
                        yield return nod;

                    if (nod._children == null || !nod._children.TryGetValue(w, out nod))
                        yield break;
                }

                if (nod._isLeaf)
                    yield return nod;
            }

            public override string ToString()
            {
                var lst = new Stack<string>();
                var nod = this;
                while (nod._parent != null)
                {
                    lst.Push(nod._word);
                    nod = nod._parent;
                }

                return string.Join(" ", lst);
            }
        }
    }
}