using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.StatTranslation;
using PoESkillTree.Model.Items.Mods;

namespace PoESkillTree.Model.Items
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
            var modsTask = DataUtils.LoadRePoEAsync<Dictionary<string, JsonMod>>("mods", true);
            var benchOptionsTask = DataUtils.LoadRePoEAsync<JsonCraftingBenchOption[]>("crafting_bench_options", true);
            var statTranslatorTask = StatTranslators.CreateFromMainFileAsync(true);
            ModDatabase = new ModDatabase(await modsTask, await benchOptionsTask);

            ItemBases = await LoadBases();
            UniqueBases = await LoadUniques();
            StatTranslator = await statTranslatorTask;

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

        private async Task<IReadOnlyList<ItemBase>> LoadBases()
        {
            var xmlList = await DataUtils.LoadXmlAsync<XmlItemList>("Equipment.Items.xml", true);
            return xmlList.ItemBases.Select(x => new ItemBase(_itemImageService, ModDatabase, x)).ToList();
        }

        private async Task<IReadOnlyList<UniqueBase>> LoadUniques()
        {
            var metadataToBase = ItemBases.ToDictionary(b => b.MetadataId);
            var xmlList = await DataUtils.LoadXmlAsync<XmlUniqueList>("Equipment.Uniques.xml", true);
            return xmlList.Uniques.Select(
                x => new UniqueBase(_itemImageService, ModDatabase, metadataToBase[x.BaseMetadataId], x)).ToList();
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