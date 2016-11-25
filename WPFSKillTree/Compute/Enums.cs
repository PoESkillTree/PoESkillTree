using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POESKillTree.Compute
{

    public enum DamageConversionSource
    {
        Gem, Equipment, Tree
    }

    [Flags]
    public enum DamageForm
    {
        Any, Melee = 1, Projectile = 2, AoE = 4, DoT = 8, OnUse = 16,
        WeaponMask = Melee | Projectile
    }

    public enum DamageSource
    {
        Any, Attack, Cast, Spell
    }

    [Flags]
    public enum DamageType
    {
        Any, Physical = 1, Fire = 2, Cold = 4, Lightning = 8, Chaos = 16,
        Elemental = Cold | Fire | Lightning,
        Total = 256
    }

    [Flags]
    public enum WeaponHand
    {
        Any = 0, Main = 1, Off = 2, DualWielded = 4,
        HandMask = Main | Off
    }

    [Flags]
    public enum WeaponType
    {
        Any,
        Bow = 1, Claw = 2, Dagger = 4, OneHandedAxe = 8, OneHandedMace = 16, OneHandedSword = 32,
        Staff = 64, TwoHandedAxe = 128, TwoHandedMace = 256, TwoHandedSword = 512, Wand = 1024,
        Quiver = 2048,
        Shield = 4096,
        Unarmed = 8192,
        Melee = Claw | Dagger | OneHandedAxe | OneHandedMace | OneHandedSword | Staff | TwoHandedAxe | TwoHandedMace | TwoHandedSword,
        OneHandedMelee = Claw | Dagger | OneHandedAxe | OneHandedMace | OneHandedSword,
        TwoHandedMelee = Staff | TwoHandedAxe | TwoHandedMace | TwoHandedSword,
        Axe = OneHandedAxe | TwoHandedAxe,
        Mace = OneHandedMace | TwoHandedMace,
        Sword = OneHandedSword | TwoHandedSword,
        Ranged = Bow | Wand,
        Weapon = Melee | Ranged
    }
}
