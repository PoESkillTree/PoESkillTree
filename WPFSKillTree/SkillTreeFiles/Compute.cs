using System;

namespace POESKillTree.SkillTreeFiles
{
    public class Compute
    {
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
        public enum WeaponHand
        {
            Any = 0, Main = 1, Off = 2, DualWielded = 4,
            HandMask = Main | Off
        }

        [Flags]
        public enum WeaponType
        {
            Any,
            Bow = 1, Claw = 2, Dagger = 4, OneHandAxe = 8, OneHandMace = 16, OneHandSword = 32,
            Staff = 64, TwoHandAxe = 128, TwoHandMace = 256, TwoHandSword = 512, Wand = 1024,
            Quiver = 2048,
            Shield = 4096,
            Unarmed = 8192,
            Melee = Claw | Dagger | OneHandAxe | OneHandMace | OneHandSword | Staff | TwoHandAxe | TwoHandMace | TwoHandSword,
            OneHandedMelee = Claw | Dagger | OneHandAxe | OneHandMace | OneHandSword,
            TwoHandedMelee = Staff | TwoHandAxe | TwoHandMace | TwoHandSword,
            Axe = OneHandAxe | TwoHandAxe,
            Mace = OneHandMace | TwoHandMace,
            Sword = OneHandSword | TwoHandSword,
            Ranged = Bow | Wand,
        }
    }
}
