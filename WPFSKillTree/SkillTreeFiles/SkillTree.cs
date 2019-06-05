using Newtonsoft.Json;
using PoESkillTree.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using log4net;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;
using PoESkillTree.Utils.Extensions;
using PoESkillTree.Common;
using PoESkillTree.Controls.Dialogs;
using PoESkillTree.Localization;
using PoESkillTree.Model;
using PoESkillTree.Utils.UrlProcessing;
using PoESkillTree.Utils.Wpf;
using HighlightState = PoESkillTree.SkillTreeFiles.NodeHighlighter.HighlightState;
using static PoESkillTree.SkillTreeFiles.Constants;

namespace PoESkillTree.SkillTreeFiles
{
    public partial class SkillTree : Notifier, ISkillTree
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SkillTree));

        public Vector2D AscButtonPosition = new Vector2D();
        /// <summary>
        /// Nodes with an attribute matching this regex are one of the "Path of the ..." nodes connection Scion
        /// Ascendant with other classes.
        /// </summary>
        private static readonly Regex AscendantClassStartRegex = new Regex(@"Can Allocate Passives from the .* starting point");

        // The absolute path of Assets folder (contains trailing directory separator).
        private static string _assetsFolderPath;

        public static readonly IReadOnlyList<(string stat, float value)> BaseAttributes = new (string, float)[]
        {
            ("+# to maximum Mana", 34),
            ("+# to maximum Life", 38),
            ("Evasion Rating: #", 53),
            ("+# to Maximum Endurance Charges", 3),
            ("+# to Maximum Frenzy Charges", 3),
            ("+# to Maximum Power Charges", 3),
            ("#% Additional Elemental Resistance per Endurance Charge", 4),
            ("#% Physical Damage Reduction per Endurance Charge", 4),
            ("#% Attack Speed Increase per Frenzy Charge", 4),
            ("#% Cast Speed Increase per Frenzy Charge", 4),
            ("#% More Damage per Frenzy Charge", 4),
            ("#% Critical Strike Chance Increase per Power Charge", 40),
        };

        private static readonly Dictionary<string, List<string>> HybridAttributes = new Dictionary<string, List<string>>
        {
            {
               "+# to Strength and Intelligence",
               new List<string> {"+# to Strength", "+# to Intelligence"}
            },
            {
                "+# to Strength and Dexterity",
                new List<string> {"+# to Strength", "+# to Dexterity"}
            },
            {
                "+# to Dexterity and Intelligence",
                new List<string> {"+# to Dexterity", "+# to Intelligence"}
            }
        };

        public static readonly Dictionary<string, string> RenameImplicitAttributes = new Dictionary<string, string>
        {
            {
                "#% increased Evasion Rating",
                "#% increased Evasion Rating from Dexterity"
            }, {
                "#% increased maximum Energy Shield",
                "#% increased maximum Energy Shield from Intelligence"
            }, {
                "#% increased Melee Physical Damage",
                "#% increased Melee Physical Damage from Strength"
            }
        };

        private static readonly IReadOnlyDictionary<string, CharacterClass> PassiveNodeNameToClass =
            new Dictionary<string, CharacterClass>
            {
                { "SEVEN", CharacterClass.Scion },
                { "MARAUDER", CharacterClass.Marauder },
                { "RANGER", CharacterClass.Ranger },
                { "WITCH", CharacterClass.Witch },
                { "DUELIST", CharacterClass.Duelist },
                { "TEMPLAR", CharacterClass.Templar },
                { "SIX", CharacterClass.Shadow }
            };

        private static readonly List<string> CharacterFaceNames = new List<string>
        {
            "centerscion",
            "centermarauder",
            "centerranger",
            "centerwitch",
            "centerduelist",
            "centertemplar",
            "centershadow"
        };

        private static readonly Dictionary<string, string> NodeBackgrounds = new Dictionary<string, string>
        {
            {"normal", "PSSkillFrame"},
            {"notable", "NotableFrameUnallocated"},
            {"keystone", "KeystoneFrameUnallocated"},
            {"jewel", "JewelFrameUnallocated"},
            {"ascendancyNormal", "PassiveSkillScreenAscendancyFrameSmallNormal"},
            {"ascendancyNotable", "PassiveSkillScreenAscendancyFrameLargeNormal"}
        };

        private static readonly Dictionary<string, string> NodeBackgroundsActive = new Dictionary<string, string>
        {
            {"normal", "PSSkillFrameActive"},
            {"notable", "NotableFrameAllocated"},
            {"keystone", "KeystoneFrameAllocated"},
            {"jewel", "JewelFrameAllocated"},
            {"ascendancyNormal", "PassiveSkillScreenAscendancyFrameSmallAllocated"},
            {"ascendancyNotable", "PassiveSkillScreenAscendancyFrameLargeAllocated"}
        };

        private static SkillIcons IconActiveSkills { get; set; }
        private static SkillIcons IconInActiveSkills { get; set; }
        public static Dictionary<ushort, SkillNode> Skillnodes => PoESkillTree.Nodes;

        private static IEnumerable<string> _allAttributes;
        /// <summary>
        /// Gets an Array of all the attributes of SkillNodes.
        /// </summary>
        public static IEnumerable<string> AllAttributes
        {
            get { return _allAttributes ?? (_allAttributes = Skillnodes.Values.SelectMany(n => n.Attributes.Keys).Distinct().ToArray()); }
        }

        public static Dictionary<CharacterClass, IReadOnlyList<(string stat, float value)>> CharBaseAttributes
        {
            get;
            private set;
        }

        public static List<ushort> RootNodeList { get; private set; }
        private static HashSet<SkillNode> AscRootNodeList { get; set; }
        public static Rect2D SkillTreeRect { get; private set; }
        private static Dictionary<CharacterClass, ushort> RootNodeClassDictionary { get; set; }
        private static Dictionary<ushort, ushort> StartNodeDictionary { get; set; }

        private static Dictionary<string, BitmapImage> Assets { get; } = new Dictionary<string, BitmapImage>();

        public readonly ObservableSet<SkillNode> SkilledNodes = new ObservableSet<SkillNode>();
        public readonly ObservableSet<SkillNode> HighlightedNodes = new ObservableSet<SkillNode>();
        public SkillTreeSerializer Serializer { get; }
        public IAscendancyClasses AscendancyClasses { get; private set; }
        public IBuildConverter BuildConverter { get; private set; }

        private CharacterClass _charClass;
        private int _asctype;
        public static int UndefinedLevel => 0;
        public static int MaximumLevel => 100;
        private int _level = UndefinedLevel;

        public static PoESkillTree PoESkillTree { get; set; } = null;
        public static PoESkillTreeOptions PoESkillTreeOptions { get; set; } = null;

        private static bool _initialized;

        private SkillTree(IPersistentData persistentData)
        {
            _persistentData = persistentData;

            Serializer = new SkillTreeSerializer(this);
        }

        private async Task InitializeAsync(string treestring, string opsstring, [CanBeNull] ProgressDialogController controller,
            AssetLoader assetLoader)
        {
            if (!_initialized)
            {
                PoESkillTree = JsonConvert.DeserializeObject<PoESkillTree>(treestring, new PoESkillTreeConverter());
                PoESkillTreeOptions = JsonConvert.DeserializeObject<PoESkillTreeOptions>(opsstring);

                controller?.SetProgress(0.25);
                await assetLoader.DownloadSkillNodeSpritesAsync(PoESkillTree, d => controller?.SetProgress(0.25 + d * 0.30));
                IconInActiveSkills = new SkillIcons();
                IconActiveSkills = new SkillIcons();
                var assetActions =
                    new List<(Task<BitmapImage>, Action<BitmapImage>)>(PoESkillTree.SkillSprites.Count + PoESkillTree.Assets.Count);
                foreach (var obj in PoESkillTree.SkillSprites)
                {
                    SkillIcons icons;
                    string prefix;
                    foreach (var i in obj.Value)
                    {
                        if (i.FileName.Contains('?'))
                            i.FileName = i.FileName.Remove(i.FileName.IndexOf('?'));
                    }
                    if (obj.Key.EndsWith("Active"))
                    {
                        // Adds active nodes to IconActiveSkills
                        icons = IconActiveSkills;
                        prefix = obj.Key.Substring(0, obj.Key.Length - "Active".Length);
                    }
                    else if (obj.Key.EndsWith("Inactive"))
                    {
                        // Adds inactive nodes to IconInActiveSkills
                        icons = IconInActiveSkills;
                        prefix = obj.Key.Substring(0, obj.Key.Length - "Inactive".Length);
                    }
                    else
                    {
                        // Adds masteries to IconInActiveSkills
                        icons = IconInActiveSkills;
                        prefix = obj.Key;
                    }
                    var sprite = obj.Value[AssetZoomLevel];
                    var path = _assetsFolderPath + sprite.FileName;
                    assetActions.Add(
                        (Task.Run(() => BitmapImageFactory.Create(path)), i => icons.Images[sprite.FileName] = i));
                    foreach (var o in sprite.Coords)
                    {
                        var iconKey = prefix + "_" + o.Key;
                        icons.SkillPositions[iconKey] = new Rect(o.Value.X, o.Value.Y, o.Value.Width, o.Value.Height);
                        icons.SkillImages[iconKey] = sprite.FileName;
                    }
                }

                controller?.SetProgress(0.55);
                // The last percent progress is reserved for rounding errors as progress must not get > 1.
                await assetLoader.DownloadAssetsAsync(PoESkillTree, d => controller?.SetProgress(0.55 + d * 0.44));
                foreach (var ass in PoESkillTree.Assets)
                {
                    var key = ass.Key;
                    var path = _assetsFolderPath + key + ".png";
                    assetActions.Add((Task.Run(() => BitmapImageFactory.Create(path)), i => Assets[key] = i));
                }

                foreach (var (task, action) in assetActions)
                {
                    action(await task);
                }

                AscendancyClasses = new AscendancyClasses(PoESkillTreeOptions.CharacterToAscendancy);

                BuildConverter = new BuildConverter(AscendancyClasses);
                BuildConverter.RegisterDefaultDeserializer(url => new NaivePoEUrlDeserializer(url, AscendancyClasses));
                BuildConverter.RegisterDeserializersFactories(
                    PoeplannerUrlDeserializer.TryCreate,
                    PathofexileUrlDeserializer.TryCreate
                );

                CharBaseAttributes = new Dictionary<CharacterClass, IReadOnlyList<(string stat, float value)>>();
                foreach (var (key, value) in PoESkillTree.CharacterData)
                {
                    CharBaseAttributes[(CharacterClass)key] = new (string stat, float value)[]
                    {
                        ("+# to Strength", value.BaseStrength),
                        ("+# to Dexterity", value.BaseDexterity),
                        ("+# to Intelligence", value.BaseIntelligence)
                    };
                }

                SkillNode.OrbitRadii = PoESkillTree.Constants.OrbitRadii ?? SkillNode.OrbitRadii;
                SkillNode.SkillsPerOrbit = PoESkillTree.Constants.SkillsPerOrbit ?? SkillNode.SkillsPerOrbit;
                RootNodeClassDictionary = new Dictionary<CharacterClass, ushort>();
                StartNodeDictionary = new Dictionary<ushort, ushort>();
                AscRootNodeList = new HashSet<SkillNode>();
                RootNodeList = new List<ushort>();

                if (PoESkillTree.Root != null)
                {
                    foreach (var i in PoESkillTree.Root.NodeIdsOut)
                    {
                        RootNodeList.Add(i);
                        if (Skillnodes.ContainsKey(i))
                        {
                            var node = Skillnodes[i];
                            node.IsRootNode = true;

                            var characterClass = PassiveNodeNameToClass[node.Name.ToUpperInvariant()];
                            if (!RootNodeClassDictionary.ContainsKey(characterClass))
                            {
                                RootNodeClassDictionary.Add(characterClass, node.Id);
                            }

                            foreach (var linkedNode in node.NodeIdsOut)
                            {
                                if (!StartNodeDictionary.ContainsKey(node.Id) && !node.IsAscendancyStart)
                                {
                                    StartNodeDictionary.Add(linkedNode, node.Id);
                                }
                            }
                        }
                    }
                }

                var regexAttrib = new Regex("[0-9]*\\.?[0-9]+");
                Regex regexString = new Regex(@"Can Allocate Passives from the .* starting point");
                foreach (var skillNode in Skillnodes)
                {
                    var n1 = skillNode.Value;
                    if (n1.Name.Contains("Jewel Socket"))
                    {
                        n1.StatDefinitions = new[] { "+1 Jewel Socket" };
                    }

                    //populate the Attributes fields with parsed attributes 
                    n1.Attributes = new Dictionary<string, IReadOnlyList<float>>();
                    foreach (string s in n1.StatDefinitions)
                    {
                        var values = new List<float>();

                        foreach (Match m in regexAttrib.Matches(s))
                        {
                            if (m.Value == "")
                                values.Add(float.NaN);
                            else
                                values.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
                        }
                        string cs = (regexAttrib.Replace(s, "#"));

                        n1.Attributes[cs] = values;
                    }

                    if (n1.IsAscendancyStart && !AscRootNodeList.Contains(n1))
                    {
                        AscRootNodeList.Add(n1);
                    }

                    foreach (var i in n1.NodeIdsOut)
                    {
                        var n2 = Skillnodes[i];
                        if (!StartNodeDictionary.ContainsKey(n1.Id) && RootNodeList.Contains(n2.Id))
                        {
                            StartNodeDictionary.Add(n1.Id, n2.Id);
                        }

                        bool isScionAscendancyNotable = false;
                        foreach (var attibute in n1.StatDefinitions)
                        {
                            if (regexString.IsMatch(attibute))
                                isScionAscendancyNotable = true;
                        }

                        foreach (var attibute in n2.StatDefinitions)
                        {
                            if (regexString.IsMatch(attibute))
                                isScionAscendancyNotable = true;
                        }

                        if (isScionAscendancyNotable && StartNodeDictionary.Keys.Contains(n1.Id))
                        {
                            if (!n2.Neighbor.Contains(n1))
                            {
                                n2.Neighbor.Add(n1);
                            }
                        }
                        else if (isScionAscendancyNotable && StartNodeDictionary.Keys.Contains(n2.Id))
                        {
                            if (!n1.Neighbor.Contains(n2))
                            {
                                n1.Neighbor.Add(n2);
                            }
                        }
                        else
                        {
                            if (!n2.Neighbor.Contains(n1))
                            {
                                n2.Neighbor.Add(n1);
                            }
                            if (!n1.Neighbor.Contains(n2))
                            {
                                n1.Neighbor.Add(n2);
                            }
                        }

                        if (n1.IsAscendancyNode == n2.IsAscendancyNode)
                        {
                            n1.VisibleNeighbors.Add(n2);
                            n2.VisibleNeighbors.Add(n1);
                        }
                    }
                }

                foreach (var gp in PoESkillTree.Groups)
                {
                    foreach (var node in gp.Value.NodeIds)
                    {
                        gp.Value.Nodes.Add(Skillnodes[node]);
                        Skillnodes[node].Group = gp.Value;
                    }
                }

                const int padding = 500; //This is to account for jewel range circles. Might need to find a better way to do it.
                SkillTreeRect = new Rect2D(new Vector2D(PoESkillTree.min_x * 1.1 - padding, PoESkillTree.min_y * 1.1 - padding),
                    new Vector2D(PoESkillTree.max_x * 1.1 + padding, PoESkillTree.max_y * 1.1 + padding));
            }

            if (_persistentData.Options.ShowAllAscendancyClasses)
                DrawAscendancy = true;

            InitialSkillTreeDrawing();
            controller?.SetProgress(1);

            _initialized = true;
        }

        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        /// <summary>
        /// This will get all skill points related to the tree both Normal and Ascendancy
        /// </summary>
        /// <returns>A Dictionary with keys of "NormalUsed", "NormalTotal", "AscendancyUsed", "AscendancyTotal", and "ScionAscendancyChoices"</returns>
        public Dictionary<string, int> GetPointCount()
        {
            Dictionary<string, int> points = new Dictionary<string, int>()
            {
                {"NormalUsed", 0},
                {"NormalTotal", 22},
                {"AscendancyUsed", 0},
                {"AscendancyTotal", 8},
            };

            var bandits = _persistentData.CurrentBuild.Bandits;
            points["NormalTotal"] += Level - 1;
            if (bandits.Choice == Bandit.None)
                points["NormalTotal"] += 2;

            foreach (var node in SkilledNodes)
            {
                if (!node.IsAscendancyNode && !node.IsRootNode)
                    points["NormalUsed"] += 1;
                else if (node.IsAscendancyNode && !node.IsAscendancyStart && !node.IsMultipleChoiceOption)
                {
                    points["AscendancyUsed"] += 1;
                    points["NormalTotal"] += node.PassivePointsGranted;
                }
            }
            return points;
        }

        public bool UpdateAscendancyClasses = true;

        public CharacterClass CharClass
        {
            get => _charClass;
            private set => SetProperty(ref _charClass, value);
        }

        public int AscType
        {
            get => _asctype;
            set
            {
                if (value < 0 || value > 3) return;
                ChangeAscClass(value);
            }
        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument",
            Justification = "Would notify changes for the not existing property 'ChangeAscClass'")]
        private void ChangeAscClass(int toType)
        {
            var changedType = _asctype != toType;
            if (toType == 0)
            {
                var remove = SkilledNodes.Where(n => n.IsAscendancyNode).ToList();
                if (!_persistentData.Options.ShowAllAscendancyClasses)
                    DrawAscendancy = false;
                SetProperty(ref _asctype, toType, propertyName: nameof(AscType));
                SkilledNodes.ExceptWith(remove);
            }
            else
            {
                var remove = new List<SkillNode>();
                SetProperty(ref _asctype, toType, propertyName: nameof(AscType));
                var sn = GetAscNode();
                if (sn != null)
                {
                    foreach (var n in SkilledNodes)
                    {
                        if (sn.AscendancyName != n.AscendancyName && n.IsAscendancyNode)
                            remove.Add(n);
                    }
                }
                SkilledNodes.ExceptAndUnionWith(remove, new[] { sn });
            }

            if (changedType)
                DrawAscendancyLayers();
        }

        public void SwitchClass(CharacterClass charClass)
        {
            if (charClass == CharClass)
                return;
            var canSwitch = CanSwitchClass(charClass);
            CharClass = charClass;

            var remove = canSwitch ? SkilledNodes.Where(n => n.IsAscendancyNode || n.IsRootNode) : SkilledNodes;
            var add = Skillnodes[RootNodeClassDictionary[charClass]];
            SkilledNodes.ExceptAndUnionWith(remove.ToList(), new[] { add });
            _asctype = 0;
        }

        public string AscendancyClassName
            => AscendancyClasses.GetAscendancyClassName(CharClass, AscType);

        public Dictionary<string, List<float>> HighlightedAttributes;

        public Dictionary<string, List<float>> SelectedAttributes
            => GetAttributes(SkilledNodes, CharClass, Level, _persistentData.CurrentBuild.Bandits);

        public static Dictionary<string, List<float>> GetAttributes(
            IEnumerable<SkillNode> skilledNodes, CharacterClass charClass, int level, BanditSettings banditSettings)
        {
            var temp = GetAttributesWithoutImplicit(skilledNodes, charClass, banditSettings);

            foreach (var a in ImplicitAttributes(temp, level))
            {
                var key = RenameImplicitAttributes.ContainsKey(a.Key) ? RenameImplicitAttributes[a.Key] : a.Key;

                if (!temp.ContainsKey(key))
                    temp[key] = new List<float>();
                for (var i = 0; i < a.Value.Count; i++)
                {
                    if (temp.ContainsKey(key) && temp[key].Count > i)
                        temp[key][i] += a.Value[i];
                    else
                    {
                        temp[key].Add(a.Value[i]);
                    }
                }
            }
            return temp;
        }

        public Dictionary<string, List<float>> SelectedAttributesWithoutImplicit
            => GetAttributesWithoutImplicit(SkilledNodes, CharClass, _persistentData.CurrentBuild.Bandits);

        private static Dictionary<string, List<float>> GetAttributesWithoutImplicit(
            IEnumerable<SkillNode> skilledNodes, CharacterClass charClass, BanditSettings banditSettings)
        {
            var temp = new Dictionary<string, List<float>>();

            foreach (var (stat, value) in
                CharBaseAttributes[charClass].Union(BaseAttributes).Union(banditSettings.Rewards))
            {
                if (!temp.ContainsKey(stat))
                    temp[stat] = new List<float> { value };
                else if (temp[stat].Any())
                    temp[stat][0] += value;
            }

            foreach (var node in skilledNodes)
            {
                foreach (var attr in ExpandHybridAttributes(node.Attributes))
                {
                    if (!temp.ContainsKey(attr.Key))
                        temp[attr.Key] = new List<float>();
                    for (int i = 0; i < attr.Value.Count; i++)
                    {
                        if (temp.ContainsKey(attr.Key) && temp[attr.Key].Count > i)
                            temp[attr.Key][i] += attr.Value[i];
                        else
                        {
                            temp[attr.Key].Add(attr.Value[i]);
                        }
                    }
                }
            }

            return temp;
        }


        public static Dictionary<string, List<float>> GetAttributesWithoutImplicitNodesOnly(IEnumerable<SkillNode> skilledNodes)
        {
            var temp = new Dictionary<string, List<float>>();

            foreach (var node in skilledNodes)
            {
                foreach (var attr in ExpandHybridAttributes(node.Attributes))
                {
                    if (!temp.ContainsKey(attr.Key))
                        temp[attr.Key] = new List<float>();
                    for (int i = 0; i < attr.Value.Count; i++)
                    {
                        if (temp.ContainsKey(attr.Key) && temp[attr.Key].Count > i)
                            temp[attr.Key][i] += attr.Value[i];
                        else
                        {
                            temp[attr.Key].Add(attr.Value[i]);
                        }
                    }
                }
            }

            return temp;
        }


        /// <summary>
        /// Returns a task that finishes with a SkillTree object once it has been initialized.
        /// </summary>
        /// <param name="persistentData"></param>
        /// <param name="controller">Null if no initialization progress should be displayed.</param>
        /// <param name="assetLoader">Can optionally be provided if the caller wants to backup assets.</param>
        /// <returns></returns>
        public static async Task<SkillTree> CreateAsync(IPersistentData persistentData,
            ProgressDialogController controller = null, AssetLoader assetLoader = null)
        {
            controller?.SetProgress(0);

            var dataFolderPath = AppData.GetFolder("Data", true);
            _assetsFolderPath = dataFolderPath + "Assets/";

            if (assetLoader == null)
                assetLoader = new AssetLoader(new HttpClient(), dataFolderPath, false);

            var skillTreeTask = LoadTreeFileAsync(dataFolderPath + "Skilltree.txt",
                () => assetLoader.DownloadSkillTreeToFileAsync());
            var optsTask = LoadTreeFileAsync(dataFolderPath + "Opts.txt",
                () => assetLoader.DownloadOptsToFileAsync());
            await Task.WhenAny(skillTreeTask, optsTask);
            controller?.SetProgress(0.1);

            var skillTreeObj = await skillTreeTask;
            var optsObj = await optsTask;
            controller?.SetProgress(0.25);

            var tree = new SkillTree(persistentData);
            await tree.InitializeAsync(skillTreeObj, optsObj, controller, assetLoader);
            return tree;
        }

        private static async Task<string> LoadTreeFileAsync(string path, Func<Task<string>> downloadFile)
        {
            var treeObj = "";
            if (File.Exists(path))
            {
                treeObj = await FileUtils.ReadAllTextAsync(path);
            }
            if (treeObj == "")
            {
                treeObj = await downloadFile();
            }
            return treeObj;
        }

        private IEnumerable<KeyValuePair<ushort, SkillNode>> FindNodesInRange(Vector2D mousePointer, int range = 50)
        {
            var nodes =
              Skillnodes.Where(n => ((n.Value.Position - mousePointer).Length < range)).ToList();
            if (!DrawAscendancy || AscType <= 0) return nodes;
            var asn = GetAscNode();
            var bitmap = Assets["Classes" + asn.AscendancyName];
            nodes = Skillnodes.Where(n => (n.Value.IsAscendancyNode || (Math.Pow(n.Value.Position.X - asn.Position.X, 2) + Math.Pow(n.Value.Position.Y - asn.Position.Y, 2)) > Math.Pow((bitmap.Height * 1.25 + bitmap.Width * 1.25) / 2, 2)) && ((n.Value.Position - mousePointer).Length < range)).ToList();
            return nodes;
        }

        public SkillNode FindNodeInRange(Vector2D mousePointer, int range = 50)
        {
            var nodes = FindNodesInRange(mousePointer, range);
            var nodeList = nodes as IList<KeyValuePair<ushort, SkillNode>> ?? nodes.ToList();
            if (!nodeList.Any()) return null;

            if (DrawAscendancy)
            {
                var dnode = nodeList.First();
                return nodeList
                    .Where(x => x.Value.AscendancyName == AscendancyClassName)
                    .DefaultIfEmpty(dnode)
                    .First()
                    .Value;
            }
            return nodeList.First().Value;
        }

        public void ResetSkilledNodesTo(IReadOnlyCollection<SkillNode> nodes)
        {
            SkilledNodes.ResetTo(nodes);
            AscType = SelectAscendancyFromNodes(nodes) ?? 0;
        }

        public void AllocateSkillNodes(IReadOnlyCollection<SkillNode> toAdd)
        {
            var toRemove = toAdd.SelectMany(SelectAscendancyNodesToRemove).ToList();
            SkilledNodes.ExceptAndUnionWith(toRemove, toAdd);
            if (SelectAscendancyFromNodes(toAdd) is int ascType)
                AscType = ascType;
        }

        private int? SelectAscendancyFromNodes(IEnumerable<SkillNode> nodes)
        {
            int? ascendancy = null;
            foreach (var node in nodes)
            {
                if (node.IsAscendancyStart)
                    ascendancy = AscendancyClasses.GetAscendancyClassNumber(node.AscendancyName);
            }
            return ascendancy;
        }

        private IEnumerable<SkillNode> SelectAscendancyNodesToRemove(SkillNode node)
        {
            if (node.IsAscendancyStart)
                return SkilledNodes.Where(x => x.IsAscendancyNode && x.AscendancyName != node.AscendancyName);
            if (node.IsMultipleChoiceOption)
                return SkilledNodes
                    .Where(x => x.IsMultipleChoiceOption)
                    .Where(x => AscendancyClasses.GetStartingClass(node.Name)
                                == AscendancyClasses.GetStartingClass(x.Name));
            return Enumerable.Empty<SkillNode>();
        }

        public void ForceRefundNode(SkillNode node)
        {
            if (!SkilledNodes.Contains(node)) return;
            var charStartNode = GetCharNode();
            var front = new HashSet<SkillNode>() { charStartNode };
            foreach (var i in charStartNode.Neighbor)
                if (SkilledNodes.Contains(i) && i != node)
                    front.Add(i);
            var reachable = new HashSet<SkillNode>(front);

            while (front.Any())
            {
                var newFront = new HashSet<SkillNode>();
                foreach (var i in front)
                {
                    foreach (var j in i.Neighbor)
                    {
                        if (reachable.Contains(j) || !SkilledNodes.Contains(j) || j == node) continue;
                        newFront.Add(j);
                        reachable.Add(j);
                    }
                }
                front = newFront;
            }
            var removable = SkilledNodes.Except(reachable).ToList();
            SkilledNodes.ExceptWith(removable);
        }

        public HashSet<SkillNode> ForceRefundNodePreview(SkillNode node)
        {
            if (!SkilledNodes.Contains(node)) return new HashSet<SkillNode>();

            var charStartNode = GetCharNode();
            var front = new HashSet<SkillNode>() { charStartNode };
            foreach (var i in charStartNode.Neighbor)
                if (SkilledNodes.Contains(i) && i != node)
                    front.Add(i);
            var reachable = new HashSet<SkillNode>(front);

            while (front.Any())
            {
                var newFront = new HashSet<SkillNode>();
                foreach (var i in front)
                {
                    foreach (var j in i.Neighbor)
                    {
                        if (j == node || reachable.Contains(j) || !SkilledNodes.Contains(j)) continue;
                        newFront.Add(j);
                        reachable.Add(j);
                    }
                }
                front = newFront;
            }
            var unreachable = new HashSet<SkillNode>(SkilledNodes);
            unreachable.ExceptWith(reachable);
            return unreachable;
        }

        public List<SkillNode> GetShortestPathTo(SkillNode targetNode, IEnumerable<SkillNode> start)
        {
            var startNodes = start as IList<SkillNode> ?? start.ToList();
            if (startNodes.Contains(targetNode))
                return new List<SkillNode>();
            var adjacent = GetAvailableNodes(startNodes);
            if (adjacent.Contains(targetNode))
                return new List<SkillNode> { targetNode };

            var visited = new HashSet<SkillNode>(startNodes);
            var distance = new Dictionary<SkillNode, int>();
            var parent = new Dictionary<SkillNode, SkillNode>();
            var newOnes = new Queue<SkillNode>();
            var toOmit = new HashSet<SkillNode>(
                         from entry in _nodeHighlighter.NodeHighlights
                         where entry.Value.HasFlag(HighlightState.Crossed)
                         select entry.Key);

            foreach (var node in adjacent)
            {
                if (toOmit.Contains(node))
                {
                    continue;
                }
                newOnes.Enqueue(node);
                distance.Add(node, 1);
            }

            while (newOnes.Count > 0)
            {
                var newNode = newOnes.Dequeue();
                var dis = distance[newNode];
                visited.Add(newNode);
                foreach (var connection in newNode.Neighbor)
                {
                    if (toOmit.Contains(connection))
                        continue;
                    if (visited.Contains(connection))
                        continue;
                    if (distance.ContainsKey(connection))
                        continue;
                    if (newNode.Character.HasValue)
                        continue;
                    if (newNode.Type == PassiveNodeType.Mastery)
                        continue;
                    if (IsAscendantClassStartNode(newNode))
                        continue;
                    distance.Add(connection, dis + 1);
                    newOnes.Enqueue(connection);

                    parent.Add(connection, newNode);

                    if (connection == targetNode)
                    {
                        newOnes.Clear();
                        break;
                    }
                }
            }

            if (!distance.ContainsKey(targetNode))
                return new List<SkillNode>();

            var curr = targetNode;
            var result = new List<SkillNode> { curr };
            while (parent.ContainsKey(curr))
            {
                result.Add(parent[curr]);
                curr = parent[curr];
            }
            result.Reverse();
            return result;
        }

        /// <summary>
        /// Returns true iff node is a Ascendant "Path of the ..." node.
        /// </summary>
        public static bool IsAscendantClassStartNode(SkillNode node)
        {
            return node.StatDefinitions.Any(s => AscendantClassStartRegex.IsMatch(s));
        }

        /// <summary>
        /// Changes the HighlightState of the node:
        /// None -> Checked -> Crossed -> None -> ...
        /// (preserves other HighlightStates than Checked and Crossed)
        /// </summary>
        /// <param name="node">Node to change the HighlightState for</param>
        public void CycleNodeTagForward(SkillNode node)
        {
            var id = node.Id;
            var build = _persistentData.CurrentBuild;
            if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Checked))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Checked);
                _nodeHighlighter.HighlightNode(node, HighlightState.Crossed);
                build.CheckedNodeIds.Remove(id);
                build.CrossedNodeIds.Add(id);
            }
            else if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Crossed))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Crossed);
                build.CrossedNodeIds.Remove(id);
            }
            else
            {
                _nodeHighlighter.HighlightNode(node, HighlightState.Checked);
                build.CheckedNodeIds.Add(id);
            }
            DrawHighlights();
        }

        /// <summary>
        /// Changes the HighlightState of the node:
        /// ... &lt;- None &lt;- Checked &lt;- Crossed &lt;- None
        /// (preserves other HighlightStates than Checked and Crossed)
        /// </summary>
        /// <param name="node">Node to change the HighlightState for</param>
        public void CycleNodeTagBackward(SkillNode node)
        {
            var id = node.Id;
            var build = _persistentData.CurrentBuild;
            if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Crossed))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Crossed);
                _nodeHighlighter.HighlightNode(node, HighlightState.Checked);
                build.CrossedNodeIds.Remove(id);
                build.CheckedNodeIds.Add(id);
            }
            else if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Checked))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Checked);
                build.CheckedNodeIds.Remove(id);
            }
            else
            {
                _nodeHighlighter.HighlightNode(node, HighlightState.Crossed);
                build.CrossedNodeIds.Add(id);
            }
            DrawHighlights();
        }

        /// <summary>
        /// Resets check and cross tagged node from <see cref="IPersistentData.CurrentBuild"/>.
        /// </summary>
        public void ResetTaggedNodes()
        {
            var build = _persistentData.CurrentBuild;
            _nodeHighlighter.ResetHighlights(SelectExistingNodesById(build.CheckedNodeIds), HighlightState.Checked);
            _nodeHighlighter.ResetHighlights(SelectExistingNodesById(build.CrossedNodeIds), HighlightState.Crossed);
            DrawHighlights();
        }

        private static IEnumerable<SkillNode> SelectExistingNodesById(IEnumerable<ushort> nodeIds)
        {
            return
                from id in nodeIds
                where Skillnodes.ContainsKey(id)
                select Skillnodes[id];
        }

        public void SetCheckTaggedNodes(IReadOnlyList<SkillNode> checkTagged)
        {
            _nodeHighlighter.ResetHighlights(checkTagged, HighlightState.Checked);
            _persistentData.CurrentBuild.CheckedNodeIds.Clear();
            _persistentData.CurrentBuild.CheckedNodeIds.UnionWith(checkTagged.Select(n => n.Id));
            DrawHighlights();
        }

        /// <param name="search">The string to search each node name and attribute for.</param>
        /// <param name="useregex">If the string should be interpreted as a regex.</param>
        /// <param name="flag">The flag to highlight found nodes with.</param>
        /// <param name="matchCount">The number of attributes of a node that must match the search to get highlighted, -1 if the count doesn't matter.</param>
        public void HighlightNodesBySearch(string search, bool useregex, HighlightState flag, int matchCount = -1)
        {
            if (search == "")
            {
                _nodeHighlighter.UnhighlightAllNodes(flag);
                DrawHighlights();
                return;
            }

            var matchFct = matchCount >= 0 ? (Func<string[], Func<string, bool>, bool>)
                 ((attributes, predicate) => attributes.Count(predicate) == matchCount)
                : (attributes, predicate) => attributes.Any(predicate);
            if (useregex)
            {
                try
                {
                    var regex = new Regex(search, RegexOptions.IgnoreCase);
                    var nodes =
                        Skillnodes.Values.Where(
                            nd => (matchFct(nd.StatDefinitions, att => regex.IsMatch(att)) ||
                                  regex.IsMatch(nd.Name) && nd.Type != PassiveNodeType.Mastery) &&
                                  (DrawAscendancy ? (_persistentData.Options.ShowAllAscendancyClasses || (nd.AscendancyName == null || nd.AscendancyName == AscendancyClassName)) : nd.AscendancyName == null));
                    _nodeHighlighter.ResetHighlights(nodes, flag);
                    DrawHighlights();
                }
                catch (Exception)
                {
                    // ?
                }
            }
            else
            {
                search = search.ToLowerInvariant();
                var nodes =
                    Skillnodes.Values.Where(
                        nd => (matchFct(nd.StatDefinitions, att => att.ToLowerInvariant().Contains(search)) ||
                              nd.Name.ToLowerInvariant().Contains(search) && nd.Type != PassiveNodeType.Mastery) &&
                              (DrawAscendancy ? (_persistentData.Options.ShowAllAscendancyClasses || (nd.AscendancyName == null || nd.AscendancyName == AscendancyClassName)) : nd.AscendancyName == null));
                _nodeHighlighter.ResetHighlights(nodes, flag);
                DrawHighlights();
            }
        }

        public void UnhighlightAllNodes()
        {
            _nodeHighlighter.UnhighlightAllNodes(HighlightState.Highlights);
        }

        public void UntagAllNodes()
        {
            _nodeHighlighter.UnhighlightAllNodes(HighlightState.Tags);
            _persistentData.CurrentBuild.CheckedNodeIds.Clear();
            _persistentData.CurrentBuild.CrossedNodeIds.Clear();
            DrawHighlights();
        }

        public void CheckAllHighlightedNodes()
        {
            var newlyChecked = _nodeHighlighter.HighlightNodesIf(HighlightState.Checked, HighlightState.Highlights)
                .Select(n => n.Id).ToList();
            _persistentData.CurrentBuild.CheckedNodeIds.UnionWith(newlyChecked);
            _persistentData.CurrentBuild.CrossedNodeIds.ExceptWith(newlyChecked);
            DrawHighlights();
        }

        public void CrossAllHighlightedNodes()
        {
            var newlyCrossed = _nodeHighlighter.HighlightNodesIf(HighlightState.Crossed, HighlightState.Highlights)
                .Select(n => n.Id).ToList();
            _persistentData.CurrentBuild.CrossedNodeIds.UnionWith(newlyCrossed);
            _persistentData.CurrentBuild.CheckedNodeIds.ExceptWith(newlyCrossed);
            DrawHighlights();
        }

        public static Dictionary<string, List<float>> ImplicitAttributes(Dictionary<string, List<float>> attribs, int level)
        {
            var retval = new Dictionary<string, List<float>>
            {
                ["+# to maximum Mana"] = new List<float>
                {
                    attribs["+# to Intelligence"][0]/IntPerMana + level*ManaPerLevel
                },
                ["#% increased maximum Energy Shield"] = new List<float>
                {
                    (float) Math.Round(attribs["+# to Intelligence"][0]/IntPerES, 0)
                },
                ["+# to maximum Life"] = new List<float>
                {
                    attribs["+# to Strength"][0]/StrPerLife + level*LifePerLevel
                }
            };
            // +# to Strength", co["base_str"].Value<int>() }, { "+# to Dexterity", co["base_dex"].Value<int>() }, { "+# to Intelligence", co["base_int"].Value<int>() } };

            // Every 10 strength grants 2% increased melee physical damage. 
            var str = (int)attribs["+# to Strength"][0];
            if (str % (int)StrPerED > 0) str += (int)StrPerED - (str % (int)StrPerED);
            retval["#% increased Melee Physical Damage"] = new List<float> { str / StrPerED };
            // Every point of Dexterity gives 2 additional base accuracy, and characters gain 2 base accuracy when leveling up.
            // @see http://pathofexile.gamepedia.com/Accuracy
            retval["+# Accuracy Rating"] = new List<float>
            {
                attribs["+# to Dexterity"][0]/DexPerAcc + (level - 1)*AccPerLevel
            };
            retval["Evasion Rating: #"] = new List<float> { level * EvasPerLevel };

            // Dexterity value is not getting rounded up any more but rounded normally to the nearest multiple of 5.
            // @see http://pathofexile.gamepedia.com/Talk:Evasion
            float dex = attribs["+# to Dexterity"][0];
            dex = (float)Math.Round(dex / DexPerEvas, 0, MidpointRounding.AwayFromZero) * DexPerEvas;
            retval["#% increased Evasion Rating"] = new List<float> { dex / DexPerEvas };

            int frenzycharges, powercharges;
            var endurancecharges = frenzycharges = powercharges = 0;
            if (attribs.ContainsKey("+# to Maximum Endurance Charges"))
                endurancecharges = (int)(attribs["+# to Maximum Endurance Charges"][0]);
            if (attribs.ContainsKey("+# to Maximum Frenzy Charges"))
                frenzycharges = (int)(attribs["+# to Maximum Frenzy Charges"][0]);
            if (attribs.ContainsKey("+# to Maximum Power Charges"))
                powercharges = (int)(attribs["+# to Maximum Power Charges"][0]);
            foreach (var key in attribs.Keys)
            {
                string newkey;
                if (key.Contains("per Endurance Charge") && endurancecharges > 0)
                {
                    newkey = key.Replace("per Endurance Charge", "with all Endurance Charges");
                    retval.Add(newkey, new List<float>());
                    foreach (var f in attribs[key])
                    {
                        retval[newkey].Add(f * endurancecharges);
                    }
                }
                if (key.Contains("per Frenzy Charge") && endurancecharges > 0)
                {
                    newkey = key.Replace("per Frenzy Charge", "with all Frenzy Charges");
                    retval.Add(newkey, new List<float>());
                    foreach (var f in attribs[key])
                    {
                        retval[newkey].Add(f * frenzycharges);
                    }
                }
                if (key.Contains("per Power Charge") && endurancecharges > 0)
                {
                    newkey = key.Replace("per Power Charge", "with all Power Charges");
                    retval.Add(newkey, new List<float>());
                    foreach (var f in attribs[key])
                    {
                        retval[newkey].Add(f * powercharges);
                    }
                }
            }

            return retval;
        }

        public static void DecodeUrl(
            string url, out HashSet<SkillNode> skilledNodes, out CharacterClass charClass, ISkillTree skillTree)
            => DecodeUrlPrivate(url, out skilledNodes, out charClass, skillTree);

        private static BuildUrlData DecodeUrl(string url, out HashSet<SkillNode> skilledNodes, ISkillTree skillTree)
            => DecodeUrlPrivate(url, out skilledNodes, out _, skillTree);

        public static BuildUrlData DecodeUrl(string url, ISkillTree skillTree)
            => DecodeUrlPrivate(url, out _, out _, skillTree);

        private static BuildUrlData DecodeUrlPrivate(
            string url, out HashSet<SkillNode> skilledNodes, out CharacterClass charClass, ISkillTree skillTree)
        {
            BuildUrlData buildData = skillTree.BuildConverter.GetUrlDeserializer(url).GetBuildData();

            charClass = buildData.CharacterClass;
            var ascType = (byte)buildData.AscendancyClassId;

            SkillNode startnode = Skillnodes[RootNodeClassDictionary[charClass]];
            skilledNodes = new HashSet<SkillNode> { startnode };

            if (ascType > 0)
            {
                string ascendancyClass = skillTree.AscendancyClasses.GetAscendancyClassName(charClass, ascType);
                SkillNode ascNode = AscRootNodeList.First(nd => nd.AscendancyName == ascendancyClass);
                skilledNodes.Add(ascNode);
            }

            var unknownNodes = 0;
            foreach (var nodeId in buildData.SkilledNodesIds)
            {
                if (Skillnodes.TryGetValue(nodeId, out var node))
                {
                    skilledNodes.Add(node);
                }
                else
                {
                    unknownNodes++;
                }
            }

            if (unknownNodes > 0)
            {
                buildData.CompatibilityIssues.Add(L10n.Message($"Some nodes ({unknownNodes}) are unknown and have been omitted."));
            }

            return buildData;
        }

        public void LoadFromUrl(string url)
        {
            var data = DecodeUrl(url, out var skillNodes, this);
            CharClass = data.CharacterClass;
            ResetSkilledNodesTo(skillNodes);
        }

        public void Reset()
        {
            var prefs = _persistentData.Options.ResetPreferences;
            if (prefs.HasFlag(ResetPreferences.MainTree))
            {
                var rootNode = Skillnodes[RootNodeClassDictionary[CharClass]];
                if (prefs.HasFlag(ResetPreferences.AscendancyTree))
                {
                    SkilledNodes.ResetTo(new[] { rootNode });
                }
                else
                {
                    var ascNodes = SkilledNodes.Where(n => n.IsAscendancyNode).ToList();
                    SkilledNodes.ResetTo(ascNodes.Append(rootNode));
                }
            }
            if (prefs.HasFlag(ResetPreferences.AscendancyTree))
            {
                AscType = 0;
            }
            if (prefs.HasFlag(ResetPreferences.Bandits))
                _persistentData.CurrentBuild.Bandits.Reset();
            UpdateAscendancyClasses = true;
        }

        /// <summary>
        /// Returns all currently Check-tagged nodes.
        /// </summary>
        public HashSet<SkillNode> GetCheckedNodes()
        {
            var nodes = new HashSet<SkillNode>();
            foreach (var entry in _nodeHighlighter.NodeHighlights)
            {
                if (!entry.Key.IsRootNode && entry.Value.HasFlag(HighlightState.Checked))
                {
                    nodes.Add(entry.Key);
                }
            }
            return nodes;
        }

        /// <summary>
        /// Returns all currently Cross-tagged nodes.
        /// </summary>
        public HashSet<SkillNode> GetCrossedNodes()
        {
            var nodes = new HashSet<SkillNode>();
            foreach (var entry in _nodeHighlighter.NodeHighlights)
            {
                if (!entry.Key.IsRootNode && entry.Value.HasFlag(HighlightState.Crossed))
                {
                    nodes.Add(entry.Key);
                }
            }
            return nodes;
        }

        private SkillNode GetCharNode()
            => Skillnodes[GetCharNodeId()];

        private ushort GetCharNodeId()
            => RootNodeClassDictionary[CharClass];

        private SkillNode GetAscNode()
        {
            var ascNodeId = GetAscNodeId();
            if (ascNodeId != 0)
                return Skillnodes[ascNodeId];
            else
                return null;
        }

        private ushort GetAscNodeId()
        {
            if (_asctype <= 0 || _asctype > 3)
                return 0;
            return AscRootNodeList.FirstOrDefault(x => x.AscendancyName == AscendancyClassName)?.Id ?? 0;
        }

        private HashSet<SkillNode> GetAvailableNodes(IEnumerable<SkillNode> skilledNodes)
        {
            var availNodes = new HashSet<SkillNode>();

            foreach (var node in skilledNodes)
            {
                foreach (var skillNode in node.Neighbor)
                {
                    if (!RootNodeList.Contains(skillNode.Id) && !SkilledNodes.Contains(skillNode))
                        availNodes.Add(skillNode);
                }
            }
            return availNodes;
        }

        public static IEnumerable<KeyValuePair<string, IReadOnlyList<float>>> ExpandHybridAttributes(Dictionary<string, IReadOnlyList<float>> attributes)
        {
            return attributes.SelectMany(ExpandHybridAttributes);
        }

        public static IEnumerable<KeyValuePair<string, IReadOnlyList<float>>> ExpandHybridAttributes(KeyValuePair<string, IReadOnlyList<float>> attribute)
        {
            if (HybridAttributes.TryGetValue(attribute.Key, out List<string> expandedAttributes))
            {
                foreach (var expandedAttribute in expandedAttributes)
                {
                    yield return new KeyValuePair<string, IReadOnlyList<float>>(expandedAttribute, attribute.Value);
                }
            }
            else
            {
                yield return attribute;
            }
        }

        private bool CanSwitchClass(CharacterClass charClass)
        {
            RootNodeClassDictionary.TryGetValue(charClass, out var rootNodeValue);
            var classSpecificStartNodes = StartNodeDictionary.Where(kvp => kvp.Value == rootNodeValue).Select(kvp => kvp.Key).ToList();

            return (
                from nodeId in classSpecificStartNodes
                let temp = GetShortestPathTo(Skillnodes[nodeId], SkilledNodes)
                where !temp.Any() && !Skillnodes[nodeId].IsAscendancyNode
                select nodeId
            ).Any();
        }

        #region ISkillTree members



        #endregion
    }
}
