using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.Model;
using POESKillTree.ViewModels;
using Item = POESKillTree.ViewModels.ItemAttributes.Item;
using Mod = POESKillTree.ViewModels.ItemAttributes.Item.Mod;

namespace POESKillTree.SkillTreeFiles
{
    public class Compute
    {
        public class AttackSkill
        {
            // The name.
            string Name;
            // The nature of attack (based on gem keywords).
            public DamageNature Nature;
            // List of attack sources (either spell or main hand and/or off hand).
            List<AttackSource> Sources;
            // Skill gem local attributes.
            public AttributeSet Local;
            // Damage effectiveness.
            float Effectiveness;
            // List of damage conversions.
            List<Damage.Converted> Converts = new List<Damage.Converted>();
            // List of damage gains.
            List<Damage.Gained> Gains = new List<Damage.Gained>();
            // The flag whether skill is usable.
            public bool Useable = true;

            // The sorted list of damage types for character sheet.
            static List<DamageType> DamageTypes = new List<DamageType>()
            {
                DamageType.Physical, DamageType.Fire, DamageType.Cold, DamageType.Lightning, DamageType.Chaos
            };

            // Creates attack from gem.
            AttackSkill(Item gem)
            {
                Name = gem.Name;
                Nature = new DamageNature(gem.Keywords);

                Local = new AttributeSet();
                foreach (Mod mod in gem.Mods)
                    Local.Add(mod.Attribute, new List<float>(mod.Value));

                Effectiveness = gem.Attributes.ContainsKey("Damage Effectiveness:  #%") ? gem.Attributes["Damage Effectiveness:  #%"][0] : 100;

                Fixup();

                Sources = GetSources();
            }

            // Applies attributes.
            public void Apply()
            {
                // Lists of damage added, increased and multipliers to apply.
                List<Damage.Added> adds = new List<Damage.Added>();
                List<Damage.Increased> increases = new List<Damage.Increased>();
                List<Damage.More> mores = new List<Damage.More>();

                // Collect damage conversions from gems, equipment and tree.
                // Collect damage added from gems and equipment.
                foreach (var attr in Local)
                {
                    Damage.Converted conv = Damage.Converted.Create(DamageConversionSource.Gem, attr);
                    if (conv != null) Converts.Add(conv);

                    // Damage added from gems is always applied.
                    Damage.Added added = Damage.Added.Create(Nature.Source, attr);
                    if (added != null) adds.Add(added);
                }

                foreach (var attr in Equipment)
                {
                    Damage.Converted conv = Damage.Converted.Create(DamageConversionSource.Equipment, attr);
                    if (conv != null) Converts.Add(conv);

                    // Damage added from equipment is applied only to attacks.
                    if (Nature.Is(DamageSource.Attack))
                    {
                        Damage.Added added = Damage.Added.Create(DamageSource.Attack, attr);
                        if (added != null) adds.Add(added);
                    }
                }

                foreach (var attr in Tree)
                {
                    Damage.Converted conv = Damage.Converted.Create(DamageConversionSource.Tree, attr);
                    if (conv != null) Converts.Add(conv);
                }

                // Merge local gems and global attributes.
                AttributeSet attrs = Global.Merge(Local);

                // Collect damage gains, increases and multipliers.
                foreach (var attr in attrs)
                {
                    Damage.Gained gained = Damage.Gained.Create(attr);
                    if (gained != null) Gains.Add(gained);

                    Damage.Increased increased = Damage.Increased.Create(attr);
                    if (increased != null) increases.Add(increased);

                    Damage.More more = Damage.More.Create(attr);
                    if (more != null) mores.Add(more);
                }

                foreach (AttackSource source in Sources)
                {
                    // Apply damage added.
                    foreach (Damage.Added added in adds)
                        if (added.Matches(source.Nature))
                            added.Apply(source, Effectiveness);

                    // Apply damage conversions and gains.
                    Convert(source.Deals);

                    // For each damage dealt apply its increases and multipliers.
                    foreach (Damage damage in source.Deals)
                    {
                        float inc = 0;
                        foreach (Damage.Increased increase in increases)
                            if (damage.Matches(increase))
                                inc += increase.Percent;
                        if (inc > 0)
                            damage.Increase(inc);

                        foreach (Damage.More more in mores)
                            if (damage.Matches(more))
                                more.Apply(damage);
                    }

                    // Avatar of Fire (remove non-Fire damage).
                    if (AvatarOfFire)
                        foreach (Damage damage in new List<Damage>(source.Deals))
                            if (!damage.Is(DamageType.Fire))
                                source.Deals.Remove(damage);
                }
             }

            // Converts damage types (applies damage conversions and gains).
            public void Convert(List<Damage> deals)
            {
                foreach (DamageType type in DamageTypes)
                {
                    List<Damage> convert = deals.FindAll(d => d.Is(type));
                    List<Damage> output = new List<Damage>();
                    List<Damage.Converted> conversions;

                    float pool = convert.Count > 0 ? 100 : 0;

                    // Apply gem conversions.
                    if (pool > 0)
                        foreach (Damage.Converted conv in Converts.FindAll(c => c.Source == DamageConversionSource.Gem && c.From == type))
                        {
                            pool -= conv.Percent;
                            conv.Apply(convert, output, 1);
                        }

                    float sum;

                    // Apply equipment conversions.
                    if (pool > 0)
                    {
                        conversions = Converts.FindAll(c => c.Source == DamageConversionSource.Equipment && c.From == type);
                        sum = 0;

                        foreach (Damage.Converted conv in conversions)
                            sum += conv.Percent;
                        if (sum > pool) // Downscale conversions.
                        {
                            foreach (Damage.Converted conv in conversions)
                                conv.Apply(convert, output, pool / sum);
                            pool = 0;
                        }
                        else if (sum > 0)
                        {
                            foreach (Damage.Converted conv in conversions)
                                conv.Apply(convert, output, 1);
                            pool -= sum;
                        }
                    }

                    // Apply skill tree conversions.
                    if (pool > 0)
                    {
                        conversions = Converts.FindAll(c => c.Source == DamageConversionSource.Tree && c.From == type);
                        sum = 0;

                        foreach (Damage.Converted conv in conversions)
                            sum += conv.Percent;
                        if (sum > pool) // Downscale conversions.
                        {
                            foreach (Damage.Converted conv in conversions)
                                conv.Apply(convert, output, pool / sum);
                            pool = 0;
                        }
                        else if (sum > 0)
                        {
                            foreach (Damage.Converted conv in conversions)
                                conv.Apply(convert, output, 1);
                            pool -= sum;
                        }
                    }

                    // Add unconverted damage.
                    if (pool > 0)
                        foreach (Damage damage in convert)
                            output.Add(damage.PercentOf(pool, type));

                    // Apply gains.
                    foreach (Damage.Gained gain in Gains.FindAll(g => g.From == type))
                        gain.Apply(convert, output);

                    // Remove processed damage and append generated output.
                    deals.RemoveAll(d => d.Is(type));
                    deals.AddRange(output);
                }
            }

            // Creates attack from gem.
            public static AttackSkill Create(Item gem)
            {
                return new AttackSkill(gem);
            }

            // Fixes properties of attack skill.
            void Fixup()
            {
                switch (Name)
                {
                    // Power Siphon requires Wands.
                    case "Power Siphon":
                        Nature.WeaponType = WeaponType.Wand;
                        break;
                }
            }
            // Returns sources of attack skill (spell, main hand and/or off hand).
            // TODO: Lightning Strike must have DamageForm.Projectile removed as only melee part is displayed on sheet.
            // TODO: Herald of Ice must have DamageSource.Spell removed/changed (no spell related bonuses applies to damage dealing part).
            // TODO: Iron Grip, Iron Will (http://pathofexile.gamepedia.com/Physical_damage)
            public List<AttackSource> GetSources()
            {
                List<AttackSource> sources = new List<AttackSource>();

                if (Nature.Is(DamageSource.Attack))
                {
                    if (MainHand.IsWeapon())
                        sources.Add(new AttackSource("Main Hand", this, MainHand));

                    if (OffHand.IsWeapon())
                        sources.Add(new AttackSource("Off Hand", this, OffHand));
                }
                else // Spell
                {
                    sources.Add(new AttackSource("Spell", this, null));
                }

                return sources;
            }

            // Returns true if gem is an attack skill, false otherwise.
            public static bool IsAttackSkill(Item gem)
            {
                // A gem is an attack if it has Attack or Spell keyword and it has damage dealing mod.
                return (gem.Keywords.Contains("Attack") || gem.Keywords.Contains("Spell"))
                        && !gem.Keywords.Contains("Trap") && !gem.Keywords.Contains("Mine") // No traps & mines.
                        && gem.Mods.Find(mod => mod.Attribute.StartsWith("Deals")) != null;
            }

             // Links support gems.
            // TODO: In case of same gems slotted only highest level one is used.
            public void Link(List<Item> gems)
            {
                foreach (Item gem in gems)
                {
                    if (!gem.Keywords.Contains("Support")) continue; // Skip non-support gems.

                    // Add all mods of support gems to attack skill gem.
                    foreach (Mod mod in gem.Mods)
                        Local.Add(mod.Attribute, new List<float>(mod.Value));
                }
            }

            // Return list group of this attack.
            public ListGroup ToListGroup()
            {
                AttributeSet props = new AttributeSet();

                foreach (AttackSource source in Sources)
                    foreach (DamageType type in DamageTypes)
                    {
                        List<Damage> deals = source.Deals.FindAll(d => d.Is(type));
                        if (deals.Count > 0)
                        {
                            if (deals.Count > 1)
                                for (int i = 1; i < deals.Count; ++i)
                                    deals[0].Add(deals[i]);
                            props.Add(source.Name + " " + deals[0].ToAttribute(), deals[0].ToValue());
                        }
                    }

                return new ListGroup(Name + (Useable ? "" : " (Unuseable)"), props);
            }
        }

        public class AttackSource
        {
            // List of damage dealt by source.
            public List<Damage> Deals = new List<Damage>();
            // The source name.
            public string Name;
            // The result nature of skill used with weapon.
            public DamageNature Nature;

            public AttackSource(string name, AttackSkill skill, Weapon weapon)
            {
                Name = name;

                if (weapon == null) // Spells get damage from gem local attributes.
                {
                    Nature = new DamageNature(skill.Nature);

                    foreach (var attr in skill.Local)
                    {
                        Damage damage = Damage.Create(skill.Nature, attr);
                        if (damage != null) Deals.Add(damage);
                    }
                }
                else
                {
                    if ((skill.Nature.WeaponType & weapon.Nature.WeaponType) == 0) // Skill can't be used.
                    {
                        // Override weapon type of skill with actual weapon (client shows damage of unuseable skills as well).
                        Nature = new DamageNature(skill.Nature) { WeaponType = weapon.Nature.WeaponType };

                        skill.Useable = false; // Flag skill as unuseable.
                    }
                    else // Narrow down weapon type of skill gem to actual weapon (e.g. Frenzy).
                        Nature = new DamageNature(skill.Nature) { WeaponType = skill.Nature.WeaponType & weapon.Nature.WeaponType };

                    foreach (Damage damage in weapon.Deals)
                        Deals.Add(new Damage(damage) { Source = Nature.Source, WeaponType = Nature.WeaponType });
                }
            }
       }

        public enum DamageConversionSource
        {
            Gem, Equipment, Tree
        }

        public enum DamageArea
        {
            Any, Area
        }

        public enum DamageForm
        {
            Any, Projectile
        }

        public enum DamageOverTime
        {
            Any, Burning
        }

        public enum DamageSource
        {
            Any, Attack, Spell
        }

        // Bitset.
        public enum DamageType
        {
            Any, Physical = 1, Fire = 2, Cold = 4, Lightning = 8, Chaos = 16,
            Elemental = Cold | Fire | Lightning
        }

        public class DamageNature
        {
            public DamageArea Area = DamageArea.Any;
            public DamageOverTime DoT = DamageOverTime.Any;
            public DamageForm Form = DamageForm.Any;
            public DamageSource Source = DamageSource.Any;
            public DamageType Type = DamageType.Any;
            public WeaponType WeaponType = WeaponType.Any;

            static Dictionary<string, DamageArea> Areas = new Dictionary<string, DamageArea>()
            {
                { "AoE",        DamageArea.Area },
                { "Area",       DamageArea.Area }
            };
            static Dictionary<string, DamageOverTime> DoTs = new Dictionary<string, DamageOverTime>()
            {
                { "Burning",    DamageOverTime.Burning }
            };
            static Dictionary<string, DamageForm> Forms = new Dictionary<string, DamageForm>()
            {
                { "Projectile", DamageForm.Projectile }
            };
            static Dictionary<string, DamageSource> Sources = new Dictionary<string, DamageSource>()
            {
                { "Attack",     DamageSource.Attack },
                { "Spell",      DamageSource.Spell },
                { "Weapon",     DamageSource.Attack }
            };
            public static Dictionary<string, DamageType> Types = new Dictionary<string, DamageType>()
            {
                { "Physical",   DamageType.Physical },
                { "Fire",       DamageType.Fire },
                { "Cold",       DamageType.Cold },
                { "Lightning",  DamageType.Lightning },
                { "Elemental",  DamageType.Elemental },
                { "Chaos",      DamageType.Chaos }
            };

            public DamageNature() { }

            public DamageNature(DamageNature nature)
            {
                Area = nature.Area;
                DoT = nature.DoT;
                Form = nature.Form;
                Source = nature.Source;
                Type = nature.Type;
                WeaponType = nature.WeaponType;
            }

            public DamageNature(DamageNature nature, string str)
            {
                Area = nature.Area;
                DoT = nature.DoT;
                Form = nature.Form;
                Source = nature.Source;
                Type = nature.Type;
                WeaponType = nature.WeaponType;

                string[] words = str.Split(' ');
                foreach (string word in words)
                {
                    if (Types.ContainsKey(word)) Type = Types[word];
                    else if (Sources.ContainsKey(word)) Source = Sources[word];
                    else if (Weapon.Types.ContainsKey(word)) WeaponType = Weapon.Types[word];
                    else if (Forms.ContainsKey(word)) Form = Forms[word];
                    else if (DoTs.ContainsKey(word)) DoT = DoTs[word];
                    else if (Areas.ContainsKey(word)) Area = Areas[word];
                    else throw new Exception("Unknown keyword: " + word);
                }
            }

            public DamageNature(string str)
            {
                string[] words = str.Split(' ');
                foreach (string word in words)
                {
                    if (Types.ContainsKey(word)) Type = Types[word];
                    else if (Sources.ContainsKey(word)) Source = Sources[word];
                    else if (Weapon.Types.ContainsKey(word)) WeaponType = Weapon.Types[word];
                    else if (Forms.ContainsKey(word)) Form = Forms[word];
                    else if (DoTs.ContainsKey(word)) DoT = DoTs[word];
                    else if (Areas.ContainsKey(word)) Area = Areas[word];
                    else throw new Exception("Unknown keyword: " + word);
                }
            }

            public DamageNature(DamageSource source, DamageType type)
            {
                Source = source;
                Type = type;
            }

            public DamageNature(DamageSource source, string str)
            {
                Source = source;

                string[] words = str.Split(' ');
                foreach (string word in words)
                {
                    if (Types.ContainsKey(word)) Type = Types[word];
                    else if (Sources.ContainsKey(word)) Source = Sources[word];
                    else if (Weapon.Types.ContainsKey(word)) WeaponType = Weapon.Types[word];
                    else if (Forms.ContainsKey(word)) Form = Forms[word];
                    else if (DoTs.ContainsKey(word)) DoT = DoTs[word];
                    else if (Areas.ContainsKey(word)) Area = Areas[word];
                    else throw new Exception("Unknown keyword: " + word);
                }
            }

            public DamageNature(List<string> keywords)
            {
                foreach (string word in keywords)
                {
                    if (Forms.ContainsKey(word)) Form = Forms[word];
                    else if (Sources.ContainsKey(word)) Source = Sources[word];
                    else if (Weapon.Types.ContainsKey(word)) WeaponType = Weapon.Types[word];
                    else if (DoTs.ContainsKey(word)) DoT = DoTs[word];
                    else if (Areas.ContainsKey(word)) Area = Areas[word];
                }
            }

            public bool Is(DamageSource source)
            {
                return Source == source;
            }

            public bool Is(DamageType type)
            {
                return (Type & type) != 0;
            }

            public bool Matches(DamageNature nature)
            {
                return (Area == DamageArea.Any || nature.Area == Area)
                       && (DoT == DamageOverTime.Any || nature.DoT == DoT)
                       && (Form == DamageForm.Any || nature.Form == Form)
                       && (Source == DamageSource.Any || nature.Source == Source)
                       && (WeaponType == WeaponType.Any || (nature.WeaponType & WeaponType) != 0)
                       && (Type == DamageType.Any || (nature.Type & Type) != 0);
            }

            public static DamageType TypeOf(string type)
            {
                return Types[type];
            }
        }

        public class Damage : DamageNature
        {
            public class Added : DamageNature
            {
                // The added damage minimum.
                float Min;
                // The added damage maximum.
                float Max;
                // The damage type to add.
                DamageType Type;

                static Regex ReAddMod = new Regex("Adds #-# ([^ ]+) Damage$");
                static Regex ReAddWithBows = new Regex("Adds #-# ([^ ]+) Damage to attacks with Bows");

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
                    Match m = ReAddMod.Match(attr.Key);
                    if (m.Success)
                        return new Added(source, m.Groups[1].Value, attr.Value[0], attr.Value[1]);
                    else
                    {
                        m = ReAddWithBows.Match(attr.Key);
                        if (m.Success)
                        {
                            return new Added(DamageSource.Attack, m.Groups[1].Value, attr.Value[0], attr.Value[1]) { WeaponType = WeaponType.Bow };
                        }
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

                static Regex ReConvertMod = new Regex("#% of ([^ ]+) Damage Converted to ([^ ]+) Damage");

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
                        return new Converted(source, attr.Value[0], DamageNature.Types[m.Groups[1].Value], DamageNature.Types[m.Groups[2].Value]);
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

            // TODO: Tempest Blast: 30% of Wand Physical Damage Added as Lightning Damage
            public class Gained
            {
                // The percentage of damage to convert.
                float Percent;
                // The damage type to convert from.
                public DamageType From;
                // The damage type to convert to.
                DamageType To;

                static Regex ReGainMod = new Regex("Gain #% of ([^ ]+) Damage as Extra ([^ ]+) Damage");

                public Gained(float percent, DamageType from, DamageType to)
                {
                    Percent = percent;
                    From = from;
                    To = to;
                }

                // Creates damage gain from attribute.
                public static Gained Create(KeyValuePair<string, List<float>> attr)
                {
                    Match m = ReGainMod.Match(attr.Key);
                    if (m.Success)
                    {
                        return new Gained(attr.Value[0], DamageNature.Types[m.Groups[1].Value], DamageNature.Types[m.Groups[2].Value]);
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

                static Regex ReIncreasedAll = new Regex("#% (increased|reduced) Damage$");
                static Regex ReIncreasedSimple = new Regex("#% (increased|reduced) (.+) Damage$");
                static Regex ReIncreasedWith = new Regex("#% (increased|reduced) (.+) Damage with (Spells|Weapons)$");

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
                    Match m = ReIncreasedSimple.Match(attr.Key);
                    if (m.Success)
                        return new Increased(m.Groups[2].Value, m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0]);
                    else
                    {
                        m = ReIncreasedWith.Match(attr.Key);
                        if (m.Success)
                            return new Increased(m.Groups[3].Value == "Spells" ? DamageSource.Spell : DamageSource.Attack, m.Groups[2].Value, m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0]);
                        else
                        {
                            m = ReIncreasedAll.Match(attr.Key);
                            if (m.Success)
                                return new Increased(m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0]);
                        }
                    }

                    return null;
                }
            }
 
            public class More : DamageNature
            {
                // The percentage of damage multiplier.
                float Percent;

                static Regex ReMoreAll = new Regex("#% (more|less) Damage$");
                static Regex ReMoreBase = new Regex("Deals #% of Base Damage$");
                static Regex ReMoreSimple = new Regex("#% (more|less) (.+) Damage$");
                static Regex ReMoreWhen = new Regex("#% more (.+) Damage when on Full Life$");

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
                public static More Create(KeyValuePair<string, List<float>> attr)
                {
                    Match m = ReMoreBase.Match(attr.Key);
                    if (m.Success)
                        return new More(attr.Value[0] - 100);
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

            // The nature from which this damage originated (due to conversion).
            DamageNature Origin;
            // The damage range minimum.
            float Min;
            // The damage range maximum.
            float Max;

            static Regex ReDamageAttribute = new Regex("([^ ]+) Damage:  #-#");
            static Regex ReDamageMod = new Regex("Deals #-# ([^ ]+) Damage$");

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
            Damage(DamageSource source, DamageType type, float min, float max)
                : base(source, type)
            {
                Origin = new DamageNature(Source, Type);
                Min = min;
                Max = max;
            }

            // Adds damage.
            public void Add (Damage damage)
            {
                Min += damage.Min;
                Max += damage.Max;
            }

            // Creates damage from attribute.
            public static Damage Create(DamageNature nature, KeyValuePair<string, List<float>> attr)
            {
                Match m = ReDamageAttribute.Match(attr.Key);
                if (m.Success)
                    return new Damage(nature, m.Groups[1].Value, attr.Value[0], attr.Value[1]);
                else
                {
                    m = ReDamageMod.Match(attr.Key);
                    if (m.Success)
                        return new Damage(nature, m.Groups[1].Value, attr.Value[0], attr.Value[1]);
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

            // Returns true if damage matches nature, false otherwise.
            new public bool Matches (DamageNature nature)
            {
                return nature.Matches(this) || nature.Matches(Origin);
            }

            // Multiplies damage by percent.
            public void Mul(float percent)
            {
                Min = Min * percent / 100;
                Max = Max * percent / 100;
            }

            public string ToAttribute()
            {
                return Type.ToString() + " Damage: #-#";
            }

            public List<float> ToValue()
            {
                return new List<float>() { Compute.RoundValue(Min, 0), Compute.RoundValue(Max, 0) };
            }
        }

        public class Weapon
        {
            // List of all damage dealt by weapon.
            public List<Damage> Deals = new List<Damage>();
            // The item.
            public Item Item;
            // Local attributes.
            public AttributeSet Local = new AttributeSet();
            // Type of weapon.
            public DamageNature Nature;

            public static Dictionary<string, WeaponType> Types = new Dictionary<string, WeaponType>()
            {
                { "Bow",                WeaponType.Bow },
                { "Claw",               WeaponType.Claw },
                { "Dagger",             WeaponType.Dagger },
                { "One Handed Axe",     WeaponType.OneHandedAxe },
                { "One Handed Mace",    WeaponType.OneHandedMace },
                { "One Handed Sword",   WeaponType.OneHandedSword },
                { "Staff",              WeaponType.Staff },
                { "Two Handed Axe",     WeaponType.TwoHandedAxe },
                { "Two Handed Mace",    WeaponType.TwoHandedMace },
                { "Two Handed Sword",   WeaponType.TwoHandedSword },
                { "Wand",               WeaponType.Wand },
                { "Melee",              WeaponType.Melee }
            };

            public Weapon(Item item)
            {
                if (item != null)
                {
                    Item = item;

                    // Get weapon type (damage nature).
                    if (item.Keywords == null) // Quiver or shield.
                    {
                        if (item.Type.Contains("Quiver"))
                            Nature = new DamageNature() { WeaponType = WeaponType.Quiver };
                        else
                            if (item.Type.Contains("Shield"))
                                Nature = new DamageNature() { WeaponType = WeaponType.Shield };
                            else
                                throw new Exception("Unknown weapon type");
                    }
                    else // Regular weapon.
                        foreach (string keyword in item.Keywords)
                            if (Types.ContainsKey(keyword))
                            {
                                Nature = new DamageNature() { WeaponType = Types[keyword] };
                                break;
                            }

                    // Create damage dealt.
                    foreach (var attr in item.Attributes)
                    {
                        Damage damage = Damage.Create(Nature, attr);
                        if (damage != null) Deals.Add(damage);
                    }

                    // Get local mods.
                    foreach (var mod in item.Mods.FindAll(m => m.isLocal))
                        Local.Add(mod.Attribute, mod.Value);
                }
            }

            public bool Is(WeaponType type)
            {
                return Nature != null && (Nature.WeaponType & type) != 0;
            }

            public bool IsShield()
            {
                return Nature != null && Nature.WeaponType == WeaponType.Shield;
            }

            public bool IsWeapon()
            {
                return Nature != null && (Nature.WeaponType & WeaponType.Weapon) != 0;
            }
        }

        // Bitset.
        public enum WeaponType
        {
            Any,
            Bow = 1, Claw = 2, Dagger = 4, OneHandedAxe = 8, OneHandedMace = 16, OneHandedSword = 32,
            Staff = 64, TwoHandedAxe = 128, TwoHandedMace = 256, TwoHandedSword = 512, Wand = 1024,
            Quiver = 8192,
            Shield = 16384,
            Melee = Claw | Dagger | OneHandedAxe | OneHandedMace | OneHandedSword | Staff | TwoHandedAxe | TwoHandedMace | TwoHandedSword,
            Ranged = Bow | Wand,
            Weapon = Melee | Ranged
        }

        // Equipped items.
        public static List<Item> Items;
        // Main hand weapon.
        public static Weapon MainHand;
        // Off hand weapon or quiver/shield.
        public static Weapon OffHand;
        // Character level.
        public static int Level;
        // Equipment attributes.
        public static AttributeSet Equipment;
        // All global attributes (includes tree, equipment, implicit).
        public static AttributeSet Global;
        // Implicit attributes derived from base attributes and level (e.g. Life, Mana).
        public static AttributeSet Implicit;
        // Skill tree attributes (includes base attributes).
        public static AttributeSet Tree;

        // Skill tree keystones.
        public static bool Acrobatics;
        public static bool AvatarOfFire;
        public static bool BloodMagic;
        public static bool ChaosInoculation;
        public static bool EldritchBattery;
        public static bool IronReflexes;
        public static bool VaalPact;
        public static bool ZealotsOath;

        // Monster average accuracy for each level (1 .. 100).
        public static int[] MonsterAverageAccuracy = new int[] { 0, // Level 0 placeholder.
              18,     19,     20,     21,     23,
              24,     25,     27,     28,     30,
              31,     33,     35,     36,     38,
              40,     42,     44,     46,     49,
              51,     54,     56,     59,     62,
              65,     68,     71,     74,     78,
              81,     85,     89,     93,     97,
             101,    106,    111,    116,    121,
             126,    132,    137,    143,    149,
             156,    162,    169,    177,    184,
             192,    200,    208,    217,    226,
             236,    245,    255,    266,    277,
             288,    300,    312,    325,    338,
             352,    366,    381,    396,    412,
             428,    445,    463,    481,    500,
             520,    540,    562,    584,    607,
             630,    655,    680,    707,    734,
             762,    792,    822,    854,    887,
             921,    956,    992,   1030,   1069,
            1110,   1152,   1196,   1241,   1288
        };

        // Chance to Evade = 1 - Attacker's Accuracy / ( Attacker's Accuracy + (Defender's Evasion / 4) ^ 0.8 )
        // Chance to hit can never be lower than 5%, nor higher than 95%.
        // @see http://pathofexile.gamepedia.com/Evasion
        public static float ChanceToEvade(int level, float evasionRating)
        {
            int maa = MonsterAverageAccuracy[level];

            float chance = RoundValue((float)(1 - maa / (maa + Math.Pow(evasionRating / 4, 0.8))) * 100, 0);
            if (chance < 5f) chance = 5f;
            else if (chance > 95f) chance = 95f;

            return chance;
        }

        // Computes defensive statistics.
        public static List<ListGroup> Defense()
        {
            AttributeSet ch = new AttributeSet();
            AttributeSet def = new AttributeSet();

            // Difficulty.
            bool difficultyNormal = true;
            bool difficultyCruel = false;
            bool difficultyMerciless = false;
            // Bandits.
            bool banditNormalKraityn = false; // +8% to all elemental resistances
            bool banditNormalAlira = false; // +40 Mana
            bool banditNormalOak = false; // +40 Life
            bool banditCruelKraityn = false; // +8% Attack Speed
            bool banditCruelAlira = false; // +4% Cast Speed
            bool banditCruelOak = false; // +18% Physical Damage
            bool banditMercilessKraityn = false; // +1 Max Frenzy Charge
            bool banditMercilessAlira = false; // +1 Max Power Charge
            bool banditMercilessOak = false; // +1 Max Endurance Charge

            float life;
            if (ChaosInoculation)
                life = Global["Maximum Life becomes #, Immune to Chaos Damage"][0];
            else
            {
                life = Global["+# to maximum Life"][0];
                if (banditNormalOak) // Bandit.
                    life += 40;
                if (Global.ContainsKey("#% increased maximum Life"))
                    life = IncreaseValueByPercentage(life, Global["#% increased maximum Life"][0]);
            }
            ch["Life: #"] = new List<float>() { RoundValue(life, 0) };

            float mana = Global["+# to maximum Mana"][0];
            float incMana = 0;
            if (banditNormalAlira) // Bandit.
                mana += 40;
            if (Global.ContainsKey("#% increased maximum Mana"))
                incMana = Global["#% increased maximum Mana"][0];

            float es = 0;
            // Add maximum shield from tree.
            if (Global.ContainsKey("+# to maximum Energy Shield"))
                es += Global["+# to maximum Energy Shield"][0];
            // Add maximum shield from items.
            if (Global.ContainsKey("Energy Shield:  #"))
                es += Global["Energy Shield:  #"][0];
            // Increase % maximum shield from intelligence.
            float incES = RoundValue(Global["+#% Energy Shield"][0], 0);
            // Increase % maximum shield from tree and items.
            if (Global.ContainsKey("#% increased maximum Energy Shield"))
                incES += Global["#% increased maximum Energy Shield"][0];

            float moreES = 0;
            // More % maximum shield from tree and items.
            if (Global.ContainsKey("#% more maximum Energy Shield"))
                moreES += Global["#% more maximum Energy Shield"][0];

            float lessArmourAndES = 0;
            if (Acrobatics)
                lessArmourAndES += Global["#% Chance to Dodge Attacks. #% less Armour and Energy Shield"][1];

            // Equipped Shield bonuses.
            float incArmourShield = 0;
            float incESShield = 0;
            float incDefencesShield = 0;
            if (Global.ContainsKey("#% increased Armour from equipped Shield"))
                incArmourShield += Global["#% increased Armour from equipped Shield"][0];
            if (Global.ContainsKey("#% increased Energy Shield from equipped Shield"))
                incESShield += Global["#% increased Energy Shield from equipped Shield"][0];
            if (Global.ContainsKey("#% increased Defences from equipped Shield"))
                incDefencesShield += Global["#% increased Defences from equipped Shield"][0];
            float shieldArmour = 0;
            float shieldEvasion = 0;
            float shieldES = 0;
            if (incDefencesShield > 0 || incArmourShield > 0 || incESShield > 0)
            {
                List<float> value = GetItemAttributeValue(Item.ItemClass.OffHand, "Armour:  #");
                if (value.Count > 0)
                    shieldArmour += PercentOfValue(value[0], incArmourShield + incDefencesShield);

                value = GetItemAttributeValue(Item.ItemClass.OffHand, "Evasion Rating:  #");
                if (value.Count > 0)
                    shieldEvasion += PercentOfValue(value[0], incDefencesShield);

                value = GetItemAttributeValue(Item.ItemClass.OffHand, "Energy Shield:  #");
                if (value.Count > 0)
                    shieldES += PercentOfValue(value[0], incESShield + incDefencesShield);
            }

            // ( Mana * %mana increases ) + ( ES * ( %ES increases + %mana increases ) * ( %ES more ) )
            // @see http://pathofexile.gamepedia.com/Eldritch_Battery
            if (EldritchBattery)
            {
                es = IncreaseValueByPercentage(es, incES + incMana);
                es += shieldES;
                if (moreES > 0)
                    es = IncreaseValueByPercentage(es, moreES);
                if (lessArmourAndES > 0)
                    es = IncreaseValueByPercentage(es, -lessArmourAndES);

                mana = IncreaseValueByPercentage(mana, incMana) + es;
                es = 0;
            }
            else
            {
                mana = IncreaseValueByPercentage(mana, incMana);
                es = IncreaseValueByPercentage(es, incES);
                es += shieldES;
                if (moreES > 0)
                    es = IncreaseValueByPercentage(es, moreES);
                if (lessArmourAndES > 0)
                    es = IncreaseValueByPercentage(es, -lessArmourAndES);
            }

            if (BloodMagic)
                mana = 0;

            ch["Mana: #"] = new List<float>() { RoundValue(mana, 0) };
            ch["Maximum Energy Shield: #"] = new List<float>() { RoundValue(es, 0) };

            // Evasion Rating from level.
            float evasion = Global["Evasion Rating: #"][0];
            // Evasion Rating from tree, items.
            if (Global.ContainsKey("Evasion Rating:  #"))
                evasion += Global["Evasion Rating:  #"][0];
            if (Global.ContainsKey("+# to Evasion Rating"))
                evasion += Global["+# to Evasion Rating"][0];
            // Increase % from dexterity, tree and items.
            float incEvasion = Global["#% increased Evasion Rating"][0];
            float incEvasionAndArmour = 0;
            if (Global.ContainsKey("#% increased Evasion Rating and Armour"))
                incEvasionAndArmour += Global["#% increased Evasion Rating and Armour"][0];

            float armour = 0;
            // Armour from items.
            if (Global.ContainsKey("Armour:  #"))
                armour += Global["Armour:  #"][0];
            float incArmour = 0;
            if (Global.ContainsKey("#% increased Armour"))
                incArmour += Global["#% increased Armour"][0];

            // Final Armour = Base Evasion * ( 1 + % increased Evasion Rating + % increased Armour + % increased Evasion Rating and Armour )
            //              + Base Armour  * ( 1 + % increased Armour                              + % increased Evasion Rating and Armour )
            // @see http://pathofexile.gamepedia.com/Iron_Reflexes
            if (IronReflexes)
            {
                // Substract "#% increased Evasion Rating" from Dexterity (it's not being applied).
                incEvasion -= Implicit["#% increased Evasion Rating"][0];
                armour = IncreaseValueByPercentage(armour, incArmour + incEvasionAndArmour) + IncreaseValueByPercentage(evasion, incEvasion + incArmour + incEvasionAndArmour);
                armour += shieldArmour + shieldEvasion;
                evasion = 0;
            }
            else
            {
                evasion = IncreaseValueByPercentage(evasion, incEvasion + incEvasionAndArmour) + shieldEvasion;
                armour = IncreaseValueByPercentage(armour, incArmour + incEvasionAndArmour) + shieldArmour;
            }
            if (lessArmourAndES > 0)
                armour = IncreaseValueByPercentage(armour, -lessArmourAndES);

            if (armour > 0)
            {
                def["Armour: #"] = new List<float>() { RoundValue(armour, 0) };
                def["Estimated Physical Damage reduction: #%"] = new List<float>() { PhysicalDamageReduction(Level, RoundValue(armour, 0)) };
            }
            if (evasion > 0)
                def["Evasion Rating: #"] = new List<float>() { RoundValue(evasion, 0) };
            def["Estimated chance to Evade Attacks: #%"] = new List<float>() { ChanceToEvade(Level, RoundValue(evasion, 0)) };

            // Dodge Attacks and Spells.
            float chanceToDodgeAttacks = 0;
            float chanceToDodgeSpells = 0;
            if (Acrobatics)
                chanceToDodgeAttacks += Global["#% Chance to Dodge Attacks. #% less Armour and Energy Shield"][0];
            if (Global.ContainsKey("#% additional chance to Dodge Attacks"))
                chanceToDodgeAttacks += Global["#% additional chance to Dodge Attacks"][0];
            if (Global.ContainsKey("#% Chance to Dodge Spell Damage"))
                chanceToDodgeSpells += Global["#% Chance to Dodge Spell Damage"][0];
            if (chanceToDodgeAttacks > 0)
                def["Chance to Dodge Attacks: #%"] = new List<float>() { chanceToDodgeAttacks };
            if (chanceToDodgeSpells > 0)
                def["Chance to Dodge Spells: #%"] = new List<float>() { chanceToDodgeSpells };

            // Energy Shield Recharge per Second.
            // @see http://pathofexile.gamepedia.com/Energy_shield
            if (es > 0)
            {
                def["Maximum Energy Shield: #"] = new List<float>() { RoundValue(es, 0) };

                float esRecharge = RoundValue(es, 0) / 3; // By default, energy shield recharges at a rate equal to a third of the character's maximum energy shield per second.
                def["Energy Shield Recharge per Second: #"] = new List<float>() { RoundValue(esRecharge, 1) };
                float esDelay = 6; // By default, the delay period for energy shield to begin to recharge is 6 seconds.
                if (Global.ContainsKey("#% faster start of Energy Shield Recharge"))
                    esDelay = esDelay * 100 / (100 + Global["#% faster start of Energy Shield Recharge"][0]);
                def["Energy Shield Recharge Delay: #s"] = new List<float>() { RoundValue(esDelay, 1) };
            }

            // Life Regeneration.
            float lifeRegen = 0;
            float lifeRegenFlat = 0;
            if (Global.ContainsKey("#% of Life Regenerated per Second"))
                lifeRegen += Global["#% of Life Regenerated per Second"][0];
            if (Global.ContainsKey("# Life Regenerated per second"))
                lifeRegenFlat += Global["# Life Regenerated per second"][0];

            if (VaalPact)
                lifeRegen = 0;

            if (ZealotsOath)
            {
                if (es > 0 && lifeRegen + lifeRegenFlat > 0)
                    def["Energy Shield Regeneration per Second: #"] = new List<float>() { RoundValue(PercentOfValue(RoundValue(es, 0), lifeRegen), 1) + lifeRegenFlat };
            }
            else
            {
                if (! ChaosInoculation && lifeRegen + lifeRegenFlat > 0)
                    def["Life Regeneration per Second: #"] = new List<float>() { RoundValue(PercentOfValue(RoundValue(life, 0), lifeRegen), 1) + lifeRegenFlat };
            }

            // Mana Regeneration.
            if (mana > 0)
            {
                float manaRegen = PercentOfValue(RoundValue(mana, 0), 1.75f);
                // manaRegen += ClarityManaRegenerationPerSecond; // Clarity provides flat mana regeneration bonus.
                float incManaRegen = 0;
                if (Global.ContainsKey("#% increased Mana Regeneration Rate"))
                    incManaRegen += Global["#% increased Mana Regeneration Rate"][0];
                manaRegen = IncreaseValueByPercentage(manaRegen, incManaRegen);
                def["Mana Regeneration per Second: #"] = new List<float>() { RoundValue(manaRegen, 1) };
            }

            // Character attributes.
            ch["Strength: #"] = Global["+# to Strength"];
            ch["Dexterity: #"] = Global["+# to Dexterity"];
            ch["Intelligence: #"] = Global["+# to Intelligence"];

            // Shield, Staff and Dual Wielding detection.
            bool hasShield = OffHand.IsShield();
            bool hasStaff = MainHand.Is(WeaponType.Staff);
            bool isDualWielding = MainHand.IsWeapon() && OffHand.IsWeapon();

            // Resistances.
            float maxResistFire = 75;
            float maxResistCold = 75;
            float maxResistLightning = 75;
            float maxResistChaos = 75;
            float resistFire = 0;
            float resistCold = 0;
            float resistLightning = 0;
            float resistChaos = 0;
            // Penalties to resistances at difficulty levels.
            if (difficultyCruel)
                resistFire = resistCold = resistLightning = resistChaos = -20;
            else if (difficultyMerciless)
                resistFire = resistCold = resistLightning = resistChaos = -60;
            if (banditNormalKraityn) // Bandit.
            {
                resistFire += 8;
                resistCold += 8;
                resistLightning += 8;
            }
            if (Global.ContainsKey("+#% to Fire Resistance"))
                resistFire += Global["+#% to Fire Resistance"][0];
            if (Global.ContainsKey("+#% to Cold Resistance"))
                resistCold += Global["+#% to Cold Resistance"][0];
            if (Global.ContainsKey("+#% to Lightning Resistance"))
                resistLightning += Global["+#% to Lightning Resistance"][0];
            if (Global.ContainsKey("+#% to Chaos Resistance"))
                resistChaos += Global["+#% to Chaos Resistance"][0];
            if (Global.ContainsKey("+#% to Fire and Cold Resistances")) // Two-Stone Ring.
            {
                float value = Global["+#% to Fire and Cold Resistances"][0];
                resistFire += value;
                resistCold += value;
            }
            if (Global.ContainsKey("+#% to Fire and Lightning Resistances")) // Two-Stone Ring.
            {
                float value = Global["+#% to Fire and Lightning Resistances"][0];
                resistFire += value;
                resistLightning += value;
            }
            if (Global.ContainsKey("+#% to Cold and Lightning Resistances")) // Two-Stone Ring.
            {
                float value = Global["+#% to Cold and Lightning Resistances"][0];
                resistCold += value;
                resistLightning += value;
            }
            if (Global.ContainsKey("+#% to all Elemental Resistances"))
            {
                float value = Global["+#% to all Elemental Resistances"][0];
                resistFire += value;
                resistCold += value;
                resistLightning += value;
            }
            if (hasShield && Global.ContainsKey("+#% Elemental Resistances while holding a Shield"))
            {
                float value = Global["+#% Elemental Resistances while holding a Shield"][0];
                resistFire += value;
                resistCold += value;
                resistLightning += value;
            }
            if (Global.ContainsKey("+#% to maximum Fire Resistance"))
                maxResistFire += Global["+#% to maximum Fire Resistance"][0];
            if (Global.ContainsKey("+#% to maximum Cold Resistance"))
                maxResistCold += Global["+#% to maximum Cold Resistance"][0];
            if (Global.ContainsKey("+#% to maximum Lightning Resistance"))
                maxResistLightning += Global["+#% to maximum Lightning Resistance"][0];
            if (ChaosInoculation)
                maxResistChaos = resistChaos = 100;
            def["Fire Resistance: #% (#%)"] = new List<float>() { MaximumValue(resistFire, maxResistFire), resistFire };
            def["Cold Resistance: #% (#%)"] = new List<float>() { MaximumValue(resistCold, maxResistCold), resistCold };
            def["Lightning Resistance: #% (#%)"] = new List<float>() { MaximumValue(resistLightning, maxResistLightning), resistLightning };
            def["Chaos Resistance: #% (#%)"] = new List<float>() { MaximumValue(resistChaos, maxResistChaos), resistChaos };

            // Chance to Block Attacks and Spells.
            // Block chance is capped at 75%. The chance to block spells is also capped at 75%.
            // @see http://pathofexile.gamepedia.com/Blocking
            float maxChanceBlockAttacks = 75;
            float maxChanceBlockSpells = 75;
            float chanceBlockAttacks = 0;
            float chanceBlockSpells = 0;
            if (Global.ContainsKey("+#% to maximum Block Chance"))
            {
                maxChanceBlockAttacks += Global["+#% to maximum Block Chance"][0];
                maxChanceBlockSpells += Global["+#% to maximum Block Chance"][0];
            }
            if (hasShield)
            {
                List<float> valueList = GetItemAttributeValue(Item.ItemClass.OffHand, "Chance to Block:  #%");
                chanceBlockAttacks += valueList[0];
            }
            else if (hasStaff)
            {
                List<float> valueList = GetItemAttributeValue(Item.ItemClass.MainHand, "#% Chance to Block");
                chanceBlockAttacks += valueList[0];
            }
            else if (isDualWielding)
            {
                chanceBlockAttacks += 15; // When dual wielding, the base chance to block is 15% no matter which weapons are used.
            }
            if (hasShield && Global.ContainsKey("#% additional Chance to Block with Shields"))
                chanceBlockAttacks += Global["#% additional Chance to Block with Shields"][0];
            if (hasStaff && Global.ContainsKey("#% additional Block Chance With Staves"))
                chanceBlockAttacks += Global["#% additional Block Chance With Staves"][0];
            if (isDualWielding && Global.ContainsKey("#% additional Chance to Block while Dual Wielding"))
                chanceBlockAttacks += Global["#% additional Chance to Block while Dual Wielding"][0];
            if ((isDualWielding || hasShield) && Global.ContainsKey("#% additional Chance to Block while Dual Wielding or holding a Shield"))
                chanceBlockAttacks += Global["#% additional Chance to Block while Dual Wielding or holding a Shield"][0];
            if (Global.ContainsKey("#% of Block Chance applied to Spells"))
                chanceBlockSpells = PercentOfValue(chanceBlockAttacks, Global["#% of Block Chance applied to Spells"][0]);
            if (hasShield && Global.ContainsKey("#% additional Chance to Block Spells with Shields"))
                chanceBlockSpells += Global["#% additional Chance to Block Spells with Shields"][0];
            if (chanceBlockAttacks > 0)
                def["Chance to Block Attacks: #%"] = new List<float>() { MaximumValue(RoundValue(chanceBlockAttacks, 0), maxChanceBlockAttacks) };
            if (chanceBlockSpells > 0)
                def["Chance to Block Spells: #%"] = new List<float>() { MaximumValue(RoundValue(chanceBlockSpells, 0), maxChanceBlockSpells) };

            // Elemental stataus ailments.
            float igniteAvoidance = 0;
            float chillAvoidance = 0;
            float freezeAvoidance = 0;
            float shockAvoidance = 0;
            if (Global.ContainsKey("#% chance to Avoid being Ignited"))
                igniteAvoidance += Global["#% chance to Avoid being Ignited"][0];
            if (Global.ContainsKey("#% chance to Avoid being Chilled"))
                chillAvoidance += Global["#% chance to Avoid being Chilled"][0];
            if (Global.ContainsKey("#% chance to Avoid being Frozen"))
                freezeAvoidance += Global["#% chance to Avoid being Frozen"][0];
            if (Global.ContainsKey("#% chance to Avoid being Shocked"))
                shockAvoidance += Global["#% chance to Avoid being Shocked"][0];
            if (Global.ContainsKey("#% chance to Avoid Elemental Status Ailments"))
            {
                float value = Global["#% chance to Avoid Elemental Status Ailments"][0];
                igniteAvoidance += value;
                chillAvoidance += value;
                freezeAvoidance += value;
                shockAvoidance += value;
            }
            if (Global.ContainsKey("Cannot be Ignited"))
                igniteAvoidance = 100;
            if (Global.ContainsKey("Cannot be Chilled"))
                chillAvoidance = 100;
            if (Global.ContainsKey("Cannot be Frozen"))
                freezeAvoidance = 100;
            if (Global.ContainsKey("Cannot be Shocked"))
                shockAvoidance = 100;
            if (igniteAvoidance > 0)
                def["Ignite Avoidance: #%"] = new List<float>() { igniteAvoidance };
            if (chillAvoidance > 0)
                def["Chill Avoidance: #%"] = new List<float>() { chillAvoidance };
            if (freezeAvoidance > 0)
                def["Freeze Avoidance: #%"] = new List<float>() { freezeAvoidance };
            if (shockAvoidance > 0)
                def["Shock Avoidance: #%"] = new List<float>() { shockAvoidance };

            List<ListGroup> groups = new List<ListGroup>();
            groups.Add(new ListGroup("Character", ch));
            groups.Add(new ListGroup("Defence", def));

            return groups;
        }

        // Returns attribute or mod value of an equipped item.
        public static List<float> GetItemAttributeValue(Item.ItemClass itemClass, string name)
        {
            Item item = Items.Find(i => i.Class == itemClass);
            if (item != null)
            {
                if (item.Attributes.ContainsKey(name))
                    return item.Attributes[name];

                Item.Mod mod = item.Mods.Find(m => m.Attribute == name);
                if (mod != null)
                    return mod.Value;
            }

            return new List<float>();
        }

        // Returns value increased by specified percentage.
        public static float IncreaseValueByPercentage(float value, float percentage)
        {
            return value * (100 + percentage) / 100;
        }

        // Initializes structures.
        public static void Initialize(SkillTree skillTree, ItemAttributes itemAttrs)
        {
            Items = itemAttrs.Equip;

            MainHand = new Weapon(Items.Find(i => i.Class == Item.ItemClass.MainHand));
            OffHand = new Weapon(Items.Find(i => i.Class == Item.ItemClass.OffHand));

            Level = skillTree._level;

            Global = new AttributeSet();

            Tree = new AttributeSet(skillTree.SelectedAttributesWithoutImplicit);
            Global.Add(Tree);

            Equipment = new AttributeSet();
            foreach (ItemAttributes.Attribute attr in itemAttrs.NonLocalMods)
                Equipment.Add(attr.TextAttribute, new List<float>(attr.Value));
            Global.Add(Equipment);

            Implicit = new AttributeSet(skillTree.ImplicitAttributes(Global));
            Global.Add(Implicit);

            // Keystones.
            Acrobatics = Tree.ContainsKey("#% Chance to Dodge Attacks. #% less Armour and Energy Shield");
            AvatarOfFire = Tree.ContainsKey("Deal no Non-Fire Damage");
            BloodMagic = Tree.ContainsKey("Removes all mana. Spend Life instead of Mana for Skills");
            ChaosInoculation = Tree.ContainsKey("Maximum Life becomes #, Immune to Chaos Damage");
            EldritchBattery = Tree.ContainsKey("Converts all Energy Shield to Mana");
            IronReflexes = Tree.ContainsKey("Converts all Evasion Rating to Armour. Dexterity provides no bonus to Evasion Rating");
            VaalPact = Tree.ContainsKey("Life Leech applies instantly at #% effectiveness. Life Regeneration has no effect.");
            ZealotsOath = Tree.ContainsKey("Life Regeneration applies to Energy Shield instead of Life");
        }

        // Returns value capped at specified maximum.
        public static float MaximumValue(float value, float maximum)
        {
            return value <= maximum ? value : maximum;
        }

        // Returns average damage done by monsters at specified character level.
        // @see http://pathofexile.gamepedia.com/Monster_Damage
        public static float MonsterAverageDamage(int level)
        {
            float a = 1.1512f;
            float b = 0.0039092f;
            float c = 17.789f;
            float d = -7.0896f;

            return (float)Math.Pow(a + b * level, c) + d;
        }

        // Computes offensive attacks.
        public static List<ListGroup> Offense()
        {
            List<ListGroup> groups = new List<ListGroup>();

            foreach (Item item in Items)
            {
                foreach (Item gem in item.Gems)
                {
                    if (AttackSkill.IsAttackSkill(gem))
                    {
                        // Skip gems linked to totems and Cast on gems for now.
                        if (item.GetLinkedGems(gem).Find(g => g.Name.Contains("Totem")
                                                              || g.Name.StartsWith("Cast on")) != null) continue;

                        AttackSkill attack = AttackSkill.Create(gem);

                        attack.Link(item.GetLinkedGems(gem));
                        attack.Apply();

                        groups.Add(attack.ToListGroup());
                    }
                }
            }

            return groups;
        }

        // Returns percent of value.
        public static float PercentOfValue(float value, float percentage)
        {
            return value * percentage / 100;
        }

        // Damage Reduction Factor = Armour / ( Armour + (12 * Damage) )
        // Damage reduction is capped at 90%.
        // @see http://pathofexile.gamepedia.com/Armour
        public static float PhysicalDamageReduction(int level, float armour)
        {
            float reduction = RoundValue(armour / (armour + 12 * MonsterAverageDamage(level)) * 100, 0);
            if (reduction > 90f) reduction = 90f;

            return reduction;
        }

        // Returns rounded value with zero fractional digits.
        public static float RoundValue(float value, int precision)
        {
            return (float)Math.Round((Decimal)value, precision, MidpointRounding.AwayFromZero);
        }
    }
}

/*
 * Skill gems:
 * ===========
 * Anger
 * Animate Guardian
 * Cleave
 * Decoy Totem
 * Determination
 * Devouring Totem
 * Dominating Blow
 * Enduring Cry
 * Flame Totem
 * Glacial Hammer                               Partial
 * Ground Slam
 * Heavy Strike
 * Herald of Ash
 * Immortal Call
 * Infernal Blow
 * Leap Slam
 * Lightning Strike
 * Molten Shell                                 Partial
 * Molten Strike
 * Punishment
 * Purity of Fire
 * Rejuvenation Totem
 * Searing Bond
 * Shield Charge
 * Shockwave Totem
 * Sweep
 * Vitality
 * Warlord's Mark
 * 
 * Animate Weapon
 * Arctic Armour
 * Barrage
 * Bear Trap
 * Blood Rage
 * Burning Arrow
 * Cyclone
 * Desecrate
 * Detonate Dead
 * Double Strike
 * Dual Strike
 * Elemental Hit
 * Ethereal Knives                              Partial
 * Explosive Arrow
 * Fire Trap
 * Flicker Strike
 * Freeze Mine
 * Frenzy
 * Grace
 * Haste
 * Hatred
 * Herald of Ice                                Partial (increase Spell Damage/Elemental Damage with Spells should be excluded)
 * Ice Shot
 * Lightning Arrow                              Partial
 * Poacher's Mark
 * Poison Arrow
 * Projectile Weakness
 * Puncture
 * Purity of Ice
 * Rain of Arrows
 * Reave
 * Smoke Mine
 * Spectral Throw
 * Split Arrow
 * Temporal Chains
 * Tornado Shot
 * Viper Strike
 * Whirling Blades
 *
 * Arc                                          Partial
 * Arctic Breath
 * Assassin's Mark
 * Ball Lightning
 * Bone Offering
 * Clarity
 * Cold Snap
 * Conductivity
 * Conversion Trap
 * Convocation
 * Critical Weakness
 * Discharge
 * Discipline
 * Elemental Weakness
 * Enfeeble
 * Fireball
 * Firestorm
 * Flameblast
 * Flame Surge
 * Flammability
 * Flesh Offering
 * Freezing Pulse
 * Frost Wall
 * Frostbite
 * Glacial Cascade
 * Ice Nova
 * Ice Spear
 * Incinerate
 * Lightning Trap
 * Lightning Warp
 * Power Siphon
 * Purity of Elements
 * Purity of Lightning
 * Raise Spectre
 * Raise Zombie
 * Righteous Fire
 * Shock Nova
 * Spark
 * Storm Call
 * Summon Raging Spirit
 * Summon Skeletons
 * Tempest Shield
 * Vulnerability
 * Wrath
 * 
 * Support gems:
 * =============
 * Added Chaos Damage
 * Added Cold Damage
 * Added Fire Damage
 * Added Lightning Damage                       Partial
 * Additional Accuracy
 * Blind
 * Block Chance Reduction
 * Blood Magic
 * Cast on Critical Strike
 * Cast on Death
 * Cast on Melee Kill
 * Cast when Damage Taken
 * Cast when Stunned
 * Chain
 * Chance to Flee
 * Chance to Ignite
 * Cold Penetration
 * Cold to Fire                                 Partial
 * Concentrated Effect                          Partial
 * Culling Strike
 * Curse on Hit
 * Elemental Proliferation
 * Empower
 * Endurance Charge on Melee Stun
 * Enhance
 * Enlighten
 * Faster Attacks
 * Faster Casting
 * Faster Projectiles                           Partial
 * Fire Penetration
 * Fork
 * Generosity
 * Greater Multiple Projectiles
 * Increased Area of Effect
 * Increased Burning Damage
 * Increased Critical Damage
 * Increased Critical Strikes
 * Increased Duration
 * Iron Grip
 * Iron Will
 * Item Quantity
 * Item Rarity
 * Knockback
 * Lesser Multiple Projectiles
 * Life Gain on Hit
 * Life Leech
 * Lightning Penetration
 * Mana Leech
 * Melee Damage on Full Life                    Partial
 * Melee Physical Damage
 * Melee Splash
 * Minion and Totem Elemental Resistance
 * Minion Damage
 * Minion Life
 * Minion Speed
 * Multiple Traps
 * Multistrike
 * Physical Projectile Attack Damage
 * Pierce
 * Point Blank
 * Power Charge On Critical
 * Ranged Attack Totem
 * Reduced Duration
 * Reduced Mana
 * Remote Mine
 * Slower Projectiles
 * Spell Echo
 * Spell Totem
 * Stun
 * Trap
 * Weapon Elemental Damage                      Partial
 */