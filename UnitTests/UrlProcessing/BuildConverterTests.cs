using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils.UrlProcessing;

namespace UnitTests.UrlProcessing
{
    [TestClass]
    public class BuildConverterTests
    {
        private IBuildConverter _buildConverter;

        [TestInitialize]
        public void TestInitialize()
        {
            _buildConverter = new BuildConverter(null);
            RegisterFactories();
        }

        [TestMethod]
        public void GetUrlDeserializerReturnsNullWhenNoFactories()
        {
            UnregisterAllFactories();

            var deserializer = _buildConverter.GetUrlDeserializer("some.url");

            Assert.IsNull(deserializer);
        }

        [TestMethod]
        public void GetUrlDeserializerReturnsCorrectDeserializer()
        {
            var deserializer = _buildConverter.GetUrlDeserializer("http://www.pathofexile.com/passive-skill-tree/AAAABAAAAA==");

            Assert.AreEqual(typeof(PathofexileUrlDeserializer), deserializer.GetType());
        }

        [TestMethod]
        public void GetUrlDeserializerReturnsDefaultDeserializerWhenUrlNotSupported()
        {
            var deserializer = _buildConverter.GetUrlDeserializer("https://example.com/AAAABAAAAA==");

            Assert.AreEqual(typeof(NaivePoEUrlDeserializer), deserializer.GetType());
        }

        #region Helpers

        private void RegisterFactories()
        {
            _buildConverter.RegisterDeserializersFactories(
                PathofexileUrlDeserializer.TryCreate,
                PoeplannerUrlDeserializer.TryCreate);

            _buildConverter.RegisterDefaultDeserializer(url => new NaivePoEUrlDeserializer(url, null));
        }

        private void UnregisterAllFactories()
        {
            _buildConverter.RegisterDefaultDeserializer(url => null as NaivePoEUrlDeserializer);
            _buildConverter.RegisterDeserializersFactories();
        }

        #endregion
    }
}
