using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using POESKillTree.SkillTreeFiles;

namespace UnitTests
{
    [TestClass]
    public class TestDPS
    {
        internal enum Rounding { None, Round, Floor, Ceil };

        internal class Spell
        {
            internal float DisplayDPS;
            internal float MinDamage;
            internal float MaxDamage;
            internal float CPS = 1;
            internal float BaseDPS;
            internal float CC;
            internal float CM;
            internal float DPS;

            internal void DamageRange(RoundingVariant csv, RoundingVariant drv)
            {
                BaseDPS = Round(CPS, csv) * ((Round(MinDamage, drv) + Round(MaxDamage, drv)) / 2);
            }

            internal void Compute(RoundingVariant bv, RoundingVariant ccv, RoundingVariant cmv)
            {
                DPS = Round(BaseDPS, bv) * (1 + Round(CC, ccv) / 100 * (Round(CM, cmv) - 100) / 100);
            }

            internal void Display(RoundingVariant dv)
            {
                DPS = Round(DPS, dv);
            }
        }

        internal class SpellWiki : Spell
        {
            internal float IncCC;
            internal float IncCM;

            new internal void Compute(RoundingVariant bv, RoundingVariant ccv, RoundingVariant cmv)
            {
                DPS = Round(BaseDPS, bv) + Round(BaseDPS, bv) * (CC / 100) * (1 + (IncCC / 100)) * (0.5f + 1.5f * (IncCM / 100));
            }
        }

        internal static float Round(float value, RoundingVariant variant)
        {
            switch (variant.Style)
            {
                case Rounding.None:
                    return value;

                case Rounding.Round:
                    return Compute.RoundValue(value, variant.Precision);

                case Rounding.Floor:
                    return Compute.FloorValue(value, variant.Precision);

                case Rounding.Ceil:
                    return Compute.CeilValue(value, variant.Precision);
            }

            throw new NotImplementedException();
        }
        
        internal class RoundingVariant
        {
            internal Rounding Style;
            internal int Precision;
        }

        [TestMethod]
        public void TestDPSRounding()
        {
            Dictionary<string, Spell> skills;
            // Initialize rounding variant for displaying DPS and variable rounding.
            RoundingVariant none = new RoundingVariant { Style = Rounding.None };
            List<RoundingVariant> displayVariants = new List<RoundingVariant>();
            List<RoundingVariant> roundingVariants = new List<RoundingVariant>() { new RoundingVariant { Style = Rounding.None, Precision = 0 } };
            foreach (Rounding style in new Rounding[] { Rounding.Round, Rounding.Floor, Rounding.Ceil })
            {
                displayVariants.Add(new RoundingVariant { Style = style, Precision = 1 });
                for (int precision = 0; precision <= 3; ++precision)
                    roundingVariants.Add(new RoundingVariant { Style = style, Precision = precision });
            }

            // Forum-based DPS (float precision)
            skills = new Dictionary<string, Spell>() {
                { "Molten Shell", new Spell { DisplayDPS = 183.4f, MinDamage = 95.76f, MaxDamage = 143.64f, CC = 35.75f, CM = 247.5f } },
                { "Tempest Shield", new Spell { DisplayDPS = 826.4f, MinDamage = 404.82f, MaxDamage = 605.5f, CC = 42.9f, CM = 247.5f } },
                { "Lightning Warp", new Spell { DisplayDPS = 1013, MinDamage = 65.74f, MaxDamage = 1259.44f, CC = 35.75f, CM = 247.5f } },

                { "Arc", new Spell { DisplayDPS = 3771.7f, MinDamage = 106.88f, MaxDamage = 2004, CPS = 1.5875f, CC = 40.9f, CM = 405f } }
            };
            int variants = 0;
            //var dv = new RoundingVariant { Style = Rounding.Floor, Precision = 1 }; // Required for match of 3.
            var drv = new RoundingVariant { Style = Rounding.Round, Precision = 0 }; // Required for match of 3.
            var bdv = none; // No rounding is wanted.
            //foreach (var drv in roundingVariants)
                foreach (var csv in roundingVariants) // While matching also Arc this variates between Round(2) and Ceil(2).
            //var csv = none; // No effect, reduce matching variants.
                    //foreach (var bdv in roundingVariants)
                        foreach (var ccv in roundingVariants) // This variates between None, Round(2), Round(3), Floor(2), Floor(3), Ceil(2), Ceil(3) (This hints full precision is needed).
                            foreach (var cmv in roundingVariants) // This variates between Round(0) and Ceil(0) (Small sample to decide which rounding is correct one)
                                foreach (var dv in displayVariants)
                                {
                                    int matches = 0;
                                    foreach (var skill in skills)
                                    {
                                        Spell spell = skill.Value;
                                        spell.DamageRange(csv, drv);
                                        spell.Compute(bdv, ccv, cmv);
                                        spell.Display(dv);
                                        if (spell.DisplayDPS == spell.DPS) ++matches;
                                    }
                                    if (matches == 4)
                                        ++variants;
                                }
            Assert.IsTrue(variants >= 1); // 24 variants matching tooltip DPS values of all 4 skills.
            /* For non-cast damage spells suitable rounding styles are following:
             * 1) Damage range using RoundValue(0) // AttackSource.Combine() does it to have correct damage range for sheet.
             * 2) No CPS rounding (it's 1 always).
             * 3) No rounding of base DPS.
             * 4) No rounding of CriticalChance.
             * 5) Either RoundValue(0) or CeilValue(0) of CriticalModifier (going with RoundValue for now).
             * 6) Display computed DPS using FloorValue(1).
             */
            /* For matching cast speed affected spells following rules are required (beside those above):
             * 2) CPS rounded using either RoundValue(2) or CeilValue(2)
             */

            /*
            // Wiki-based net DPS.
            // Can't match Tempest Shield at all, which has same value in sheet and tooltip.
            skills = new Dictionary<string, Spell>() {
                { "Molten Shell", new SpellWiki { DisplayDPS = 183.5f, MinDamage = 95.76f, MaxDamage = 143.64f, CC = 5f, IncCC = 615, IncCM = 65 } },
                { "Tempest Shield", new SpellWiki { DisplayDPS = 826.4f, MinDamage = 404.82f, MaxDamage = 605.5f, CC = 6f, IncCC = 615, IncCM = 65 } },
                { "Lightning Warp", new SpellWiki { DisplayDPS = 1013, MinDamage = 65.74f, MaxDamage = 1259.44f, CC = 5f, IncCC = 615, IncCM = 65 } }
            };
            RoundingVariant none = new RoundingVariant { Style = Rounding.None };
            foreach (var drv in roundingVariants)
                foreach (var bdv in roundingVariants)
                    foreach (var dv in displayVariants)
                    {
                        int matches = 0;
                        foreach (var skill in skills)
                        {
                            SpellWiki spell = skill.Value as SpellWiki;
                            spell.DamageRange(none, drv);
                            spell.Compute(bdv, null, null);
                            spell.Display(dv);
                            if (spell.DisplayDPS == spell.DPS) ++matches;
                        }
                        Assert.IsFalse(matches == 1);
                    }
            */
        }
    }
}
