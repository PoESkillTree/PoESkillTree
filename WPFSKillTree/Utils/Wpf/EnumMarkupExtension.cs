using System;
using System.Windows.Markup;
using EnumsNET;

namespace PoESkillTree.Utils.Wpf
{
    public class EnumMarkupExtension : MarkupExtension
    {
        private readonly Type _enumType;

        public EnumMarkupExtension(Type enumType)
        {
            _enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) =>
            Enums.GetValues(_enumType);
    }
}