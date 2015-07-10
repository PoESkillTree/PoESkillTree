using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://github.com/EmmittJ/PoESkillTree/l10n", "POESKillTree.Localization.XAML")]
[assembly: XmlnsPrefix("http://github.com/EmmittJ/PoESkillTree/l10n", "l")]
namespace POESKillTree.Localization.XAML
{
    /* Usage:
     * 1) XAML document root element must have XML namespace defined to enable use of markup extension.
     *    e.g.: <controls:MetroWindow ... xmlns:l="clr-namespace:POESKillTree.Localization.XAML" ...>
     * 
     * 2) Examples of localized messages in XAML document:
     *    <Label Grid.Row="1" Grid.Column="0">
     *        <l:Catalog Message="Some label"/>
     *    </Label>
     *    
     *    <MenuItem Click="Menu_Exit">
     *        <MenuItem.Header>  <!-- Note the object format used instead of property format for Header property -->
     *            <l:Catalog Message="E_xit" />
     *        </MenuItem.Header>
     *        ...
     *    </MenuItem>
     */
    [ContentProperty("Message")]
    [MarkupExtensionReturnType(typeof(string))]
    public class Catalog : MarkupExtension
    {
        /// <summary>
        /// empty contructor is required for specifying all properties manually
        /// </summary>
        public Catalog()
        {

        }

        public Catalog(string message)
        {
            this.Message = message;
        }

        // The translation context.
        public string Context { get; set; }
        // The message to translate.
        public string Message { get; set; }
        // The 'n' value to dertermine plural form.
        public uint N { get; set; }
        // The plural message.
        public string Plural { get; set; }

        // Returns translated message.
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Message == null) throw new ArgumentException("Missing Message property");

            if (Plural != null)
                return L10n.Plural(Message, Plural, N, Context);
            else
                return L10n.Message(Message, Context);
        }
    }
}
