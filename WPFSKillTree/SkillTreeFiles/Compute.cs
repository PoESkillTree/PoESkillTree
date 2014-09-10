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
            // List of all damage added.
            private List<Damage.Added> Adds = new List<Damage.Added>();
            // List of all damage converted.
            private List<Damage.Converted> Converts = new List<Damage.Converted>();
            // List of all damage it deals.
            private List<Damage> Deals = new List<Damage>();
            // List of all damage multipliers.
            private List<Damage.More> Mores = new List<Damage.More>();
            private bool HasAoE;
            // Flag whether attack has melee component.
            private bool HasMelee;
            // Flag whether attack has projectile component.
            private bool HasProjectile;
            // Name.
            private string Name;
            // Type (either Attack or Spell).
            private AttackType Type;
            // Damage Effectiveness.
            private float DamageEffectivness;
            // Local mods.
            private List<Mod> Mods;

            private Attack(Item gem)
            {
                Name = gem.Name;
                Type = gem.Keywords.Contains("Attack") ? AttackType.Attack : AttackType.Spell;
                Mods = gem.Mods;

                DamageEffectivness = gem.Attributes.ContainsKey("Damage Effectiveness:  #%") ? gem.Attributes["Damage Effectiveness:  #%"][0] : 100;

                HasAoE = gem.Keywords.Contains("AoE");
                HasMelee = gem.Keywords.Contains("Melee");
                HasProjectile = gem.Keywords.Contains("Projectile");

                foreach (Mod mod in gem.Mods)
                {
                    Damage.Converted conv = Damage.Converted.Create(mod);
                    if (conv != null) Converts.Add(conv);

                    Damage.More more = Damage.More.Create(mod);
                    if (more != null) Mores.Add(more);

                    Damage damage = Damage.Create(mod);
                    if (damage != null) Deals.Add(damage);
                }
            }

            // Applies modifiers to attack.
            public void Apply(Dictionary<string, List<float>> globalAttrs, ItemAttributes itemAttrs)
            {
                // Merge global attributes and local mods.
                Dictionary<string, List<float>> attrs = Compute.Merge(globalAttrs, Mods);

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
                        added.Apply(Deals, DamageEffectivness);

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
                            if (more.CanApply(damage) && HasMelee == more.IsMelee)
                                more.Apply(damage);

                        damage.Source = "Spell";
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

                    List<Damage> weapon = itemAttrs.GetWeaponDamage(Item.ItemClass.MainHand);

                    foreach (Damage.Converted conv in Converts)
                        conv.Apply(weapon);

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
                            if (more.CanApply(damage) && HasMelee == more.IsMelee)
                                more.Apply(damage);

                        damage.Source = "Main Hand";

                        Damage exists = Deals.Find(d => d.Type == damage.Type);
                        if (exists != null) exists.Add(damage);
                        else Deals.Add(damage);
                    }

                    weapon = itemAttrs.GetWeaponDamage(Item.ItemClass.OffHand);

                    foreach (Damage.Converted conv in Converts)
                        conv.Apply(weapon);

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
                            if (more.CanApply(damage) && HasMelee == more.IsMelee)
                                more.Apply(damage);

                        damage.Source = "Off Hand";

                        Deals.Add(damage);
                    }
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
            public void Link(List<Item> gems)
            {
                foreach (Item gem in gems)
                {
                    if (!gem.Keywords.Contains("Support")) continue; // Skip non-support gems.

                    foreach (Item.Mod mod in gem.Mods)
                    {
                        Damage.Converted conv = Damage.Converted.Create(mod);
                        if (conv != null) Converts.Add(conv);

                        Damage.Added added = Damage.Added.Create(mod);
                        if (added != null) Adds.Add(added);

                        Damage.More more = Damage.More.Create(mod);
                        if (more != null) Mores.Add(more);
                    }
                }
            }

            // Return list group of this attack.
            public ListGroup ToListGroup()
            {
                Dictionary<string, List<float>> props = new Dictionary<string,List<float>>();

                foreach (var damage in Deals)
                {
                    string name = damage.Source + " " + damage.ToAttribute();
                    if (props.ContainsKey(name)) name += "(2)";
                    props.Add(name, damage.ToValue());
                    //props.Add(damage.Source + " " + damage.ToAttribute(), damage.ToValue());
                }
                    

                return new ListGroup(Name, props);
            }
        }

        public enum DamageArea
        {
            Area, Any
        }

        // Multiple form spells: Lightning Strike, Molten Strike (Melee + Projectile)
        public enum DamageForm
        {
            Melee, Projectile, Any
        }

        public enum DamageSource
        {
            MainHand, OffHand, Spell
        }

        public enum DamageType
        {
            Physical, Cold, Fire, Lightning, Chaos, Any
        }

        public enum DamageWeaponType // Tempest Blast keystone?
        {
            Wand, Any
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

                // Creates added damage modifier from mod.
                public static Added Create(Mod mod)
                {
                    Match m = ReAddMod.Match(mod.Attribute);
                    if (m.Success)
                    {
                        return new Added(Damage.DamageTypes[m.Groups[1].Value], mod.Value[0], mod.Value[1]);
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
            // TODO: Conversion downscaling: If the combined value of all Converted to modifiers for a given damage type is greater than 100%, the values are scaled so that the total is 100%.
            // TODO: Tempest Blast: 30% of Wand Physical Damage Added as Lightning Damage
            // @see: http://pathofexile.gamepedia.com/Damage_conversion
            public class Converted
            {
                // The percentage of damage to convert.
                float Percent;
                // The damage type to convert from.
                DamageType From;
                // The damage type to convert to.
                DamageType To;
                // The flag whether conversion is gain.
                bool IsGain = false;

                static Regex ReConvertMod = new Regex("#% of ([^ ]+) Damage Converted to ([^ ]+) Damage");
                static Regex ReGainMod = new Regex("Gain #% of ([^ ]+) Damage as Extra ([^ ]+) Damage");

                Converted(float percent, DamageType from, DamageType to)
                {
                    Percent = percent;
                    From = from;
                    To = to;
                }

                // Creates damage conversion from mod.
                public static Converted Create(Mod mod)
                {
                    Match m = ReConvertMod.Match(mod.Attribute);
                    if (m.Success)
                    {
                        return new Converted(mod.Value[0], Damage.DamageTypes[m.Groups[1].Value], Damage.DamageTypes[m.Groups[2].Value]);
                    }
                    else
                    {
                        m = ReGainMod.Match(mod.Attribute);
                        if (m.Success)
                        {
                            Converted conv = new Converted(mod.Value[0], Damage.DamageTypes[m.Groups[1].Value], Damage.DamageTypes[m.Groups[2].Value]);
                            conv.IsGain = true;

                            return conv;
                        }
                    }

                    return null;
                }

                // Applies conversion.
                public void Apply(List<Damage> deals)
                {
                    Damage from = deals.Find(d => d.Type == From);
                    if (from == null) return;

                    deals.Add(from.PercentOf(Percent, To));

                    if (! IsGain) from.Sub(from.PercentOf(Percent, To));
                }
            }

            public class Increased
            {
                // The increase.
                private float Percent;
                // The damage type.
                DamageType Type;

                static Regex ReIncreasedMod = new Regex("#% increased ([^ ]+) Damage");

                public Increased(DamageType type, float percent)
                {
                    Type = type;
                    Percent = percent;
                }

                // Creates added damage modifier from mod.
                public static Increased Create(Mod mod)
                {
                    Match m = ReIncreasedMod.Match(mod.Attribute);
                    if (m.Success)
                    {
                        return new Increased(Damage.DamageTypes[m.Groups[1].Value], mod.Value[0]);
                    }

                    return null;
                }

                // Applies modifier.
                public void Apply(List<Damage> deals)
                {
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

                More(DamageType type, float percent)
                {
                    Percent = percent;
                    Type = type;
                }

                // Returns true if multiplier can be applied to damage, false otherwise.
                public bool CanApply(Damage damage)
                {
                    return Type == DamageType.Any || damage.Type == Type || damage.Origin == Type;
                }

                // Creates "more" damage multiplier from mod.
                public static More Create(Mod mod)
                {
                    More more = null;

                    if (mod.Attribute == "#% less Damage")
                    {
                        more = new More(DamageType.Any, -mod.Value[0]);
                    }
                    else if (mod.Attribute == "#% more Damage")
                    {
                        more = new More(DamageType.Any, mod.Value[0]);
                    }
                    else if (mod.Attribute == "Deals #% of Base Damage")
                    {
                        more = new More(DamageType.Any, mod.Value[0] - 100);
                        more.IsMelee = true; // Doesn't need to be melee.
                    }
                    else if (mod.Attribute == "#% more Melee Physical Damage when on Full Life")
                    {
                        more = new More(DamageType.Physical, mod.Value[0]);
                        more.IsMelee = true;
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
            // The name of source (Spell, Main Hand, Off Hand).
            public string Source;

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

            // Creates damage from mod.
            public static Damage Create(Mod mod)
            {
                if (mod.Attribute.StartsWith("Deals"))
                {
                    Match m = ReDamageMod.Match(mod.Attribute);
                    if (m.Success)
                    {
                        return new Damage(DamageTypes[m.Groups[1].Value], mod.Value[0], mod.Value[1]);
                    }
                }

                return null;
            }

            // Creates damage from attribute.
            public static Damage Create(string name, List<float> value)
            {
                Match m = ReDamageAttribute.Match(name);
                if (m.Success)
                {
                    return new Damage(DamageTypes[m.Groups[1].Value], value[0], value[1]);
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

        // Returns attribute values merged with mods.
        public static Dictionary<string, List<float>> Merge(Dictionary<string, List<float>> attrs, List<Mod> mods)
        {
            Dictionary<string, List<float>> merged = new Dictionary<string, List<float>>();

            // Copy attribute values.
            foreach (var attr in attrs) merged.Add(attr.Key, new List<float>(attr.Value));

            // Increase values of existing attributes or insert new ones.
            foreach (Mod mod in mods)
                if (merged.ContainsKey(mod.Attribute))
                {
                    if (mod.Value.Count > 0)
                        for (int i = 0; i < mod.Value.Count; ++i)
                            merged[mod.Attribute][i] += mod.Value[i];
                }
                else merged.Add(mod.Attribute, mod.Value);

            return merged;
        }
  
        // Computes offensive attacks.
        public static List<ListGroup> Offense(Dictionary<string, List<float>> attrs, ItemAttributes itemAttrs)
        {
            List<ListGroup> groups = new List<ListGroup>();

            foreach (Item item in itemAttrs.Equip)
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
                        attack.Apply(attrs, itemAttrs);

                        groups.Add(attack.ToListGroup());
                    }
                }
            }

            return groups;
        }

    }
}
