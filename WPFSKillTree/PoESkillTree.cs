using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace PoEClasses
{
   
    internal class Character
    {
        
        public int base_str { get; set; }
        
        public int base_dex { get; set; }
        
        public int base_int { get; set; }
    }

   
    internal class NodeGroup
    {
        
        public double x { get; set; }
        
        public double y { get; set; }
        public Dictionary<int, bool> oo { get; set; }
        
        public List<int> n { get; set; }
    }

   
    internal class Main
    {
        
        public int g { get; set; }
        
        public int o { get; set; }
        
        public int oidx { get; set; }
        
        public int sa { get; set; }
        
        public int da { get; set; }
        
        public int ia { get; set; }

        [JsonProperty("out")]
        public int[] ot { get; set; }
    }
       
    internal class Node
    {
        
        public ushort id { get; set; }
        
        public string icon { get; set; }
        
        public bool ks { get; set; }
        
        public bool not { get; set; }
        
        public string dn { get; set; }
        
        public bool m { get; set; }
        
        public int[] spc { get; set; }
        
        public string[] sd { get; set; }
        
        public int g { get; set; }
        
        public int o { get; set; }
        
        public int oidx { get; set; }
        
        public int sa { get; set; }
        
        public int da { get; set; }
        
        public int ia { get; set; }


        [JsonProperty("out")]
        public List<int> ot { get; set; }
    }

    internal class Constants
    {
        
        public Dictionary<string, int> classes { get; set; }
        
        public Dictionary<string, int> characterAttributes { get; set; }
        
        public int PSSCentreInnerRadius { get; set; }
    }

    internal class Art2D
    {
        
        public int x { get; set; }
        
        public int y { get; set; }
        
        public int w { get; set; }
        
        public int h { get; set; }
    }

    internal class SkillSprite
    {
        
        public string filename { get; set; }
        
        public Dictionary<string, Art2D> coords { get; set; }
    }


    internal class PoESkillTree
    {

        
        public Dictionary<int, Character> characterData { get; set; }
        
        public Dictionary<int,NodeGroup> groups { get; set; }
        
        public Main main { get; set; }
        
        public Node[] nodes { get; set; }
        
        public int min_x { get; set; }
        
        public int min_y { get; set; }
        
        public int max_x { get; set; }
        
        public int max_y { get; set; }
        
        public Dictionary<string, Dictionary<float, string>> assets { get; set; }
        
        public Constants constants { get; set; }
        
        public string imageRoot { get; set; }
        
        public Dictionary<string, SkillSprite[]> skillSprites { get; set; }
        
        public double[] imageZoomLevels { get; set; }
    }

}
