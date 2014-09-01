using System;
using System.Xml.Serialization;

namespace POESKillTree.ViewModels
{
    public class PoEBuild
    {
        public string Name { get; set; }
        public string Class { get; set; }
        public string PointsUsed { get; set; }
        public string Url { get; set; }
        public string Note { get; set; }

        [XmlIgnoreAttribute]
        public string Image { get { return "/POESKillTree;component/Images/" + Class + ".jpg"; } }
        [XmlIgnoreAttribute]
        public string Description {get { return Class + ", " + PointsUsed + " points used"; }}

        public PoEBuild()
        {
        }

        public PoEBuild(string name, string poeClass, string pointsUsed, string url, string note)
        {
            Name = name;
            Class = poeClass;
            PointsUsed = pointsUsed;
            Url = url;
            Note = note;
        }

        public override string ToString()
        {
            return Name + '\n' + Description;
        }
    }
}