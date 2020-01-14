using System;
using System.Globalization;
using System.Windows.Data;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Localization;

namespace PoESkillTree.Utils.Converter
{
    [ValueConversion(typeof(ItemSlot), typeof(string))]
    public class ItemSlotToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            (value as ItemSlot?) switch
            {
                ItemSlot.Unequipable => L10n.Message("Not equippable"),
                ItemSlot.BodyArmour => L10n.Message("Body Armour"),
                ItemSlot.MainHand => L10n.Message("Main-Hand"),
                ItemSlot.OffHand => L10n.Message("Off-Hand"),
                ItemSlot.Ring => L10n.Message("Ring 1"),
                ItemSlot.Ring2 => L10n.Message("Ring 2"),
                ItemSlot.Amulet => L10n.Message("Amulet"),
                ItemSlot.Helm => L10n.Message("Helmet"),
                ItemSlot.Gloves => L10n.Message("Gloves"),
                ItemSlot.Boots => L10n.Message("Boots"),
                ItemSlot.Belt => L10n.Message("Belt"),
                ItemSlot.Flask1 => L10n.Message("Flask 1"),
                ItemSlot.Flask2 => L10n.Message("Flask 2"),
                ItemSlot.Flask3 => L10n.Message("Flask 3"),
                ItemSlot.Flask4 => L10n.Message("Flask 4"),
                ItemSlot.Flask5 => L10n.Message("Flask 5"),
                ItemSlot.SkillTree => L10n.Message("Passive Tree"),
                null => "",
                _ => throw new ArgumentOutOfRangeException()
            };

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}