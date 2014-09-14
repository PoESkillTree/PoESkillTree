using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.ViewModels;
using Item = POESKillTree.ViewModels.ItemAttributes.Item;
using Mod = POESKillTree.ViewModels.ItemAttributes.Item.Mod;

namespace POESKillTree.SkillTreeFiles
{
    public class Compute
    {
        public enum AttackType
        {
            Attack, Spell
        }

        // TODO: Multi-part attacks Lightning Strike, Molten Strike (Melee + Projectile)
        // TODO: Weapon local attributes.
        public class Attack
        {
            // The name.
            string Name;
            // The type.
            AttackType Type;
            // The nature of attack (based on gem keywords).
            public DamageNature Nature;
            // List of attack sources (either spell or main hand and/or off hand).
            List<AttackSource> Sources;
            // Skill gem local attributes.
            public Dictionary<string, List<float>> Local;
            // Damage effectiveness.
            float Effectiveness;
            // List of damage conversions.
            List<Damage.Converted> Converts = new List<Damage.Converted>();
            // List of damage gains.
            List<Damage.Gained> Gains = new List<Damage.Gained>();

            // The sorted list of damage types for character sheet.
            static List<DamageType> DamageTypes = new List<DamageType>()
            {
                DamageType.Physical, DamageType.Fire, DamageType.Cold, DamageType.Lightning, DamageType.Chaos
            };

            // Creates attack from gem.
            Attack(Item gem)
            {
                Name = gem.Name;
                Type = gem.Keywords.Contains("Attack") ? AttackType.Attack : AttackType.Spell;
                Nature = new DamageNature(gem.Keywords);

                Local = new Dictionary<string, List<float>>();
                foreach (Mod mod in gem.Mods)
                    Local.Add(mod.Attribute, new List<float>(mod.Value));

                Effectiveness = gem.Attributes.ContainsKey("Damage Effectiveness:  #%") ? gem.Attributes["Damage Effectiveness:  #%"][0] : 100;

                Sources = AttackSource.GetSources(this);
            }

            // Applies attributes.
            public void Apply()
            {
                // Damage conversion from gems, equipment and tree.
                foreach (var attr in Local)
                {
                    Damage.Converted conv = Damage.Converted.Create(DamageConversionSource.Gem, attr);
                    if (conv != null) Converts.Add(conv);
                }
                foreach (var attr in Equipment)
                {
                    Damage.Converted conv = Damage.Converted.Create(DamageConversionSource.Equipment, attr);
                    if (conv != null) Converts.Add(conv);
                }
                foreach (var attr in Tree)
                {
                    Damage.Converted conv = Damage.Converted.Create(DamageConversionSource.Tree, attr);
                    if (conv != null) Converts.Add(conv);
                }

                // Merge local gem and global attributes.
                Dictionary<string, List<float>> attrs = Compute.Merge(Global, Local);

                foreach (AttackSource source in Sources)
                {
                    List<Damage.Increased> increases = new List<Damage.Increased>();
                    List<Damage.More> mores = new List<Damage.More>();

                    foreach (var attr in attrs)
                    {
                        Damage.Added added = Damage.Added.Create(Nature, attr);
                        if (added != null) added.Apply(source.Deals, Effectiveness);

                        Damage.Gained gained = Damage.Gained.Create(attr);
                        if (gained != null) Gains.Add(gained);

                        Damage.Increased increased = Damage.Increased.Create(attr);
                        if (increased != null) increases.Add(increased);

                        Damage.More more = Damage.More.Create(attr);
                        if (more != null) mores.Add(more);
                    }

                    Convert(source.Deals);

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
            public static Attack Create(Item gem)
            {
                return new Attack(gem);
            }

            // The flag whether attack is an attack.
            public bool IsAttack
            {
                get
                {
                    return Type == AttackType.Attack;
                }
            }

            // Returns true if gem is an attack skill, false otherwise.
            public static bool IsAttackSkill(Item gem)
            {
                // A gem is an attack if it has Attack or Spell keyword and it has damage dealing mod.
                return (gem.Keywords.Contains("Attack") || gem.Keywords.Contains("Spell"))
                        && gem.Mods.Find(mod => mod.Attribute.StartsWith("Deals")) != null;
            }

             // Links support gems.
            // TODO: In case of same gems slotted only highest level one is used.
            public void Link(List<Item> gems)
            {
                foreach (Item gem in gems)
                {
                    if (!gem.Keywords.Contains("Support")) continue; // Skip non-support gems.

                    foreach (Mod mod in gem.Mods)
                        Local.Add(mod.Attribute, new List<float>(mod.Value));
                }
            }

            // Return list group of this attack.
            public ListGroup ToListGroup()
            {
                Dictionary<string, List<float>> props = new Dictionary<string,List<float>>();

                foreach (AttackSource source in Sources)
                    foreach (DamageType type in DamageTypes)
                    {
                        List<Damage> deals = source.Deals.FindAll(d => d.Is(type));
                        if (deals.Count > 0)
                        {
                            if (deals.Count > 1)
                                for (int i = 1; i < deals.Count; ++i)
                                    deals[0].Add(deals[i]);
                            props.Add(source.Source + " " + deals[0].ToAttribute(), deals[0].ToValue());
                        }
                    }

                return new ListGroup(Name, props);
            }
        }

        // TODO: Local attributes of weapons (Added damage, increase Physical Damage, etc.).
        public class AttackSource
        {
            // List of damage dealt.
            public List<Damage> Deals;
            // The source name.
            public string Source;

            AttackSource(string source, List<Damage> deals)
            {
                Source = source;
                Deals = deals;
            }

            // Returns sources of attack (spell, main hand and/or off hand).
            public static List<AttackSource> GetSources(Attack attack)
            {
                List<AttackSource> sources = new List<AttackSource>();

                if (attack.IsAttack)
                {
                    foreach (Item weapon in Compute.GetWeapons())
                    {
                        List<Damage> deals = new List<Damage>();

                        foreach (var attr in weapon.Attributes)
                        {
                            Damage damage = Damage.Create(attack.Nature, attr);
                            if (damage != null) deals.Add(damage);
                        }

                        if (deals.Count > 0)
                            sources.Add(new AttackSource(weapon.Class == Item.ItemClass.MainHand ? "Main Hand" : "Off Hand", deals));
                    }
                }
                else
                {
                    List<Damage> deals = new List<Damage>();

                    foreach (var attr in attack.Local)
                    {
                        Damage damage = Damage.Create(attack.Nature, attr);
                        if (damage != null) deals.Add(damage);
                    }

                    if (deals.Count > 0)
                        sources.Add(new AttackSource("Spell", deals));
                }

                return sources;
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
            Any, Melee, Projectile, 
        }

        public enum DamageSource
        {
            Any, Weapon, Spell
        }

        public enum DamageType
        {
            Any, Physical = 1, Fire = 2, Cold = 4, Lightning = 8, Elemental = Cold | Fire | Lightning, Chaos = 16
        }

        public enum DamageWeaponType // Tempest Blast keystone?
        {
            Any, Wand
        }

        public class DamageNature
        {
            protected DamageArea Area = DamageArea.Any;
            protected DamageForm Form = DamageForm.Any;
            protected DamageSource Source = DamageSource.Any;
            protected DamageType Type = DamageType.Any;
            //DamageWeaponType WeaponType = DamageWeaponType.Any; // Templest Blast keystone?

            static Dictionary<string, DamageArea> Areas = new Dictionary<string, DamageArea>()
            {
                { "AoE",        DamageArea.Area },
                { "Area",       DamageArea.Area }
            };
            static Dictionary<string, DamageForm> Forms = new Dictionary<string, DamageForm>()
            {
                { "Melee",      DamageForm.Melee },
                { "Projectile", DamageForm.Projectile }
            };
            static Dictionary<string, DamageSource> Sources = new Dictionary<string, DamageSource>()
            {
                { "Attack",     DamageSource.Weapon },
                { "Spell",      DamageSource.Spell },
                { "Weapon",     DamageSource.Weapon }
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

            public DamageNature()
            {
            }

            public DamageNature(DamageNature nature)
            {
                Area = nature.Area;
                Form = nature.Form;
                Source = nature.Source;
                Type = nature.Type;
            }

            public DamageNature(DamageNature nature, string str)
            {
                Area = nature.Area;
                Form = nature.Form;
                Source = nature.Source;
                Type = nature.Type;

                string[] words = str.Split(' ');
                foreach (string word in words)
                {
                    if (Types.ContainsKey(word)) Type = Types[word];
                    else if (Sources.ContainsKey(word)) Source = Sources[word];
                    else if (Forms.ContainsKey(word)) Form = Forms[word];
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
                    else if (Forms.ContainsKey(word)) Form = Forms[word];
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
                    else if (Forms.ContainsKey(word)) Form = Forms[word];
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
                    else if (Areas.ContainsKey(word)) Area = Areas[word];
                }
            }

            public bool Is (DamageType type)
            {
                return (Type & type) != 0;
            }

            public bool Matches(DamageNature nature)
            {
                return (Area == DamageArea.Any || nature.Area == Area)
                       && (Form == DamageForm.Any || nature.Form == Form)
                       && (Source == DamageSource.Any || nature.Source == Source)
                       && (Type == DamageType.Any || (nature.Type & Type) != 0);
            }
        }

        public class Damage : DamageNature
        {
            public class Added : DamageNature
            {
                // The added damage minimum.
                private float Min;
                // The added damage maximum.
                private float Max;

                static Regex ReAddMod = new Regex("Adds #-# ([^ ]+) Damage");

                public Added(DamageNature nature, string type, float min, float max)
                    : base(nature, type)
                {
                    Min = min;
                    Max = max;
                }

                // Creates added damage.
                public static Added Create(DamageNature nature, KeyValuePair<string, List<float>> attr)
                {
                    Match m = ReAddMod.Match(attr.Key);
                    if (m.Success)
                    {
                        return new Added(nature, m.Groups[1].Value, attr.Value[0], attr.Value[1]);
                    }

                    return null;
                }

                // Applies modifier.
                public void Apply(List<Damage> deals, float effectiveness)
                {
                    Damage damage = new Damage(this, Min, Max);

                    damage.Mul(effectiveness);

                    deals.Add(damage);
                }
            }

            // TODO: Tempest Blast: 30% of Wand Physical Damage Added as Lightning Damage
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
                            return new Increased(m.Groups[3].Value == "Spells" ? DamageSource.Spell : DamageSource.Weapon, m.Groups[2].Value, m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0]);
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

                    return null;
                }

                // Applies modifier.
                public void Apply(Damage damage)
                {
                    damage.Mul(100 + Percent);
                }
            }

            // The damage nature from which this damage originated (due to conversion).
            DamageNature Origin;
            // The damage range minimum.
            float Min;
            // The damage range maximum.
            float Max;

            static Regex ReDamageAttribute = new Regex("([^ ]+) Damage:  #-#");
            static Regex ReDamageMod = new Regex("Deals #-# ([^ ]+) Damage");

            // Creates damage with same origin as specified damage but with different damage type.
            Damage(Damage damage, DamageType type, float min, float max)
                : base(damage)
            {
                Origin = new DamageNature(damage.Origin);
                Type = type;
                Min = min;
                Max = max;
            }

            // Creates damage with specified nature.
            Damage(DamageNature nature, float min, float max)
                : base(nature)
            {
                Origin = new DamageNature(this);
                Min = min;
                Max = max;
            }

            Damage(DamageNature nature, string type, float min, float max)
                : base(nature, type)
            {
                Origin = new DamageNature(this);
                Min = min;
                Max = max;
            }

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
                return new Damage(this, type, Min * percent / 100, Max * percent / 100);
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
                return new List<float>() { (float)Math.Round((Double)Min, MidpointRounding.AwayFromZero), (float)Math.Round((Double)Max, MidpointRounding.AwayFromZero) };
            }
        }

        // Equipped items.
        public static List<Item> Items;
        // Equipment attributes.
        public static Dictionary<string, List<float>> Equipment;
        // All global attributes (includes tree, equipment, implicit).
        public static Dictionary<string, List<float>> Global;
        // Implicit attributes derived from base attributes and level (e.g. Life, Mana).
        public static Dictionary<string, List<float>> Implicit;
        // Skill tree attributes (includes base attributes).
        public static Dictionary<string, List<float>> Tree;
        // Skill tree keystones.
        public static bool AvatarOfFire;

        // Returns equipped weapons.
        public static List<Item> GetWeapons()
        {
            return Items.FindAll(i => i.Class == Item.ItemClass.MainHand || i.Class == Item.ItemClass.OffHand);
        }

        // Includes attributes into target dictionary.
        public static void Include(Dictionary<string, List<float>> target, Dictionary<string, List<float>> include)
        {
            foreach (var attr in include)
                if (target.ContainsKey(attr.Key))
                {
                    if (attr.Value.Count > 0)
                        for (int i = 0; i < attr.Value.Count; ++i)
                            target[attr.Key][i] += attr.Value[i];
                }
                else target.Add(attr.Key, new List<float>(attr.Value));
        }

        // Initializes structures.
        public static void Initialize(SkillTree skillTree, ItemAttributes itemAttrs)
        {
            Items = itemAttrs.Equip;

            Global = new Dictionary<string, List<float>>();

            Tree = skillTree.SelectedAttributesWithoutImplicit;
            Include(Global, Tree);

            Equipment = new Dictionary<string, List<float>>();
            foreach (ItemAttributes.Attribute attr in itemAttrs.NonLocalMods)
                Equipment.Add(attr.TextAttribute, new List<float>(attr.Value));
            Include(Global, Equipment);

            Implicit = skillTree.ImplicitAttributes(Global);
            Include(Global, Implicit);

            // Keystones.
            AvatarOfFire = Tree.ContainsKey("Deal no Non-Fire Damage");
        }

        // Returns new attribute set merged from two attribute sets.
        public static Dictionary<string, List<float>> Merge(Dictionary<string, List<float>> attrs1, Dictionary<string, List<float>> attrs2)
        {
            Dictionary<string, List<float>> merged = new Dictionary<string, List<float>>();

            // Copy first set.
            foreach (var attr in attrs1)
                merged.Add(attr.Key, new List<float>(attr.Value));

            // Merge second set.
            foreach (var attr in attrs2)
                if (merged.ContainsKey(attr.Key))
                {
                    if (attr.Value.Count > 0)
                        for (int i = 0; i < attr.Value.Count; ++i)
                            merged[attr.Key][i] += attr.Value[i];
                }
                else merged.Add(attr.Key, new List<float>(attr.Value));

            return merged;
        }

        // Computes offensive attacks.
        public static List<ListGroup> Offense()
        {
            List<ListGroup> groups = new List<ListGroup>();

            foreach (Item item in Items)
            {
                foreach (Item gem in item.Gems)
                {
                    if (Attack.IsAttackSkill(gem))
                    {
                        // Skip totems, Cast on X for now.
                        if (item.GetLinkedGems(gem).Find(g => g.Name.Contains("Totem")) != null
                            || item.GetLinkedGems(gem).Find(g => g.Name.StartsWith("Cast on")) != null) continue;

                        Attack attack = Attack.Create(gem);

                        attack.Link(item.GetLinkedGems(gem));
                        attack.Apply();

                        groups.Add(attack.ToListGroup());
                    }
                }
            }

            return groups;
        }

    }
}
