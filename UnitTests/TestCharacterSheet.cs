using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Model.Items;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;
using POESKillTree.ViewModels;

namespace UnitTests
{
    [TestClass]
    public class TestCharacterSheet
    {
        private TestContext TestContextInstance;
        public TestContext TestContext
        {
            get { return TestContextInstance; }
            set { TestContextInstance = value; }
        }

        static SkillTree Tree;

        [ClassInitialize]
        public static void Initalize(TestContext testContext)
        {
            AppData.SetApplicationData(Environment.CurrentDirectory);

            if (ItemDB.IsEmpty())
                ItemDB.Load("Items.xml", true);
            Tree = SkillTree.CreateSkillTree((string dummy) => { Debug.WriteLine("Download started"); }, (double dummy1, double dummy2) => { }, () => { Debug.WriteLine("Download finished"); });
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
        public void TestBuild()
        {
            // Read build entry.
            string treeURL = TestContext.DataRow["TreeURL"].ToString();
            int level = Convert.ToInt32(TestContext.DataRow["Level"]);
            string buildFile = @"..\..\TestBuilds\" + TestContext.DataRow["BuildFile"].ToString();
            List<string> expectDefense = new List<string>();
            List<string> expectOffense = new List<string>();
            if (TestContext.DataRow.Table.Columns.Contains("ExpectDefence"))
            {
                using (StringReader reader = new StringReader(TestContext.DataRow["ExpectDefence"].ToString()))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length > 0 && !line.StartsWith("#"))
                            expectDefense.Add(line.Trim());
                    }
                }
            }
            if (TestContext.DataRow.Table.Columns.Contains("ExpectOffence"))
            {
                using (StringReader reader = new StringReader(TestContext.DataRow["ExpectOffence"].ToString()))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.Length > 0 && !line.StartsWith("#"))
                            expectOffense.Add(line.Trim());
                    }
                        
                }
            }

            // Initialize structures.
            Tree.LoadFromURL(treeURL);
            Tree.Level = level;

            string itemData = File.ReadAllText(buildFile);
            ItemAttributes itemAttributes = new ItemAttributes(itemData, new EquipmentData());
            Compute.Initialize(Tree, itemAttributes);

            // Compare defense properties.
            Dictionary<string, List<string>> defense = new Dictionary<string, List<string>>();
            if (expectDefense.Count > 0)
            {
                foreach (ListGroup grp in Compute.Defense())
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
                        Assert.IsNotNull(group, "Missing defence group [" + TestContext.DataRow["BuildFile"].ToString() + "]");
                        Assert.IsTrue(group.Contains(entry), "Wrong " + entry + " [" + TestContext.DataRow["BuildFile"].ToString() + "]");
                    }
                    else // Group
                    {
                        Assert.IsTrue(defense.ContainsKey(entry), "No such defence group: " + entry + " [" + TestContext.DataRow["BuildFile"].ToString() + "]");
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
                        Assert.IsNotNull(group, "Missing offence group [" + TestContext.DataRow["BuildFile"].ToString() + "]");
                        Assert.IsTrue(group.Contains(entry), "Wrong " + entry + " [" + TestContext.DataRow["BuildFile"].ToString() + "]");
                    }
                    else // Group
                    {
                        Assert.IsTrue(offense.ContainsKey(entry), "No such offence group: " + entry + " [" + TestContext.DataRow["BuildFile"].ToString() + "]");
                        group = offense[entry];
                    }
                }
            }
        }
    }
}
