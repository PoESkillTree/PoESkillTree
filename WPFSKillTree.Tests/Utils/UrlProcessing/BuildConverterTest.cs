using NUnit.Framework;
using PoESkillTree.Utils.UrlProcessing.Decoders;

namespace PoESkillTree.Utils.UrlProcessing
{
    [TestFixture]
    public class BuildConverterTest
    {
        [Test]
        public void GetUrlDeserializerReturnsNullWhenNoFactories()
        {
            var data = SkillTreeUrlDecoders.TryDecode("some.url");
            Assert.IsFalse(data.IsValid);
        }

        [Test]
        public void SkilLTreeUrlDecodersReturnValidDataWhenUrlIsNotSupported()
        {
            var data = SkillTreeUrlDecoders.TryDecode("https://example.com/AAAABgMAAAAA");

            Assert.IsTrue(data.IsValid);
        }

        [Test]
        public void SkilLTreeUrlDecodersReturnValidDataWhenUrlIsSupported()
        {
            var data = SkillTreeUrlDecoders.TryDecode("https://www.pathofexile.com/passive-skill-tree/AAAABgMAAAAA");

            Assert.IsTrue(data.IsValid);
        }

        [Test]
        public void SkilLTreeUrlDecodersReturnValidDataWhenUrlIsSupportedFullscreen()
        {
            var data = SkillTreeUrlDecoders.TryDecode("https://www.pathofexile.com/fullscreen-passive-skill-tree/AAAABgMAAAAA");

            Assert.IsTrue(data.IsValid);
        }

        [Test]
        public void SkilLTreeUrlDecodersReturnValidDataWhenUrlIsSupportedFullscreenWithVersion()
        {
            var data = SkillTreeUrlDecoders.TryDecode("https://www.pathofexile.com/fullscreen-passive-skill-tree/3.16.0/AAAABgMAAAAA");

            Assert.IsTrue(data.IsValid);
        }
    }
}
