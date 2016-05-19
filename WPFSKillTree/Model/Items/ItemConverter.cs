using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using POESKillTree.Utils;

namespace POESKillTree.Model.Items
{
    public class TextBlockHelper
    {
        public static readonly DependencyProperty TextBlockInlinesProperty = DependencyProperty.RegisterAttached(
            "TextBlockInlines",
            typeof(IEnumerable<Inline>),
            typeof(TextBlockHelper),
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
        private static readonly SolidColorBrush LocallyAffectedColor = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0xFF));
        private static readonly SolidColorBrush FireAffectedColor = new SolidColorBrush(Color.FromRgb(0x96, 0x00, 0x04));
        private static readonly SolidColorBrush ColdAffectedColor = new SolidColorBrush(Color.FromRgb(0x36, 0x64, 0x92));
        private static readonly SolidColorBrush LightningAffectedColor = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00));
        private static readonly SolidColorBrush ChaosAffectedColor = new SolidColorBrush(Color.FromRgb(0xD0, 0x20, 0x90));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mod = value as ItemMod;
            if (mod == null)
                throw new NotSupportedException();

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
                var prefix = "";
                if (parameter != null && !string.IsNullOrEmpty(istring) && istring.Last() == '+')
                {
                    istring = istring.Substring(0, istring.Length - 1);
                    prefix = "+";
                }
                r = new Run(istring);

                SolidColorBrush clr = GetColoringFor(mod, i);

                if ((istring == "-" || istring == "/" || istring == "+") && parameter != null)
                    r.Foreground = clr;

                if (parameter != null && !string.IsNullOrEmpty(istring) && istring[0] == '%')
                    r.Foreground = GetColoringFor(mod, i - 1);

                inlines.Add(r);

                r = new Run(prefix + mod.Value[i].ToString("###0.##"));
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
                        return LocallyAffectedColor;
                    case ItemMod.ValueColoring.Fire:
                        return FireAffectedColor;
                    case ItemMod.ValueColoring.Cold:
                        return ColdAffectedColor;
                    case ItemMod.ValueColoring.Lightning:
                        return LightningAffectedColor;
                    case ItemMod.ValueColoring.Chaos:
                        return ChaosAffectedColor;
                }

            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }


    [ValueConversion(typeof(string), typeof(ImageSource))]
    class ItemTypeToImageConverter : IValueConverter
    {
        private static readonly Dictionary<string, BitmapImage> TypeCache = new Dictionary<string, BitmapImage>();

        protected BitmapImage GetImageForBase(string ibase)
        {
            lock (TypeCache)
            {
                BitmapImage img;
                if (!TypeCache.TryGetValue(ibase, out img))
                {
                    //default
                    var imgfile = new FileInfo(Path.Combine(AppData.GetFolder(@"Data\Equipment\Assets"), ibase + ".png"));

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


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itm = value as string;
            if (itm != null)
            {
                return GetImageForBase(itm);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(Item), typeof(ImageSource))]
    class ItemToImageConverter : ItemTypeToImageConverter, IValueConverter
    {
        private static readonly Dictionary<ItemGroup, BitmapImage> DefaultCache = new Dictionary<ItemGroup, BitmapImage>();

        public new object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var itm = value as Item;
            BitmapImage img = null;
            if (itm != null)
            {
                img = GetImageForBase(itm.BaseType.Name);
                if (img != null)
                    return img;

                lock (DefaultCache) //i am not entirely sure that WPF isnt using converter concurently
                {
                    if (!DefaultCache.TryGetValue(itm.ItemGroup, out img))
                    {
                        img = new BitmapImage();

                        //default

                        img.BeginInit();
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        var bu = BaseUriHelper.GetBaseUri(Application.Current.MainWindow);
                        var bus = bu.ToString();
                        bus = bus.Substring(0, bus.IndexOf(";component/") + 11);
                        string image = "Images/EquipmentUI/ItemDefaults/" + itm.ItemGroup + ".png";
                        Uri u = new Uri(bus + image);

                        if (!ResourceExists(image))
                            return null;

                        img.UriSource = u;
                        img.EndInit();

                        DefaultCache.Add(itm.ItemGroup, img);
                    }
                }
            }
            return img;
        }


        private static bool ResourceExists(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return ResourceExists(assembly, resourcePath);
        }

        private static bool ResourceExists(Assembly assembly, string resourcePath)
        {
            if (_resourceKeyCache == null)
                _resourceKeyCache = GetResourcePaths(assembly).Select(o => o as string).Where(s => s != null).ToList();

            return _resourceKeyCache.Contains(resourcePath.ToLower());
        }

        private static List<string> _resourceKeyCache;

        private static IEnumerable<object> GetResourcePaths(Assembly assembly)
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            var resourceName = assembly.GetName().Name + ".g";
            var resourceManager = new ResourceManager(resourceName, assembly);

            try
            {
                var resourceSet = resourceManager.GetResourceSet(culture, true, true);

                foreach (DictionaryEntry resource in resourceSet)
                {
                    yield return resource.Key;
                }
            }
            finally
            {
                resourceManager.ReleaseAllResources();
            }
        }

        public new object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
