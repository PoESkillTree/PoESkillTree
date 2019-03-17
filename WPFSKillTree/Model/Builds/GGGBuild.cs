using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using PoESkillTree.SkillTreeFiles;
using PoESkillTree.Utils;
using Newtonsoft.Json;

namespace PoESkillTree.Model.Builds
{
    class GGGBuild
    {
        private string _name;
        private string _version;
        private List<GGGBuildPart> _parts;

        /// <summary>
        /// Gets or sets the name of the build (most likely the folder name).
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get => _name; set => _name = value; }
        /// <summary>
        /// Gets or sets the GGG patch version (i.e. 2.6.1, 3.0.0, etc.). 
        /// This does not include patch versions with a letter after them (i.e. 2.6.1b, 3.0.0b, etc.).
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string Version { get => _version; set => _version = value; }

        /// <summary>
        /// Gets or sets the parts of the build.
        /// </summary>
        [JsonProperty(PropertyName = "parts")]
        internal List<GGGBuildPart> Parts { get => _parts; set => _parts = value; }
    }

    class GGGBuildPart
    {
        private string _label;
        private string _link;

        [JsonProperty(PropertyName = "label")]
        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }

        [JsonProperty(PropertyName = "link")]
        public string Link
        {
            get { return _link; }
            set { _link = value; }
        }

        public GGGBuildPart Clone() => new GGGBuildPart() { Label = new string(Label.ToCharArray()), Link = new string(Link.ToCharArray()) };
    }
}
