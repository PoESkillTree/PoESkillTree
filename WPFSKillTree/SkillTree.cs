using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;
using Newtonsoft.Json;

namespace POESKillTree
{
    public partial class SkillTree
    {
        public delegate void startLoadingWindow();
        public delegate void closeLoadingWindow();
        public delegate void UpdateLoadingWindow(double current, double max);


        string TreeAddress = "http://www.pathofexile.com/passive-skill-tree/";
        public List<NodeGroup> NodeGroups = new List<NodeGroup>();
        public Dictionary<UInt16, SkillNode> Skillnodes = new Dictionary<UInt16, SkillNode>();
        public List<string> AttributeTypes = new List<string>();
        // public Bitmap iconActiveSkills;
        public SkillIcons iconInActiveSkills = new SkillIcons();
        public SkillIcons iconActiveSkills = new SkillIcons();
        public Dictionary<string, string> nodeBackgrounds = new Dictionary<string, string>() { { "normal", "PSSkillFrame" }, { "notable", "NotableFrameUnallocated" }, { "keystone", "KeystoneFrameUnallocated" } };
        public Dictionary<string, string> nodeBackgroundsActive = new Dictionary<string, string>() { { "normal", "PSSkillFrameActive" }, { "notable", "NotableFrameAllocated" }, { "keystone", "KeystoneFrameAllocated" } };
        public List<string> FaceNames = new List<string>() {"centerscion", "centermarauder", "centerranger", "centerwitch", "centerduelist", "centertemplar", "centershadow"  };
        public List<string> CharName = new List<string>() { "SEVEN","MARAUDER", "RANGER", "WITCH", "DUELIST", "TEMPLAR", "SIX" };
        public Dictionary<string, float>[] CharBaseAttributes = new Dictionary<string, float>[7];
        public Dictionary<string, float> BaseAttributes = new Dictionary<string, float>()
                                                              {
                                                                  {"+# to maximum Mana",36},
                                                                  {"+# to maximum Life",44},
                                                                  {"Evasion Rating: #",50},
                                                                  {"+# Maximum Endurance Charge",3},
                                                                  {"+# Maximum Frenzy Charge",3},
                                                                  {"+# Maximum Power Charge",3},
                                                                  {"#% Additional Elemental Resistance per Endurance Charge",4},
                                                                  {"#% Physical Damage Reduction per Endurance Charge",4},
                                                                  {"#% Attack Speed Increase per Frenzy Charge",5},
                                                                  {"#% Cast Speed Increase per Frenzy Charge",5},
                                                                  {"#% Critical Strike Chance Increase per Power Charge",50},
                                                              };
        public static float LifePerLevel = 12;
        public static float EvasPerLevel = 3;
        public static float ManaPerLevel = 4;
        public static float IntPerMana = 2;
        public static float IntPerES = 5; //%
        public static float StrPerLife = 2;
        public static float StrPerED = 5; //%
        public static float DexPerAcc = 0.5f;
        public static float DexPerEvas = 5; //%
        private List<SkillTree.SkillNode> highlightnodes;
        private int level = 1;
        private int chartype = 0;
        public HashSet<ushort> SkilledNodes = new HashSet<ushort>();
        public HashSet<ushort> AvailNodes = new HashSet<ushort>();
        Dictionary<string, Asset> assets = new Dictionary<string, Asset>();
        public Rect2D TRect = new Rect2D();
        public float scaleFactor = 1;
        public HashSet<int[]> Links = new HashSet<int[]>();
        public void Reset()
        {
            SkilledNodes.Clear();
            var node = Skillnodes.First(nd => nd.Value.name.ToUpper() == CharName[chartype]);
            SkilledNodes.Add(node.Value.id);
            UpdateAvailNodes();
        }
        static Action emptyDelegate = delegate
        {
        };
        public static SkillTree CreateSkillTree(startLoadingWindow start = null, UpdateLoadingWindow update = null, closeLoadingWindow finish = null)
        {

            string skilltreeobj = "";
            if (Directory.Exists("Data"))
            {
                if (File.Exists("Data\\Skilltree.txt"))
                {
                    skilltreeobj = File.ReadAllText("Data\\Skilltree.txt");
                }
            }
            else
            {
                Directory.CreateDirectory("Data");
                Directory.CreateDirectory("Data\\Assets");
            }

            if (skilltreeobj == "")
            {
                bool displayProgress = (start != null && update != null && finish != null);
                if (displayProgress)
                    start();
                //loadingWindow.Dispatcher.Invoke(DispatcherPriority.Background,new Action(delegate { }));


                string uriString = "http://www.pathofexile.com/passive-skill-tree/";
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(uriString);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                string code = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                Regex regex = new Regex("var passiveSkillTreeData.*");
                skilltreeobj = regex.Match(code).Value.Replace("root", "main").Replace("\\/", "/");
                skilltreeobj = skilltreeobj.Substring(27, skilltreeobj.Length - 27 - 2) + "";
                File.WriteAllText("Data\\Skilltree.txt", skilltreeobj);
                if (displayProgress)
                    finish();
            }

            return new SkillTree(skilltreeobj, start, update, finish);
        }
        public SkillTree(String treestring, startLoadingWindow start = null, UpdateLoadingWindow update = null, closeLoadingWindow finish = null)
        {
            bool displayProgress = ( start != null && update != null && finish != null );
           // RavenJObject jObject = RavenJObject.Parse( treestring.Replace( "Additional " , "" ) );
          JsonSerializerSettings jss = new JsonSerializerSettings
            {
            Error = delegate(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
            {
            Debug.WriteLine(args.ErrorContext.Error.Message);
            args.ErrorContext.Handled = true;
            }
            };

          var inTree = JsonConvert.DeserializeObject<PoEClasses.PoESkillTree>(treestring.Replace("Additional ", ""), jss);
            int qindex = 0;


            foreach (var obj in inTree.skillSprites)
            {
                if (obj.Key.Contains("inactive"))
                    continue;
                iconActiveSkills.Images[obj.Value[3].filename] = null;
                foreach (var o in obj.Value[3].coords)
                {
                    iconActiveSkills.SkillPositions[o.Key] = new KeyValuePair<Rect, string>(new Rect(o.Value.x, o.Value.y, o.Value.w, o.Value.h), obj.Value[3].filename);
                }
            }
            foreach (var obj in inTree.skillSprites)
            {
                if (obj.Key.Contains("active"))
                    continue;
                iconActiveSkills.Images[obj.Value[3].filename] = null;
                foreach (var o in obj.Value[3].coords)
                {
                    iconActiveSkills.SkillPositions[o.Key] = new KeyValuePair<Rect, string>(new Rect(o.Value.x, o.Value.y, o.Value.w, o.Value.h), obj.Value[3].filename);
                }
            }   
            qindex = 0;    

            foreach(var ass in inTree.assets)
            {
               
                assets[ass.Key] = new Asset(ass.Key,ass.Value.ContainsKey(0.3835f)?ass.Value[0.3835f]:ass.Value.Values.First());
                                     
            }
           
            if ( displayProgress )
                start( );
            iconActiveSkills.OpenOrDownloadImages(update );
            iconInActiveSkills.OpenOrDownloadImages(update );
            if ( displayProgress )
                finish( );
            foreach( var c in inTree.characterData)
            {
                CharBaseAttributes[c.Key] = new Dictionary<string, float>() { { "+# to Strength", c.Value.base_str }, { "+# to Dexterity", c.Value.base_dex }, { "+# to Intelligence", c.Value.base_int } };
            }
           foreach (var nd in inTree.nodes)
           {
               Skillnodes.Add(nd.id, new SkillTree.SkillNode()
               {
                   id = nd.id,                
                   name = nd.dn,
                   attributes = nd.sd,
                   orbit = nd.o,
                   orbitIndex =nd.oidx,
                   icon = nd.icon,
                   linkID =nd.ot,
                   g = nd.g,
                   da = nd.da,
                   ia = nd.ia,
                   ks = nd.ks,
                   not = nd.not,
                   sa = nd.sa,
                   Mastery = nd.m,
                   spc=nd.spc.Count()>0?(int?)nd.spc[0]:null
               });
           }         
            List<ushort[]> links = new List<ushort[]>( );
            foreach ( var skillNode in Skillnodes )
            {
                foreach ( ushort i in skillNode.Value.linkID )
                {
                    if (
                        links.Count(
                            nd => ( nd[ 0 ] == i && nd[ 1 ] == skillNode.Key ) || nd[ 0 ] == skillNode.Key && nd[ 1 ] == i ) ==
                        1 )
                    {
                        continue;
                    }
                    links.Add( new ushort[] { skillNode.Key , i } );
                }
            }
            foreach ( ushort[] ints in links )
            {
                if ( !Skillnodes[ ints[ 0 ] ].Neighbor.Contains( Skillnodes[ ints[ 1 ] ] ) )
                    Skillnodes[ ints[ 0 ] ].Neighbor.Add( Skillnodes[ ints[ 1 ] ] );
                if ( !Skillnodes[ ints[ 1 ] ].Neighbor.Contains( Skillnodes[ ints[ 0 ] ] ) )
                    Skillnodes[ ints[ 1 ] ].Neighbor.Add( Skillnodes[ ints[ 0 ] ] );
            }
           
            foreach(var gp in inTree.groups )
            {
                NodeGroup ng = new NodeGroup();

                ng.OcpOrb = gp.Value.oo;
                ng.Position = new Vector2D(gp.Value.x, gp.Value.y);
                ng.Nodes = gp.Value.n;
                NodeGroups.Add(ng);
            }
          
            foreach ( SkillTree.NodeGroup group in NodeGroups )
            {
                foreach ( ushort node in group.Nodes )
                {
                    Skillnodes[ node ].NodeGroup = group;
                }
            }

            TRect = new Rect2D( new Vector2D( inTree.min_x * 1.1 , inTree.min_y * 1.1 ) ,
                               new Vector2D(inTree.max_x * 1.1, inTree.max_y * 1.1));




            InitNodeSurround( );
            DrawNodeSurround( );
            DrawNodeBaseSurround( );
            DrawSkillIconLayer( );
            DrawBackgroundLayer( );
            InitFaceBrushesAndLayer( );
            DrawLinkBackgroundLayer( links );
            InitOtherDynamicLayers( );
            CreateCombineVisual( );


            Regex regexAttrib = new Regex( "[0-9]*\\.?[0-9]+" );
            foreach ( var skillNode in Skillnodes )
            {
                skillNode.Value.Attributes = new Dictionary<string , List<float>>( );
                foreach ( string s in skillNode.Value.attributes )
                {

                    List<float> values = new List<float>( );

                    foreach ( Match m in regexAttrib.Matches( s ) )
                    {

                        if ( !AttributeTypes.Contains( regexAttrib.Replace( s , "#" ) ) )
                            AttributeTypes.Add( regexAttrib.Replace( s , "#" ) );
                        if ( m.Value == "" )
                            values.Add( float.NaN );
                        else
                            values.Add( float.Parse( m.Value , System.Globalization.CultureInfo.InvariantCulture ) );

                    }
                    string cs = ( regexAttrib.Replace( s , "#" ) );

                    skillNode.Value.Attributes[ cs ] = values;



                }
            }


        }
        public Dictionary<string, List<float>> ImplicitAttributes(Dictionary<string, List<float>> attribs)
        {
            Dictionary<string, List<float>> retval = new Dictionary<string, List<float>>();
            // +# to Strength", co["base_str"].Value<int>() }, { "+# to Dexterity", co["base_dex"].Value<int>() }, { "+# to Intelligence", co["base_int"].Value<int>() } };
            retval["+# to maximum Mana"] = new List<float>() { attribs["+# to Intelligence"][0] / IntPerMana + level * ManaPerLevel };
            retval["+#% Energy Shield"] = new List<float>() { attribs["+# to Intelligence"][0] / IntPerES };

            retval["+# to maximum Life"] = new List<float>() { attribs["+# to Strength"][0] / IntPerMana + level * LifePerLevel };
            retval["+#% increased Melee Physical Damage"] = new List<float>() { attribs["+# to Strength"][0] / StrPerED };

            retval["+# Accuracy Rating"] = new List<float>() { attribs["+# to Dexterity"][0] / DexPerAcc };
            retval["Evasion Rating: #"] = new List<float>() { level * EvasPerLevel };
            retval["#% increased Evasion Rating"] = new List<float>() { attribs["+# to Dexterity"][0] / DexPerEvas };
            return retval;
        }
        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                level = value;
            }
        }
        public int Chartype
        {
            get
            {
                return chartype;
            }
            set
            {

                chartype = value;
                SkilledNodes.Clear();
                var node = Skillnodes.First(nd => nd.Value.name.ToUpper() == CharName[chartype]);
                SkilledNodes.Add(node.Value.id);
                UpdateAvailNodes();
                DrawFaces();
            }
        }
        public List<ushort> GetShortestPathTo(ushort targetNode)
        {
            if (SkilledNodes.Contains(targetNode))
                return new List<ushort>();
            if (AvailNodes.Contains(targetNode))
                return new List<ushort>() { targetNode };
            HashSet<ushort> visited = new HashSet<ushort>(SkilledNodes);
            Dictionary<int, int> distance = new Dictionary<int, int>();
            Dictionary<ushort, ushort> parent = new Dictionary<ushort, ushort>();
            Queue<ushort> newOnes = new Queue<ushort>();
            foreach (var node in SkilledNodes)
            {
                distance.Add(node, 0);
            }
            foreach (var node in AvailNodes)
            {
                newOnes.Enqueue(node);
                distance.Add(node, 1);
            }
            while (newOnes.Count > 0)
            {
                ushort newNode = newOnes.Dequeue();
                int dis = distance[newNode];
                visited.Add(newNode);
                foreach (var connection in Skillnodes[newNode].Neighbor.Select(nd => nd.id))
                {
                    if (visited.Contains(connection))
                        continue;
                    if (distance.ContainsKey(connection))
                        continue;
                    if (Skillnodes[newNode].spc.HasValue)
                        continue;
                    if (Skillnodes[newNode].Mastery)
                        continue;
                    distance.Add(connection, dis + 1);
                    newOnes.Enqueue(connection);

                    parent.Add(connection, newNode);

                    if (connection == targetNode)
                        break;
                }
            }

            if (!distance.ContainsKey(targetNode))
                return new List<ushort>();

            Stack<ushort> path = new Stack<ushort>();
            ushort curr = targetNode;
            path.Push(curr);
            while (parent.ContainsKey(curr))
            {
                path.Push(parent[curr]);
                curr = parent[curr];
            }

            List<ushort> result = new List<ushort>();
            while (path.Count > 0)
                result.Add(path.Pop());

            return result;
        }
        public HashSet<ushort> ForceRefundNodePreview(ushort nodeId)
        {
            if (!SkilledNodes.Remove(nodeId))
                return new HashSet<ushort>();

            SkilledNodes.Remove(nodeId);

            HashSet<ushort> front = new HashSet<ushort>();
            front.Add(SkilledNodes.First());
            foreach (var i in Skillnodes[SkilledNodes.First()].Neighbor)
                if (SkilledNodes.Contains(i.id))
                    front.Add(i.id);

            HashSet<ushort> skilled_reachable = new HashSet<ushort>(front);
            while (front.Count > 0)
            {
                HashSet<ushort> newFront = new HashSet<ushort>();
                foreach (var i in front)
                    foreach (var j in Skillnodes[i].Neighbor.Select(nd => nd.id))
                        if (!skilled_reachable.Contains(j) && SkilledNodes.Contains(j))
                        {
                            newFront.Add(j);
                            skilled_reachable.Add(j);
                        }

                front = newFront;
            }

            HashSet<ushort> unreachable = new HashSet<ushort>(SkilledNodes);
            foreach (var i in skilled_reachable)
                unreachable.Remove(i);
            unreachable.Add(nodeId);

            SkilledNodes.Add(nodeId);

            return unreachable;
        }
        public void ForceRefundNode(ushort nodeId)
        {
            if (!SkilledNodes.Remove(nodeId))
                throw new InvalidOperationException();

            //SkilledNodes.Remove(nodeId);

            HashSet<ushort> front = new HashSet<ushort>();
            front.Add(SkilledNodes.First());
            foreach (var i in Skillnodes[SkilledNodes.First()].Neighbor)
                if (SkilledNodes.Contains(i.id))
                    front.Add(i.id);
            HashSet<ushort> skilled_reachable = new HashSet<ushort>(front);
            while (front.Count > 0)
            {
                HashSet<ushort> newFront = new HashSet<ushort>();
                foreach (var i in front)
                    foreach (var j in Skillnodes[i].Neighbor.Select(nd => nd.id))
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
        public void LoadFromURL(string url)
        {
            string s = url.Substring(TreeAddress.Length + (url.StartsWith("https") ? 1 : 0)).Replace("-", "+").Replace("_", "/");
            byte[] decbuff = Convert.FromBase64String(s);
            var i = BitConverter.ToInt32(new byte[] { decbuff[3], decbuff[2], decbuff[1], decbuff[1] }, 0);
            var b = decbuff[4];
            var j = 0L;
            if (i > 0)
                j = decbuff[5];
            List<UInt16> nodes = new List<UInt16>();
            for (int k = 6; k < decbuff.Length; k += 2)
            {
                byte[] dbff = new byte[] { decbuff[k + 1], decbuff[k + 0] };
                if (Skillnodes.Keys.Contains(BitConverter.ToUInt16(dbff, 0)))
                    nodes.Add((BitConverter.ToUInt16(dbff, 0)));

            }
            Chartype = b;
            SkilledNodes.Clear();
            SkillTree.SkillNode startnode = Skillnodes.First(nd => nd.Value.name.ToUpper() == CharName[Chartype].ToUpper()).Value;
            SkilledNodes.Add(startnode.id);
            foreach (ushort node in nodes)
            {
                SkilledNodes.Add(node);
            }
            UpdateAvailNodes();
        }
        public string SaveToURL()
        {
            byte[] b = new byte[(SkilledNodes.Count - 1) * 2 + 6];
            var b2 = BitConverter.GetBytes(2);
            b[0] = b2[3];
            b[1] = b2[2];
            b[2] = b2[1];
            b[3] = b2[0];
            b[4] = (byte)(Chartype);
            b[5] = (byte)(0);
            int pos = 6;
            foreach (var inn in SkilledNodes)
            {
                if (CharName.Contains(Skillnodes[inn].name.ToUpper()))
                    continue;
                byte[] dbff = BitConverter.GetBytes((Int16)inn);
                b[pos++] = dbff[1];
                b[pos++] = dbff[0];
            }
            return TreeAddress + Convert.ToBase64String(b).Replace("/", "_").Replace("+", "-");

        }
        public void UpdateAvailNodes()
        {
            AvailNodes.Clear();
            foreach (ushort inode in SkilledNodes)
            {
                SkillNode node = Skillnodes[inode];
                foreach (SkillNode skillNode in node.Neighbor)
                {
                    if (!CharName.Contains(skillNode.name) && !SkilledNodes.Contains(skillNode.id))
                        AvailNodes.Add(skillNode.id);
                }
            }
            //  picActiveLinks = new DrawingVisual();

            Pen pen2 = new Pen(Brushes.Yellow, 15f);

            using (DrawingContext dc = picActiveLinks.RenderOpen())
            {
                foreach (var n1 in SkilledNodes)
                {
                    foreach (var n2 in Skillnodes[n1].Neighbor)
                    {
                        if (SkilledNodes.Contains(n2.id))
                        {
                            DrawConnection(dc, pen2, n2, Skillnodes[n1]);
                        }
                    }
                }
            }
            // picActiveLinks.Clear();
            DrawNodeSurround();
        }
        public Dictionary<string, List<float>> SelectedAttributes
        {
            get
            {
                Dictionary<string, List<float>> temp = SelectedAttributesWithoutImplicit;

                foreach (var a in ImplicitAttributes(temp))
                {
                    if (!temp.ContainsKey(a.Key))
                        temp[a.Key] = new List<float>();
                    for (int i = 0; i < a.Value.Count; i++)
                    {

                        if (temp.ContainsKey(a.Key) && temp[a.Key].Count > i)
                            temp[a.Key][i] += a.Value[i];
                        else
                        {
                            temp[a.Key].Add(a.Value[i]);
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
                Dictionary<string, List<float>> temp = new Dictionary<string, List<float>>();
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
                    foreach (var attr in node.Attributes)
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
        public class SkillIcons
        {

            public enum IconType
            {
                normal,
                notable,
                keystone
            }

            public Dictionary<string, KeyValuePair<Rect, string>> SkillPositions = new Dictionary<string, KeyValuePair<Rect, string>>();
            public Dictionary<String, BitmapImage> Images = new Dictionary<string, BitmapImage>();
            public static string urlpath = "http://www.pathofexile.com/image/build-gen/passive-skill-sprite/";
            public void OpenOrDownloadImages(UpdateLoadingWindow update = null)
            {
                //Application
                int count = 0;
                foreach (var image in Images.Keys.ToArray())
                {
                    if (!File.Exists("Data\\Assets\\" + image))
                    {
                        System.Net.WebClient _WebClient = new System.Net.WebClient();
                        _WebClient.DownloadFile(urlpath + image, "Data\\Assets\\" + image);
                    }
                    Images[image] = new BitmapImage(new Uri("Data\\Assets\\" + image, UriKind.Relative));
                    if (update != null)
                        update(count * 100 / Images.Count, 100);
                    ++count;
                }
            }
        }
        public class NodeGroup
        {
            public Vector2D Position;// "x": 1105.14,"y": -5295.31,
            public Dictionary<int, bool> OcpOrb = new Dictionary<int, bool>(); //  "oo": {"1": true},
            public List<int> Nodes = new List<int>();// "n": [-28194677,769796679,-1093139159]

        }
        public class SkillNode
        {
            static public float[] skillsPerOrbit = { 1, 6, 12, 12, 12 };
            static public float[] orbitRadii = { 0, 81.5f, 163, 326, 489 };
            public HashSet<int> Connections = new HashSet<int>();
            public bool skilled = false;
            public UInt16 id; // "id": -28194677,
            public string icon;// icon "icon": "Art/2DArt/SkillIcons/passives/tempint.png",
            public bool ks; //"ks": false,
            public bool not;   // not": false,
            public string name;//"dn": "Block Recovery",
            public int a;// "a": 3,
            public string[] attributes;// "sd": ["8% increased Block Recovery"],
            public Dictionary<string, List<float>> Attributes;
            // public List<string> AttributeNames;
            //public List<> AttributesValues;
            public int g;// "g": 1,
            public int orbit;//  "o": 1,
            public int orbitIndex;// "oidx": 3,
            public int sa;//s "sa": 0,
            public int da;// "da": 0,
            public int ia;//"ia": 0,
            public List<int> linkID = new List<int>();// "out": []
            public bool Mastery;
            public int? spc;

            public List<SkillNode> Neighbor = new List<SkillNode>();
            public NodeGroup NodeGroup;
            public Vector2D Position
            {
                get
                {
                if(NodeGroup==null) return new Vector2D();
                    double d = orbitRadii[this.orbit];
                    double b = (2 * Math.PI * this.orbitIndex / skillsPerOrbit[this.orbit]);
                    return (NodeGroup.Position - new Vector2D(d * Math.Sin(-b), d * Math.Cos(-b)));
                }
            }
            public double Arc
            {
                get
                {
                    return (2 * Math.PI * this.orbitIndex / skillsPerOrbit[this.orbit]);
                }
            }
        }
        public class Asset
        {
            public string Name;
            public BitmapImage PImage;
            public string URL;
            public Asset(string name, string url)
            {
                Name = name;
                URL = url;
                if (!File.Exists("Data\\Assets\\" + Name + ".png"))
                {

                    System.Net.WebClient _WebClient = new System.Net.WebClient();
                    _WebClient.DownloadFile(URL, "Data\\Assets\\" + Name + ".png");



                }
                PImage = new BitmapImage(new Uri("Data\\Assets\\" + Name + ".png", UriKind.Relative));

            }

        }

        public void HighlightNodes(string search, bool useregex)
        {
            if (search == "")
            {
                DrawHighlights(highlightnodes = new List<SkillTree.SkillNode>());
                highlightnodes = null;
                return;
            }

            if (useregex)
            {
                try
                {
                    List<SkillTree.SkillNode> nodes = highlightnodes = Skillnodes.Values.Where(nd => nd.attributes.Where(att => new Regex(search, RegexOptions.IgnoreCase).IsMatch(att)).Count() > 0 || new Regex(search, RegexOptions.IgnoreCase).IsMatch(nd.name) && !nd.Mastery).ToList();
                    DrawHighlights(highlightnodes);
                }
                catch (Exception)
                {
                }

            }
            else
            {
                highlightnodes = Skillnodes.Values.Where(nd => nd.attributes.Where(att => att.ToLower().Contains(search.ToLower())).Count() != 0 || nd.name.ToLower().Contains(search.ToLower()) && !nd.Mastery).ToList();

                DrawHighlights(highlightnodes);
            }
        }
        public void SkillAllHighligtedNodes()
        {
            if (highlightnodes == null)
                return;
            HashSet<int> nodes = new HashSet<int>();
            foreach (var nd in highlightnodes)
            {
                nodes.Add(nd.id);
            }
            SkillStep(nodes);

        }
        private HashSet<int> SkillStep(HashSet<int> hs)
        {
            List<List<ushort>> pathes = new List<List<ushort>>();
            foreach (var nd in highlightnodes)
            {
                pathes.Add(GetShortestPathTo(nd.id));


            }
            pathes.Sort((p1, p2) => p1.Count.CompareTo(p2.Count));
            pathes.RemoveAll(p => p.Count == 0);
            foreach (ushort i in pathes[0])
            {
                hs.Remove(i);
                SkilledNodes.Add(i);
            }
            UpdateAvailNodes();

            return hs.Count == 0 ? hs : SkillStep(hs);
        }

    }


}
