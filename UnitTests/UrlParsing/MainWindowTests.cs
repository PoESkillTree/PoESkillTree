using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.Views;

namespace UnitTests.UrlParsing
{
    [TestClass]
    public class MainWindowTests
    {
        [TestMethod]
        public async Task NormalizeBuildUrlAsyncExtractsUrlFromValidGoogleLinkTest()
        {
            // #267
            var targetUrl = "https://www.google.com/url?q=http://poeurl.com/xer&sa=D&ust=1456857460554000&usg=AFQjCNH6POtjRXIVzd_kSRbH7sOVYuZW7A";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAAAwYAsNjjdaLZMjIjNsgUEJJTEPVv2YZqjLxvFr8mlWHiVUuMNonTcFIqC13yKwoOSBEvAx7jhBGW62MfQQ3RBx5_xrVINj1jQ5UuoqOTJ52q8NXB8--IbIxJUZUg-TdirGsXLR_Xz7TFvOpMs0mxwuzAVAQHIG6usypbBbUI9GpD6-SD21AwQZZ59ujWjb9fKlXG217Q9UM2IuqbJlFH1CM6WFLswzoyAedUN9QwfDBxGYqbtSo4shkk_e0_jX3dqJcGhMXPev_eI_ZLeMSiYetNkjpCu-N6U_66fXXTfgBeGY7tgwgu";

            Func<string, Task, Task> loadingWrapper = (msg, task) => task;

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(targetUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        [Ignore]
        public async Task NormalizeBuildUrlAsyncLoadsFromShortValidGoogleLinkTest()
        {
            // TODO: Fix. Provided url contains valid build link.

            var treeUrl = "https://www.google.com/url?q=http://poeurl.com/xer";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(treeUrl, null);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeBuildUrlAsyncFailsOnInvalidGoogleLinkTest()
        {
            var treeUrl = "https://www.google.com/url?ust=1456857460554000&usg=AFQjCNH6POtjRXIVzd_kSRbH7sOVYuZW7A";

            Exception exception = null;
            try
            {
                await CreateMainWindow().NormalizeBuildUrlAsync(treeUrl, null);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Assert.IsNotNull(exception);
            Assert.AreEqual("The URL you are trying to load is invalid.", exception.Message);
        }

        [TestMethod]
        public async Task NormalizeBuildUrlAsyncLoadsFromValidTinyurlLinkTest()
        {
            var tinyurlTreeUrl = "http://tinyurl.com/glcmaeo";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(tinyurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeBuildUrlAsyncLoadsFromValidPoeurlLinkTest()
        {
            var poeurlTreeUrl = "http://poeurl.com/0dE";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeBuildUrlAsyncLoadsFromValidPoeurlLinkWithRedirectTest()
        {
            var poeurlTreeUrl = "http://poeurl.com/redirect.php?url=0dE";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        [Ignore]
        public async Task NormalizeBuildUrlAsyncLoadsFromValidPoeurlLinkWithoutProtocolTest()
        {
            /*
             * TODO: Implement. Poeurl provides urls with no protocol relying on browsers.
             * It would be nice to autocomplete urls with appropriate protocols.
             */

            var poeurlTreeUrl = "poeurl.com/0dE";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            Func<string, Task, Task> loadingWrapper = (url, task) => task;

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeBuildUrlAsyncProvidesMessageToLoadingWrapperTest()
        {
            var poeurlTreeUrl = "http://poeurl.com/0dE";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAMBAAFvDXwOSA-rD8QRDxEvEVARlhV-FdcV7Ra_GkgbJR1PIG4i9CSLKgsqOCy_LOE1uTY9Ow07fD1fRwZJE0lRSbFLrkyzUDBSU1S9VmNW9VxAXGtd8l9qYqxjQ2nYbAhsC2yMbRlwUnBWdZ51_XzwfeN-oX_GhEiEb4auh8uJ4It6joqP-pAbkQeTH5MnlouXlZfQl_SaE52qoS-io6crpzSnm6xmrJi0DLTFtUi4yrk-u_y-isHFwzrDbcrTzxXQH9DQ1ELVudfP2HbZW9tu29Tb59-K4vfmWOkC6rrrY-wY74jv6_DV8Yry4fQo9vz31_k3";

            string message = null;

            Func<string, Task, Task> loadingWrapper = (msg, task) =>
            {
                message = msg;
                return task;
            };

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(poeurlTreeUrl, loadingWrapper);

            Assert.AreEqual(expectedUrl, actualUrl);
            Assert.AreEqual("Resolving shortened tree address", message);
        }

        [TestMethod]
        public async Task NormalizeBuildUrlAsyncSkipsReadyLinkTest()
        {
            var treeUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAAAAA==";

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(treeUrl, null);

            Assert.AreEqual(treeUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeBuildUrlAsyncSkipsUnsupportedLinkTest()
        {
            var treeUrl = "http://www.unsupported.com";

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(treeUrl, null);

            Assert.AreEqual(treeUrl, actualUrl);
        }

        [TestMethod]
        public async Task NormalizeBuildUrlAsyncRemovesSpecialQueryParametersTest()
        {
            var targetUrl = "http://www.pathofexile.com/passive-skill-tree/AAAABAAAAA==?characterName=Character&accountName=Account";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAAAAA==";

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(targetUrl, null);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        [TestMethod]
        [Ignore]
        public async Task NormalizeBuildUrlAsyncRemovesUnsupportedQueryParametersTest()
        {
            // TODO: Fix. Remove all query parameters

            var targetUrl = "http://www.pathofexile.com/passive-skill-tree/AAAABAAAAA==?unsupported=FAIL";
            var expectedUrl = "https://www.pathofexile.com/passive-skill-tree/AAAABAAAAA==";

            var actualUrl = await CreateMainWindow().NormalizeBuildUrlAsync(targetUrl, null);

            Assert.AreEqual(expectedUrl, actualUrl);
        }

        private MainWindow CreateMainWindow()
        {
            return (MainWindow)FormatterServices.GetUninitializedObject(typeof(MainWindow));
        }
    }
}
