using Newtonsoft.Json;
using POESKillTree.Localization;
using POESKillTree.Utils;
using System;
using System.Collections.Generic;
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
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model;
using POESKillTree.TreeGenerator.ViewModels;
using HighlightState = POESKillTree.SkillTreeFiles.NodeHighlighter.HighlightState;
using static POESKillTree.SkillTreeFiles.Constants;

namespace POESKillTree.SkillTreeFiles
{
    public partial class SkillTree : Notifier
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

        public static readonly Dictionary<string, float> BaseAttributes = new Dictionary<string, float>
        {
            {"+# to maximum Mana", 34},
            {"+# to maximum Life", 38},
            {"Evasion Rating: #", 53},
            {"+# to Maximum Endurance Charges", 3},
            {"+# to Maximum Frenzy Charges", 3},
            {"+# to Maximum Power Charges", 3},
            {"#% Additional Elemental Resistance per Endurance Charge", 4},
            {"#% Physical Damage Reduction per Endurance Charge", 4},
            {"#% Attack Speed Increase per Frenzy Charge", 4},
            {"#% Cast Speed Increase per Frenzy Charge", 4},
            {"#% More Damage per Frenzy Charge", 4},
            {"#% Critical Strike Chance Increase per Power Charge", 50},
        };

        public static readonly Dictionary<string, List<string>> HybridAttributes = new Dictionary<string, List<string>>
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

        public static readonly List<string> CharName = new List<string>
        {
            CharacterNames.Scion,
            CharacterNames.Marauder,
            CharacterNames.Ranger,
            CharacterNames.Witch,
            CharacterNames.Duelist,
            CharacterNames.Templar,
            CharacterNames.Shadow
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
        public static Dictionary<ushort, SkillNode> Skillnodes { get; private set; }

        private static IEnumerable<string> _allAttributes;
        /// <summary>
        /// Gets an Array of all the attributes of SkillNodes.
        /// </summary>
        public static IEnumerable<string> AllAttributes
        {
            get { return _allAttributes ?? (_allAttributes = Skillnodes.Values.SelectMany(n => n.Attributes.Keys).Distinct().ToArray()); }
        }

        public static Dictionary<string, float>[] CharBaseAttributes { get; private set; }
        public static List<int> RootNodeList { get; private set; }
        public static List<SkillNodeGroup> NodeGroups { get; private set; }
        public static Rect2D SkillTreeRect { get; private set; }
        private static Dictionary<string, int> RootNodeClassDictionary { get; set; }
        private static List<string> AttributeTypes { get; } = new List<string>();
        private static Dictionary<int, int> StartNodeDictionary { get; set; }

        private static readonly Dictionary<string, BitmapImage> _assets = new Dictionary<string, BitmapImage>();
        public static Dictionary<string, BitmapImage> Assets => _assets;
        private static readonly List<ushort[]> Links = new List<ushort[]>();
        public readonly ObservableSet<SkillNode> SkilledNodes = new ObservableSet<SkillNode>(); 
        public readonly ObservableSet<SkillNode> HighlightedNodes = new ObservableSet<SkillNode>();
        private static AscendancyClasses _ascClasses;
        public AscendancyClasses AscClasses => _ascClasses;

        private int _chartype;
        private int _asctype;
        public static int UndefinedLevel => 0;
        public static int MaximumLevel => 100;
        private int _level = UndefinedLevel;
        
        private BanditSettings _banditSettings = new BanditSettings();
        public BanditSettings BanditSettings
        {
            get { return _banditSettings; }
            set { SetProperty(ref _banditSettings, value); }
        }

        private readonly IDialogCoordinator _dialogCoordinator;

        private static bool _initialized;

        private SkillTree(IPersistentData persistentData, IDialogCoordinator dialogCoordinator)
        {
            _persistentData = persistentData;
            _dialogCoordinator = dialogCoordinator;
        }

        private async Task InitializeAsync(string treestring, string opsstring, [CanBeNull] ProgressDialogController controller,
            AssetLoader assetLoader)
        {
            if (!_initialized)
            {
                var jss = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        // This one is known: "515":{"x":_,"y":_,"oo":[],"n":[]}} has an Array in "oo".
                        if (args.ErrorContext.Path != "groups.515.oo")
                            Log.Error("Exception while deserializing Json tree", args.ErrorContext.Error);
                        args.ErrorContext.Handled = true;
                    }
                };
                
                var inTree = JsonConvert.DeserializeObject<PoESkillTree>(treestring, jss);
                var inOpts = JsonConvert.DeserializeObject<Opts>(opsstring, jss);

                controller?.SetProgress(0.25);
                await assetLoader.DownloadSkillNodeSpritesAsync(inTree, d => controller?.SetProgress(0.25 + d * 0.30));
                IconInActiveSkills = new SkillIcons();
                IconActiveSkills = new SkillIcons();
                foreach (var obj in inTree.skillSprites)
                {
                    SkillIcons icons;
                    string prefix;
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
                    var path = _assetsFolderPath + sprite.filename;
                    icons.Images[sprite.filename] = ImageHelper.OnLoadBitmapImage(new Uri(path, UriKind.Absolute));
                    foreach (var o in sprite.coords)
                    {
                        var iconKey = prefix + "_" + o.Key;
                        icons.SkillPositions[iconKey] = new Rect(o.Value.x, o.Value.y, o.Value.w, o.Value.h);
                        icons.SkillImages[iconKey] = sprite.filename;
                    }
                }

                controller?.SetProgress(0.55);
                // The last percent progress is reserved for rounding errors as progress must not get > 1.
                await assetLoader.DownloadAssetsAsync(inTree, d => controller?.SetProgress(0.55 + d * 0.44));
                foreach (var ass in inTree.assets)
                {
                    var path = _assetsFolderPath + ass.Key + ".png";
                    Assets[ass.Key] = ImageHelper.OnLoadBitmapImage(new Uri(path, UriKind.Absolute));
                }

                RootNodeList = new List<int>();
                if (inTree.root != null)
                {
                    foreach (int i in inTree.root.ot)
                    {
                        RootNodeList.Add(i);
                    }
                }
                else if (inTree.main != null)
                {
                    foreach (int i in inTree.main.ot)
                    {
                        RootNodeList.Add(i);
                    }
                }

                _ascClasses = new AscendancyClasses();
                if(inOpts != null)
                {
                    foreach(KeyValuePair<int, baseToAscClass> ascClass in inOpts.ascClasses )
                    {
                        var classes = new List<AscendancyClasses.Class>();
                        foreach (KeyValuePair<int, classes> asc in ascClass.Value.classes)
                        {
                            var newClass = new AscendancyClasses.Class
                            {
                                Order = asc.Key,
                                DisplayName = asc.Value.displayName,
                                Name = asc.Value.name,
                                FlavourText = asc.Value.flavourText,
                                FlavourTextColour = asc.Value.flavourTextColour.Split(',').Select(int.Parse).ToArray()
                            };
                            int[] tempPointList = asc.Value.flavourTextRect.Split(',').Select(int.Parse).ToArray();
                            newClass.FlavourTextRect = new Vector2D(tempPointList[0], tempPointList[1]);
                            classes.Add(newClass);

                        }
                        AscClasses.Classes.Add(ascClass.Value.name, classes);
                    }
                }

                CharBaseAttributes = new Dictionary<string, float>[7];
                foreach (var c in inTree.characterData)
                {
                    CharBaseAttributes[c.Key] = new Dictionary<string, float>
                    {
                        {"+# to Strength", c.Value.base_str},
                        {"+# to Dexterity", c.Value.base_dex},
                        {"+# to Intelligence", c.Value.base_int}
                    };
                }

                Skillnodes = new Dictionary<ushort, SkillNode>();
                RootNodeClassDictionary = new Dictionary<string, int>();
                StartNodeDictionary = new Dictionary<int, int>();

                foreach (var nd in inTree.nodes)
                {
                    var skillNode = new SkillNode
                    {
                        Id = nd.id,
                        Name = nd.dn,
                        //this value should not be split on '\n' as it causes the attribute list to seperate nodes
                        attributes = nd.dn.Contains("Jewel Socket") ? new[] {"+1 Jewel Socket"} : nd.sd,
                        Orbit = nd.o,
                        OrbitIndex = nd.oidx,
                        Icon = nd.icon,
                        LinkId = nd.ot,
                        G = nd.g,
                        Da = nd.da,
                        Ia = nd.ia,
                        Sa = nd.sa,
                        Spc = nd.spc.Length > 0 ? (int?) nd.spc[0] : null,
                        IsMultipleChoice = nd.isMultipleChoice,
                        IsMultipleChoiceOption = nd.isMultipleChoiceOption,
                        passivePointsGranted = nd.passivePointsGranted,
                        ascendancyName = nd.ascendancyName,
                        IsAscendancyStart = nd.isAscendancyStart,
                        reminderText = nd.reminderText
                    };
                    if (nd.ks && !nd.not && !nd.isJewelSocket && !nd.m)
                    {
                        skillNode.Type = NodeType.Keystone;
                    }
                    else if (!nd.ks && nd.not && !nd.isJewelSocket && !nd.m)
                    {
                        skillNode.Type = NodeType.Notable;
                    }
                    else if (!nd.ks && !nd.not && nd.isJewelSocket && !nd.m)
                    {
                        skillNode.Type = NodeType.JewelSocket;
                    }
                    else if (!nd.ks && !nd.not && !nd.isJewelSocket && nd.m)
                    {
                        skillNode.Type = NodeType.Mastery;
                    }
                    else if (!nd.ks && !nd.not && !nd.isJewelSocket && !nd.m)
                    {
                        skillNode.Type = NodeType.Normal;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid node type for node {skillNode.Name}");
                    }
                    Skillnodes.Add(nd.id, skillNode);
                    if (RootNodeList.Contains(nd.id))
                    {
                        if (!RootNodeClassDictionary.ContainsKey(nd.dn.ToUpperInvariant()))
                        {
                            RootNodeClassDictionary.Add(nd.dn.ToUpperInvariant(), nd.id);
                        }
                        foreach (var linkedNode in nd.ot)
                        {
                            if (!StartNodeDictionary.ContainsKey(nd.id) && !nd.isAscendancyStart)
                            {
                                StartNodeDictionary.Add(linkedNode, nd.id);
                            }
                        }
                    }
                    foreach (var node in nd.ot)
                    {
                        if (!StartNodeDictionary.ContainsKey(nd.id) && RootNodeList.Contains(node))
                        {
                            StartNodeDictionary.Add(nd.id, node);
                        }
                    }

                }

                foreach (var skillNode in Skillnodes)
                {
                    foreach (var i in skillNode.Value.LinkId)
                    {
                        if (Links.Count(nd => (nd[0] == i && nd[1] == skillNode.Key) || nd[0] == skillNode.Key && nd[1] == i) != 1)
                            Links.Add(new[] { skillNode.Key, i });
                    }
                }
                foreach (var ints in Links)
                {
                    Regex regexString = new Regex(@"Can Allocate Passives from the .* starting point");
                    bool isScionAscendancyNotable = false;
                    foreach(var attibute in Skillnodes[ints[0]].attributes)
                    {
                        if(regexString.IsMatch(attibute))
                            isScionAscendancyNotable = true;
                    }
                    foreach (var attibute in Skillnodes[ints[1]].attributes)
                    {
                        if (regexString.IsMatch(attibute))
                            isScionAscendancyNotable = true;
                    }

                    if (isScionAscendancyNotable && StartNodeDictionary.Keys.Contains(ints[0]))
                    {
                        if (!Skillnodes[ints[1]].Neighbor.Contains(Skillnodes[ints[0]]))
                            Skillnodes[ints[1]].Neighbor.Add(Skillnodes[ints[0]]);
                    }
                    else if (isScionAscendancyNotable && StartNodeDictionary.Keys.Contains(ints[1]))
                    {
                        if (!Skillnodes[ints[0]].Neighbor.Contains(Skillnodes[ints[1]]))
                            Skillnodes[ints[0]].Neighbor.Add(Skillnodes[ints[1]]);
                    }
                    else
                    {
                        if (!Skillnodes[ints[0]].Neighbor.Contains(Skillnodes[ints[1]]))
                            Skillnodes[ints[0]].Neighbor.Add(Skillnodes[ints[1]]);
                        if (!Skillnodes[ints[1]].Neighbor.Contains(Skillnodes[ints[0]]))
                            Skillnodes[ints[1]].Neighbor.Add(Skillnodes[ints[0]]);
                    }
                }

                var regexAttrib = new Regex("[0-9]*\\.?[0-9]+");
                foreach (var skillnode in Skillnodes)
                {
                    //add each other as visible neighbors
                    foreach (var snn in skillnode.Value.Neighbor)
                    {
                        if (snn.IsAscendancyStart && skillnode.Value.LinkId.Contains(snn.Id))
                            continue;
                        skillnode.Value.VisibleNeighbors.Add(snn);
                    }

                    //populate the Attributes fields with parsed attributes 
                    skillnode.Value.Attributes = new Dictionary<string, List<float>>();
                    foreach (string s in skillnode.Value.attributes)
                    {
                        var values = new List<float>();

                        foreach (Match m in regexAttrib.Matches(s))
                        {
                            if (!AttributeTypes.Contains(regexAttrib.Replace(s, "#")))
                                AttributeTypes.Add(regexAttrib.Replace(s, "#"));
                            if (m.Value == "")
                                values.Add(float.NaN);
                            else
                                values.Add(float.Parse(m.Value, CultureInfo.InvariantCulture));
                        }
                        string cs = (regexAttrib.Replace(s, "#"));

                        skillnode.Value.Attributes[cs] = values;
                    }
                }

                NodeGroups = new List<SkillNodeGroup>();
                foreach (var gp in inTree.groups)
                {
                    var ng = new SkillNodeGroup();

                    ng.OcpOrb = gp.Value.oo;
                    ng.Position = new Vector2D(gp.Value.x, gp.Value.y);
                    foreach (var node in gp.Value.n)
                    {
                        ng.Nodes.Add(Skillnodes[node]);
                    }
                    NodeGroups.Add(ng);
                }
                foreach (SkillNodeGroup group in NodeGroups)
                {
                    foreach (SkillNode node in group.Nodes)
                    {
                        node.SkillNodeGroup = group;
                    }
                }

                const int padding = 500; //This is to account for jewel range circles. Might need to find a better way to do it.
                SkillTreeRect = new Rect2D(new Vector2D(inTree.min_x * 1.1 - padding, inTree.min_y * 1.1 - padding),
                    new Vector2D(inTree.max_x * 1.1 + padding, inTree.max_y * 1.1 + padding));
            }

            if (_persistentData.Options.ShowAllAscendancyClasses)
                DrawAscendancy = true;

            InitialSkillTreeDrawing();
            controller?.SetProgress(1);

            _initialized = true;
        }

        public int Level
        {
            get { return _level; }
            set { SetProperty(ref _level, value); }
        }

        /// <summary>
        /// This will get all skill points related to the tree both Normal and Ascendancy
        /// </summary>
        /// <returns>A Dictionary with keys of "NormalUsed", "NormalTotal", "AscendancyUsed", "AscendancyTotal", and "ScionAscendancyChoices"</returns>
        public Dictionary<string, int> GetPointCount()
        {
            Dictionary<string, int> points = new Dictionary<string,int>()
            {
                {"NormalUsed", 0},
                {"NormalTotal", 21},
                {"AscendancyUsed", 0},
                {"AscendancyTotal", 8},
                {"ScionAscendancyChoices", 0}
            };

            points["NormalTotal"] += Level - 1;
            if (BanditSettings.Normal == Bandit.None)
                points["NormalTotal"]++;
            if (BanditSettings.Cruel == Bandit.None)
                points["NormalTotal"]++;
            if (BanditSettings.Merciless == Bandit.None)
                points["NormalTotal"]++;

            foreach (var node in SkilledNodes)
            {
                if (node.ascendancyName == null && !RootNodeList.Contains(node.Id))
                    points["NormalUsed"] += 1;
                else if (node.ascendancyName != null && !node.IsAscendancyStart && !node.IsMultipleChoiceOption)
                {
                    points["AscendancyUsed"] += 1;
                    points["NormalTotal"] += node.passivePointsGranted;
                }
                else if (node.IsMultipleChoiceOption)
                {
                    points["ScionAscendancyChoices"] += 1;
                }
            }
            return points;
        }

        public bool UpdateAscendancyClasses = true;
        public int Chartype
        {
            get { return _chartype; }
            set
            {
                SetProperty(ref _chartype, value, CharacterTypeUpdate);
            }
        }

        public int AscType
        {
            get { return _asctype; }
            set
            {
                SetProperty(ref _asctype, value, () => 
                {
                    if (value < 0 || value > 3)
                    {
                        _asctype = 0;
                        value = 0;
                    }
                    CharacterTypeUpdate();
                });
            }
        }

        private void CharacterTypeUpdate()
        {
            var add = new HashSet<SkillNode>();
            if (AscType != -1)
            {
                var sn = GetAscNode();
                if(sn != null)
                {
                    add.Add(sn);
                    foreach (var n in SkilledNodes)
                    {
                        if (sn.ascendancyName != n.ascendancyName) continue;
                        add.Add(n);
                    }
                }
            }
            if (CanSwitchClass(CharName[_chartype]))
            {
                foreach (var n in SkilledNodes)
                {
                    if (n.ascendancyName != null) continue;
                    if (RootNodeList.Contains(n.Id)) continue;
                    add.Add(n);
                }
            }
            add.ExceptWith(SkilledNodes);
            //SkilledNodes.Clear();
            //add.Add(GetCharNode());
            AllocateSkillNodes(add);
            DrawAscendancy = _persistentData.Options.ShowAllAscendancyClasses;
        }

        public Dictionary<string, List<float>> HighlightedAttributes;

        public Dictionary<string, List<float>> SelectedAttributes
            => GetAttributes(SkilledNodes, Chartype, Level, BanditSettings);

        public static Dictionary<string, List<float>> GetAttributes(IEnumerable<SkillNode> skilledNodes, int chartype, int level, BanditSettings banditSettings)
        {
            var temp = GetAttributesWithoutImplicit(skilledNodes,chartype, banditSettings);

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
            => GetAttributesWithoutImplicit(SkilledNodes, Chartype, BanditSettings);

        public static Dictionary<string, List<float>> GetAttributesWithoutImplicit(IEnumerable<SkillNode> skilledNodes, int chartype, BanditSettings banditSettings)
        {
            var temp = new Dictionary<string, List<float>>();

            foreach (var attr in CharBaseAttributes[chartype].Union(BaseAttributes).Union(banditSettings.Rewards))
            {
                if (!temp.ContainsKey(attr.Key))
                    temp[attr.Key] = new List<float> {attr.Value};
                else if (temp[attr.Key].Any())
                    temp[attr.Key][0] += attr.Value;
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

        /// <summary>
        /// Returns a task that finishes with a SkillTree object once it has been initialized.
        /// </summary>
        /// <param name="persistentData"></param>
        /// <param name="dialogCoordinator">Can be null if the resulting tree is not used.</param>
        /// <param name="controller">Null if no initialization progress should be displayed.</param>
        /// <param name="assetLoader">Can optionally be provided if the caller wants to backup assets.</param>
        /// <returns></returns>
        public static async Task<SkillTree> CreateAsync(IPersistentData persistentData, IDialogCoordinator dialogCoordinator,
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

            var tree = new SkillTree(persistentData, dialogCoordinator);
            await tree.InitializeAsync(skillTreeObj, optsObj, controller, assetLoader);
            return tree;
        }

        private static async Task<string> LoadTreeFileAsync(string path, Func<Task<string>> downloadFile)
        {
            var treeObj = "";
            if (File.Exists(path))
            {
                treeObj = await FileEx.ReadAllTextAsync(path);
            }
            if (treeObj == "")
            {
                treeObj = await downloadFile();
            }
            return treeObj;
        }

        public IEnumerable<KeyValuePair<ushort, SkillNode>> FindNodesInRange(Vector2D mousePointer, int range = 50)
        {
            var nodes =
              SkillTree.Skillnodes.Where(n => ((n.Value.Position - mousePointer).Length < range)).ToList();
            if (!DrawAscendancy || AscType <= 0) return nodes;
            var asn = GetAscNode();
            var bitmap = Assets["Classes" + asn.ascendancyName];
            nodes = SkillTree.Skillnodes.Where(n => (n.Value.ascendancyName != null || (Math.Pow(n.Value.Position.X - asn.Position.X, 2) + Math.Pow(n.Value.Position.Y - asn.Position.Y, 2)) > Math.Pow((bitmap.Height * 1.25 + bitmap.Width * 1.25) / 2, 2)) && ((n.Value.Position - mousePointer).Length < range)).ToList();
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
                return
                    nodeList.Where(x => x.Value.ascendancyName == AscClasses.GetClassName(Chartype, AscType))
                        .DefaultIfEmpty(dnode)
                        .First()
                        .Value;
            }
            return nodeList.First().Value;
        }

        public void AllocateSkillNodes(IEnumerable<SkillNode> nodes)
        {
            if (nodes == null) return;
            var skillNodes = nodes as IList<SkillNode> ?? nodes.ToList();
            foreach (var i in skillNodes)
            {
                AllocateSkillNode(i, true);
            }
            SkilledNodes.UnionWith(skillNodes);
        }

        public void AllocateSkillNode(SkillNode node, bool bulk = false)
        {
            if (node == null) return;
            if (node.IsAscendancyStart)
            {
                var remove = SkilledNodes.Where(x => x.ascendancyName != null && x.ascendancyName != node.ascendancyName).ToArray();
                SkilledNodes.ExceptWith(remove);
            }
            else if (node.IsMultipleChoiceOption)
            {
                var remove = SkilledNodes.Where(x => x.IsMultipleChoiceOption && AscClasses.GetStartingClass(node.Name) == AscClasses.GetStartingClass(x.Name)).ToArray();
                SkilledNodes.ExceptWith(remove);
            }
            if (!bulk)
                SkilledNodes.Add(node);
        }

        public void ForceRefundNode(SkillNode node)
        {
            if (!SkilledNodes.Contains(node)) return;
            var charStartNode = GetCharNode();
            var front = new HashSet<SkillNode>() {charStartNode};
            foreach(var i in charStartNode.Neighbor)
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
                         from entry in _nodeHighlighter.nodeHighlights
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
                    if (newNode.Spc.HasValue)
                        continue;
                    if (newNode.Type == NodeType.Mastery)
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
        private static bool IsAscendantClassStartNode(ushort node)
        {
            return IsAscendantClassStartNode(Skillnodes[node]);
        }

        /// <summary>
        /// Returns true iff node is a Ascendant "Path of the ..." node.
        /// </summary>
        public static bool IsAscendantClassStartNode(SkillNode node)
        {
            return node.attributes.Any(s => AscendantClassStartRegex.IsMatch(s));
        }

        /// <summary>
        /// Changes the HighlightState of the node:
        /// None -> Checked -> Crossed -> None -> ...
        /// (preserves other HighlightStates than Checked and Crossed)
        /// </summary>
        /// <param name="node">Node to change the HighlightState for</param>
        public void CycleNodeTagForward(SkillNode node)
        {
            if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Checked))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Checked);
                _nodeHighlighter.HighlightNode(node, HighlightState.Crossed);
            } 
            else if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Crossed))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Crossed);
            }
            else
            {
                _nodeHighlighter.HighlightNode(node, HighlightState.Checked);
            }
            DrawHighlights();
        }

        /// <summary>
        /// Changes the HighlightState of the node:
        /// ... <- None <- Checked <- Crossed <- None
        /// (preserves other HighlightStates than Checked and Crossed)
        /// </summary>
        /// <param name="node">Node to change the HighlightState for</param>
        public void CycleNodeTagBackward(SkillNode node)
        {
            if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Crossed))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Crossed);
                _nodeHighlighter.HighlightNode(node, HighlightState.Checked);
            }
            else if (_nodeHighlighter.NodeHasHighlights(node, HighlightState.Checked))
            {
                _nodeHighlighter.UnhighlightNode(node, HighlightState.Checked);
            }
            else
            {
                _nodeHighlighter.HighlightNode(node, HighlightState.Crossed);
            }
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
                            nd => (matchFct(nd.attributes, att => regex.IsMatch(att)) ||
                                  regex.IsMatch(nd.Name) && nd.Type != NodeType.Mastery) &&
                                  (DrawAscendancy ? (_persistentData.Options.ShowAllAscendancyClasses || (nd.ascendancyName == GetAscendancyClass(SkilledNodes) || nd.ascendancyName == null)) : nd.ascendancyName == null));
                    _nodeHighlighter.ReplaceHighlights(nodes, flag);
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
                        nd => (matchFct(nd.attributes, att => att.ToLowerInvariant().Contains(search)) ||
                              nd.Name.ToLowerInvariant().Contains(search) && nd.Type != NodeType.Mastery) &&
                              (DrawAscendancy ? (_persistentData.Options.ShowAllAscendancyClasses || (nd.ascendancyName == GetAscendancyClass(SkilledNodes) || nd.ascendancyName == null)) : nd.ascendancyName == null ));
                _nodeHighlighter.ReplaceHighlights(nodes, flag);
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
            DrawHighlights();
        }

        public void CheckAllHighlightedNodes()
        {
            _nodeHighlighter.HighlightNodesIf(HighlightState.Checked, HighlightState.Highlights);
            DrawHighlights();
        }

        public void CrossAllHighlightedNodes()
        {
            _nodeHighlighter.HighlightNodesIf(HighlightState.Crossed, HighlightState.Highlights);
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

        public static void DecodeUrl(string url, out HashSet<SkillNode> skillednodes, out int chartype, out int asctype)
        {
            skillednodes = new HashSet<SkillNode>();
            chartype = 0;
            asctype = 0;

            if (string.IsNullOrEmpty(url))
                return;

            url = Regex.Replace(url, @"\t| |\n|\r", "");
            string s =
                url.Substring(TreeAddress.Length + (url.StartsWith("https") ? 0 : -1))
                    .Replace("-", "+")
                    .Replace("_", "/");
            byte[] decbuff = Convert.FromBase64String(s);
            int i = BitConverter.ToInt32(new[] { decbuff[3], decbuff[2], decbuff[1], decbuff[0] }, 0);
            byte b = decbuff[4];
            long j = 0L;
            byte asc = decbuff[5];
            if (decbuff.Length >= 7)
                j = decbuff[6];
            var nodes = new List<UInt16>();
            for (int k = (i > 3 ? 7 : 6); k < decbuff.Length; k += 2)
            {
                byte[] dbff = { decbuff[k + 1], decbuff[k + 0] };
                if (Skillnodes.Keys.Contains(BitConverter.ToUInt16(dbff, 0)))
                    nodes.Add((BitConverter.ToUInt16(dbff, 0)));
            }
            chartype = b;
            asctype = asc;

            SkillNode startnode = Skillnodes.First(nd => nd.Value.Name.ToUpperInvariant() == CharName[b]).Value;
            skillednodes.Add(startnode);
            if (asc > 0)
            {
                SkillNode ascNode = Skillnodes.First(nd => nd.Value.ascendancyName == _ascClasses.GetClassName(b, asc) && nd.Value.IsAscendancyStart).Value;
                skillednodes.Add(ascNode);
            }

            foreach (var node in nodes)
            {
                skillednodes.Add(Skillnodes[node]);
            }

        }

        public void LoadFromUrl(string url)
        {
            int b;
            int asc;
            HashSet<SkillNode> snodes;
            DecodeUrl(url, out snodes, out b, out asc);
            Chartype = b;
            AscType = asc;
            SkilledNodes.Clear();
            AllocateSkillNodes(snodes);
        }

        public void Reset()
        {
            var prefs = _persistentData.Options.ResetPreferences;
            var ascNodes = SkilledNodes.Where(n => n.ascendancyName != null).ToList();
            if (prefs.HasFlag(ResetPreferences.MainTree))
            {
                SkilledNodes.Clear();
                if (prefs.HasFlag(ResetPreferences.AscendancyTree))
                    AscType = 0;
                else
                    SkilledNodes.UnionWith(ascNodes);
                var rootNode = Skillnodes.First(nd => nd.Value.Name.ToUpperInvariant() == CharName[_chartype]);
                SkilledNodes.Add(rootNode.Value);
            }
            else if (prefs.HasFlag(ResetPreferences.AscendancyTree))
            {
                SkilledNodes.ExceptWith(ascNodes);
                AscType = 0;
            }
            if (prefs.HasFlag(ResetPreferences.Bandits))
                BanditSettings.Reset();
            UpdateAscendancyClasses = true;
        }

        public string SaveToUrl()
        {
            var points = GetPointCount();
            var count = points["NormalUsed"] + points["AscendancyUsed"] + points["ScionAscendancyChoices"];
            var b = new byte[7 + count * 2];
            AscType = AscClasses.GetClassNumber(GetAscendancyClass(SkilledNodes));
            var b2 = GetCharacterBytes((byte)Chartype, (byte) AscType);
            for (var i = 0; i < b2.Length; i++)
                b[i] = b2[i];
            int pos = 7;
            foreach (var inn in SkilledNodes)
            {
                if (CharName.Contains(inn.Name.ToUpperInvariant()))
                    continue;
                if (inn.IsAscendancyStart)
                    continue;
                byte[] dbff = BitConverter.GetBytes((short)inn.Id);
                b[pos++] = dbff[1];
                b[pos++] = dbff[0];
            }
            return TreeAddress + Convert.ToBase64String(b).Replace("/", "_").Replace("+", "-");
        }

        public static string GetCharacterUrl(byte charTypeByte = 0, byte ascTypeByte = 0)
        {
            var b = GetCharacterBytes(charTypeByte, ascTypeByte);
            return Convert.ToBase64String(b).Replace("/", "_").Replace("+", "-");
        }

        public static byte[] GetCharacterBytes(byte charTypeByte = 0, byte ascTypeByte = 0)
        {
            var b = new byte[7];
            byte[] b2 = BitConverter.GetBytes(4); //skilltree version
            for (var i = 0; i < b2.Length; i++)
                b[i] = b2[(b2.Length - 1) - i];
            b[4] = (byte)(charTypeByte);
            b[5] = (byte)(ascTypeByte); //ascedancy class
            b[6] = 0;
            return b;
        }

        /// <summary>
        /// Returns all currently Check-tagged nodes.
        /// </summary>
        public HashSet<SkillNode> GetCheckedNodes()
        {
            var nodes = new HashSet<SkillNode>();
            foreach (var entry in _nodeHighlighter.nodeHighlights)
            {
                if (!RootNodeList.Contains(entry.Key.Id) && entry.Value.HasFlag(HighlightState.Checked))
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
            foreach (var entry in _nodeHighlighter.nodeHighlights)
            {
                if (!RootNodeList.Contains(entry.Key.Id) && entry.Value.HasFlag(HighlightState.Crossed))
                {
                    nodes.Add(entry.Key);
                }
            }
            return nodes;
        }

        public async Task SkillAllTaggedNodesAsync()
        {
            if (!GetCheckedNodes().Except(SkilledNodes).Any())
            {
                await _dialogCoordinator.ShowInfoAsync(this,
                    L10n.Message("Please tag non-skilled nodes by right-clicking them."));
                return;
            }

#if !DEBUG
            try
            {
#endif
                // Use the SettingsViewModel without View and with a fixed SteinerTabViewModel.
                var settingsVm = new SettingsViewModel(this, SettingsDialogCoordinator.Instance,
                    new SteinerTabViewModel(this)) {Iterations = 1};
                var registration = DialogParticipation.GetAssociation(this);
                DialogParticipation.SetRegister(registration, settingsVm);
                await settingsVm.RunAsync();
                DialogParticipation.SetRegister(registration, this);
#if !DEBUG
            }
            catch (Exception e)
            {
                await _dialogCoordinator.ShowErrorAsync(L10n.Message("Error while trying to find solution"), e.Message);
            }
#endif
        }
        public SkillNode GetCharNode()
        {
            return Skillnodes[GetCharNodeId()];
        }
        public ushort GetCharNodeId()
        {
            return (ushort)RootNodeClassDictionary[CharName[_chartype]];
        }
        public SkillNode GetAscNode()
        {
            var ascNodeId = GetAscNodeId();
            if (ascNodeId != 0)
                return Skillnodes[ascNodeId];
            else
                return null;
        }
        public ushort GetAscNodeId()
        {
            if (AscType <= 0 || AscType > 3)
                return 0;
            var className = CharacterNames.GetClassNameFromChartype(_chartype);
            var ascendancyClassName = AscClasses.GetClassName(className, AscType);
            try
            {
                return Skillnodes.First(x => x.Value.ascendancyName == ascendancyClassName && x.Value.IsAscendancyStart).Key;
            }
            catch
            {
                return 0;
            }
        }

        public string GetAscendancyClass(IEnumerable<SkillNode> skilledNodes)
        {
            foreach (var node in skilledNodes)
            {
                if (node.ascendancyName != null)
                    return node.ascendancyName;
            }
            return null;
        }

        /// <summary>
        /// Splits multiline attribute strings (i.e. strings containing "\n" characters) into multiple attribute strings.
        /// </summary>
        /// <param name="attrs">An array of attribute strings to split.</param>
        /// <returns>An array of attributes strings.</returns>
        private static string[] SplitMultilineAttributes(string[] attrs)
        {
            if (attrs == null || attrs.Length == 0)
                return attrs;

            List<string> split = new List<string>();
            for (int i = 0; i < attrs.Length; ++i)
                split.AddRange(attrs[i].Split('\n'));

            return split.ToArray();
        }

        private HashSet<SkillNode> GetAvailableNodes(IEnumerable<SkillNode> skilledNodes)
        {
            var availNodes = new HashSet<SkillNode>();

            foreach (var node in skilledNodes)
            {
                foreach (var skillNode in node.Neighbor)
                {
                    if (!CharName.Contains(skillNode.Name.ToUpperInvariant()) && !SkilledNodes.Contains(skillNode))
                        availNodes.Add(skillNode);
                }
            }
            return availNodes;
        }

        public static IEnumerable<KeyValuePair<string, List<float>>> ExpandHybridAttributes(Dictionary<string, List<float>> attributes)
        {
            return attributes.SelectMany(ExpandHybridAttributes);
        }

        public static IEnumerable<KeyValuePair<string, List<float>>> ExpandHybridAttributes(KeyValuePair<string, List<float>> attribute)
        {
            List<string> expandedAttributes;
            if (HybridAttributes.TryGetValue(attribute.Key, out expandedAttributes))
            {
                foreach (var expandedAttribute in expandedAttributes)
                {
                    yield return new KeyValuePair<string, List<float>>(expandedAttribute, attribute.Value);
                }
            }
            else
            {
                yield return attribute;
            }
        }

        public bool CanSwitchClass(string className)
        {
            int rootNodeValue;

            RootNodeClassDictionary.TryGetValue(className.ToUpperInvariant(), out rootNodeValue);
            var classSpecificStartNodes = StartNodeDictionary.Where(kvp => kvp.Value == rootNodeValue).Select(kvp => kvp.Key).ToList();

            foreach(var node in classSpecificStartNodes)
            {
                var temp = GetShortestPathTo(Skillnodes[(ushort)node], SkilledNodes);

                if (!temp.Any() && Skillnodes[(ushort) node].ascendancyName == null)
                    return true;
            }
            return false;
        }
    }
}
