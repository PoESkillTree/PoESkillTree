using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Model.Builds;
using POESKillTree.Model.Items;
using POESKillTree.Model.Serialization;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.ViewModels;
using POESKillTree.Compute;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;
using POESKillTree.Model.Gems;

namespace UnitTests
{
    [TestClass]
    public class TestCharacterSheet
    {
        [Serializable]
        public class TestBuild
        {
            public string Level { get; set; }
            public string TreeUrl { get; set; }
            public string BuildFile { get; set; }
            public string ExpectDefence { get; set; }
            public string ExpectOffence { get; set; }
        }


        readonly Regex _backreplace = new Regex("#");
        string InsertNumbersInAttributes(KeyValuePair<string, List<float>> attrib)
        {
            string s = attrib.Key;
            foreach (float f in attrib.Value)
            {
                s = _backreplace.Replace(s, f.ToString(CultureInfo.InvariantCulture.NumberFormat), 1);
            }
            return s;
        }

        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", @"..\..\TestBuilds\Builds.xml", "TestBuild", DataAccessMethod.Sequential)]
        [TestMethod]
        public async Task Build_Test()
        {
            // Read build entry.
            var resultTree = FileEx.GetResource<TestCharacterSheet>("UnitTests.TestBuilds/Builds.xml");
            var xmlFile = XDocument.Parse(resultTree);
            var build = XmlHelpers.DeserializeXml<TestBuild>(xmlFile.Root.Element("TestBuild").ToString());

            AbstractPersistentData _persistentData;

            var db = GemDB.LoadFromText(FileEx.GetResource<GemDB>(@"POESKillTree.Data.ItemDB.GemList.xml"));
            _persistentData = new BarePersistentData { CurrentBuild = new PoEBuild() };


            int level = Convert.ToInt32(build.Level);
            List<string> expectDefense = new List<string>();
            List<string> expectOffense = new List<string>();

            using (StringReader reader = new StringReader(build.ExpectDefence))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length > 0 && !line.StartsWith("#"))
                        expectDefense.Add(line.Trim());
                }
            }

            using (StringReader reader = new StringReader(build.ExpectOffence))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length > 0 && !line.StartsWith("#"))
                        expectOffense.Add(line.Trim());
                }

            }

            _persistentData.EquipmentData = await EquipmentData.CreateAsync(_persistentData.Options);
            var tree = await SkillTree.CreateAsync(_persistentData, null);
            // Initialize structures.
            tree.LoadFromUrl(build.TreeUrl);
            tree.Level = level;

            string itemData = FileEx.GetResource<TestCharacterSheet>("UnitTests.TestBuilds." + build.BuildFile);
            ItemAttributes itemAttributes = new ItemAttributes(_persistentData, itemData);
            var Compute = new Computation(tree, itemAttributes); //failing here because "Staff" isn't recognized.

            // Compare defense properties.
            Dictionary<string, List<string>> defense = new Dictionary<string, List<string>>();
            if (expectDefense.Count > 0)
            {
                foreach (ListGroup grp in Compute.GetDefensiveAttributes())
                {
                    List<string> props = new List<string>();
                    foreach (string item in grp.Properties.Select(InsertNumbersInAttributes))
                        props.Add(item);
                    defense.Add(grp.Name, props);
                }

                List<string> group = null;
                foreach (string entry in expectDefense)
                {
                    if (entry.Contains(':')) // Property: Value
                    {
                        Assert.IsNotNull(group, "Missing defence group [" + build.BuildFile + "]");
                        Assert.IsTrue(group.Contains(entry), "Wrong " + entry + " [" + build.BuildFile + "]");
                    }
                    else // Group
                    {
                        Assert.IsTrue(defense.ContainsKey(entry), "No such defence group: " + entry + " [" + build.BuildFile + "]");
                        group = defense[entry];
                    }
                }
            }

            // Compare offense properties.
            Dictionary<string, List<string>> offense = new Dictionary<string, List<string>>();
            if (expectOffense.Count > 0)
            {
                foreach (ListGroup grp in Compute.Offense())
                {
                    List<string> props = new List<string>();
                    foreach (string item in grp.Properties.Select(InsertNumbersInAttributes))
                        props.Add(item);
                    offense.Add(grp.Name, props);
                }

                List<string> group = null;
                foreach (string entry in expectOffense)
                {
                    if (entry.Contains(':')) // Property: Value
                    {
                        Assert.IsNotNull(group, "Missing offence group [" + build.BuildFile + "]");
                        Assert.IsTrue(group.Contains(entry), "Wrong " + entry + " [" + build.BuildFile + "]");
                    }
                    else // Group
                    {
                        Assert.IsTrue(offense.ContainsKey(entry), "No such offence group: " + entry + " [" + build.BuildFile + "]");
                        group = offense[entry];
                    }
                }
            }
        }
    }
}
