using NUnit.Framework;

namespace PoESkillTree.Utils.UrlProcessing
{
    [TestFixture]
    public class BuildConverterTest
    {
        private IBuildConverter _buildConverter = default!;

        [SetUp]
        public void TestInitialize()
        {
            _buildConverter = new BuildConverter(null!,
                url => new NaivePoEUrlDeserializer(url, null!),
                PathofexileUrlDeserializer.TryCreate,
                PoeplannerUrlDeserializer.TryCreate);
        }

        [Test]
        public void GetUrlDeserializerReturnsNullWhenNoFactories()
        {
            var sut = new BuildConverter(null!, _ => null!);

            var deserializer = sut.GetUrlDeserializer("some.url");

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
    }
}
