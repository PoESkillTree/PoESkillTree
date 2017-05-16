using System;
using System.Collections.Generic;
using System.Linq;
using MB.Algodat;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Model.Items.Mods;

namespace POESKillTree.Model.Items.Affixes
{
    public class Stat
    {
        public string Name { get; }

        public IReadOnlyList<Range<float>> Ranges { get; }

        private readonly ItemType _itemType;

        private readonly ModGroup _modGroup;
        private readonly int _level;

        public Stat(Stat stat)
        {
            Name = stat.Name;
            Ranges = stat.Ranges;
            _itemType = stat._itemType;
            _modGroup = stat._modGroup;
            _level = stat._level;
        }

        private Stat(string name, ItemType itemType, ModGroup modGroup, Range<float> range)
        {
            Name = name;
            _itemType = itemType;
            _modGroup = modGroup;
            Ranges = new[] { range };
        }

        public static Stat CreateProperty(string name, Range<float> range)
        {
            return new Stat(name, ItemType.Unknown, ModGroup.Property, range);
        }

        public ItemMod ToItemMod(IReadOnlyList<float> values)
        {
            if (values.Count != Ranges.Count)
                throw new ArgumentException("There must be one value for each range");
            return ToItemMod(Name, values.ToList());
        }

        private ItemMod ToItemMod(string attribute, List<float> values)
        {
            // replace "+#" by "#" if the value for that placeholder is negative
            var attr = "";
            var parts = attribute.Split('#');
            for (var i = 0; i < values.Count; i++)
            {
                var part = parts[i];
                if (part.EndsWith("+") && values[i] < 0)
                {
                    attr += part.Substring(0, part.Length - 1);
                }
                else
                {
                    attr += part;
                }
                attr += "#";
            }
            attr += parts.Last();
            // todo
            var isLocal = StatLocalityChecker.DetermineLocal((ItemClass) _itemType, _modGroup, attr);
            return new ItemMod(attr, isLocal, _level, values, values.Select(_ => ItemMod.ValueColoring.White));
        }
    }
}