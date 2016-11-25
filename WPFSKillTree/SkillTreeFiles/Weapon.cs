using System;
using System.Collections.Generic;
using POESKillTree.Model;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Enums;
using POESKillTree.Compute;

namespace POESKillTree.SkillTreeFiles
{


    public class Weapon
    {
        // Default attack speed of unarmed weapon.
        const float UnarmedAttacksPerSecond = 1.2f;

        // List of all damage dealt by weapon.
        public List<Damage> Deals = new List<Damage>();
        // List of all non-physical damage added.
        public List<Damage.Added> Added = new List<Damage.Added>();
        // Which hand is used to hold this weapon.
        public WeaponHand Hand;
        // The item.
        public Item Item;
        // All attributes and mods.
        public AttributeSet Attributes = new AttributeSet();
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
                { "Unarmed",            WeaponType.Unarmed }
            };

        // Copy constructor.
        Weapon(Weapon weapon)
        {
            Item = weapon.Item;
            Nature = new DamageNature(weapon.Nature);
            Attributes = new AttributeSet(weapon.Attributes);

            foreach (Damage damage in weapon.Deals)
                Deals.Add(new Damage(damage));
            Added = weapon.Added;
        }

        public Weapon(WeaponHand hand, Item item)
        {
            Hand = hand;

            if (item != null)
            {
                Item = item;

                // Get weapon type (damage nature).
                if (item.ItemGroup != ItemGroup.OneHandedWeapon && item.ItemGroup != ItemGroup.TwoHandedWeapon)
                // Quiver or shield.
                {
                    if (item.BaseType.Name.Contains("Quiver"))
                        Nature = new DamageNature() { WeaponType = WeaponType.Quiver };
                    else if (item.BaseType.Name.Contains("Shield") || item.BaseType.Name.Contains("Buckler") ||
                             item.BaseType.Name == "Spiked Bundle")
                        Nature = new DamageNature() { WeaponType = WeaponType.Shield };
                    else
                        throw new Exception("Unknown weapon type: " + item.BaseType);
                }
                else // Regular weapon.
                {
                    WeaponType weaponType;
                    if (!Enum.TryParse(item.ItemType.ToString(), out weaponType))
                    {
                        if (item.ItemType == ItemType.ThrustingOneHandedSword)
                            weaponType = WeaponType.OneHandedSword;
                        else if (item.ItemType == ItemType.Sceptre)
                            weaponType = WeaponType.OneHandedMace;
                        else
                            throw new Exception("Unknown weapon type: " + item.BaseType);
                    }
                    Nature = new DamageNature { WeaponType = weaponType };
                }

                // If weapon is melee, it defaults to melee form. If it's ranged then projectile.
                if (Nature.Is(WeaponType.Melee))
                    Nature.Form = DamageForm.Melee;
                else
                    if (Nature.Is(WeaponType.Ranged))
                    Nature.Form = DamageForm.Projectile;

                // Copy attributes and create damage dealt.
                foreach (var prop in item.Properties)
                {
                    Attributes.Add(prop);

                    Damage damage = Damage.Create(Nature, prop.Attribute, prop.Value);
                    if (damage != null && damage.Type == DamageType.Physical) // Create only physical damage from item properties.
                        Deals.Add(damage);
                }

                // Copy local and non-local mods and collect added non-physical damage.
                foreach (var mod in item.Mods)
                {
                    if (mod.IsLocal)
                    {
                        Damage.Added added = Damage.Added.Create(Nature.Source, mod);
                        if (added != null && added.Type != DamageType.Physical)
                            Added.Add(added);
                    }

                    Attributes.Add(mod);
                }
            }
            else // No item.
                if (hand == WeaponHand.Main) // Only Main Hand can be Unarmed.
            {
                Nature = new DamageNature() { WeaponType = WeaponType.Unarmed };

                // Implicit Unarmed attributes.
                Attributes.Add("Attacks per Second: #", new List<float>() { UnarmedAttacksPerSecond });

                // Unarmed damage.
                Damage damage = Damage.Create(Nature);
                Deals.Add(damage);
            }
        }

        // Returns clone of weapon for specified hand.
        public Weapon Clone(WeaponHand forHand)
        {
            return new Weapon(this) { Hand = forHand };
        }

        // Returns attribute's list of values, or empty list if not found.
        public List<float> GetValues(string attr)
        {
            return Attributes.ContainsKey(attr) ? Attributes[attr] : new List<float>();
        }

        // Returns true if weapon is in specified hand, false otherwise.
        public bool Is(WeaponHand hand)
        {
            return hand == WeaponHand.Any || (Hand & hand) != 0;
        }

        // Returns true if weapon is of specified type, false otherwise.
        public bool Is(WeaponType type)
        {
            return Nature != null && (Nature.WeaponType & type) != 0;
        }

        // Returns true if weapon is dual wielded, false otherwise.
        public bool IsDualWielded()
        {
            return (Hand & WeaponHand.DualWielded) != 0;
        }

        // Returns true if weapon is a shield, false otherwise.
        public bool IsShield()
        {
            return Nature != null && Nature.WeaponType == WeaponType.Shield;
        }

        // Returns true if weapon counts as Unarmed, false otherwise.
        public bool IsUnarmed()
        {
            return Nature != null && (Nature.WeaponType & WeaponType.Unarmed) != 0;
        }

        // Returns true if weapon is a regular weapon, false otherwise.
        public bool IsWeapon()
        {
            return Nature != null && (Nature.WeaponType & WeaponType.Weapon) != 0;
        }
    }
}
