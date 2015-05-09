using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace POESKillTree.Utils
{
    public static class Imaging
    {
        public static BitmapSource BitmapToBitmapSourceConverter(Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("Bitmap");
            }

            using (var str = new MemoryStream())
            {
                bitmap.Save(str, ImageFormat.Bmp);
                try
                {
                    str.Seek(0, SeekOrigin.Begin);
                    BitmapDecoder bdc = new BmpBitmapDecoder(str, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                    return bdc.Frames[0];
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}
