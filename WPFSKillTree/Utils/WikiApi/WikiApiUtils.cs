using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Utils.WikiApi
{
    public static class WikiApiUtils
    {
        /// <summary>
        /// The factor by which item images from the Wiki have to be resized to fit into the inventory/stash slots.
        /// </summary>
        private const double ItemImageResizeFactor = 0.6;

        /// <summary>
        /// Saves the image to a file.
        /// </summary>
        /// <param name="imageData">the binary image data</param>
        /// <param name="fileName">the file to save the image to</param>
        /// <param name="resize">true iff the image is from the wiki and should be resized to match the stash</param>
        public static void SaveImage(byte[] imageData, string fileName, bool resize)
        {
            using (var outputStream = File.Create(fileName))
            using (var ms = new MemoryStream(imageData))
            using (var image = Image.FromStream(ms))
            {
                var resized = image;
                if (resize)
                {
                    var newWidth = (int) Math.Ceiling(image.Width * ItemImageResizeFactor);
                    var newHeight = (int) Math.Ceiling(image.Height * ItemImageResizeFactor);
                    resized = image.Resize(newWidth, newHeight);
                }
                resized.Save(outputStream, ImageFormat.Png);
            }
        }
    }
}