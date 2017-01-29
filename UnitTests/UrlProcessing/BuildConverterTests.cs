using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Utils.UrlProcessing;

namespace UnitTests.UrlProcessing
{
    [TestClass]
    public class BuildConverterTests
    {
        [TestMethod]
        public void GetUrlDeserializerReturnsNullWhenNoFactories()
        {
            UnregisterAllFactories();

            var deserializer = BuildConverter.GetUrlDeserializer("some.url");

            Assert.IsNull(deserializer);
        }

        [TestMethod]
        public void GetUrlDeserializerReturnsCorrectDeserializer()
        {
            RegisterFactories();

            var deserializer = BuildConverter.GetUrlDeserializer("http://www.pathofexile.com/passive-skill-tree/AAAABAAAAA==");

            Assert.AreEqual(typeof(PathofexileUrlDeserializer), deserializer.GetType());
        }

        [TestMethod]
        public void GetUrlDeserializerReturnsDefaultDeserializerWhenUrlNotSupported()
        {
            RegisterFactories();

            var deserializer = BuildConverter.GetUrlDeserializer("https://example.com/AAAABAAAAA==");

            Assert.AreEqual(typeof(NaivePoEUrlDeserializer), deserializer.GetType());
        }

        #region Helpers

        private void RegisterFactories()
        {
            UnregisterAllFactories();

            BuildConverter.RegisterDeserializersFactories(
                PathofexileUrlDeserializer.TryCreate,
                PoeplannerUrlDeserializer.TryCreate);

            BuildConverter.RegisterDefaultDeserializer(url => new NaivePoEUrlDeserializer(url));
        }

        private void UnregisterAllFactories()
        {
            BuildConverter.RegisterDefaultDeserializer(url => null as NaivePoEUrlDeserializer);
            BuildConverter.RegisterDeserializersFactories();
        }

        #endregion
    }
}
