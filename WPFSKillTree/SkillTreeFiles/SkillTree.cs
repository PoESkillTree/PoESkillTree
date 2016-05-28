using Newtonsoft.Json;
using POESKillTree.Localization;
using POESKillTree.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model;
using POESKillTree.TreeGenerator.ViewModels;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
using HighlightState = POESKillTree.SkillTreeFiles.NodeHighlighter.HighlightState;

namespace POESKillTree.SkillTreeFiles
{
    public partial class SkillTree : Notifier
    {
        public delegate void UpdateLoadingWindow(double current, double max);

        public delegate void CloseLoadingWindow();

        public delegate void StartLoadingWindow(string infoText);

        public static readonly float LifePerLevel = 12;
        public static readonly float AccPerLevel = 2;
        public static readonly float EvasPerLevel = 3;
        public static readonly float ManaPerLevel = 6;
        public static readonly float IntPerMana = 2;
        public static readonly float IntPerES = 5; //%
        public static readonly float StrPerLife = 2;
        public static readonly float StrPerED = 5; //%
        public static readonly float DexPerAcc = 0.5f;
        public static readonly float DexPerEvas = 5; //%
        public static readonly string TreeAddress = "https://www.pathofexile.com/passive-skill-tree/";
        public static readonly string TreeRegex = @"(http(|s):\/\/|).*?(character(\/|)|passive-skill-tree(\/|)|fullscreen-passive-skill-tree(\/|)|#|poeplanner.com(\/|))";

        public Vector2D ascedancyButtonPos = new Vector2D();
        /// <summary>
        /// Nodes with an attribute matching this regex are one of the "Path of the ..." nodes connection Scion
        /// Ascendant with other classes.
        /// </summary>
        private static readonly Regex AscendantClassStartRegex = new Regex(@"Can Allocate Passives from the .* starting point");

        // The absolute path of Assets folder (contains trailing directory separator).
        public static string AssetsFolderPath;
        // The absolute path of Data folder (contains trailing directory separator).
        public static string DataFolderPath;

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

        public static readonly List<string> FaceNames = new List<string>
        {
            "centerscion",
            "centermarauder",
            "centerranger",
            "centerwitch",
            "centerduelist",
            "centertemplar",
            "centershadow"
        };

        public static readonly Dictionary<string, string> NodeBackgrounds = new Dictionary<string, string>
        {
            {"normal", "PSSkillFrame"},
            {"notable", "NotableFrameUnallocated"},
            {"keystone", "KeystoneFrameUnallocated"},
            {"jewel", "JewelFrameUnallocated"},
            {"ascendancyNormal", "PassiveSkillScreenAscendancyFrameSmallNormal"},
            {"ascendancyNotable", "PassiveSkillScreenAscendancyFrameLargeNormal"}
        };

        public static readonly Dictionary<string, string> NodeBackgroundsActive = new Dictionary<string, string>
        {
            {"normal", "PSSkillFrameActive"},
            {"notable", "NotableFrameAllocated"},
            {"keystone", "KeystoneFrameAllocated"},
            {"jewel", "JewelFrameAllocated"},
            {"ascendancyNormal", "PassiveSkillScreenAscendancyFrameSmallAllocated"},
            {"ascendancyNotable", "PassiveSkillScreenAscendancyFrameLargeAllocated"}
        };

        private static SkillIcons _IconActiveSkills;

        public static SkillIcons IconActiveSkills
        {
            get { return SkillTree._IconActiveSkills; }
        }
        private static SkillIcons _IconInActiveSkills;

        public static SkillIcons IconInActiveSkills
        {
            get { return SkillTree._IconInActiveSkills; }
        }

        private static Dictionary<UInt16, SkillNode> _Skillnodes;

        public static Dictionary<UInt16, SkillNode> Skillnodes
        {
            get { return SkillTree._Skillnodes; }
        }

        private static string[] _allAttributes;
        /// <summary>
        /// Gets an Array of all the attributes of SkillNodes.
        /// </summary>
        public static string[] AllAttributes
        {
            get { return _allAttributes ?? (_allAttributes = Skillnodes.Values.SelectMany(n => n.Attributes.Keys).Distinct().ToArray()); }
        }

        private static Dictionary<string, float>[] _CharBaseAttributes;

        public static Dictionary<string, float>[] CharBaseAttributes
        {
            get { return SkillTree._CharBaseAttributes; }
        }

        private static List<int> _rootNodeList;

        public static List<int> rootNodeList
        {
            get { return SkillTree._rootNodeList; }
        }

        private static List<SkillNodeGroup> _NodeGroups;

        public static List<SkillNodeGroup> NodeGroups
        {
            get { return SkillTree._NodeGroups; }
        }

        private static Rect2D _TRect;

        public static Rect2D TRect
        {
            get { return SkillTree._TRect; }
        }

        public static Dictionary<string, int> rootNodeClassDictionary
        {
            get { return SkillTree._rootNodeClassDictionary; }
        }

        private static List<string> _AttributeTypes = new List<string>();

        public static List<string> AttributeTypes
        {
            get { return _AttributeTypes; }
        }

        private static Dictionary<int, int> _startNodeDictionary = new Dictionary<int, int>();

        public static Dictionary<int, int> startNodeDictionary
        {
            get { return SkillTree._startNodeDictionary; }
        }

        private static readonly Dictionary<string, Asset> _assets = new Dictionary<string, Asset>();

        public Dictionary<string, Asset> Assets { get { return _assets; } }

        private static Dictionary<string, int> _rootNodeClassDictionary = new Dictionary<string, int>();

        private static readonly List<ushort[]> _links = new List<ushort[]>();
        
        public HashSet<ushort> SkilledNodes = new HashSet<ushort>();

        public HashSet<ushort> HighlightedNodes = new HashSet<ushort>();

        private int _chartype;

        private int _asctype;
        
        public static int UndefinedLevel { get { return 0; } }

        public static int MaximumLevel { get { return 100; } }

        private int _level = UndefinedLevel;

        private static AscendancyClasses _asendancyClasses;
        
        public AscendancyClasses AscendancyClasses 
        {
            get { return SkillTree._asendancyClasses; }
        }

        private BanditSettings _banditSettings = new BanditSettings();
        public BanditSettings BanditSettings
        {
            get { return _banditSettings; }
            set { SetProperty(ref _banditSettings, value); }
        }

        private readonly IDialogCoordinator _dialogCoordinator;

        private static bool _Initialized = false;

        private SkillTree(IPersistentData persistentData, IDialogCoordinator dialogCoordinator, string treestring,
            string opsstring , bool displayProgress, UpdateLoadingWindow update)
        {
            _persistentData = persistentData;
            _dialogCoordinator = dialogCoordinator;

            PoESkillTree inTree = null;
            if (!_Initialized)
            {
                var jss = new JsonSerializerSettings
                {
                    Error = delegate(object sender, ErrorEventArgs args)
                    {
                        Debug.WriteLine(args.ErrorContext.Error.Message);
                        args.ErrorContext.Handled = true;
                    }
                };
                
                inTree = JsonConvert.DeserializeObject<PoESkillTree>(treestring, jss);
                var inOpts = JsonConvert.DeserializeObject<Opts>(opsstring, jss);

                _IconInActiveSkills = new SkillIcons();
                _IconActiveSkills = new SkillIcons();
                foreach (var obj in inTree.skillSprites)
                {
                    if (obj.Key.Contains("Active"))
                    {
                        //Adds active nodes to IconActiveSkills
                        _IconActiveSkills.Images[obj.Value[3].filename] = null;
                        foreach (var o in obj.Value[3].coords)
                        {
                            _IconActiveSkills.SkillPositions[o.Key + "_" + o.Value.w] =
                                new KeyValuePair<Rect, string>(new Rect(o.Value.x, o.Value.y, o.Value.w, o.Value.h),
                                    obj.Value[3].filename);
                        }
                    }
                    else 
                    {
                        //Adds inactive nodes and masteries to IconInActiveSkills
                        _IconInActiveSkills.Images[obj.Value[3].filename] = null;
                        foreach (var o in obj.Value[3].coords)
                        {
                            _IconInActiveSkills.SkillPositions[o.Key + "_" + o.Value.w] =
                                new KeyValuePair<Rect, string>(new Rect(o.Value.x, o.Value.y, o.Value.w, o.Value.h),
                                    obj.Value[3].filename);
                        }
                    }
                }

                foreach (var ass in inTree.assets)
                {
                    _assets[ass.Key] = new Asset(ass.Key,
                        ass.Value.ContainsKey(0.3835f) ? ass.Value[0.3835f] : ass.Value.Values.First());
                }

                _rootNodeList = new List<int>();
                if (inTree.root != null)
                {
                    foreach (int i in inTree.root.ot)
                    {
                        _rootNodeList.Add(i);
                    }
                }
                else if (inTree.main != null)
                {
                    foreach (int i in inTree.main.ot)
                    {
                        _rootNodeList.Add(i);
                    }
                }

                _asendancyClasses = new AscendancyClasses();
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
                        _asendancyClasses.Classes.Add(ascClass.Value.name, classes);
                    }
                }

            }

            if (displayProgress)
                update(50, 100);

            if (!_Initialized)
                _IconActiveSkills.OpenOrDownloadImages(update);

            if (displayProgress)
                update(75, 100);

            if (!_Initialized)
            {
                _IconInActiveSkills.OpenOrDownloadImages(update);

                _CharBaseAttributes = new Dictionary<string, float>[7];
                foreach (var c in inTree.characterData)
                {
                    _CharBaseAttributes[c.Key] = new Dictionary<string, float>
                {
                    {"+# to Strength", c.Value.base_str},
                    {"+# to Dexterity", c.Value.base_dex},
                    {"+# to Intelligence", c.Value.base_int}
                };
                }

                _Skillnodes = new Dictionary<ushort, SkillNode>();
                _rootNodeClassDictionary = new Dictionary<string, int>();
                _startNodeDictionary = new Dictionary<int, int>();

                foreach (Node nd in inTree.nodes)
                {
                    _Skillnodes.Add(nd.id, new SkillNode
                    {
                        Id = nd.id,
                        Name = nd.dn,
                        attributes = nd.dn.Contains("Jewel Socket") ? new string[1] { "+1 Jewel Socket" } : SplitMultilineAttributes(nd.sd),
                        Orbit = nd.o,
                        OrbitIndex = nd.oidx,
                        Icon = nd.icon,
                        LinkId = nd.ot,
                        G = nd.g,
                        Da = nd.da,
                        Ia = nd.ia,
                        IsKeyStone = nd.ks,
                        IsNotable = nd.not,
                        IsJewelSocket = nd.isJewelSocket,
                        Sa = nd.sa,
                        IsMastery = nd.m,
                        Spc = nd.spc.Count() > 0 ? (int?)nd.spc[0] : null,
                        IsMultipleChoice = nd.isMultipleChoice,
                        IsMultipleChoiceOption = nd.isMultipleChoiceOption,
                        passivePointsGranted = nd.passivePointsGranted,
                        ascendancyName = nd.ascendancyName,
                        IsAscendancyStart = nd.isAscendancyStart,
                        reminderText = nd.reminderText
                    });
                    if (_rootNodeList.Contains(nd.id))
                    {
                        if (!_rootNodeClassDictionary.ContainsKey(nd.dn.ToString().ToUpperInvariant()))
                        {
                            _rootNodeClassDictionary.Add(nd.dn.ToString().ToUpperInvariant(), nd.id);
                        }
                        foreach (int linkedNode in nd.ot)
                        {
                            if (!_startNodeDictionary.ContainsKey(nd.id) && !nd.isAscendancyStart)
                            {
                                _startNodeDictionary.Add(linkedNode, nd.id);
                            }
                        }
                    }
                    foreach (int node in nd.ot)
                    {
                        if (!_startNodeDictionary.ContainsKey(nd.id) && _rootNodeList.Contains(node))
                        {
                            _startNodeDictionary.Add(nd.id, node);
                        }
                    }

                }

                foreach (var skillNode in Skillnodes)
                {
                    foreach (ushort i in skillNode.Value.LinkId)
                    {
                        if (_links.Count(nd => (nd[0] == i && nd[1] == skillNode.Key) || nd[0] == skillNode.Key && nd[1] == i) != 1)
                            _links.Add(new[] { skillNode.Key, i });
                    }
                }
                foreach (var ints in _links)
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

                    if (isScionAscendancyNotable && _startNodeDictionary.Keys.Contains(ints[0]))
                    {
                        if (!Skillnodes[ints[1]].Neighbor.Contains(Skillnodes[ints[0]]))
                            Skillnodes[ints[1]].Neighbor.Add(Skillnodes[ints[0]]);
                    }
                    else if (isScionAscendancyNotable && _startNodeDictionary.Keys.Contains(ints[1]))
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

                _NodeGroups = new List<SkillNodeGroup>();
                foreach (var gp in inTree.groups)
                {
                    var ng = new SkillNodeGroup();

                    ng.OcpOrb = gp.Value.oo;
                    ng.Position = new Vector2D(gp.Value.x, gp.Value.y);
                    foreach (ushort node in gp.Value.n)
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
                _TRect = new Rect2D(new Vector2D(inTree.min_x * 1.1 - padding, inTree.min_y * 1.1 - padding),
                    new Vector2D(inTree.max_x * 1.1 + padding, inTree.max_y * 1.1 + padding));
            }


            if (_persistentData.Options.ShowAllAscendancyClasses)
                drawAscendancy = true;
            InitializeLayers();
            DrawInitialLayers();
            CreateCombineVisual();

            if (displayProgress)
                update(100, 100);

            _Initialized = true;
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
                var nodeInfo = Skillnodes[node];
                if (nodeInfo.ascendancyName == null && !rootNodeList.Contains(node))
                    points["NormalUsed"] += 1;
                else if (nodeInfo.ascendancyName != null && !nodeInfo.IsAscendancyStart && !nodeInfo.IsMultipleChoiceOption)
                {
                    points["AscendancyUsed"] += 1;
                    points["NormalTotal"] += nodeInfo.passivePointsGranted;
                }
                else if (nodeInfo.IsMultipleChoiceOption)
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
            var add = new HashSet<ushort>();
            if (AscType != -1)
            {
                ushort sn = GetAscNodeId();
                if (sn != 0)
                {
                    add.Add(sn);
                    foreach (var n in SkilledNodes)
                    {
                        if (Skillnodes[sn].ascendancyName != Skillnodes[n].ascendancyName) continue;
                        add.Add(n);
                    }
                }
            }
            if (CanSwitchClass(CharName[_chartype]))
            {
                foreach (var n in SkilledNodes)
                {
                    if (Skillnodes[n].ascendancyName != null) continue;
                    if (_rootNodeList.Contains(n)) continue;
                    add.Add(n);
                }
            }
            SkilledNodes = add;
            SkilledNodes.Add(GetCharNodeId());
            drawAscendancy = _persistentData.Options.ShowAllAscendancyClasses;
            DrawAscendancyLayers();
            UpdateAscendancyClasses = true;
        }

        public Dictionary<string, List<float>> HighlightedAttributes;

        public Dictionary<string, List<float>> SelectedAttributes
        {
            get
            {
                return GetAttributes(SkilledNodes, Chartype, Level, BanditSettings);
            }
        }

        public static Dictionary<string, List<float>> GetAttributes(IEnumerable<ushort> skilledNodes, int chartype, int level, BanditSettings banditSettings)
        {
            Dictionary<string, List<float>> temp = GetAttributesWithoutImplicit(skilledNodes,chartype, banditSettings);

            foreach (var a in ImplicitAttributes(temp, level))
            {
                string key = RenameImplicitAttributes.ContainsKey(a.Key) ? RenameImplicitAttributes[a.Key] : a.Key;

                if (!temp.ContainsKey(key))
                    temp[key] = new List<float>();
                for (int i = 0; i < a.Value.Count; i++)
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
        {
            get
            {
                return GetAttributesWithoutImplicit(SkilledNodes, Chartype, BanditSettings);
            }
        }


        public static Dictionary<string, List<float>> GetAttributesWithoutImplicit(IEnumerable<ushort> skilledNodes, int chartype, BanditSettings banditSettings)
        {
            var temp = new Dictionary<string, List<float>>();

            foreach (var attr in CharBaseAttributes[chartype].Union(BaseAttributes).Union(banditSettings.Rewards))
            {
                if (!temp.ContainsKey(attr.Key))
                    temp[attr.Key] = new List<float> {attr.Value};
                else if (temp[attr.Key].Any())
                    temp[attr.Key][0] += attr.Value;
            }

            foreach (ushort inode in skilledNodes)
            {
                SkillNode node = Skillnodes[inode];
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

        public static SkillTree CreateSkillTree(IPersistentData persistentData, IDialogCoordinator dialogCoordinator,
            StartLoadingWindow start = null, UpdateLoadingWindow update = null, CloseLoadingWindow finish = null)
        {
            AssetsFolderPath = AppData.GetFolder(Path.Combine("Data", "Assets"), true);
            DataFolderPath = AppData.GetFolder("Data", true);

            string skillTreeFile = DataFolderPath + "Skilltree.txt";
            string skilltreeobj = "";
            if (File.Exists(skillTreeFile))
            {
                skilltreeobj = File.ReadAllText(skillTreeFile);
            }

            bool displayProgress = false;
            if (skilltreeobj == "")
            {
                displayProgress = (start != null && update != null && finish != null);
                if (displayProgress)
                    start(L10n.Message("Downloading Skill tree assets"));
                string uriString = SkillTree.TreeAddress;
                var req = (HttpWebRequest)WebRequest.Create(uriString);
                var resp = (HttpWebResponse)req.GetResponse();
                string code = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                var regex = new Regex("var passiveSkillTreeData.*");
                skilltreeobj = regex.Match(code).Value.Replace("\\/", "/");
                skilltreeobj = skilltreeobj.Substring(27, skilltreeobj.Length - 27 - 1) + "";
                File.WriteAllText(skillTreeFile, skilltreeobj);
            }

            string optsFile = DataFolderPath + "Opts.txt";
            string optsobj = "";
            if (File.Exists(optsFile))
            {
                optsobj = File.ReadAllText(optsFile);
            }
            if (optsobj == "")
            {
                string uriString = SkillTree.TreeAddress;
                var req = (HttpWebRequest)WebRequest.Create(uriString);
                var resp = (HttpWebResponse)req.GetResponse();
                string code = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                var regex = new Regex(@"ascClasses:.*");
                optsobj = regex.Match(code).Value.Replace("ascClasses", "{ \"ascClasses\"");
                optsobj = optsobj.Substring(0, optsobj.Length - 1) + "}";
                File.WriteAllText(optsFile, optsobj);
            }
            if (displayProgress)
                update(25, 100);
            var skillTree = new SkillTree(persistentData, dialogCoordinator, skilltreeobj, optsobj, displayProgress,
                update);
            if (displayProgress)
                finish();
            return skillTree;
        }

        public void ForceRefundNode(ushort nodeId)
        {
            if (!SkilledNodes.Remove(nodeId))
                throw new InvalidOperationException();

            //SkilledNodes.Remove(nodeId);

            var charStart = GetCharNodeId();
            var front = new HashSet<ushort>();
            front.Add(charStart);
            foreach (SkillNode i in Skillnodes[charStart].Neighbor)
                if (SkilledNodes.Contains(i.Id))
                    front.Add(i.Id);
            var skilled_reachable = new HashSet<ushort>(front);
            while (front.Count > 0)
            {
                var newFront = new HashSet<ushort>();
                foreach (ushort i in front)
                    foreach (ushort j in Skillnodes[i].Neighbor.Select(nd => nd.Id))
                        if (!skilled_reachable.Contains(j) && SkilledNodes.Contains(j))
                        {
                            newFront.Add(j);
                            skilled_reachable.Add(j);
                        }

                front = newFront;
            }

            SkilledNodes = skilled_reachable;
        }

        public HashSet<ushort> ForceRefundNodePreview(ushort nodeId)
        {
            if (!SkilledNodes.Remove(nodeId))
                return new HashSet<ushort>();

            SkilledNodes.Remove(nodeId);

            var charStart = GetCharNodeId();
            var front = new HashSet<ushort>();
            front.Add(charStart);
            foreach (SkillNode i in Skillnodes[charStart].Neighbor)
                if (SkilledNodes.Contains(i.Id))
                    front.Add(i.Id);

            var skilled_reachable = new HashSet<ushort>(front);
            while (front.Count > 0)
            {
                var newFront = new HashSet<ushort>();
                foreach (ushort i in front)
                    foreach (ushort j in Skillnodes[i].Neighbor.Select(nd => nd.Id))
                        if (!skilled_reachable.Contains(j) && SkilledNodes.Contains(j))
                        {
                            newFront.Add(j);
                            skilled_reachable.Add(j);
                        }

                front = newFront;
            }

            var unreachable = new HashSet<ushort>(SkilledNodes);
            foreach (ushort i in skilled_reachable)
                unreachable.Remove(i);
            unreachable.Add(nodeId);

            SkilledNodes.Add(nodeId);

            return unreachable;
        }

        public List<ushort> GetShortestPathTo(ushort targetNode, HashSet<ushort> start)
        {
            if (start.Contains(targetNode))
                return new List<ushort>();
            var adjacent = GetAvailableNodes(start);
            if (adjacent.Contains(targetNode))
                return new List<ushort> { targetNode };

            var visited = new HashSet<ushort>(start);
            var distance = new Dictionary<int, int>();
            var parent = new Dictionary<ushort, ushort>();
            var newOnes = new Queue<ushort>();
            var toOmit = new HashSet<ushort>(
                         from entry in _nodeHighlighter.nodeHighlights
                         where entry.Value.HasFlag(HighlightState.Crossed)
                         select entry.Key.Id);

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
                foreach (var connection in Skillnodes[newNode].Neighbor.Select(x => x.Id))
                {
                    if (toOmit.Contains(connection))
                        continue;
                    if (visited.Contains(connection))
                        continue;
                    if (distance.ContainsKey(connection))
                        continue;
                    if (Skillnodes[newNode].Spc.HasValue)
                        continue;
                    if (Skillnodes[newNode].IsMastery)
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
                return new List<ushort>();

            var curr = targetNode;
            var result = new List<ushort> { curr };
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
                                  regex.IsMatch(nd.Name) && !nd.IsMastery) &&
                                  (drawAscendancy ? (_persistentData.Options.ShowAllAscendancyClasses ? true : nd.ascendancyName == GetAscendancyClass(SkilledNodes) || nd.ascendancyName == null) : nd.ascendancyName == null));
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
                              nd.Name.ToLowerInvariant().Contains(search) && !nd.IsMastery) &&
                              (drawAscendancy ? (_persistentData.Options.ShowAllAscendancyClasses ? true : nd.ascendancyName == GetAscendancyClass(SkilledNodes) || nd.ascendancyName == null) : nd.ascendancyName == null ));
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
            var retval = new Dictionary<string, List<float>>();
            // +# to Strength", co["base_str"].Value<int>() }, { "+# to Dexterity", co["base_dex"].Value<int>() }, { "+# to Intelligence", co["base_int"].Value<int>() } };
            retval["+# to maximum Mana"] = new List<float>
            {
                attribs["+# to Intelligence"][0]/IntPerMana + level*ManaPerLevel
            };
            retval["#% increased maximum Energy Shield"] = new List<float>
            {
                (float) Math.Round(attribs["+# to Intelligence"][0]/IntPerES, 0)
            };

            retval["+# to maximum Life"] = new List<float>
            {
                attribs["+# to Strength"][0]/StrPerLife + level*LifePerLevel
            };
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

            int endurancecharges, frenzycharges, powercharges;
            endurancecharges = frenzycharges = powercharges = 0;
            if (attribs.ContainsKey("+# to Maximum Endurance Charges"))
                endurancecharges = (int)(attribs["+# to Maximum Endurance Charges"][0]);
            if (attribs.ContainsKey("+# to Maximum Frenzy Charges"))
                frenzycharges = (int)(attribs["+# to Maximum Frenzy Charges"][0]);
            if (attribs.ContainsKey("+# to Maximum Power Charges"))
                powercharges = (int)(attribs["+# to Maximum Power Charges"][0]);
            string newkey;
            foreach (string key in attribs.Keys)
            {
                if (key.Contains("per Endurance Charge") && endurancecharges > 0)
                {
                    newkey = key.Replace("per Endurance Charge", "with all Endurance Charges");
                    retval.Add(newkey, new List<float>());
                    foreach (float f in attribs[key])
                    {
                        retval[newkey].Add(f * endurancecharges);
                    }
                }
                if (key.Contains("per Frenzy Charge") && endurancecharges > 0)
                {
                    newkey = key.Replace("per Frenzy Charge", "with all Frenzy Charges");
                    retval.Add(newkey, new List<float>());
                    foreach (float f in attribs[key])
                    {
                        retval[newkey].Add(f * frenzycharges);
                    }
                }
                if (key.Contains("per Power Charge") && endurancecharges > 0)
                {
                    newkey = key.Replace("per Power Charge", "with all Power Charges");
                    retval.Add(newkey, new List<float>());
                    foreach (float f in attribs[key])
                    {
                        retval[newkey].Add(f * powercharges);
                    }
                }
            }

            return retval;
        }

        public static void DecodeURL(string url, out HashSet<ushort> skillednodes, out int chartype, out int asctype)
        {
            skillednodes = new HashSet<ushort>();
            chartype = 0;
            asctype = 0;

            if (url == "" || url == null)
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
            skillednodes.Add(startnode.Id);
            if (asc > 0)
            {
                SkillNode ascNode = Skillnodes.First(nd => nd.Value.ascendancyName == _asendancyClasses.GetClassName(CharName[b], asc) && nd.Value.IsAscendancyStart).Value;
                skillednodes.Add(ascNode.Id);
            }

            foreach (ushort node in nodes)
            {
                skillednodes.Add(node);
            }

        }

        public void LoadFromURL(string url)
        {
            int b;
            int asc;
            HashSet<ushort> snodes;
            DecodeURL(url, out snodes, out b, out asc);
            Chartype = b;
            AscType = asc;
            SkilledNodes = snodes;
            UpdateAvailNodes();
        }

        public void Reset()
        {
            var prefs = _persistentData.Options.ResetPreferences;
            var ascNodes = SkilledNodes.Where(n => Skillnodes[n].ascendancyName != null).ToList();
            if (prefs.HasFlag(ResetPreferences.MainTree))
            {
                SkilledNodes.Clear();
                if (!prefs.HasFlag(ResetPreferences.AscendancyTree))
                    SkilledNodes.UnionWith(ascNodes);
                var rootNode = Skillnodes.First(nd => nd.Value.Name.ToUpperInvariant() == CharName[_chartype]);
                AscType = 0;
                SkilledNodes.Add(rootNode.Value.Id);
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

        public string SaveToURL()
        {
            var points = GetPointCount();
            var count = points["NormalUsed"] + points["AscendancyUsed"] + points["ScionAscendancyChoices"];
            var b = new byte[7 + count * 2];
            AscType = AscendancyClasses.GetClassNumber(GetAscendancyClass(SkilledNodes));
            var b2 = GetCharacterBytes((byte)Chartype, (byte) AscType);
            for (var i = 0; i < b2.Length; i++)
                b[i] = b2[i];
            int pos = 7;
            foreach (ushort inn in SkilledNodes)
            {
                if (CharName.Contains(Skillnodes[inn].Name.ToUpperInvariant()))
                    continue;
                if (Skillnodes[inn].IsAscendancyStart)
                    continue;
                byte[] dbff = BitConverter.GetBytes((Int16)inn);
                b[pos++] = dbff[1];
                b[pos++] = dbff[0];
            }
            return TreeAddress + Convert.ToBase64String(b).Replace("/", "_").Replace("+", "-");
        }

        public static string GetCharacterURL(byte CharTypeByte = 0, byte AscTypeByte = 0)
        {
            var b = GetCharacterBytes(CharTypeByte, AscTypeByte);
            return Convert.ToBase64String(b).Replace("/", "_").Replace("+", "-");
        }

        public static byte[] GetCharacterBytes(byte CharTypeByte = 0, byte AscTypeByte = 0)
        {
            var b = new byte[7];
            byte[] b2 = BitConverter.GetBytes(4); //skilltree version
            for (var i = 0; i < b2.Length; i++)
                b[i] = b2[(b2.Length - 1) - i];
            b[4] = (byte)(CharTypeByte);
            b[5] = (byte)(AscTypeByte); //ascedancy class
            b[6] = 0;
            return b;
        }

        /// <summary>
        /// Returns all currently Check-tagged nodes.
        /// </summary>
        public HashSet<ushort> GetCheckedNodes()
        {
            var nodes = new HashSet<ushort>();
            foreach (var entry in _nodeHighlighter.nodeHighlights)
            {
                if (!rootNodeList.Contains(entry.Key.Id) && entry.Value.HasFlag(HighlightState.Checked))
                {
                    nodes.Add(entry.Key.Id);
                }
            }
            return nodes;
        }

        /// <summary>
        /// Returns all currently Cross-tagged nodes.
        /// </summary>
        public HashSet<ushort> GetCrossedNodes()
        {
            var nodes = new HashSet<ushort>();
            foreach (var entry in _nodeHighlighter.nodeHighlights)
            {
                if (!rootNodeList.Contains(entry.Key.Id) && entry.Value.HasFlag(HighlightState.Crossed))
                {
                    nodes.Add(entry.Key.Id);
                }
            }
            return nodes;
        }

        public async Task SkillAllTaggedNodes()
        {
            if (!GetCheckedNodes().Except(SkilledNodes).Any())
            {
                await _dialogCoordinator.ShowInfoAsync(this, L10n.Message("Please tag non-skilled nodes by right-clicking them."));
                return;
            }

#if !DEBUG
            try
            {
#endif
                // Use the SettingsViewModel without View and with a fixed SteinerTabViewModel.
                var settingsVm = new SettingsViewModel(this, SettingsDialogCoordinator.Instance, new SteinerTabViewModel(this));
                var registration = DialogParticipation.GetAssociation(this);
                DialogParticipation.SetRegister(registration, settingsVm);
                await settingsVm.RunAsync();
                DialogParticipation.SetRegister(registration, this);
#if !DEBUG
            }
            catch (Exception e)
            {
                _dialogCoordinator.ShowErrorAsync(L10n.Message("Error while trying to find solution"), e.Message);
            }
#endif
        }

        public ushort GetCharNodeId()
        {
            return (ushort)rootNodeClassDictionary[CharName[_chartype]];
        }

        public ushort GetAscNodeId()
        {
            if (AscType <= 0 || AscType > 3)
                return 0;
            var className = CharacterNames.GetClassNameFromChartype(_chartype);
            var ascendancyClassName = AscendancyClasses.GetClassName(className, AscType);
            try
            {
                return Skillnodes.First(x => x.Value.ascendancyName == ascendancyClassName && x.Value.IsAscendancyStart).Key;
            }
            catch
            {
                return 0;
            }
        }

        public string GetAscendancyClass(HashSet<ushort> skilledNodes)
        {
            HashSet<ushort> availNodes = new HashSet<ushort>();

            foreach (ushort inode in skilledNodes)
            {
                SkillNode node = Skillnodes[inode];
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

        public void UpdateAvailNodes(bool draw = true)
        {
            if (draw)
                UpdateAvailNodesDraw();
        }

        private HashSet<ushort> GetAvailableNodes(HashSet<ushort> skilledNodes)
        {
            HashSet<ushort> availNodes = new HashSet<ushort>();

            foreach (ushort inode in skilledNodes)
            {
                SkillNode node = Skillnodes[inode];
                foreach (SkillNode skillNode in node.Neighbor)
                {
                    if (!CharName.Contains(skillNode.Name.ToUpperInvariant()) && !SkilledNodes.Contains(skillNode.Id))
                        availNodes.Add(skillNode.Id);
                }
            }
            return availNodes;
        }

        public static Dictionary<string, List<float>> ExpandHybridAttributes(Dictionary<string, List<float>> attributes)
        {
            foreach (var attribute in attributes.ToList())
            {
                List<string> expandedAttributes;
                if (HybridAttributes.TryGetValue(attribute.Key, out expandedAttributes))
                {
                    attributes.Remove(attribute.Key);

                    foreach (string expandedAttribute in expandedAttributes)
                    {
                        attributes.Add(expandedAttribute, attribute.Value);
                    }
                }
            }

            return attributes;
        }

        public bool CanSwitchClass(string className)
        {
            int rootNodeValue;
            var temp = new List<ushort>();

            _rootNodeClassDictionary.TryGetValue(className.ToUpperInvariant(), out rootNodeValue);
            var classSpecificStartNodes = _startNodeDictionary.Where(kvp => kvp.Value == rootNodeValue).Select(kvp => kvp.Key).ToList();

            foreach(int node in classSpecificStartNodes)
            {
                temp = GetShortestPathTo((ushort) node, SkilledNodes);

                if (!temp.Any() && Skillnodes[(ushort) node].ascendancyName == null)
                    return true;
            }
            return false;
        }
    }
}
