using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.StatTranslation;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Utils;

namespace POESKillTree.Model.Items
{
    public class EquipmentData
    {
        public ModDatabase ModDatabase { get; private set; }
        public StatTranslator StatTranslator { get; private set; }

        public IReadOnlyList<ItemBase> ItemBases { get; private set; }

        public IReadOnlyDictionary<string, ItemBase> ItemBaseDictionary { get; private set; }

        public IReadOnlyList<UniqueBase> UniqueBases { get; private set; }

        public IReadOnlyDictionary<string, UniqueBase> UniqueBaseDictionary { get; private set; }

        private readonly ItemImageService _itemImageService;

        // ReSharper disable once ConvertToAutoPropertyWhenPossible 
        // (private field is used to reduce ambiguity between class and property)
        public ItemImageService ItemImageService => _itemImageService;

        private WordSetTreeNode _root;

        private EquipmentData(Options options)
        {
            _itemImageService = new ItemImageService(options);
        }

        private async Task InitializeAsync()
        {
            var modsTask = DataUtils.LoadRePoEAsync<Dictionary<string, JsonMod>>("mods");
            var benchOptionsTask = DataUtils.LoadRePoEAsync<JsonCraftingBenchOption[]>("crafting_bench_options");
            var statTranslatorTask = StatTranslationLoader.LoadAsync(StatTranslationLoader.MainFileName);
            ModDatabase = new ModDatabase(await modsTask, await benchOptionsTask);
            StatTranslator = await statTranslatorTask;

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

        private async Task<IEnumerable<ItemBase>> LoadBases()
        {
            var xmlList = await DeserializeXmlResourceAsync<XmlItemList>("Items.xml");
            return xmlList.ItemBases.Select(x => new ItemBase(_itemImageService, ModDatabase, x));
        }

        private async Task<IEnumerable<UniqueBase>> LoadUniques()
        {
            var metadataToBase = ItemBases.ToDictionary(b => b.MetadataId);
            var xmlList = await DeserializeXmlResourceAsync<XmlUniqueList>("Uniques.xml");
            return xmlList.Uniques.Select(
                x => new UniqueBase(_itemImageService, ModDatabase, metadataToBase[x.BaseMetadataId], x));
        }

        private static async Task<T> DeserializeXmlResourceAsync<T>(string file)
        {
            var text = await DataUtils.LoadTextAsync("Equipment." + file);
            return SerializationUtils.XmlDeserializeString<T>(text);
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