using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MoreLinq;
using NUnit.Framework;

namespace PoESkillTree.Utils.UrlProcessing
{
    [TestFixture]
    public class BuildUrlNormalizerTest
    {
        private const string ResolvedObsoleteAssassin =
            "https://www.pathofexile.com/passive-skill-tree/AAAAAwYAsNjjdaLZMjIjNsgUEJJTEPVv2YZqjLxvFr8mlWHiVUuMNonTcFIqC13yKwoOSBEvAx7jhBGW62MfQQ3RBx5_xrVINj1jQ5UuoqOTJ52q8NXB8--IbIxJUZUg-TdirGsXLR_Xz7TFvOpMs0mxwuzAVAQHIG6usypbBbUI9GpD6-SD21AwQZZ59ujWjb9fKlXG217Q9UM2IuqbJlFH1CM6WFLswzoyAedUN9QwfDBxGYqbtSo4shkk_e0_jX3dqJcGhMXPev_eI_ZLeMSiYetNkjpCu-N6U_66fXXTfgBeGY7tgwgu";

        private const string ObsoleteAssassin =
            "https://pathofexile.com/passive-skill-tree/AAAAAwYAsNjjdaLZMjIjNsgUEJJTEPVv2YZqjLxvFr8mlWHiVUuMNonTcFIqC13yKwoOSBEvAx7jhBGW62MfQQ3RBx5_xrVINj1jQ5UuoqOTJ52q8NXB8--IbIxJUZUg-TdirGsXLR_Xz7TFvOpMs0mxwuzAVAQHIG6usypbBbUI9GpD6-SD21AwQZZ59ujWjb9fKlXG217Q9UM2IuqbJlFH1CM6WFLswzoyAedUN9QwfDBxGYqbtSo4shkk_e0_jX3dqJcGhMXPev_eI_ZLeMSiYetNkjpCu-N6U_66fXXTfgBeGY7tgwgu";

        private const string Ascendant =
            "https://pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

        private const string RuOccultist =
            "https://ru.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhXXFe0WvxslHU8gbiL0JIsqCyo4LL8s4TW5Nj07DTt8PV9HBkkTSVFJsUuuTLNQMFJTVL1WY1b1XEBca13yX2pirGNDbAhsC2yMbRlwUnBWdZ51_X3jf8aESIRvhq6Hy4ngi3qOio_6kBuTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-vorBxcM6w23K088V0B_Q0NRC1bnXz9h22VvbbtvU2-ffiuL35ljpAuq662PsGO-I7-vw1fGK8uH2_PfX-Tc=";

        [TestCaseSource(nameof(CreateTestData))]
        public async Task NormalizeAsyncReturnsCorrectResult(NormalizationTestData data)
        {
            var sut = data.CreateNormalizer();

            var actualUrl = await sut.NormalizeAsync(data.OriginalUrl);

            Assert.AreEqual(data.ExpectedUrl, actualUrl);
        }

        private static IEnumerable<NormalizationTestData> CreateTestData()
        {
            yield return NormalizationTestData.CreateShortened(
                "https://www.google.com/url?q=http://poeurl.com/xer&amp;sa=D&amp;ust=1456857460554000&amp;usg=AFQjCNH6POtjRXIVzd_kSRbH7sOVYuZW7A",
                "http://poeurl.com/redirect.php?url=xer",
                ResolvedObsoleteAssassin, ObsoleteAssassin);
            yield return NormalizationTestData.CreateShortened(
                "https://www.google.com/url?q=http://poeurl.com/xer",
                "http://poeurl.com/redirect.php?url=xer",
                ResolvedObsoleteAssassin, ObsoleteAssassin);
            yield return NormalizationTestData.CreateShortened(
                "goo.gl/44tsqv",
                "https://goo.gl/44tsqv",
                ResolvedObsoleteAssassin, ObsoleteAssassin);

            yield return NormalizationTestData.CreateShortened(
                "http://tinyurl.com/glcmaeo",
                "https://tinyurl.com/glcmaeo",
                Ascendant);
            yield return NormalizationTestData.CreateShortened(
                "http://poeurl.com/0dE",
                "http://poeurl.com/redirect.php?url=0dE",
                Ascendant);
            yield return NormalizationTestData.CreateShortened(
                "http://poeurl.com/redirect.php?url=0dE",
                "http://poeurl.com/redirect.php?url=0dE",
                Ascendant);
            yield return NormalizationTestData.CreateShortened(
                "www.poeurl.com/0dE",
                "http://poeurl.com/redirect.php?url=0dE",
                Ascendant);
            yield return NormalizationTestData.CreateShortened(
                "poeurl.com/0dE",
                "http://poeurl.com/redirect.php?url=0dE",
                Ascendant);
            yield return NormalizationTestData.CreateTwiceShortened(
                "https://goo.gl/kKjcuK",
                "https://goo.gl/kKjcuK",
                "poeurl.com/0dE",
                "http://poeurl.com/redirect.php?url=0dE",
                Ascendant);

            yield return NormalizationTestData.CreateUnchanged(
                RuOccultist);
            yield return NormalizationTestData.Create(
                "https://ru.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhXXFe0WvxslHU8gbiL0JIsqCyo4LL8s4TW5Nj07DTt8PV9HBkkTSVFJsUuuTLNQMFJTVL1WY1b1XEBca13yX2pirGNDbAhsC2yMbRlwUnBWdZ51_X3jf8aESIRvhq6Hy4ngi3qOio_6kBuTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-vorBxcM6w23K088V0B_Q0NRC1bnXz9h22VvbbtvU2-ffiuL35ljpAuq662PsGO-I7-vw1fGK8uH2_PfX-Tc%3D",
                RuOccultist);

            yield return NormalizationTestData.CreateUnchanged(
                "https://pathofexile.com/passive-skill-tree/AAAABAAAAA==");
            yield return NormalizationTestData.CreateUnchanged(
                "http://unsupported.com");
        }

        [Test]
        public void NormalizeAsyncFailsOnInvalidGoogleLink()
        {
            var treeUrl = "https://google.com/url?ust=1456857460554000&usg=AFQjCNH6POtjRXIVzd_kSRbH7sOVYuZW7A";
            var sut = CreateBuildUrlNormalizer();

            Assert.Throws<ArgumentException>(() => sut.NormalizeAsync(treeUrl).GetAwaiter().GetResult(),
                "The URL doesn't contain required query parameter 'q'.");
        }

        [Test]
        public async Task NormalizeAsyncProvidesMessageToLoadingWrapper()
        {
            var data = NormalizationTestData.CreateShortened(
                "http://poeurl.com/0dE",
                "http://poeurl.com/redirect.php?url=0dE",
                Ascendant);
            var sut = data.CreateNormalizer();
            string? message = null;

            Task<HttpResponseMessage> LoadingWrapper(string msg, Task<HttpResponseMessage> task)
            {
                message = msg;
                return task;
            }

            var actualUrl = await sut.NormalizeAsync(data.OriginalUrl, LoadingWrapper);

            Assert.AreEqual(data.ExpectedUrl, actualUrl);
            Assert.AreEqual("Resolving shortened tree address", message);
        }

        private static BuildUrlNormalizer CreateBuildUrlNormalizer(params (string, string)[] requests)
        {
            var requestDict = requests.ToDictionary();
            return new BuildUrlNormalizer(GetResponse);

            Task<HttpResponseMessage> GetResponse(string uri, HttpCompletionOption _)
            {
                if (!requestDict.TryGetValue(uri, out var outputUri))
                    Assert.Fail($"Unexpected request: {uri}");

                var response = new HttpResponseMessage
                {
                    ReasonPhrase = "",
                    StatusCode = HttpStatusCode.OK,
                    RequestMessage = new HttpRequestMessage(HttpMethod.Get, outputUri),
                };
                return Task.FromResult(response);
            }
        }

        public class NormalizationTestData
        {
            private NormalizationTestData(string originalUrl, string expectedUrl, params (string, string)[] requests)
            {
                OriginalUrl = originalUrl;
                ExpectedUrl = expectedUrl;
                Requests = requests;
            }

            public string OriginalUrl { get; }
            public string ExpectedUrl { get; }
            private IReadOnlyList<(string requestUrl, string responseUrl)> Requests { get; }

            public static NormalizationTestData Create(string originalUrl, string expectedUrl)
                => new NormalizationTestData(originalUrl, expectedUrl);

            public static NormalizationTestData CreateShortened(
                string originalUrl, string canonicalUrl, string resolvedUrl, string expectedUrl)
                => new NormalizationTestData(originalUrl, expectedUrl, (canonicalUrl, resolvedUrl));

            public static NormalizationTestData CreateShortened(
                string originalUrl, string canonicalUrl, string expectedUrl)
                => new NormalizationTestData(originalUrl, expectedUrl, (canonicalUrl, expectedUrl));

            public static NormalizationTestData CreateTwiceShortened(
                string firstOriginalUrl, string firstCanonicalUrl, string secondOriginalUrl, string secondCanonicalUrl,
                string expectedUrl)
                => new NormalizationTestData(firstOriginalUrl, expectedUrl,
                    (firstCanonicalUrl, secondOriginalUrl), (secondCanonicalUrl, expectedUrl));

            public static NormalizationTestData CreateUnchanged(string originalUrl)
                => new NormalizationTestData(originalUrl, originalUrl);

            public BuildUrlNormalizer CreateNormalizer()
                => CreateBuildUrlNormalizer(Requests.ToArray());
        }
    }
}