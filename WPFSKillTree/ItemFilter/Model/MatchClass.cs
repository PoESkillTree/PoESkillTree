using System;
using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.ItemFilter.Model
{
    /*
     * Class:
     * Life Flasks
     * Mana Flasks
     * Hybrid Flasks
     * Currency
     * Amulets
     * Rings
     * Claws
     * Daggers
     * Wands
     * One Hand Swords
     * Thrusting One Hand Swords
     * One Hand Axes
     * One Hand Maces
     * Bows
     * Staves
     * Two Hand Swords
     * Two Hand Axes
     * Two Hand Maces
     * Active Skill Gems
     * Support Skill Gems
     * Quivers
     * Belts
     * Gloves
     * Boots
     * Body Armours
     * Helmets
     * Shields
     * Stackable Currency
     * Quest Items
     * Sceptres
     * Utility Flasks
     * Maps
     * Fishing Rods
     * Map Fragments
     * Divination Card
     * Jewel
     * Trinket
     */
    public class MatchClass : MatchStrings
    {
        /// <summary>
        /// Dictionary of BaseType strings which belong to specific Class identified by string.
        /// </summary>
        private static Dictionary<string,List<string>> BaseTypesOfClass = new Dictionary<string,List<string>>();

        /// <summary>
        /// The BaseType match narrowing this Class match within same rule.
        /// </summary>
        public MatchBaseType NarrowedBy;

        protected MatchClass(MatchClass copy) : base(copy)
        {
            NarrowedBy = copy.NarrowedBy;
        }

        public MatchClass(string[] classes) : base(classes)
        {
            Keyword = "Class";
            Priority = Type.Class;
        }

        public override Match Clone()
        {
            return new MatchClass(this);
        }

        /// <summary>
        /// Determines whether BaseType match is contained in this Class match based on learnt strings.
        /// </summary>
        /// <param name="match">The BaseType match.</param>
        /// <returns>true, if this Class match contains all strings of BaseType match; otherwise false.</returns>
        public bool Contains(MatchBaseType match)
        {
            // Find this Class strings defined in dictionary as whole or partially.
            foreach (string clazz in FindMatched(BaseTypesOfClass.Keys.ToArray()))
            {
                foreach (string baseType in match.Values)
                {
                    if (BaseTypesOfClass[clazz].Exists(bt => bt.Contains(baseType)))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Learn BaseTypes which are subset of this class.
        /// </summary>
        /// <param name="match">The BaseType match to learn BaseTypes from.</param>
        public void Learn(MatchBaseType match)
        {
            // First, to learn BaseType of Class relationship, this Class must have only one string defined.
            if (Values.Count == 1)
            {
                // If this Class has already learnt some BaseType(s), then add only new BaseType strings.
                if (BaseTypesOfClass.ContainsKey(Values[0]))
                    foreach (string str in match.Values)
                    {
                        if (!BaseTypesOfClass[Values[0]].Contains(str))
                            BaseTypesOfClass[Values[0]].Add(str);
                    }
                else // Otherwise create new entry with copy of all BaseType strings.
                    BaseTypesOfClass.Add(Values[0], new List<string>(match.Values));
                    
            }
        }

        /// <summary>
        /// Narrows Class match by BaseType match within same rule.
        /// Also invokes Learn.
        /// </summary>
        /// <param name="match"></param>
        public void NarrowBy(MatchBaseType match)
        {
            NarrowedBy = match;

            Learn(match);
        }
    }
}
