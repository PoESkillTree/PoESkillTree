using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Utils.UrlProcessing;
using UnitTests.TestBuilds.Utils;

namespace UnitTests.UrlProcessing
{
    [TestClass]
    public class BuildUrlNormalizerTests
    {
        private static BuildUrlCollection _builds;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _builds = TestBuildUrlLoader.LoadFromXmlFile("../../TestBuilds/BuildUrls.xml");
        }

        [TestMethod]
        public async Task NormalizeAsyncExtractsUrlFromValidGoogleLinkTest()
        {
            // #267
            var build = _builds.FindByName("ObsoleteShadowAssassin");
            var targetUrl = build.GetAlternativeUrl("googleQuery");

            Func<string, Task, Task> loadingWrapper = (msg, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(targetUrl, loadingWrapper);

            Assert.AreEqual(build.DefaultUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromParameterlessValidGoogleLinkTest()
        {
            // #267
            var build = _builds.FindByName("ObsoleteShadowAssassin");
            var targetUrl = build.GetAlternativeUrl("googleQueryShort");

            Func<string, Task, Task> loadingWrapper = (msg, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(targetUrl, loadingWrapper);

            Assert.AreEqual(build.DefaultUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromShortenedGoogleLinkTest()
        {
            var build = _builds.FindByName("ObsoleteShadowAssassin");
            var targetUrl = build.GetAlternativeUrl("googl");

            Func<string, Task, Task> loadingWrapper = (msg, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(targetUrl, loadingWrapper);

            Assert.AreEqual(build.DefaultUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncFailsOnInvalidGoogleLinkTest()
        {
            // #267
            var treeUrl = "https://google.com/url?ust=1456857460554000&usg=AFQjCNH6POtjRXIVzd_kSRbH7sOVYuZW7A";

            Exception exception = null;
            try
            {
                await CreateBuildUrlNormalizer().NormalizeAsync(treeUrl, null);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("The URL doesn't contain required query parameter 'q'.", exception.Message);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromValidTinyurlLinkTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var tinyurlTreeUrl = build.GetAlternativeUrl("tinyurl");
            var expectedUrl = build.GetAlternativeUrl("pathofexileWindowed");

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(tinyurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromValidPoeurlLinkTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var poeurlTreeUrl = build.GetAlternativeUrl("poeurl");
            var expectedUrl = build.GetAlternativeUrl("pathofexileWindowed");

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromValidPoeurlLinkWithRedirectTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var poeurlTreeUrl = build.GetAlternativeUrl("poeurlRedirect");
            var expectedUrl = build.GetAlternativeUrl("pathofexileWindowed");

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromValidPoeurlLinkWithoutProtocolTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var poeurlTreeUrl = build.GetAlternativeUrl("poeurlNoProto");
            var expectedUrl = build.GetAlternativeUrl("pathofexileWindowed");

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromValidPoeurlLinkWithoutWwwPrefixTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var poeurlTreeUrl = build.GetAlternativeUrl("poeurlNoPrefix");
            var expectedUrl = build.GetAlternativeUrl("pathofexileWindowed");

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromTwiceShortenedLinkTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var poeurlTreeUrl = build.GetAlternativeUrl("doubleShortened");
            var expectedUrl = build.GetAlternativeUrl("pathofexileWindowed");

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncProvidesMessageToLoadingWrapperTest()
        {
            var build = _builds.FindByName("PoeplannerWitchOccultistAscendant");
            var poeurlTreeUrl = build.GetAlternativeUrl("poeurl");
            var expectedUrl = build.GetAlternativeUrl("pathofexileWindowed");

            string message = null;

            Func<string, Task, Task> loadingWrapper = (msg, task) =>
            {
                message = msg;
                return task;
            };

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
            Assert.AreEqual("Resolving shortened tree address", message);
        }

        [TestMethod]
        public async Task NormalizeAsyncSkipsReadyLinkTest()
        {
            var treeUrl = "https://pathofexile.com/passive-skill-tree/AAAABAAAAA==";

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(treeUrl, null);

            Assert.AreEqual(treeUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncSkipsUnsupportedLinkTest()
        {
            var treeUrl = "http://unsupported.com";

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(treeUrl, null);

            Assert.AreEqual(treeUrl, actualUrl);
        }

        [TestMethod]
        public void EnsureProtocolCompletesKnownUrlsTest()
        {
            var targetPath = "/abc";
            var completions = new Dictionary<string, string>
            {
                { "goo.gl", "https://goo.gl" },
                { "poeurl.com", "http://poeurl.com" },
                { "tinyurl.com", "https://tinyurl.com" },
                { "pathofexile.com", "https://pathofexile.com" }
            };

            var urlNormalizer = CreateBuildUrlNormalizerMock();
            foreach (var completion in completions)
            {
                var actualUrl = urlNormalizer.EnsureProtocolProxy(completion.Key + targetPath);
                Assert.AreEqual(completion.Value + targetPath, actualUrl);
            }
        }

        [TestMethod]
        public void EnsureProtocolSkipsUnknownUrlsTest()
        {
            var expectedPath = "example.com/a";
            var urlNormalizer = CreateBuildUrlNormalizerMock();

            var actualUrl = urlNormalizer.EnsureProtocolProxy(expectedPath);
            Assert.AreEqual(expectedPath, actualUrl);

        }

        [TestMethod]
        public void ExtractUrlFromQueryTest()
        {
            var expectedUrl = "https://pathofexile.com";
            var targetUrl = $"https://example.com?q={expectedUrl}&x=42";
            var urlNormalizer = CreateBuildUrlNormalizerMock();

            var actualUrl = urlNormalizer.ExtractUrlFromQueryProxy(targetUrl, "q");

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        #region Helpers

        private static BuildUrlNormalizer CreateBuildUrlNormalizer()
        {
            return new BuildUrlNormalizer();
        }

        private static BuildUrlNormalizerMock CreateBuildUrlNormalizerMock()
        {
            return new BuildUrlNormalizerMock();
        }

        private class BuildUrlNormalizerMock : BuildUrlNormalizer
        {
            public string EnsureProtocolProxy(string buildUrl) => EnsureProtocol(buildUrl);

            public string ExtractUrlFromQueryProxy(string buildUrl, string parameterName) => ExtractUrlFromQuery(buildUrl, parameterName);
        }

        #endregion
    }
}
