using POESKillTree.SkillTreeFiles;
using System.Collections.Generic;
using static POESKillTree.Compute.ComputeGlobal;

namespace POESKillTree.Compute
{

    public class DamageNature
    {
        public DamageForm Form = DamageForm.Any;
        public DamageSource Source = DamageSource.Any;
        public DamageType Type = DamageType.Any;
        public WeaponHand WeaponHand = WeaponHand.Any;
        public WeaponType WeaponType = WeaponType.Any;

        public static Dictionary<string, DamageForm> Forms = new Dictionary<string, DamageForm>()
            {
                { "Melee",      DamageForm.Melee },
                { "Projectile", DamageForm.Projectile },
                { "AoE",        DamageForm.AoE },
                { "Area",       DamageForm.AoE },
                { "Burning",    DamageForm.DoT },
                { "Trigger",    DamageForm.OnUse }
            };
        static Dictionary<string, DamageSource> Sources = new Dictionary<string, DamageSource>()
            {
                { "Attack",     DamageSource.Attack },
                { "Cast",       DamageSource.Cast },
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
                { "Chaos",      DamageType.Chaos },
                { "Burning",    DamageType.Fire }
            };

        public DamageNature() { }

        public DamageNature(DamageNature nature)
        {
            Form = nature.Form;
            Source = nature.Source;
            Type = nature.Type;
            WeaponHand = nature.WeaponHand;
            WeaponType = nature.WeaponType;
        }

        public DamageNature(DamageNature nature, string str)
        {
            Form = nature.Form;
            Source = nature.Source;
            Type = nature.Type;
            WeaponHand = nature.WeaponHand;
            WeaponType = nature.WeaponType;

            string[] words = str.Split(' ');
            foreach (string word in words)
            {
                if (Forms.ContainsKey(word)) Form |= Forms[word];
                if (Types.ContainsKey(word)) Type = Types[word];
                if (Weapon.Types.ContainsKey(word)) WeaponType = Weapon.Types[word];
                if (Sources.ContainsKey(word)) Source = Sources[word];
            }
        }

        public DamageNature(string str)
        {
            string[] words = str.Split(' ');
            foreach (string word in words)
            {
                if (Forms.ContainsKey(word)) Form |= Forms[word];
                if (Types.ContainsKey(word)) Type = Types[word];
                if (Weapon.Types.ContainsKey(word)) WeaponType = Weapon.Types[word];
                if (Sources.ContainsKey(word)) Source = Sources[word];
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
                if (Forms.ContainsKey(word)) Form |= Forms[word];
                if (Types.ContainsKey(word)) Type = Types[word];
                if (Weapon.Types.ContainsKey(word)) WeaponType = Weapon.Types[word];
                if (Sources.ContainsKey(word)) Source = Sources[word];
            }
        }

        public DamageNature(IEnumerable<string> keywords)
        {
            foreach (string word in keywords)
            {
                if (Forms.ContainsKey(word)) Form |= Forms[word];
                if (Weapon.Types.ContainsKey(word)) WeaponType = Weapon.Types[word];
                if (Sources.ContainsKey(word)) Source = Sources[word];
            }
        }

        // Returns damage form narrowed down according to weapon.
        public DamageForm ChooseWeaponForm(DamageNature weapon)
        {
            return (Form & ~DamageForm.WeaponMask) | (Form & weapon.Form);
        }

        public bool Is(DamageForm form)
        {
            return (Form & form) != 0;
        }

        public bool Is(DamageSource source)
        {
            return Source == source;
        }

        public bool Is(DamageType type)
        {
            return (Type & type) != 0;
        }

        public bool Is(WeaponHand weaponHand)
        {
            return (WeaponHand & weaponHand) != 0;
        }

        public bool Is(WeaponType weaponType)
        {
            return (WeaponType & weaponType) != 0;
        }

        public bool Matches(DamageNature nature)
        {
            return (Form == DamageForm.Any || (nature.Form & Form) != 0)
                   && (Type == DamageType.Any || (nature.Type & Type) != 0)
                   && (WeaponHand == WeaponHand.Any || (nature.WeaponHand & WeaponHand) != 0)
                   && (WeaponType == WeaponType.Any || (nature.WeaponType & WeaponType) != 0)
                   && (Source == DamageSource.Any || nature.Source == Source);
        }

        public bool MatchesExceptType(DamageNature nature)
        {
            return (Form == DamageForm.Any || (nature.Form & Form) != 0)
                   && (WeaponHand == WeaponHand.Any || (nature.WeaponHand & WeaponHand) != 0)
                   && (WeaponType == WeaponType.Any || (nature.WeaponType & WeaponType) != 0)
                   && (Source == DamageSource.Any || nature.Source == Source);
        }

        public static DamageType TypeOf(string type)
        {
            return Types[type];
        }
    }
}