using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using MoreLinq;
using POESKillTree.Model.Items.Mods;
using POESKillTree.Model.Items.StatTranslation;
using POESKillTree.Utils;

namespace POESKillTree.Model.Items
{
    public class EquipmentData
    {
        private const string ResourcePath =
            "pack://application:,,,/PoESkillTree;component/Data/Equipment/";

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

        private readonly RePoELoader _rePoELoader;

        private WordSetTreeNode _root;

        private EquipmentData(Options options)
        {
            if (!UriParser.IsKnownScheme("pack"))
            {
                // Necessary for unit tests. Accessing PackUriHelper triggers static initialization.
                // Without it, creating resource URIs from unit tests would throw UriFormatExceptions.
                var _ = System.IO.Packaging.PackUriHelper.UriSchemePack;
            }
            _itemImageService = new ItemImageService(options);
            _rePoELoader = new RePoELoader(new HttpClient(), false);
        }

        private async Task InitializeAsync()
        {
            var modsTask = _rePoELoader.LoadAsync<Dictionary<string, JsonMod>>("mods");
            var benchOptionsTask = _rePoELoader.LoadAsync<JsonCraftingBenchOption[]>("crafting_bench_options");
            var npcMastersTask = _rePoELoader.LoadAsync<Dictionary<string, JsonNpcMaster>>("npc_master");
            var statTranslationsTask = _rePoELoader.LoadAsync<List<JsonStatTranslation>>("stat_translations");
            ModDatabase = new ModDatabase(await modsTask, await benchOptionsTask, await npcMastersTask);
            StatTranslator = new StatTranslator(await statTranslationsTask);

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
            var xmlList = await DeserializeResourceAsync<XmlItemList>("Items.xml");
            return xmlList.ItemBases.Select(x => new ItemBase(_itemImageService, ModDatabase, x));
        }

        private async Task<IEnumerable<UniqueBase>> LoadUniques()
        {
            var metadataToBase = ItemBases.ToDictionary(b => b.MetadataId);
            var xmlList = await DeserializeResourceAsync<XmlUniqueList>("Uniques.xml");
            return xmlList.Uniques.Select(
                x => new UniqueBase(_itemImageService, ModDatabase, metadataToBase[x.BaseMetadataId], x));
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