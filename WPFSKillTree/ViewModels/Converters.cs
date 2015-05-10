using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.IO;
using POESKillTree.ViewModels.Items;

namespace POESKillTree.ViewModels
{
    class AttributeToTextblockConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var attr = value as Attribute;
            if (attr == null)
                throw new NotImplementedException();

            var tb = new TextBlock() { TextWrapping = TextWrapping.Wrap };
            var txt = new Run(attr.Text);
            if (attr.Missing)
                txt.Foreground = Brushes.Red;

            tb.Inlines.Add(txt);

            foreach (var i in attr.Deltas)
            {
                if (i != 0)
                {
                    tb.Inlines.Add(" ");
                    txt = new Run(i.ToString("+#;-#;0"));
                    txt.Foreground = (i < 0) ? Brushes.Red : Brushes.Green;
                    tb.Inlines.Add(txt);
                }
            }

            return tb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class DebugViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }

    public class Attached
    {
        public static readonly DependencyProperty TextBlockInlinesProperty = DependencyProperty.RegisterAttached(
            "TextBlockInlines",
            typeof(IEnumerable<Inline>),
            typeof(Attached),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, TextboxInlinesPropertyChanged));


        public static IEnumerable<Inline> GetTextBlockInlines(TextBlock textBlock)
        {
            return textBlock.Inlines.ToList();
        }

        public static void SetTextBlockInlines(TextBlock textBlock, IEnumerable<Inline> value)
        {
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange(value);
        }

        private static void TextboxInlinesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = d as TextBlock;
            if (textBlock == null)
                return;
            SetTextBlockInlines(textBlock, e.NewValue as IEnumerable<Inline>);
        }
    }

    [ValueConversion(typeof(ItemMod), typeof(IEnumerable<Inline>))]
    class ItemModToInlinesConverter : IValueConverter
    {
        private static SolidColorBrush locallyAffectedColor = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0xFF));
        private static SolidColorBrush fireAffectedColor = new SolidColorBrush(Color.FromRgb(0x96, 0x00, 0x04));
        private static SolidColorBrush coldAffectedColor = new SolidColorBrush(Color.FromRgb(0x36, 0x64, 0x92));
        private static SolidColorBrush lightningAffectedColor = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mod = value as ItemMod;
            if (mod == null)
                throw new NotImplementedException();

            var inlines = new List<Inline>();

            var backrep = ItemAttributes.Attribute.Backreplace;

            var matches = backrep.Matches(mod.Attribute).Cast<Match>().ToArray();
            int from = 0;
            string istring;
            Run r;
            for (int i = 0; i < matches.Length && i < mod.Value.Count; i++)
            {
                var m = matches[i];
                istring = mod.Attribute.Substring(from, m.Index - from);
                r = new Run(istring);

                SolidColorBrush clr = GetColoringFor(mod, i);

                if ((istring == "-" || istring == "/" || istring == "+") && parameter != null)
                    r.Foreground = clr;

                if (parameter != null && !string.IsNullOrEmpty(istring) && istring[0] == '%')
                    r.Foreground = GetColoringFor(mod, i - 1);

                inlines.Add(r);

                r = new Run(mod.Value[i].ToString("###0.#"));
                if (parameter != null)
                    r.Foreground = clr;

                inlines.Add(r);

                from = m.Index + m.Length;
            }

            istring = mod.Attribute.Substring(from, mod.Attribute.Length - from);
            r = new Run(istring);
            if (parameter != null && !string.IsNullOrEmpty(istring) && istring[0] == '%')
                r.Foreground = GetColoringFor(mod, matches.Length - 1);
            inlines.Add(r);


            return inlines;
        }

        private static SolidColorBrush GetColoringFor(ItemMod mod, int i)
        {
            if (mod.ValueColor.Count > i && i >= 0)
                switch (mod.ValueColor[i])
                {
                    case ItemMod.ValueColoring.LocallyAffected:
                        return locallyAffectedColor;
                    case ItemMod.ValueColoring.Fire:
                        return fireAffectedColor;
                    case ItemMod.ValueColoring.Cold:
                        return coldAffectedColor;
                    case ItemMod.ValueColoring.Lightning:
                        return lightningAffectedColor;
                }

            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }





    [ValueConversion(typeof(string), typeof(ImageSource))]
    class ItemTypeToImageConverter : IValueConverter
    {
        protected static Dictionary<string, BitmapImage> TypeCache = new Dictionary<string, BitmapImage>();
        protected BitmapImage GetImageForBase(string ibase)
        {
            BitmapImage img;
            lock (TypeCache)
            {
                if (!TypeCache.TryGetValue(ibase, out img))
                {
                    //default
                    var imgfile = new FileInfo(Path.Combine("./Data/Equipment/Assets/", ibase + ".png"));

                    if (imgfile.Exists)
                    {

                        img = new BitmapImage();
                        img.BeginInit();
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.UriSource = new Uri(imgfile.FullName, UriKind.Absolute);
                        img.EndInit();

                        TypeCache.Add(ibase, img);
                        return img;
                    }
                }
                else
                    return img;
            }

            return null;
        }


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var itm = value as string;
            BitmapImage img = null;
            if (itm != null)
            {
                return GetImageForBase(itm);
            }
            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

    [ValueConversion(typeof(Item), typeof(ImageSource))]
    class ItemToImageConverter : ItemTypeToImageConverter, IValueConverter
    {
        protected static Dictionary<ItemClass, BitmapImage> DefaultCache = new Dictionary<ItemClass, BitmapImage>();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var itm = value as Item;
            BitmapImage img = null;
            if (itm != null)
            {
                img = GetImageForBase(itm.BaseType);
                if (img != null)
                    return img;

                lock (DefaultCache)//i am not entirely sure that WPF isnt using converter concurently
                {
                    if (!DefaultCache.TryGetValue(itm.Class, out img))
                    {
                        img = new BitmapImage();

                        //default

                        img.BeginInit();
                        img.UriSource = new Uri("/images/EquipmentUI/ItemDefaults/" + itm.Class + ".png", UriKind.Relative);
                        img.EndInit();

                        DefaultCache.Add(itm.Class, img);
                    }
                }

            }
            return img;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
