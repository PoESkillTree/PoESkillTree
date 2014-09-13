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
        public class Attack
        {
            // Name.
            private string Name;
            // Type (either Attack or Spell).
            private AttackType Type;
            // Local attributes.
            private Dictionary<string, List<float>> Local;
            // Damage effectiveness.
            private float Effectiveness;

            // List of all damage added.
            private List<Damage.Added> Adds = new List<Damage.Added>();
            // List of damage converted.
            private List<Damage.Converted> Converts = new List<Damage.Converted>();
            // List of all damage it deals.
            private List<Damage> Deals = new List<Damage>();
            // List of all damage gained.
            private List<Damage.Gained> Gains = new List<Damage.Gained>();
            // List of all damage multipliers.
            private List<Damage.More> Mores = new List<Damage.More>();
            private bool HasAoE;
            // Flag whether attack has melee component.
            private bool HasMelee;
            // Flag whether attack has projectile component.
            private bool HasProjectile;
            // The list of damage sources.
            private static List<DamageSource> DamageSources = new List<DamageSource>()
            {
                DamageSource.MainHand, DamageSource.OffHand, DamageSource.Spell
            };
            // The damage source names.
            private static Dictionary<DamageSource, string> DamageSourceNames = new Dictionary<DamageSource, string>()
            {
                { DamageSource.MainHand, "Main Hand" },
                { DamageSource.OffHand, "Off Hand"},
                { DamageSource.Spell, "Spell"}
            };
            // The list of damage types.
            private static List<DamageType> DamageTypes = new List<DamageType>()
            {
                DamageType.Physical, DamageType.Fire, DamageType.Cold, DamageType.Lightning, DamageType.Chaos
            };

            private Attack(Item gem)
            {
                Name = gem.Name;
                Type = gem.Keywords.Contains("Attack") ? AttackType.Attack : AttackType.Spell;

                Local = new Dictionary<string, List<float>>();
                foreach (Mod mod in gem.Mods)
                    Local.Add(mod.Attribute, new List<float>(mod.Value));

                Effectiveness = gem.Attributes.ContainsKey("Damage Effectiveness:  #%") ? gem.Attributes["Damage Effectiveness:  #%"][0] : 100;

                HasAoE = gem.Keywords.Contains("AoE");
                HasMelee = gem.Keywords.Contains("Melee");
                HasProjectile = gem.Keywords.Contains("Projectile");
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

                // Merge local and global attributes.
                Dictionary<string, List<float>> attrs = Compute.Merge(Global, Local);

                // Gather damage modifiers.
                foreach (var attr in attrs)
                {
                    Damage.Added added = Damage.Added.Create(attr);
                    if (added != null) Adds.Add(added);

                    Damage.Gained gained = Damage.Gained.Create(attr);
                    if (gained != null) Gains.Add(gained);

                    Damage.More more = Damage.More.Create(attr);
                    if (more != null) Mores.Add(more);

                    // Spell damage.
                    //Damage damage = Damage.Create(attr);
                    //if (damage != null) Deals.Add(damage);
                }

                float incPhysicalDamage = 0;
                float incFireDamage = 0;
                float incColdDamage = 0;
                float incLightningDamage = 0;
                float incElementalDamage = 0;

                if (attrs.ContainsKey("#% increased Physical Damage"))
                    incPhysicalDamage += attrs["#% increased Physical Damage"][0];
                if (attrs.ContainsKey("#% increased Fire Damage"))
                    incFireDamage += attrs["#% increased Fire Damage"][0];
                if (attrs.ContainsKey("#% increased Cold Damage"))
                    incColdDamage += attrs["#% increased Cold Damage"][0];
                if (attrs.ContainsKey("#% increased Lightning Damage"))
                    incLightningDamage += attrs["#% increased Lightning Damage"][0];
                if (attrs.ContainsKey("#% increased Elemental Damage"))
                    incElementalDamage += attrs["#% increased Elemental Damage"][0];

                float incAoE = 0;
                if (HasAoE && attrs.ContainsKey("#% increased Area Damage"))
                    incAoE += attrs["#% increased Area Damage"][0];

                if (Type == AttackType.Spell)
                {
                    float incSpellDamage = 0;
                    float incElementalSpellDamage = 0;

                    if (attrs.ContainsKey("#% increased Spell Damage"))
                        incSpellDamage += attrs["#% increased Spell Damage"][0];
                    if (attrs.ContainsKey("#% reduced Spell Damage"))
                        incSpellDamage -= attrs["#% reduced Spell Damage"][0];

                    if (attrs.ContainsKey("#% increased Elemental Damage with Spells"))
                        incElementalSpellDamage += attrs["#% increased Elemental Damage with Spells"][0];

                    foreach (Damage.Added added in Adds)
                        added.Apply(Deals, Effectiveness);

                    foreach (Damage damage in Deals)
                    {
                        float inc = incSpellDamage + incAoE; // Herald of Ice doesn't get Spell Damage applied.
                        if (damage.Type == DamageType.Physical)
                            inc += incPhysicalDamage;
                        if (damage.Type == DamageType.Fire)
                            inc += incFireDamage;
                        if (damage.Type == DamageType.Cold)
                            inc += incColdDamage;
                        if (damage.Type == DamageType.Lightning)
                            inc += incLightningDamage;
                        if (damage.IsElemental())
                            inc += incElementalSpellDamage + incElementalDamage; // // Herald of Ice doesn't get Elemental Damage with Spells applied.

                        if (inc > 0) damage.Increase(inc);

                        foreach (Damage.More more in Mores)
                            if (more.CanApply(damage)/* && HasMelee == more.IsMelee*/)
                                more.Apply(damage);

                        damage.Source = DamageSource.Spell;
                    }
                }
                else // Type == AttackType.Attack
                {
                    float incMeleeDamage = 0;
                    float incMeleePhysicalDamage = 0;
                    if (HasMelee)
                    {
                        if (attrs.ContainsKey("#% increased Melee Damage"))
                            incMeleeDamage += attrs["#% increased Melee Damage"][0];
                        if (attrs.ContainsKey("+#% increased Melee Physical Damage"))
                            incMeleePhysicalDamage += attrs["+#% increased Melee Physical Damage"][0];
                    }

                    float incElementalWeaponDamage = 0;
                    if (attrs.ContainsKey("#% increased Elemental Damage with Weapons"))
                        incElementalWeaponDamage += attrs["#% increased Elemental Damage with Weapons"][0];

                    List<Damage> weapon = Compute.GetWeaponDamage(Item.ItemClass.MainHand);

                    foreach (Damage.Added added in Adds)
                        added.Apply(weapon, Effectiveness);

                    Convert(weapon);

                    foreach (Damage damage in weapon)
                    {
                        float inc = incAoE;
                        if (HasMelee) inc += incMeleeDamage;
                        if (damage.Type == DamageType.Physical || damage.Origin == DamageType.Physical) // Physical damage bonuses applies to converted non-physical damage.
                        {
                            inc += incPhysicalDamage;
                            if (HasMelee) inc += incMeleePhysicalDamage;
                        }
                        if (damage.Type == DamageType.Fire)
                            inc += incFireDamage;
                        if (damage.Type == DamageType.Cold)
                            inc += incColdDamage;
                        if (damage.Type == DamageType.Lightning)
                            inc += incLightningDamage;
                        if (damage.IsElemental())
                            inc += incElementalDamage + incElementalWeaponDamage;

                        if (inc > 0) damage.Increase(inc);

                        foreach (Damage.More more in Mores)
                            if (more.CanApply(damage)/* && HasMelee == more.IsMelee*/)
                                more.Apply(damage);

                        damage.Source = DamageSource.MainHand;

                        Deals.Add(damage);
                    }
                }

                // Avatar of Fire (remove non-Fire damage).
                if (AvatarOfFire)
                    foreach (Damage damage in new List<Damage>(Deals))
                        if (damage.Type != DamageType.Fire)
                            Deals.Remove(damage);
            }

            // Converts damage types (applies damage conversions and gains).
            public void Convert(List<Damage> deals)
            {
                foreach (DamageType type in DamageTypes)
                {
                    List<Damage> convert = deals.FindAll(d => d.Type == type);
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

                    // Remove processed input and merge output.
                    deals.RemoveAll(d => d.Type == type);
                    deals.AddRange(output);
                }
            }

            // Creates attack from gem.
            // TODO: Multiform/Multipart/Multicomponent skills.
            public static Attack Create(Item gem)
            {
                return new Attack(gem);
            }

            // Returns true if gem is an attack skill, false otherwise.
            public static bool IsAttack(Item gem)
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

                foreach (DamageSource source in DamageSources)
                    foreach (DamageType type in DamageTypes)
                    {
                        List<Damage> deals = Deals.FindAll(d => d.Source == source && d.Type == type);
                        if (deals.Count > 0)
                        {
                            if (deals.Count > 1)
                                for (int i = 1; i < deals.Count; ++i)
                                    deals[0].Add(deals[i]);
                            props.Add(DamageSourceNames[source] + " " + deals[0].ToAttribute(), deals[0].ToValue());
                        }
                    }

                return new ListGroup(Name, props);
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

        // Multiple form spells: Lightning Strike, Molten Strike (Melee + Projectile)
        public enum DamageForm
        {
            Any, Melee, Projectile, 
        }

        public enum DamageSource
        {
            MainHand, OffHand, Spell
        }

        public enum DamageType
        {
            Any, Physical = 1, Cold = 2, Fire = 4, Lightning = 8, Chaos = 16
        }

        public enum DamageWeaponType // Tempest Blast keystone?
        {
            Any, Wand
        }

        public class DamageNature
        {
            DamageArea Area = DamageArea.Any;
            DamageForm Form = DamageForm.Any;
            DamageType Type = DamageType.Any;
            //DamageWeaponType WeaponType = DamageWeaponType.Any; // Templest Blast keystone?

            static Dictionary<string, DamageArea> Areas = new Dictionary<string, DamageArea>()
            {
                { "Area",       DamageArea.Area }
            };
            static Dictionary<string, DamageForm> Forms = new Dictionary<string, DamageForm>()
            {
                { "Melee",      DamageForm.Melee },
                { "Projectile", DamageForm.Projectile }
            };
            static Dictionary<string, DamageType> Types = new Dictionary<string, DamageType>()
            {
                { "Physical",   DamageType.Physical },
                { "Cold",       DamageType.Cold },
                { "Fire",       DamageType.Fire },
                { "Lightning",  DamageType.Lightning },
                { "Chaos",      DamageType.Chaos }
            };

            // Nature constructor from string containing keywords.
            public DamageNature(string str)
            {

            }
        }

        public class Damage
        {
            public class Added
            {
                // The added damage minimum.
                private float Min;
                // The added damage maximum.
                private float Max;
                // The added damage type.
                DamageType Type;

                static Regex ReAddMod = new Regex("Adds #-# ([^ ]+) Damage");

                public Added(DamageType type, float min, float max)
                {
                    Type = type;
                    Min = min;
                    Max = max;
                }

                // Creates added damage.
                public static Added Create(KeyValuePair<string, List<float>> attr)
                {
                    Match m = ReAddMod.Match(attr.Key);
                    if (m.Success)
                    {
                        return new Added(Damage.DamageTypes[m.Groups[1].Value], attr.Value[0], attr.Value[1]);
                    }

                    return null;
                }

                // Applies damage modifier.
                public void Apply(List<Damage> deals, float damageEffectivness)
                {
                    Damage damage = deals.Find(d => d.Type == Type);

                    Damage add = new Damage(Type, Min, Max);
                    add.Mul(damageEffectivness);

                    if (damage == null)
                        deals.Add(add);
                    else
                        damage.Add(add);
                }
            }

            // TODO: Conversion chaining: Physical => Fire (affected by physical & fire mods) => Cold (affected by physical, fire & cold mods).
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
                        return new Converted(source, attr.Value[0], Damage.DamageTypes[m.Groups[1].Value], Damage.DamageTypes[m.Groups[2].Value]);
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
                        return new Gained(attr.Value[0], Damage.DamageTypes[m.Groups[1].Value], Damage.DamageTypes[m.Groups[2].Value]);
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

            public class More
            {
               // The percentage of "more" damage multiplier.
                float Percent;
                // The applicable damage type.
                DamageType Type;
                // The flag whether it is melee multiplier.
                public bool IsMelee = false;

                public More(DamageType type, float percent)
                {
                    Percent = percent;
                    Type = type;
                }

                // Returns true if multiplier can be applied to damage, false otherwise.
                public bool CanApply(Damage damage)
                {
                    return Type == DamageType.Any || damage.Type == Type || damage.Origin == Type;
                }

                // Creates damage multiplier.
                public static More Create(KeyValuePair<string, List<float>> attr)
                {
                    More more = null;

                    if (attr.Key == "#% less Damage")
                    {
                        more = new More(DamageType.Any, -attr.Value[0]);
                    }
                    else if (attr.Key == "#% more Damage")
                    {
                        more = new More(DamageType.Any, attr.Value[0]);
                    }
                    else if (attr.Key == "Deals #% of Base Damage")
                    {
                        more = new More(DamageType.Any, attr.Value[0] - 100);
                        //more.IsMelee = true; // Doesn't need to be melee.
                    }
                    else if (attr.Key == "#% more Melee Physical Damage when on Full Life")
                    {
                        more = new More(DamageType.Physical, attr.Value[0]);
                        //more.IsMelee = true;
                    }

                    return more;
                }

                // Applies "more" damage multiplier.
                public void Apply(Damage damage)
                {
                    damage.Mul(100 + Percent);
                }
            }
 
            // The damage range minimum.
            private float Min;
            // The damage range maximum.
            private float Max;
            // The damage type.
            public DamageType Type;
            // The damage type from which this damage originated (due to conversion).
            public DamageType Origin;
            // The source of damage.
            public DamageSource Source;

            Damage(DamageType type, float min, float max)
            {
                Type = Origin = type;
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
            public static Damage Create(KeyValuePair<string, List<float>> attr)
            {
                Match m = ReDamageAttribute.Match(attr.Key);
                if (m.Success)
                {
                    return new Damage(DamageTypes[m.Groups[1].Value], attr.Value[0], attr.Value[1]);
                }

                return null;
            }

            // Increases damage by percent.
            public void Increase(float percent)
            {
                Min = Min * (100 + percent) / 100;
                Max = Max * (100 + percent) / 100;
            }

            public bool IsElemental()
            {
                return Type == DamageType.Fire || Type == DamageType.Cold || Type == DamageType.Lightning;
            }

            // Returns percent of damage.
            public Damage PercentOf(float percent, DamageType type)
            {
                Damage damage = new Damage(type, Min * percent / 100, Max * percent / 100);

                // Preserve origin and source.
                damage.Origin = Origin;
                damage.Source = Source;

                return damage;
            }

            // Multiplies damage by percent.
            public void Mul(float percent)
            {
                Min = Min * percent / 100;
                Max = Max * percent / 100;
            }

            // Substracts damage.
            public void Sub(Damage damage)
            {
                Min -= damage.Min;
                Max -= damage.Max;
            }

            public string ToAttribute()
            {
                return Type.ToString() + " Damage: #-#";
            }

            public List<float> ToValue()
            {
                return new List<float>() { (float)Math.Round((Double)Min, MidpointRounding.AwayFromZero), (float)Math.Round((Double)Max, MidpointRounding.AwayFromZero) };
            }

            static Regex ReDamageAttribute = new Regex("([^ ]+) Damage:  #-#");
            static Regex ReDamageMod = new Regex("Deals #-# ([^ ]+) Damage");
            static Dictionary<string, DamageForm> DamageForms = new Dictionary<string, DamageForm>()
            {
                { "Melee",      DamageForm.Melee },
                { "Projectile", DamageForm.Projectile }
            };
            static Dictionary<string, DamageType> DamageTypes = new Dictionary<string, DamageType>()
            {
                { "Physical",   DamageType.Physical },
                { "Cold",       DamageType.Cold },
                { "Fire",       DamageType.Fire },
                { "Lightning",  DamageType.Lightning },
                { "Chaos",      DamageType.Chaos }
            };
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

        // Returns damage dealt by weapon.
        public static List<Damage> GetWeaponDamage(Item.ItemClass itemClass)
        {
            List<Damage> deals = new List<Damage>();

            foreach (Item item in Items)
            {
                if (item.Class == itemClass)
                {
                    foreach (var attr in item.Attributes)
                    {
                        Damage damage = Damage.Create(attr);
                        if (damage != null) deals.Add(damage);
                    }

                    break;
                }
            }

            return deals;
        }

        // Includes attributes into target.
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
                    if (Attack.IsAttack(gem))
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
