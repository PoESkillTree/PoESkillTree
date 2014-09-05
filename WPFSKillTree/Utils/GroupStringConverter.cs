using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace POESKillTree.Utils
{
    [ValueConversion(typeof (string), typeof (string))]
    public class GroupStringConverter : IValueConverter
    {
        public static List<string[]> Groups = new List<string[]>
        {
            new[] {"all attrib", "BaseStats"},
            new[] {"dex", "BaseStats"},
            new[] {"intel", "BaseStats"},
            new[] {"stre", "BaseStats"},
            new[] {"armour", "Defense"},
            new[] {"defence", "Defense"},
            new[] {"evasi", "Defense"},
            new[] {"life", "Defense"},
            new[] {"move", "Defense"},
            new[] {"resist", "Defense"},
            new[] {"shield", "Defense"},
            new[] {"charge", "Charge"},
            new[] {"area", "Spell"},
            new[] {"buff", "Spell"},
            new[] {"cast", "Spell"},
            new[] {"mana", "Spell"},
            new[] {"spell", "Spell"},
            new[] {"accur", "Weapon"},
            new[] {"attack", "Weapon"},
            new[] {"axe", "Weapon"},
            new[] {"bow", "Weapon"},
            new[] {"claw", "Weapon"},
            new[] {"dagg", "Weapon"},
            new[] {"dual wiel", "Weapon"},
            new[] {"mace", "Weapon"},
            new[] {"melee phys", "Weapon"},
            new[] {"physical dam", "Weapon"},
            new[] {"pierc", "Weapon"},
            new[] {"proj", "Weapon"},
            new[] {"staff", "Weapon"},
            new[] {"stav", "Weapon"},
            new[] {"wand", "Weapon"},
            new[] {"weapon", "Weapon"},
            new[] {"crit", "Crit"},
            new[] {"trap", "Traps"},
            new[] {"spectre", "Minion"},
            new[] {"skele", "Minion"},
            new[] {"zombie", "Minion"},
            new[] {"minio", "Minion"},
        };

        static GroupStringConverter()
        {
            if (!File.Exists("groups.txt"))
                return;
            Groups.Clear();
            foreach (string s in File.ReadAllLines("groups.txt"))
            {
                string[] sa = s.Split(',');
                Groups.Add(sa);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value.ToString();
            foreach (var gp in Groups)
            {
                if (s.ToLower().Contains(gp[0].ToLower()))
                {
                    return gp[1];
                }
            }
            return "Everything else";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class StatisticsGroupStringConverter : IValueConverter
    {
        public static List<string[]> Groups = new List<string[]>
        {
            new []{ "life:", "Character" },
            new []{ "mana:", "Character" },
            new []{ "maximum energy shield:", "Character" },
            new []{ "strength", "Character" },
            new []{ "dexterity", "Character" },
            new []{ "intelligence", "Character" },

            new []{ "armour", "Defence" },
            new []{ "physical damage reduction", "Defence" },
            new []{ "evasion rating", "Defence" },
            new []{ "chance to evade attacks", "Defence" },
            new []{ "recharge per second", "Defence" },
            new []{ "regeneration per second", "Defence" },
            new []{ "energy shield recharge", "Defence" },
            new []{ "resistance", "Defence" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value.ToString();

            foreach (var gp in Groups)
            {
                if (s.ToLower().Contains(gp[0].ToLower())) return gp[1];
            }

            return "Miscellaneous";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}