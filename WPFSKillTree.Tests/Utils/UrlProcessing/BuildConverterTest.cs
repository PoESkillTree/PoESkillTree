using NUnit.Framework;
using POESKillTree.Utils.UrlProcessing;

namespace PoESkillTree.Tests.Utils.UrlProcessing
{
    [TestFixture]
    public class BuildConverterTest
    {
        private IBuildConverter _buildConverter;

        [SetUp]
        public void TestInitialize()
        {
            _buildConverter = new BuildConverter(null);
            RegisterFactories();
        }

        [Test]
        public void GetUrlDeserializerReturnsNullWhenNoFactories()
        {
            UnregisterAllFactories();

            var deserializer = _buildConverter.GetUrlDeserializer("some.url");

            Assert.IsNull(deserializer);
        }

        [Test]
        public void GetUrlDeserializerReturnsCorrectDeserializer()
        {
            var deserializer = _buildConverter.GetUrlDeserializer("http://www.pathofexile.com/passive-skill-tree/AAAABAAAAA==");

            Assert.AreEqual(typeof(PathofexileUrlDeserializer), deserializer.GetType());
        }

        [Test]
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
