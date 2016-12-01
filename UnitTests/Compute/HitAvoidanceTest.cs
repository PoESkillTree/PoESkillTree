using NUnit.Framework;
using POESKillTree.Compute;
using POESKillTree.Model;
using static UnitTests.Compute.AttributeHelpers;

namespace UnitTests.Compute
{
    [TestFixture]
    public class HitAvoidanceTest
    {
        public void Test(string attribute, string resultAtt)
        {
            TestNormal(attribute, resultAtt);
            TestWithAllAvoid(attribute, resultAtt);
            TestWithZero(attribute, resultAtt);
        }

        public void TestNormal(string attribute, string resultAtt)
        {
            var c = new Computation();

            var attSet = new AttributeSet();
            c.Global.Add(attribute, 10f);
            c.HitAvoidance(attSet);

            CheckEquality(new[] { 10f }, attSet[resultAtt]);
        }

        public void TestWithAllAvoid(string toTest, string resultAtt)
        {
            var c = new Computation();

            var attSet = new AttributeSet();
            c.Global.Add(toTest, 10f);
            c.Global.Add("#% chance to Avoid Elemental Status Ailments", 10f);
            c.HitAvoidance(attSet);

            CheckEquality(new[] { 20f }, attSet[resultAtt]);
        }

        public void TestWithZero(string toTest, string resultAtt)
        {
            var c = new Computation();

            var attSet = new AttributeSet();
            c.Global.Add(toTest, 0f);
            c.HitAvoidance(attSet);

            Assert.False(c.Global.ContainsKey(resultAtt));
        }

        [Test]
        public void TestIgnite()
        {
            Test("#% chance to Avoid being Ignited", "Ignite Avoidance: #%");
        }

        [Test]
        public void TestChill()
        {
            Test("#% chance to Avoid being Chilled", "Chill Avoidance: #%");
        }

        [Test]
        public void TestFreeze()
        {
            Test("#% chance to Avoid being Frozen", "Freeze Avoidance: #%");
        }

        [Test]
        public void TestShock()
        {
            Test("#% chance to Avoid being Shocked", "Shock Avoidance: #%");
        }

    }
}
