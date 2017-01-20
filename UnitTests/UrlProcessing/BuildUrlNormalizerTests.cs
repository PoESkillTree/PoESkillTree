using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Utils.UrlProcessing;

namespace UnitTests.UrlProcessing
{
    [TestClass]
    public class BuildUrlNormalizerTests
    {
        [TestMethod]
        public async Task NormalizeAsyncExtractsUrlFromValidGoogleLinkTest()
        {
            // #267
            var targetUrl = "https://www.google.com/url?q=http://poeurl.com/xer&sa=D&ust=1456857460554000&usg=AFQjCNH6POtjRXIVzd_kSRbH7sOVYuZW7A";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAAAwYAsNjjdaLZMjIjNsgUEJJTEPVv2YZqjLxvFr8mlWHiVUuMNonTcFIqC13yKwoOSBEvAx7jhBGW62MfQQ3RBx5_xrVINj1jQ5UuoqOTJ52q8NXB8--IbIxJUZUg-TdirGsXLR_Xz7TFvOpMs0mxwuzAVAQHIG6usypbBbUI9GpD6-SD21AwQZZ59ujWjb9fKlXG217Q9UM2IuqbJlFH1CM6WFLswzoyAedUN9QwfDBxGYqbtSo4shkk_e0_jX3dqJcGhMXPev_eI_ZLeMSiYetNkjpCu-N6U_66fXXTfgBeGY7tgwgu";

            Func<string, Task, Task> loadingWrapper = (msg, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(targetUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromParameterlessValidGoogleLinkTest()
        {
            // #267
            var treeUrl = "https://www.google.com/url?q=http://poeurl.com/xer";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAAAwYAsNjjdaLZMjIjNsgUEJJTEPVv2YZqjLxvFr8mlWHiVUuMNonTcFIqC13yKwoOSBEvAx7jhBGW62MfQQ3RBx5_xrVINj1jQ5UuoqOTJ52q8NXB8--IbIxJUZUg-TdirGsXLR_Xz7TFvOpMs0mxwuzAVAQHIG6usypbBbUI9GpD6-SD21AwQZZ59ujWjb9fKlXG217Q9UM2IuqbJlFH1CM6WFLswzoyAedUN9QwfDBxGYqbtSo4shkk_e0_jX3dqJcGhMXPev_eI_ZLeMSiYetNkjpCu-N6U_66fXXTfgBeGY7tgwgu";

            Func<string, Task, Task> loadingWrapper = (msg, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(treeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromShortenedGoogleLinkTest()
        {
            var treeUrl = "goo.gl/44tsqv";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAAAwYAsNjjdaLZMjIjNsgUEJJTEPVv2YZqjLxvFr8mlWHiVUuMNonTcFIqC13yKwoOSBEvAx7jhBGW62MfQQ3RBx5_xrVINj1jQ5UuoqOTJ52q8NXB8--IbIxJUZUg-TdirGsXLR_Xz7TFvOpMs0mxwuzAVAQHIG6usypbBbUI9GpD6-SD21AwQZZ59ujWjb9fKlXG217Q9UM2IuqbJlFH1CM6WFLswzoyAedUN9QwfDBxGYqbtSo4shkk_e0_jX3dqJcGhMXPev_eI_ZLeMSiYetNkjpCu-N6U_66fXXTfgBeGY7tgwgu";

            Func<string, Task, Task> loadingWrapper = (msg, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(treeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncFailsOnInvalidGoogleLinkTest()
        {
            // #267
            var treeUrl = "https://www.google.com/url?ust=1456857460554000&usg=AFQjCNH6POtjRXIVzd_kSRbH7sOVYuZW7A";

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
            Assert.AreEqual("The URL you are trying to load is invalid.", exception.Message);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromValidTinyurlLinkTest()
        {
            var tinyurlTreeUrl = "http://tinyurl.com/glcmaeo";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(tinyurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromValidPoeurlLinkTest()
        {
            var poeurlTreeUrl = "http://poeurl.com/0dE";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromValidPoeurlLinkWithRedirectTest()
        {
            var poeurlTreeUrl = "http://poeurl.com/redirect.php?url=0dE";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromValidPoeurlLinkWithoutProtocolTest()
        {
            var poeurlTreeUrl = "www.poeurl.com/0dE";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromValidPoeurlLinkWithoutWwwPrefixTest()
        {
            var poeurlTreeUrl = "poeurl.com/0dE";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncLoadsFromTwiceShortenedLinkTest()
        {
            var poeurlTreeUrl = "https://goo.gl/kKjcuK"; // "poeurl.com/0dE"
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncProvidesMessageToLoadingWrapperTest()
        {
            var poeurlTreeUrl = "http://poeurl.com/0dE";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

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
            var treeUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAAAAA==";

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(treeUrl, null);

            Assert.AreEqual(treeUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeAsyncSkipsUnsupportedLinkTest()
        {
            var treeUrl = "http://www.unsupported.com";

            var actualUrl = await CreateBuildUrlNormalizer().NormalizeAsync(treeUrl, null);

            Assert.AreEqual(treeUrl, actualUrl);
        }

        [TestMethod]
        public void EnsureProtocolCompletesKnownUrlsTest()
        {
            var targetPath = "/abc";
            var completions = new Dictionary<string, string>
            {
                { "goo.gl", "https://www.goo.gl" },
                { "poeurl.com", "http://www.poeurl.com" },
                { "tinyurl.com", "https://www.tinyurl.com" },
                { "pathofexile.com", "https://www.pathofexile.com" }
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
    }
}
