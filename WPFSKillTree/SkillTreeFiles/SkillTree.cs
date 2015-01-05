using Newtonsoft.Json;
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

namespace POESKillTree.SkillTreeFiles
{
    public partial class SkillTree
    {
        public delegate void UpdateLoadingWindow(double current, double max);

        public delegate void CloseLoadingWindow();

        public delegate void StartLoadingWindow();

        public static float LifePerLevel = 12;
        public static float AccPerLevel = 2;
        public static float EvasPerLevel = 3;
        public static float ManaPerLevel = 4;
        public static float IntPerMana = 2;
        public static float IntPerES = 5; //%
        public static float StrPerLife = 2;
        public static float StrPerED = 5; //%
        public static float DexPerAcc = 0.5f;
        public static float DexPerEvas = 5; //%
        private static Action _emptyDelegate = delegate { };
        private readonly Dictionary<string, Asset> _assets = new Dictionary<string, Asset>();
        public List<string> AttributeTypes = new List<string>();
        public List<int> rootNodeList = new List<int>();
        public Dictionary<string, int> rootNodeClassDictionary = new Dictionary<string, int>();
        public Dictionary<int, int> startNodeDictionary = new Dictionary<int, int>();
        public HashSet<ushort> AvailNodes = new HashSet<ushort>();

        public Dictionary<string, float> BaseAttributes = new Dictionary<string, float>
        {
            {"+# to maximum Mana", 36},
            {"+# to maximum Life", 38},
            {"Evasion Rating: #", 53},
            {"+# Maximum Endurance Charge", 3},
            {"+# Maximum Frenzy Charge", 3},
            {"+# Maximum Power Charge", 3},
            {"#% Additional Elemental Resistance per Endurance Charge", 4},
            {"#% Physical Damage Reduction per Endurance Charge", 4},
            {"#% Attack Speed Increase per Frenzy Charge", 5},
            {"#% Cast Speed Increase per Frenzy Charge", 5},
            {"#% Critical Strike Chance Increase per Power Charge", 50},
        };

        private readonly Dictionary<string, List<string>> _hybridAttributes = new Dictionary<string, List<string>>
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

        public Dictionary<string, float>[] CharBaseAttributes = new Dictionary<string, float>[7];

        public List<string> CharName = new List<string>
        {
            "SEVEN",
            "MARAUDER",
            "RANGER",
            "WITCH",
            "DUELIST",
            "TEMPLAR",
            "SIX"
        };

        public List<string> FaceNames = new List<string>
        {
            "centerscion",
            "centermarauder",
            "centerranger",
            "centerwitch",
            "centerduelist",
            "centertemplar",
            "centershadow"
        };

        public HashSet<int[]> Links = new HashSet<int[]>();
        public List<SkillNodeGroup> NodeGroups = new List<SkillNodeGroup>();
        public HashSet<ushort> SkilledNodes = new HashSet<ushort>();

        public HashSet<ushort> HighlightedNodes = new HashSet<ushort>();

        public Dictionary<UInt16, SkillNode> Skillnodes = new Dictionary<UInt16, SkillNode>();

        public Rect2D TRect = new Rect2D();
        private const string TreeAddress = "http://www.pathofexile.com/passive-skill-tree/";
        private int _chartype;
        public SkillIcons IconActiveSkills = new SkillIcons();
        public SkillIcons IconInActiveSkills = new SkillIcons();
        public int _level = 1;

        public Dictionary<string, string> NodeBackgrounds = new Dictionary<string, string>
        {
            {"normal", "PSSkillFrame"},
            {"notable", "NotableFrameUnallocated"},
            {"keystone", "KeystoneFrameUnallocated"}
        };

        public Dictionary<string, string> NodeBackgroundsActive = new Dictionary<string, string>
        {
            {"normal", "PSSkillFrameActive"},
            {"notable", "NotableFrameAllocated"},
            {"keystone", "KeystoneFrameAllocated"}
        };

        public float ScaleFactor = 1;

        public SkillTree(String treestring, bool displayProgress, UpdateLoadingWindow update)
        {
            var jss = new JsonSerializerSettings
            {
                Error = delegate(object sender, ErrorEventArgs args)
                {
                    Debug.WriteLine(args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                }
            };

            var inTree = JsonConvert.DeserializeObject<PoESkillTree>(treestring.Replace("Additional ", ""), jss);
            int qindex = 0;

            //TODO: (SpaceOgre) This is not used atm, so no need to run it.
            foreach (var obj in inTree.skillSprites)
            {
                if (obj.Key.Contains("inactive"))
                    continue;
                IconInActiveSkills.Images[obj.Value[3].filename] = null;
                foreach (var o in obj.Value[3].coords)
                {
                    IconInActiveSkills.SkillPositions[o.Key + "_" + o.Value.w] =
                        new KeyValuePair<Rect, string>(new Rect(o.Value.x, o.Value.y, o.Value.w, o.Value.h),
                            obj.Value[3].filename);
                }
            }
            foreach (var obj in inTree.skillSprites)
            {
                if (obj.Key.Contains("active"))
                    continue;
                IconActiveSkills.Images[obj.Value[3].filename] = null;
                foreach (var o in obj.Value[3].coords)
                {
                    IconActiveSkills.SkillPositions[o.Key + "_" + o.Value.w] =
                        new KeyValuePair<Rect, string>(new Rect(o.Value.x, o.Value.y, o.Value.w, o.Value.h),
                            obj.Value[3].filename);
                }
            }

            foreach (var ass in inTree.assets)
            {
                _assets[ass.Key] = new Asset(ass.Key,
                    ass.Value.ContainsKey(0.3835f) ? ass.Value[0.3835f] : ass.Value.Values.First());
            }
            if (inTree.root != null)
            {
                foreach (int i in inTree.root.ot)
                {
                    rootNodeList.Add(i);
                }
            }
            else if (inTree.main != null)
            {
                foreach (int i in inTree.main.ot)
                {
                    rootNodeList.Add(i);
                }
            }

            if (displayProgress)
                update(50, 100);
            IconActiveSkills.OpenOrDownloadImages(update);
            if (displayProgress)
                update(75, 100);
            IconInActiveSkills.OpenOrDownloadImages(update);
            foreach (var c in inTree.characterData)
            {
                CharBaseAttributes[c.Key] = new Dictionary<string, float>
                {
                    {"+# to Strength", c.Value.base_str},
                    {"+# to Dexterity", c.Value.base_dex},
                    {"+# to Intelligence", c.Value.base_int}
                };
            }
            foreach (Node nd in inTree.nodes)
            {
                Skillnodes.Add(nd.id, new SkillNode
                {
                    Id = nd.id,
                    Name = nd.dn,
                    attributes = nd.sd,
                    Orbit = nd.o,
                    OrbitIndex = nd.oidx,
                    Icon = nd.icon,
                    LinkId = nd.ot,
                    G = nd.g,
                    Da = nd.da,
                    Ia = nd.ia,
                    IsKeyStone = nd.ks,
                    IsNotable = nd.not,
                    Sa = nd.sa,
                    IsMastery = nd.m,
                    Spc = nd.spc.Count() > 0 ? (int?) nd.spc[0] : null
                });
                if (rootNodeList.Contains(nd.id))
                {
                    rootNodeClassDictionary.Add(nd.dn.ToString().ToUpper(), nd.id);
                    foreach (int linkedNode in nd.ot)
                    {
                        startNodeDictionary.Add(linkedNode, nd.id);
                    }
                }
                foreach (int node in nd.ot)
                {
                    if (!startNodeDictionary.ContainsKey(nd.id) && rootNodeList.Contains(node))
                    {
                        startNodeDictionary.Add(nd.id, node);
                    }
                }

            }
            var links = new List<ushort[]>();
            foreach (var skillNode in Skillnodes)
            {
                foreach (ushort i in skillNode.Value.LinkId)
                {
                    if (
                        links.Count(nd => (nd[0] == i && nd[1] == skillNode.Key) || nd[0] == skillNode.Key && nd[1] == i) ==
                        1)
                    {
                        continue;
                    }
                    links.Add(new[] {skillNode.Key, i});
                }
            }
            foreach (var ints in links)
            {
                if (!Skillnodes[ints[0]].Neighbor.Contains(Skillnodes[ints[1]]))
                    Skillnodes[ints[0]].Neighbor.Add(Skillnodes[ints[1]]);
                if (!Skillnodes[ints[1]].Neighbor.Contains(Skillnodes[ints[0]]))
                    Skillnodes[ints[1]].Neighbor.Add(Skillnodes[ints[0]]);
            }

            foreach (var gp in inTree.groups)
            {
                var ng = new SkillNodeGroup();

                ng.OcpOrb = gp.Value.oo;
                ng.Position = new Vector2D(gp.Value.x, gp.Value.y);
                ng.Nodes = gp.Value.n;
                NodeGroups.Add(ng);
            }

            foreach (SkillNodeGroup group in NodeGroups)
            {
                foreach (ushort node in group.Nodes)
                {
                    Skillnodes[node].SkillNodeGroup = group;
                }
            }
            TRect = new Rect2D(new Vector2D(inTree.min_x*1.1, inTree.min_y*1.1),
                new Vector2D(inTree.max_x*1.1, inTree.max_y*1.1));


            InitNodeSurround();
            DrawNodeSurround();
            DrawNodeBaseSurround();
            InitSkillIconLayers();
            DrawSkillIconLayer();
            DrawBackgroundLayer();
            InitFaceBrushesAndLayer();
            DrawLinkBackgroundLayer(links);
            InitOtherDynamicLayers();
            CreateCombineVisual();


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
            if (displayProgress)
                update(100, 100);
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
                Dictionary<string, List<float>> temp = SelectedAttributesWithoutImplicit;

                foreach (var a in ImplicitAttributes(temp))
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
        }

        public Dictionary<string, List<float>> SelectedAttributesWithoutImplicit
        {
            get
            {
                var temp = new Dictionary<string, List<float>>();

                foreach (var attr in CharBaseAttributes[Chartype])
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

                foreach (ushort inode in SkilledNodes)
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
        }

        public static SkillTree CreateSkillTree(StartLoadingWindow start = null, UpdateLoadingWindow update = null,
            CloseLoadingWindow finish = null)
        {
            string skilltreeobj = "";
            if (Directory.Exists("Data"))
            {
                if (File.Exists("Data\\Skilltree.txt"))
                {
                    skilltreeobj = File.ReadAllText("Data\\Skilltree.txt");
                }
                if (!File.Exists("Data\\Assets"))
                {
                    Directory.CreateDirectory("Data\\Assets");
                }
            }
            else
            {
                Directory.CreateDirectory("Data");
                Directory.CreateDirectory("Data\\Assets");
            }

            bool displayProgress = false;
            if (skilltreeobj == "")
            {
                displayProgress = (start != null && update != null && finish != null);
                if (displayProgress)
                    start();
                string uriString = "http://www.pathofexile.com/passive-skill-tree/";
                var req = (HttpWebRequest) WebRequest.Create(uriString);
                var resp = (HttpWebResponse) req.GetResponse();
                string code = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                var regex = new Regex("var passiveSkillTreeData.*");
                skilltreeobj = regex.Match(code).Value.Replace("root", "main").Replace("\\/", "/");
                skilltreeobj = skilltreeobj.Substring(27, skilltreeobj.Length - 27 - 2) + "";
                File.WriteAllText("Data\\Skilltree.txt", skilltreeobj);
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

        public List<ushort> GetShortestPathTo(ushort targetNode)
        {
            if (SkilledNodes.Contains(targetNode))
                return new List<ushort>();
            if (AvailNodes.Contains(targetNode))
                return new List<ushort> {targetNode};

            var visited = new HashSet<ushort>(SkilledNodes);
            var distance = new Dictionary<int, int>();
            var parent = new Dictionary<ushort, ushort>();
            var newOnes = new Queue<ushort>();
            foreach (var node in AvailNodes)
            {
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
            var result = new List<ushort> {curr};
            while (parent.ContainsKey(curr))
            {
                result.Add(parent[curr]);
                curr = parent[curr];
            }
            result.Reverse();
            return result;
        }

        public void ToggleNodeHighlight(SkillNode node)
        {
            _nodeHighlighter.ToggleHighlightNode(node, HighlightState.FromNode);
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
                    Debugger.Break();
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

        public Dictionary<string, List<float>> ImplicitAttributes(Dictionary<string, List<float>> attribs)
        {
            var retval = new Dictionary<string, List<float>>();
            // +# to Strength", co["base_str"].Value<int>() }, { "+# to Dexterity", co["base_dex"].Value<int>() }, { "+# to Intelligence", co["base_int"].Value<int>() } };
            retval["+# to maximum Mana"] = new List<float>
            {
                attribs["+# to Intelligence"][0]/IntPerMana + _level*ManaPerLevel
            };
            retval["#% increased maximum Energy Shield"] = new List<float>
            {
                (float) Math.Round(attribs["+# to Intelligence"][0]/IntPerES, 0)
            };

            retval["+# to maximum Life"] = new List<float>
            {
                attribs["+# to Strength"][0]/StrPerLife + _level*LifePerLevel
            };
            // Every 10 strength grants 2% increased melee physical damage. 
            var str = (int) attribs["+# to Strength"][0];
            if (str%(int) StrPerED > 0) str += (int) StrPerED - (str%(int) StrPerED);
            retval["#% increased Melee Physical Damage"] = new List<float> {str/StrPerED};
            // Every point of Dexterity gives 2 additional base accuracy, and characters gain 2 base accuracy when leveling up.
            // @see http://pathofexile.gamepedia.com/Accuracy
            retval["+# Accuracy Rating"] = new List<float>
            {
                attribs["+# to Dexterity"][0]/DexPerAcc + (_level - 1)*AccPerLevel
            };
            retval["Evasion Rating: #"] = new List<float> {_level*EvasPerLevel};

            // Dexterity value is not getting rounded up any more but rounded normally to the nearest multiple of 5.
            // @see http://pathofexile.gamepedia.com/Talk:Evasion
            float dex = attribs["+# to Dexterity"][0];
            dex = (float) Math.Round(dex/DexPerEvas, 0, MidpointRounding.AwayFromZero)*DexPerEvas;
            retval["#% increased Evasion Rating"] = new List<float> {dex/DexPerEvas};

            return retval;
        }

        public void LoadFromURL(string url)
        {
            url = Regex.Replace(url, @"\t| |\n|\r", "");
            string s =
                url.Substring(TreeAddress.Length + (url.StartsWith("https") ? 1 : 0))
                    .Replace("-", "+")
                    .Replace("_", "/");
            byte[] decbuff = Convert.FromBase64String(s);
            int i = BitConverter.ToInt32(new[] {decbuff[3], decbuff[2], decbuff[1], decbuff[1]}, 0);
            byte b = decbuff[4];
            long j = 0L;
            if (i > 0)
                j = decbuff[5];
            var nodes = new List<UInt16>();
            for (int k = 6; k < decbuff.Length; k += 2)
            {
                byte[] dbff = {decbuff[k + 1], decbuff[k + 0]};
                if (Skillnodes.Keys.Contains(BitConverter.ToUInt16(dbff, 0)))
                    nodes.Add((BitConverter.ToUInt16(dbff, 0)));
            }
            Chartype = b;
            SkilledNodes.Clear();
            SkillNode startnode = Skillnodes.First(nd => nd.Value.Name.ToUpper() == CharName[Chartype].ToUpper()).Value;
            SkilledNodes.Add(startnode.Id);
            foreach (ushort node in nodes)
            {
                SkilledNodes.Add(node);
            }
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
            var b = new byte[(SkilledNodes.Count - 1)*2 + 6];
            byte[] b2 = BitConverter.GetBytes(2);
            b[0] = b2[3];
            b[1] = b2[2];
            b[2] = b2[1];
            b[3] = b2[0];
            b[4] = (byte) (Chartype);
            b[5] = 0;
            int pos = 6;
            foreach (ushort inn in SkilledNodes)
            {
                if (CharName.Contains(Skillnodes[inn].Name.ToUpper()))
                    continue;
                byte[] dbff = BitConverter.GetBytes((Int16) inn);
                b[pos++] = dbff[1];
                b[pos++] = dbff[0];
            }
            return TreeAddress + Convert.ToBase64String(b).Replace("/", "_").Replace("+", "-");
        }

        public void SkillAllHighligtedNodes()
        {
            if (_nodeHighlighter == null)
                return;
            var nodes = new HashSet<int>();
            foreach (SkillNode nd in _nodeHighlighter.nodeHighlights.Keys)
            {
                if(!rootNodeList.Contains(nd.Id))
                    nodes.Add(nd.Id);
            }
            SkillNodeList(nodes);
        }

        private void SkillNodeList(HashSet<int> hs)
        {
            while (hs.Count != 0)
            {
                var currentShortestPath = new List<ushort>();
                var removeList = new List<ushort>();
                foreach (ushort id in hs)
                {
                    var shortestPathTemp = GetShortestPathTo(id);
                    if (shortestPathTemp.Count <= 0)
                    {
                        removeList.Add(id);
                    }
                    else if (shortestPathTemp.Count == 1)
                    {
                        currentShortestPath = shortestPathTemp;
                        break;
                    }
                    else if (currentShortestPath.Count == 0 || shortestPathTemp.Count < currentShortestPath.Count)
                    {
                        currentShortestPath = shortestPathTemp;
                    }
                }
                removeList.ForEach(x => hs.Remove(x));
                foreach (ushort i in currentShortestPath)
                {
                    hs.Remove(i);
                    SkilledNodes.Add(i);
                }
                UpdateAvailNodesList();
            }
            UpdateAvailNodesDraw();
        }

        public void UpdateAvailNodes(bool draw = true)
        {
            UpdateAvailNodesList();
            if(draw)
                UpdateAvailNodesDraw();
        }

        private void UpdateAvailNodesList()
        {
            AvailNodes.Clear();
            foreach (ushort inode in SkilledNodes)
            {
                SkillNode node = Skillnodes[inode];
                foreach (SkillNode skillNode in node.Neighbor)
                {
                    if (!CharName.Contains(skillNode.Name) && !SkilledNodes.Contains(skillNode.Id))
                        AvailNodes.Add(skillNode.Id);
                }
            }
        }

        private void UpdateAvailNodesDraw()
        {
            var pen2 = new Pen(Brushes.Yellow, 15f);

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

        private Dictionary<string, List<float>> ExpandHybridAttributes(Dictionary<string, List<float>> attributes)
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

            if(className.ToUpper() == "SHADOW")
            {
                className = "SIX";
            }
            if(className.ToUpper() == "SCION")
            {
                className = "SEVEN";
            }
            rootNodeClassDictionary.TryGetValue(className.ToUpper(), out rootNodeValue);
            var classSpecificStartNodes = startNodeDictionary.Where(kvp => kvp.Value == rootNodeValue).Select(kvp => kvp.Key);

            foreach (int node in classSpecificStartNodes)
            {
                temp = GetShortestPathTo((ushort) node);
                
                if (!temp.Any())
                    return true;
            }
            return false;
        }
    }
}