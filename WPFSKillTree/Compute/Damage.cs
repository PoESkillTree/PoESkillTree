using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.Model.Items.Affixes;
using static POESKillTree.Compute.ComputeGlobal;

namespace POESKillTree.Compute
{

    public class Damage : DamageNature
    {
        public class Added : DamageNature
        {
            // The added damage minimum.
            float Min;
            // The added damage maximum.
            float Max;
            // The weapon hand to be applied to only.
            // TODO: Migrate to DamageNature's WeaponHand property.
            public WeaponHand Hand = WeaponHand.Any;

            private static readonly Regex ReAddMod = new Regex("Adds # to # ([^ ]+) Damage$");
            private static readonly Regex ReAddInHandMod = new Regex("Adds # to # ([^ ]+) Damage in (Main|Off) Hand$");
            private static readonly Regex ReAddWithBows = new Regex("Adds # to # ([^ ]+) Damage to attacks with Bows");
            private static readonly Regex ReAddToAttacks = new Regex("Adds # to # ([^ ]+) Damage to Attacks");
            private static readonly Regex ReAddToSpells = new Regex("Adds # to # ([^ ]+) Damage to Spells");

            public Added(DamageSource source, string type, float min, float max)
                : base()
            {
                Source = source;
                Type = DamageNature.TypeOf(type);
                Min = min;
                Max = max;
            }

            // Creates added damage.
            public static Added Create(DamageSource source, KeyValuePair<string, List<float>> attr)
            {
                if (source == DamageSource.Attack)
                {
                    var m = ReAddToAttacks.Match(attr.Key);
                    if (m.Success)
                        return new Added(source, m.Groups[1].Value, attr.Value[0], attr.Value[1]);
                    m = ReAddWithBows.Match(attr.Key);
                    if (m.Success)
                        return new Added(source, m.Groups[1].Value, attr.Value[0], attr.Value[1]) { WeaponType = WeaponType.Bow };
                }
                else if (source == DamageSource.Spell)
                {
                    var m = ReAddToSpells.Match(attr.Key);
                    if (m.Success)
                        return new Added(source, m.Groups[1].Value, attr.Value[0], attr.Value[1]);
                }

                return null;
            }

            // Creates added damage from weapon local mod.
            public static Added Create(DamageSource source, ItemMod itemMod)
            {
                Match m = ReAddMod.Match(itemMod.Attribute);
                if (m.Success)
                    return new Added(source, m.Groups[1].Value, itemMod.Value[0], itemMod.Value[1]);
                else
                {
                    m = ReAddInHandMod.Match(itemMod.Attribute);
                    if (m.Success)
                        return new Added(source, m.Groups[1].Value, itemMod.Value[0], itemMod.Value[1]) { Hand = m.Groups[2].Value == "Main" ? WeaponHand.Main : WeaponHand.Off };
                }

                return null;
            }

            // Applies damage added with nature of source.
            public void Apply(AttackSource source, float effectiveness)
            {
                Damage damage = new Damage(source.Nature, Min, Max) { Type = Type };

                damage.Mul(effectiveness);

                source.Deals.Add(damage);
            }
        }

        // @see: http://pathofexile.gamepedia.com/Damage_conversion
        public class Converted
        {
            // The source of conversion.
            public DamageConversionSource Source;
            // The percentage of damage to convert.
            public float Percent;
            // The damage type to convert from.
            public DamageType From;
            // The damage type to convert to.
            DamageType To;

            static Regex ReConvertMod = new Regex("^#% of ([^ ]+) Damage (C|c)onverted to ([^ ]+) Damage$");

            public Converted(DamageConversionSource source, float percent, DamageType from, DamageType to)
            {
                Source = source;
                Percent = percent;
                From = from;
                To = to;
            }

            // Creates damage conversion from attribute.
            public static Converted Create(DamageConversionSource source, KeyValuePair<string, List<float>> attr)
            {
                Match m = ReConvertMod.Match(attr.Key);
                if (m.Success)
                {
                    return new Converted(source, attr.Value[0], DamageNature.Types[m.Groups[1].Value], DamageNature.Types[m.Groups[3].Value]);
                }

                return null;
            }

            // Applies conversion.
            public void Apply(List<Damage> input, List<Damage> output, float scale)
            {
                foreach (Damage damage in input)
                    output.Add(damage.PercentOf(Percent * scale, To));
            }
        }

        public class Gained : DamageNature
        {
            // The percentage of damage to convert.
            float Percent;
            // The damage type to convert to.
            DamageType To;

            static Regex ReGainMod = new Regex("Gain #% of ([^ ]+) Damage as Extra ([^ ]+) Damage");
            static Regex ReGainAddedMod = new Regex("#% of (.+) Damage Added as ([^ ]+) Damage");

            public Gained(float percent, string from, DamageType to)
                : base(from)
            {
                Percent = percent;
                To = to;
            }

            // Creates damage gain from attribute.
            public static Gained Create(KeyValuePair<string, List<float>> attr)
            {
                Match m = ReGainMod.Match(attr.Key);
                if (m.Success)
                    return new Gained(attr.Value[0], m.Groups[1].Value, DamageNature.Types[m.Groups[2].Value]);
                else
                {
                    m = ReGainAddedMod.Match(attr.Key);
                    if (m.Success)
                        return new Gained(attr.Value[0], m.Groups[1].Value, DamageNature.Types[m.Groups[2].Value]);
                }

                return null;
            }

            // Applies gain.
            public void Apply(List<Damage> input, List<Damage> output)
            {
                foreach (Damage damage in input)
                    output.Add(damage.PercentOf(Percent, To));
            }
        }

        public class Increased : DamageNature
        {
            // The percentage of damage increase.
            public float Percent;

            static Regex ReIncreasedAll = new Regex("^#% (increased|reduced) Damage$");
            static Regex ReIncreasedAllWithWeaponType = new Regex("#% (increased|reduced) Damage with (.+)$");
            static Regex ReIncreasedType = new Regex("^#% (increased|reduced) (.+) Damage$");
            static Regex ReIncreasedTypeWithWeaponTypeOrHand = new Regex("#% (increased|reduced) (.+) Damage with (.+)$");
            static Regex ReIncreasedWithSource = new Regex("#% (increased|reduced) (.+) Damage with (Spells|Attacks|Weapons)$");

            public Increased(float percent)
                : base()
            {
                Percent = percent;
            }

            public Increased(string str, float percent)
                : base(str)
            {
                Percent = percent;
            }

            public Increased(DamageSource source, string type, float percent)
                : base(source, type)
            {
                Percent = percent;
            }

            // Creates modifier.
            public static Increased Create(KeyValuePair<string, List<float>> attr)
            {
                Match m = ReIncreasedType.Match(attr.Key);
                if (m.Success)
                    return new Increased(m.Groups[2].Value, m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0]);
                else
                {
                    m = ReIncreasedWithSource.Match(attr.Key);
                    if (m.Success)
                        return new Increased(m.Groups[3].Value == "Spells" ? DamageSource.Spell : DamageSource.Attack, m.Groups[2].Value, m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0]);
                    else
                    {
                        m = ReIncreasedTypeWithWeaponTypeOrHand.Match(attr.Key);
                        if (m.Success)
                        {
                            if (WithWeaponHand.ContainsKey(m.Groups[3].Value))
                                return new Increased(m.Groups[2].Value, m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0]) { WeaponHand = WithWeaponHand[m.Groups[3].Value] };
                            else if (WithWeaponType.ContainsKey(m.Groups[3].Value))
                                return new Increased(m.Groups[2].Value, m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0]) { WeaponType = WithWeaponType[m.Groups[3].Value] };
                        }
                        else
                        {
                            m = ReIncreasedAll.Match(attr.Key);
                            if (m.Success)
                                return new Increased(m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0]);
                            else
                            {
                                m = ReIncreasedAllWithWeaponType.Match(attr.Key);
                                if (m.Success && WithWeaponType.ContainsKey(m.Groups[2].Value))
                                    return new Increased(m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0]) { WeaponType = WithWeaponType[m.Groups[2].Value] };
                            }
                        }
                    }
                }

                return null;
            }
        }

        public class More : DamageNature
        {
            // The percentage of damage multiplier.
            public float Percent;
            // The flag whether multiplier is actualy dividier.
            public bool IsLess { get { return Percent < 0; } }

            static Regex ReMoreAll = new Regex("^#% (more|less) Damage( to main target)?$");
            static Regex ReMoreBase = new Regex("^Deals #% of Base Damage$");
            static Regex ReMoreBaseType = new Regex("^Deals #% of Base (.+) Damage$");
            static Regex ReMoreSimple = new Regex("^#% (more|less) (.+) Damage$");
            static Regex ReMoreWhen = new Regex("^#% more (.+) Damage when on Full Life$");
            static Regex ReMoreWith = new Regex("^#% more (.+) Damage with Weapons$");

            public More(float percent)
                : base()
            {
                Percent = percent;
            }

            public More(string str, float percent)
                : base(str)
            {
                Percent = percent;
            }

            // Creates damage multiplier.
            public static More Create(KeyValuePair<string, List<float>> attr, Computation compute)
            {
                Match m = ReMoreBase.Match(attr.Key);
                if (m.Success)
                    return new More(attr.Value[0] - 100);
                else
                {
                    m = ReMoreBaseType.Match(attr.Key);
                    if (m.Success)
                        return new More(m.Groups[1].Value, attr.Value[0] - 100);
                    else
                    {
                        m = ReMoreSimple.Match(attr.Key);
                        if (m.Success)
                            return new More(m.Groups[2].Value, m.Groups[1].Value == "more" ? attr.Value[0] : -attr.Value[0]);
                        else
                        {
                            m = ReMoreAll.Match(attr.Key);
                            if (m.Success)
                                return new More(m.Groups[1].Value == "more" ? attr.Value[0] : -attr.Value[0]);
                            else
                            {
                                m = ReMoreWhen.Match(attr.Key);
                                if (m.Success)
                                    return new More(m.Groups[1].Value, attr.Value[0]);
                                else
                                {
                                    m = ReMoreWith.Match(attr.Key);
                                    if (m.Success)
                                        return new More(m.Groups[1].Value, attr.Value[0]) { Source = DamageSource.Attack };
                                    else
                                    {
                                        if (compute.IsDualWielding && attr.Key == "When Dual Wielding, Deals #% Damage from each Weapon combined")
                                            return new More(attr.Value[0] - 100);
                                    }
                                }
                            }
                        }
                    }
                }

                return null;
            }

            // Applies multiplier.
            public void Apply(Damage damage)
            {
                damage.Mul(100 + Percent);
            }
        }

        // Default unarmed damage range.
        const float UnarmedDamageRangeMin = 2;
        const float UnarmedDamageRangeMax = 8;

        // The nature from which this damage originated (due to conversion).
        DamageNature Origin;
        // The damage range minimum.
        float Min;
        // The damage range maximum.
        float Max;

        static Regex ReDamageAttribute = new Regex("([^ ]+) Damage: #-#");
        static Regex ReDamageMod = new Regex("Deals # to # ([^ ]+) Damage$");

        // Copy constructor.
        public Damage(Damage damage)
            : base(damage)
        {
            Origin = new DamageNature(damage.Origin);
            Min = damage.Min;
            Max = damage.Max;
        }

        // Damage originated from specified damage but with different type.
        // Used in Damage.PercentOf.
        Damage(Damage damage, DamageType type, float min, float max)
            : base(damage)
        {
            Origin = new DamageNature(damage);
            Type = type;
            Min = min;
            Max = max;
        }

        // Damage with specified nature.
        Damage(DamageNature nature, float min, float max)
            : base(nature)
        {
            Origin = new DamageNature(this);
            Min = min;
            Max = max;
        }

        // Damage with specified nature but with different type.
        Damage(DamageNature nature, string type, float min, float max)
            : base(nature, type)
        {
            Origin = new DamageNature(this);
            Min = min;
            Max = max;
        }

        // Damage from specified source with specified type.
        public Damage(DamageSource source, DamageType type, float min, float max)
            : base(source, type)
        {
            Origin = new DamageNature(Source, Type);
            Min = min;
            Max = max;
        }

        // Adds damage.
        public void Add(Damage damage)
        {
            Min += damage.Min;
            Max += damage.Max;
        }

        // Returns average hit.
        public float AverageHit()
        {
            return (Min + Max) / 2;
        }

        // Creates unarmed damage.
        public static Damage Create(DamageNature nature)
        {
            return new Damage(nature, UnarmedDamageRangeMin, UnarmedDamageRangeMax) { Type = DamageType.Physical };
        }

        // Creates damage from attribute.
        public static Damage Create(DamageNature nature, string attrName, IReadOnlyList<float> attrValues)
        {
            Match m = ReDamageAttribute.Match(attrName);
            if (m.Success)
                return new Damage(nature, m.Groups[1].Value, attrValues[0], attrValues[1]);
            else
            {
                m = ReDamageMod.Match(attrName);
                if (m.Success)
                    return new Damage(nature, m.Groups[1].Value, attrValues[0], attrValues[1]);
            }

            return null;
        }

        // Increases damage.
        public void Increase(float percent)
        {
            Min = Min * (100 + percent) / 100;
            Max = Max * (100 + percent) / 100;
        }

        // Returns percent of damage with specific damage type.
        public Damage PercentOf(float percent, DamageType type)
        {
            Damage damage = new Damage(this, type, Min * percent / 100, Max * percent / 100);

            // Origin damage type of new damage is union of our damage type (set in constructor above) and our origin's damage type.
            damage.Origin.Type |= Origin.Type;

            return damage;
        }

        // Returns true if damage or its origin matches nature, false otherwise.
        new public bool Matches(DamageNature nature)
        {
            return nature.Matches(this) || nature.Matches(Origin);
        }

        // Multiplies damage by percent.
        public void Mul(float percent)
        {
            Min = Min * percent / 100;
            Max = Max * percent / 100;
        }

        // Multiplies damage by multiplier.
        public void Multiply(float multiplier)
        {
            Min *= multiplier;
            Max *= multiplier;
        }

        public void Round()
        {
            Min = RoundValue(Min, 0);
            Max = RoundValue(Max, 0);
        }

        public void RoundHalfDown()
        {
            Min = RoundHalfDownValue(Min, 0);
            Max = RoundHalfDownValue(Max, 0);
        }

        public string ToAttribute()
        {
            return (Type == DamageType.Total ? "Total Combined" : Type.ToString()) + " Damage: #-#";
        }

        public List<float> ToValue()
        {
            return new List<float>() { Min, Max };
        }
    }
}
