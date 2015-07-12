using Newtonsoft.Json;
using POESKillTree.Localization;
using POESKillTree.Utils;
using POESKillTree.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;
using HighlightState = POESKillTree.SkillTreeFiles.NodeHighlighter.HighlightState;
using MessageBox = POESKillTree.Views.MetroMessageBox;

namespace POESKillTree.SkillTreeFiles
{
    public partial class SkillTree
    {
        public delegate void UpdateLoadingWindow(double current, double max);

        public delegate void CloseLoadingWindow();

        public delegate void StartLoadingWindow();

        public static readonly float LifePerLevel = 12;
        public static readonly float AccPerLevel = 2;
        public static readonly float EvasPerLevel = 3;
        public static readonly float ManaPerLevel = 4;
        public static readonly float IntPerMana = 2;
        public static readonly float IntPerES = 5; //%
        public static readonly float StrPerLife = 2;
        public static readonly float StrPerED = 5; //%
        public static readonly float DexPerAcc = 0.5f;
        public static readonly float DexPerEvas = 5; //%
        private const string TreeAddress = "http://www.pathofexile.com/passive-skill-tree/";

        // The absolute path of Assets folder (contains trailing directory separator).
        public static string AssetsFolderPath;
        // The absolute path of Data folder (contains trailing directory separator).
        public static string DataFolderPath;

        public static readonly Dictionary<string, float> BaseAttributes = new Dictionary<string, float>
        {
            {"+# to maximum Mana", 36},
            {"+# to maximum Life", 38},
            {"Evasion Rating: #", 53},
            {"+# Maximum Endurance Charge", 3},
            {"+# Maximum Frenzy Charge", 3},
            {"+# Maximum Power Charge", 3},
            {"#% Additional Elemental Resistance per Endurance Charge", 4},
            {"#% Physical Damage Reduction per Endurance Charge", 4},
            {"#% Attack Speed Increase per Frenzy Charge", 4},
            {"#% Cast Speed Increase per Frenzy Charge", 4},
            {"#% More Damage per Frenzy Charge", 4},
            {"#% Critical Strike Chance Increase per Power Charge", 50},
        };

        private static readonly Dictionary<string, List<string>> _hybridAttributes = new Dictionary<string, List<string>>
        {
            {
               "+# to Strength and Intelligence", 
               new List<string> {"+# to Strength", "+# to Intelligence"} 
            },
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
            "SEVEN",
            "MARAUDER",
            "RANGER",
            "WITCH",
            "DUELIST",
            "TEMPLAR",
            "SIX"
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
            {"jewel", "JewelFrameUnallocated"}
        };

        public static readonly Dictionary<string, string> NodeBackgroundsActive = new Dictionary<string, string>
        {
            {"normal", "PSSkillFrameActive"},
            {"notable", "NotableFrameAllocated"},
            {"keystone", "KeystoneFrameAllocated"},
            {"jewel", "JewelFrameAllocated"}
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

        private static Dictionary<string, int> _rootNodeClassDictionary = new Dictionary<string, int>();

        private static readonly List<ushort[]> _links = new List<ushort[]>();

        public Window MainWindow;

        public HashSet<ushort> AvailNodes = new HashSet<ushort>();

        public HashSet<ushort> SkilledNodes = new HashSet<ushort>();

        public HashSet<ushort> HighlightedNodes = new HashSet<ushort>();

        private int _chartype;
        private List<SkillNode> _highlightnodes;

        private int _level = 1;


        private static bool _Initialized = false;
        public SkillTree(String treestring, bool displayProgress, UpdateLoadingWindow update)
        {
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
            }
            int qindex = 0;

            if (!_Initialized)
            {
                SkillTree._IconInActiveSkills = new SkillIcons();
                //TODO: (SpaceOgre) This is not used atm, so no need to run it.
                foreach (var obj in inTree.skillSprites)
                {
                    if (obj.Key.Contains("inactive"))
                        continue;
                    _IconInActiveSkills.Images[obj.Value[3].filename] = null;
                    foreach (var o in obj.Value[3].coords)
                    {
                        _IconInActiveSkills.SkillPositions[o.Key + "_" + o.Value.w] =
                            new KeyValuePair<Rect, string>(new Rect(o.Value.x, o.Value.y, o.Value.w, o.Value.h),
                                obj.Value[3].filename);
                    }
                }
            }

            if (!_Initialized)
            {
                SkillTree._IconActiveSkills = new SkillIcons();
                foreach (var obj in inTree.skillSprites)
                {
                    if (obj.Key.Contains("active"))
                        continue;
                    _IconActiveSkills.Images[obj.Value[3].filename] = null;
                    foreach (var o in obj.Value[3].coords)
                    {
                        _IconActiveSkills.SkillPositions[o.Key + "_" + o.Value.w] =
                            new KeyValuePair<Rect, string>(new Rect(o.Value.x, o.Value.y, o.Value.w, o.Value.h),
                                obj.Value[3].filename);
                    }
                }
            }

            if (!_Initialized)
            {
                foreach (var ass in inTree.assets)
                {
                    _assets[ass.Key] = new Asset(ass.Key,
                        ass.Value.ContainsKey(0.3835f) ? ass.Value[0.3835f] : ass.Value.Values.First());
                }
            }

            if (!_Initialized)
            {
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
            }

            if (displayProgress)
                update(50, 100);

            if (!_Initialized)
                _IconActiveSkills.OpenOrDownloadImages(update);

            if (displayProgress)
                update(75, 100);

            if (!_Initialized)
                _IconInActiveSkills.OpenOrDownloadImages(update);


            if (!_Initialized)
            {
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
            }



            if (!_Initialized)
            {
                _Skillnodes = new Dictionary<ushort, SkillNode>();
                _rootNodeClassDictionary = new Dictionary<string, int>();
                _startNodeDictionary = new Dictionary<int, int>();

                foreach (Node nd in inTree.nodes)
                {
                    _Skillnodes.Add(nd.id, new SkillNode
                    {
                        Id = nd.id,
                        Name = nd.dn,
                        attributes = nd.dn.Contains("Jewel Socket") ? new string[1] {"+1 Jewel Socket"} : nd.sd,
                        Orbit = nd.o,
                        OrbitIndex = nd.oidx,
                        Icon = nd.icon,
                        LinkId = nd.ot,
                        G = nd.g,
                        Da = nd.da,
                        Ia = nd.ia,
                        IsKeyStone = nd.ks,
                        IsNotable = nd.not,
                        IsJewelSocket = nd.dn.Contains("Jewel Socket"),
                        Sa = nd.sa,
                        IsMastery = nd.m,
                        Spc = nd.spc.Count() > 0 ? (int?)nd.spc[0] : null
                    });
                    if (_rootNodeList.Contains(nd.id))
                    {
                        if (!_rootNodeClassDictionary.ContainsKey(nd.dn.ToString().ToUpper()))
                        {
                            _rootNodeClassDictionary.Add(nd.dn.ToString().ToUpper(), nd.id);
                        }
                        foreach (int linkedNode in nd.ot)
                        {
                            if (!_startNodeDictionary.ContainsKey(nd.id))
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
                        if (
                            _links.Count(nd => (nd[0] == i && nd[1] == skillNode.Key) || nd[0] == skillNode.Key && nd[1] == i) ==
                            1)
                        {
                            continue;
                        }
                        _links.Add(new[] { skillNode.Key, i });
                    }
                }
                foreach (var ints in _links)
                {
                    if (!Skillnodes[ints[0]].Neighbor.Contains(Skillnodes[ints[1]]))
                        Skillnodes[ints[0]].Neighbor.Add(Skillnodes[ints[1]]);
                    if (!Skillnodes[ints[1]].Neighbor.Contains(Skillnodes[ints[0]]))
                        Skillnodes[ints[1]].Neighbor.Add(Skillnodes[ints[0]]);
                }
            }



            if (!_Initialized)
            {
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

            }

            if (!_Initialized)
            {
                const int padding = 500; //This is to account for jewel range circles. Might need to find a better way to do it.
                _TRect = new Rect2D(new Vector2D(inTree.min_x * 1.1 - padding, inTree.min_y * 1.1 - padding),
                    new Vector2D(inTree.max_x * 1.1 + padding, inTree.max_y * 1.1 + padding));
            }


            InitNodeSurround();//

            DrawNodeSurround();
            DrawNodeBaseSurround();
            InitSkillIconLayers();
            DrawSkillIconLayer();
            DrawBackgroundLayer();
            InitFaceBrushesAndLayer();
            DrawLinkBackgroundLayer(_links);
            InitOtherDynamicLayers();
            CreateCombineVisual();

            if (_links != null)
            {
                var regexAttrib = new Regex("[0-9]*\\.?[0-9]+");
                foreach (var skillNode in Skillnodes)
                {
                    skillNode.Value.Attributes = new Dictionary<string, List<float>>();
                    foreach (string s in skillNode.Value.attributes)
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

                        skillNode.Value.Attributes[cs] = values;
                    }
                }
            }
            if (displayProgress)
                update(100, 100);

            _Initialized = true;
        }

        public int Level
        {
            get { return _level; }
            set { _level = value; }
        }

        public int Chartype
        {
            get { return _chartype; }
            set
            {
                _chartype = value;
                SkilledNodes.Clear();
                KeyValuePair<ushort, SkillNode> node =
                    Skillnodes.First(nd => nd.Value.Name.ToUpper() == CharName[_chartype]);
                SkilledNodes.Add(node.Value.Id);
                UpdateAvailNodes();
                DrawFaces();
            }
        }


        public Dictionary<string, List<float>> HighlightedAttributes;

        public Dictionary<string, List<float>> SelectedAttributes
        {
            get
            {
                return GetAttributes(SkilledNodes, Chartype, _level);
            }
        }

        public static Dictionary<string, List<float>> GetAttributes(IEnumerable<ushort> skilledNodes, int chartype, int level)
        {
            Dictionary<string, List<float>> temp = GetAttributesWithoutImplicit(skilledNodes,chartype);

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
                return GetAttributesWithoutImplicit(SkilledNodes,Chartype);
            }
        }


        public static Dictionary<string, List<float>> GetAttributesWithoutImplicit( IEnumerable<ushort> skilledNodes, int chartype)
        {
            var temp = new Dictionary<string, List<float>>();

            foreach (var attr in CharBaseAttributes[chartype])
            {
                if (!temp.ContainsKey(attr.Key))
                    temp[attr.Key] = new List<float>();

                if (temp.ContainsKey(attr.Key) && temp[attr.Key].Count > 0)
                    temp[attr.Key][0] += attr.Value;
                else
                {
                    temp[attr.Key].Add(attr.Value);
                }
            }

            foreach (var attr in BaseAttributes)
            {
                if (!temp.ContainsKey(attr.Key))
                    temp[attr.Key] = new List<float>();

                if (temp.ContainsKey(attr.Key) && temp[attr.Key].Count > 0)
                    temp[attr.Key][0] += attr.Value;
                else
                {
                    temp[attr.Key].Add(attr.Value);
                }
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

        public static SkillTree CreateSkillTree(StartLoadingWindow start = null, UpdateLoadingWindow update = null,
            CloseLoadingWindow finish = null)
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
                    start();
                string uriString = "http://www.pathofexile.com/passive-skill-tree/";
                var req = (HttpWebRequest)WebRequest.Create(uriString);
                var resp = (HttpWebResponse)req.GetResponse();
                string code = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                var regex = new Regex("var passiveSkillTreeData.*");
                skilltreeobj = regex.Match(code).Value.Replace("\\/", "/");
                skilltreeobj = skilltreeobj.Substring(27, skilltreeobj.Length - 27 - 1) + "";
                File.WriteAllText(skillTreeFile, skilltreeobj);
            }

            if (displayProgress)
                update(25, 100);
            var skillTree = new SkillTree(skilltreeobj, displayProgress, update);
            if (displayProgress)
                finish();
            return skillTree;
        }

        public void ForceRefundNode(ushort nodeId)
        {
            if (!SkilledNodes.Remove(nodeId))
                throw new InvalidOperationException();

            //SkilledNodes.Remove(nodeId);

            var front = new HashSet<ushort>();
            front.Add(SkilledNodes.First());
            foreach (SkillNode i in Skillnodes[SkilledNodes.First()].Neighbor)
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
            AvailNodes = new HashSet<ushort>();
            UpdateAvailNodes();
        }

        public HashSet<ushort> ForceRefundNodePreview(ushort nodeId)
        {
            if (!SkilledNodes.Remove(nodeId))
                return new HashSet<ushort>();

            SkilledNodes.Remove(nodeId);

            var front = new HashSet<ushort>();
            front.Add(SkilledNodes.First());
            foreach (SkillNode i in Skillnodes[SkilledNodes.First()].Neighbor)
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
            DrawHighlights(_nodeHighlighter);
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
            DrawHighlights(_nodeHighlighter);
        }

        public void HighlightNodesBySearch(string search, bool useregex, bool fromSearchBox)
        {
            HighlightState flag = fromSearchBox ? HighlightState.FromSearch : HighlightState.FromAttrib;
            if (search == "")
            {
                _nodeHighlighter.UnhighlightAllNodes(flag);
                DrawHighlights(_nodeHighlighter);
                return;
            }

            if (useregex)
            {
                try
                {
                    List<SkillNode> nodes =
                            Skillnodes.Values.Where(
                                nd =>
                                    nd.attributes.Any(att => new Regex(search, RegexOptions.IgnoreCase).IsMatch(att)) ||
                                    new Regex(search, RegexOptions.IgnoreCase).IsMatch(nd.Name) && !nd.IsMastery)
                                .ToList();
                    _nodeHighlighter.ReplaceHighlights(nodes, flag);
                    DrawHighlights(_nodeHighlighter);
                }
                catch (Exception)
                {
                    // ?
                }
            }
            else
            {
                List<SkillNode> nodes =
                    Skillnodes.Values.Where(
                        nd =>
                            nd.attributes.Count(att => att.ToLower().Contains(search.ToLower())) != 0 ||
                            nd.Name.ToLower().Contains(search.ToLower()) && !nd.IsMastery).ToList();
                _nodeHighlighter.ReplaceHighlights(nodes, flag);
                DrawHighlights(_nodeHighlighter);
            }
        }

        public void UnhighlightAllNodes()
        {
            _nodeHighlighter.UnhighlightAllNodes(HighlightState.Highlights);
        }

        public void UntagAllNodes()
        {
            _nodeHighlighter.UnhighlightAllNodes(HighlightState.Tags);
            DrawHighlights(_nodeHighlighter);
        }

        public void CheckAllHighlightedNodes()
        {
            _nodeHighlighter.HighlightNodesIf(HighlightState.Checked, HighlightState.Highlights);
            DrawHighlights(_nodeHighlighter);
        }

        public void CrossAllHighlightedNodes()
        {
            _nodeHighlighter.HighlightNodesIf(HighlightState.Crossed, HighlightState.Highlights);
            DrawHighlights(_nodeHighlighter);
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

            return retval;
        }


        public static void DecodeURL(string url, out HashSet<ushort> skillednodes, out int chartype)
        {
            skillednodes = new HashSet<ushort>();
            url = Regex.Replace(url, @"\t| |\n|\r", "");
            string s =
                url.Substring(TreeAddress.Length + (url.StartsWith("https") ? 1 : 0))
                    .Replace("-", "+")
                    .Replace("_", "/");
            byte[] decbuff = Convert.FromBase64String(s);
            int i = BitConverter.ToInt32(new[] { decbuff[3], decbuff[2], decbuff[1], decbuff[1] }, 0);
            byte b = decbuff[4];
            long j = 0L;
            if (i > 0)
                j = decbuff[5];
            var nodes = new List<UInt16>();
            for (int k = 6; k < decbuff.Length; k += 2)
            {
                byte[] dbff = { decbuff[k + 1], decbuff[k + 0] };
                if (Skillnodes.Keys.Contains(BitConverter.ToUInt16(dbff, 0)))
                    nodes.Add((BitConverter.ToUInt16(dbff, 0)));
            }
            chartype = b;


            SkillNode startnode = Skillnodes.First(nd => nd.Value.Name.ToUpper() == CharName[b].ToUpper()).Value;
            skillednodes.Add(startnode.Id);
            foreach (ushort node in nodes)
            {
                skillednodes.Add(node);
            }

        }


        public void LoadFromURL(string url)
        {
            int b;
            HashSet<ushort> snodes;
            SkillTree.DecodeURL(url, out snodes, out b);
            Chartype = b;
            SkilledNodes = snodes;
            
            UpdateAvailNodes();
        }

        public void Reset()
        {
            SkilledNodes.Clear();
            KeyValuePair<ushort, SkillNode> node = Skillnodes.First(nd => nd.Value.Name.ToUpper() == CharName[_chartype]);
            SkilledNodes.Add(node.Value.Id);
            UpdateAvailNodes();
        }

        public string SaveToURL()
        {
            var b = new byte[(SkilledNodes.Count - 1) * 2 + 6];
            byte[] b2 = BitConverter.GetBytes(3); //skilltree version
            b[0] = b2[3];
            b[1] = b2[2];
            b[2] = b2[1];
            b[3] = b2[0]; 
            b[4] = (byte)(Chartype);
            b[5] = 0;
            int pos = 6;
            foreach (ushort inn in SkilledNodes)
            {
                if (CharName.Contains(Skillnodes[inn].Name.ToUpper()))
                    continue;
                byte[] dbff = BitConverter.GetBytes((Int16)inn);
                b[pos++] = dbff[1];
                b[pos++] = dbff[0];
            }
            return TreeAddress + Convert.ToBase64String(b).Replace("/", "_").Replace("+", "-");
        }

        public void SkillAllTaggedNodes()
        {
            if (_nodeHighlighter == null)
                return;
            var nodes = new HashSet<ushort>();
            var toOmit = new HashSet<ushort>();
            foreach (var entry in _nodeHighlighter.nodeHighlights)
            {
                if (!(rootNodeList.Contains(entry.Key.Id) || SkilledNodes.Contains(entry.Key.Id)))
                {
                    // Crossed has precedence.
                    if (entry.Value.HasFlag(HighlightState.Crossed))
                    {
                        toOmit.Add(entry.Key.Id);
                    }
                    else if (entry.Value.HasFlag(HighlightState.Checked))
                    {
                        nodes.Add(entry.Key.Id);
                    }
                }
            }
            SkillNodeList(nodes, toOmit);
        }

        private void SkillNodeList(HashSet<ushort> targetNodeIds, HashSet<ushort> omitNodeIds)
        {
            if (targetNodeIds.Count == 0)
            {
                Popup.Info(L10n.Message("Please tag non-skilled nodes by right-clicking them."));
                return;
            }

            /// These are used for visualization of the simulation progress, so
            /// they're saved for restoring them afterwards.
            var savedHighlights = HighlightedNodes;

            OptimizerControllerWindow optimizerDialog = new OptimizerControllerWindow(this, targetNodeIds, omitNodeIds);
            optimizerDialog.Owner = MainWindow;
            optimizerDialog.ShowDialog();
            if (optimizerDialog.DialogResult == true)
                foreach (ushort node in optimizerDialog.bestSoFar)
                    SkilledNodes.Add(node);

            HighlightedNodes = savedHighlights;
            DrawNodeBaseSurroundHighlight();

            this.DrawHighlights(_nodeHighlighter);

            UpdateAvailNodes();
        }

        public void UpdateAvailNodes(bool draw = true)
        {
            AvailNodes = GetAvailableNodes(SkilledNodes);
            
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
                    if (!CharName.Contains(skillNode.Name) && !SkilledNodes.Contains(skillNode.Id))
                        availNodes.Add(skillNode.Id);
                }
            }
            return availNodes;
        }

        private void UpdateAvailNodesDraw()
        {
            var pen2 = new Pen(Brushes.DarkKhaki, 15f);

            using (DrawingContext dc = picActiveLinks.RenderOpen())
            {
                foreach (ushort n1 in SkilledNodes)
                {
                    foreach (SkillNode n2 in Skillnodes[n1].Neighbor)
                    {
                        if (SkilledNodes.Contains(n2.Id))
                        {
                            DrawConnection(dc, pen2, n2, Skillnodes[n1]);
                        }
                    }
                }
            }
            DrawActiveNodeIcons();
            DrawNodeSurround();
        }

        private static Dictionary<string, List<float>> ExpandHybridAttributes(Dictionary<string, List<float>> attributes)
        {
            foreach (var attribute in attributes.ToList())
            {
                List<string> expandedAttributes;
                if (_hybridAttributes.TryGetValue(attribute.Key, out expandedAttributes))
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

            if (className.ToUpper() == "SHADOW")
            {
                className = "SIX";
            }
            if (className.ToUpper() == "SCION")
            {
                className = "SEVEN";
            }
            _rootNodeClassDictionary.TryGetValue(className.ToUpper(), out rootNodeValue);
            var classSpecificStartNodes = _startNodeDictionary.Where(kvp => kvp.Value == rootNodeValue).Select(kvp => kvp.Key);

            foreach (int node in classSpecificStartNodes)
            {
                temp = GetShortestPathTo((ushort)node, SkilledNodes);

                if (!temp.Any())
                    return true;
            }
            return false;
        }
    }
}
