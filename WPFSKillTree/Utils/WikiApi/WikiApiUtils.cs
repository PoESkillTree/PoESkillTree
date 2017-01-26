using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Newtonsoft.Json.Linq;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Utils.WikiApi
{
    public static class WikiApiUtils
    {
        /// <summary>
        /// The factor by which item images from the Wiki have to be resized to fit into the inventory/stash slots.
        /// </summary>
        private const double ItemImageResizeFactor = 0.6;

        public static T SingularValue<T>(JToken printouts, string rdfPredicate)
        {
            return printouts[rdfPredicate].First.Value<T>();
        }

        public static T SingularValue<T>(JToken printouts, string rdfPredicate, T defaultValue)
        {
            var token = printouts[rdfPredicate];
            return token.HasValues ? token.First.Value<T>() : defaultValue;
        }

        public static IEnumerable<T> PluralValue<T>(JToken printouts, string rdfPredicate)
        {
            return printouts[rdfPredicate].Values<T>();
        }

        public static void ResizeAndSaveImage(byte[] imageData, Stream outputStream)
        {
            using (var ms = new MemoryStream(imageData))
            using (var image = Image.FromStream(ms))
            {
                var resized = image.Resize((int)(image.Width * ItemImageResizeFactor),
                    (int)(image.Height * ItemImageResizeFactor));
                resized.Save(outputStream, ImageFormat.Png);
            }
        }
    }
}