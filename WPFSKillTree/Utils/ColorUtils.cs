using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Media;

namespace POESKillTree.Utils.Converter
{
    public class ColorUtils
    {
        public class NamedColor
        {
            public Color Color;

            public string Name;
        }

        public static List<NamedColor> StandardColors = new List<NamedColor>
        {
            new NamedColor { Color = Colors.Black, Name = "Black" },
            new NamedColor { Color = Colors.Gray, Name = "Gray" },
            new NamedColor { Color = Colors.DarkRed, Name = "Dark Red" },
            new NamedColor { Color = Colors.Red, Name = "Red" },
            new NamedColor { Color = Colors.Orange, Name = "Orange" },
            new NamedColor { Color = Colors.Yellow, Name = "Yellow" },
            new NamedColor { Color = Colors.Green, Name = "Green" },
            new NamedColor { Color = Colors.Turquoise, Name = "Turquoise" },
            new NamedColor { Color = Colors.Indigo, Name = "Indigo" },
            new NamedColor { Color = Colors.Purple, Name = "Purple" },
            new NamedColor { Color = Colors.White, Name = "White" },
            new NamedColor { Color = Colors.LightGray, Name = "Light Gray" },
            new NamedColor { Color = Colors.Brown, Name = "Brown" },
            new NamedColor { Color = Colors.MistyRose, Name = "Misty Rose" },
            new NamedColor { Color = Colors.Gold, Name = "Gold" },
            new NamedColor { Color = Colors.LightYellow, Name = "Light Yellow" },
            new NamedColor { Color = Colors.Lime, Name = "Lime" },
            new NamedColor { Color = Colors.PaleTurquoise, Name = "Pale Turquoise" },
            new NamedColor { Color = Colors.CornflowerBlue, Name = "Cornflower Blue" },
            new NamedColor { Color = Colors.Lavender, Name = "Lavender" }
        };

        public static Color FromRgbString(string str)
        {
            if (str.StartsWith("#"))
            {
                if (str.Length == 7) // #RRGGBB
                {
                    uint rgb;
                    if (uint.TryParse(str.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out rgb))
                    {
                        byte[] component = BitConverter.GetBytes(rgb);

                        return Color.FromArgb(255, component[2], component[1], component[0]);
                    }
                }
                else if (str.Length == 9) // #AARRGGBB
                {
                    uint argb;
                    if (uint.TryParse(str.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out argb))
                    {
                        byte[] component = BitConverter.GetBytes(argb);

                        return Color.FromArgb(component[3], component[2], component[1], component[0]);
                    }
                }
            }

            throw new ArgumentException("Invalid RGB color string: " + str);
        }

        public static bool IsStandardColor(Color color, bool ignoreAlpha = false)
        {
            foreach (NamedColor nc in StandardColors)
                if (ignoreAlpha ? nc.Color.R == color.R && nc.Color.G == color.G && nc.Color.B == color.B : nc.Color == color)
                    return true;

            return false;
        }
    }
}
