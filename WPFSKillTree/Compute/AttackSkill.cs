using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.Model;
using POESKillTree.ViewModels;
using System.Linq;
using POESKillTree.Model.Items;
using POESKillTree.Model.Items.Affixes;
using static POESKillTree.Compute.ComputeGlobal;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Model.Gems;

namespace POESKillTree.Compute
{

    public class AttackSkill
    {
        // The skill gem.
        public Item Gem;
        // The name.
        string Name;
        // The nature of attack (based on gem keywords).
        public DamageNature Nature;
        // List of attack sources (either spell or main hand and/or off hand).
        List<AttackSource> Sources = new List<AttackSource>();
        // Skill gem local attributes.
        public AttributeSet Local = new AttributeSet();
        // Damage effectiveness.
        float Effectiveness;
        // List of damage conversions.
        List<Damage.Converted> Converts = new List<Damage.Converted>();
        // List of damage gains.
        List<Damage.Gained> Gains = new List<Damage.Gained>();
        // The number of hits skill does per single attack.
        public float HitsPerAttack;
        // The flag whether skill strikes with both weapons at once instead of alternating weapons while dual wielding.
        public bool IsStrikingWithBothWeaponsAtOnce;
        // The flag whether skill is useable.
        public bool IsUseable = true;

        // The nature to match physical weapon damage while dual wielding.
        static DamageNature PhysicalWeaponDamage = new DamageNature() { Source = DamageSource.Attack, Type = DamageType.Physical };
        // Gem support from item modifier pattern.
        static Regex ReGemSupportFromItem = new Regex(@"Socketed Gems are Supported by level # (.+)$");

        Computation Compute;

        // Creates attack from gem.
        AttackSkill(Item gem, Computation compute)
        {
            Gem = gem;
            Name = gem.Name;
            Nature = GemDB.Instance.NatureOf(gem);
            HitsPerAttack = GemDB.Instance.HitsPerAttackOf(gem);
            IsStrikingWithBothWeaponsAtOnce = GemDB.Instance.IsStrikingWithBothWeaponsAtOnce(gem);

            Effectiveness = gem.Properties.First("Damage Effectiveness: #%", 0, 100);
            Compute = compute;
        }

        AttributeSet Equipment => Compute.Equipment;
        AttributeSet Tree => Compute.Tree;
        AttributeSet Implicit => Compute.Implicit;
        Weapon OffHand => Compute.OffHand;
        Weapon MainHand => Compute.MainHand;

        // Applies item modifiers.
        public void Apply(Item item)
        {
            // Add skill gem attributes.
            Local.Add(GemDB.Instance.AttributesOf(Gem, item));

            CreateSources();

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

                // Whether damage added from equipment applies to the current DamageSource is decided in Create().
                Damage.Added added = Damage.Added.Create(Nature.Source, attr);
                if (added != null) adds.Add(added);
            }

            foreach (var attr in Tree)
            {
                Damage.Converted conv = Damage.Converted.Create(DamageConversionSource.Tree, attr);
                if (conv != null)
                    Converts.Add(conv);
            }

            // Merge local gems and global attributes.
            AttributeSet attrs = Compute.Global.Merge(Local);

            // Iron Grip.
            if (Compute.IronGrip || attrs.ContainsKey("Strength's damage bonus applies to Projectile Attacks made with Supported Skills"))
            {
                // Create projectile attack damage bonus from value of implicit melee physical damage increase.
                float bonus = Implicit["#% increased Melee Physical Damage"][0];
                attrs.AddAsSum("#% increased Projectile Weapon Damage", bonus);
            }

            // Iron Will.
            if (attrs.ContainsKey("Strength's damage bonus applies to Spell Damage as well for Supported Skills"))
            {
                // Create spell damage bonus from value of implicit melee physical damage increase.
                float bonus = Implicit["#% increased Melee Physical Damage"][0];
                attrs.AddAsSum("#% increased Spell Damage", bonus);
            }

            // Collect damage gains, increases and multipliers.
            foreach (var attr in attrs)
            {
                var gained = Damage.Gained.Create(attr);
                if (gained != null)
                    Gains.Add(gained);

                var increased = Damage.Increased.Create(attr);
                if (increased != null)
                    increases.Add(increased);

                var more = Damage.More.Create(attr, Compute);
                if (more != null)
                    mores.Add(more);
            }

            foreach (AttackSource source in Sources)
            {
                ApplyAttackSource(adds, increases, mores, attrs, source);
            }
        }

        private void ApplyAttackSource(List<Damage.Added> adds, List<Damage.Increased> increases, List<Damage.More> mores, AttributeSet attrs, AttackSource source)
        {
            // Apply damage added.
            foreach (Damage.Added added in adds)
                if (added.MatchesExceptType(source.Nature))
                    added.Apply(source, Effectiveness);

            // Apply damage conversions and gains.
            Convert(source.Deals);

            // For each damage dealt apply its increases and multipliers.
            foreach (Damage damage in source.Deals)
            {
                ApplyDamageSource(increases, mores, attrs, source, damage);
            }

            // Avatar of Fire (remove non-Fire damage).
            if (Compute.AvatarOfFire)
                foreach (Damage damage in new List<Damage>(source.Deals))
                    if (!damage.Is(DamageType.Fire))
                        source.Deals.Remove(damage);

            // Summarize, round and combine damage dealt.
            source.Combine();

            source.AccuracyRating(attrs);

            source.AttackSpeed(this, attrs);

            source.CriticalStrike(attrs);
        }

        private void ApplyDamageSource(List<Damage.Increased> increases, List<Damage.More> mores, AttributeSet attrs, AttackSource source, Damage damage)
        {
            float inc = 0;
            foreach (Damage.Increased increase in increases)
                if (damage.Matches(increase))
                    inc += increase.Percent;
            if (Compute.IsDualWielding && damage.Matches(PhysicalWeaponDamage))
                inc += attrs.GetOrDefault("#% increased Physical Weapon Damage while Dual Wielding");
            if (OffHand.IsShield() && damage.Matches(PhysicalWeaponDamage))
                inc += attrs.GetOrDefault("#% increased Physical Weapon Damage while holding a Shield");
            if (OffHand.IsShield() && source.Nature.Is(DamageSource.Attack))
                inc += attrs.GetOrDefault("#% increased Physical Attack Damage while holding a Shield");
            if (OffHand.IsShield() && source.Nature.Is(DamageForm.Melee))
                inc += attrs.GetOrDefault("#% increased Melee Physical Damage while holding a Shield");
            if (source.Nature.Is(DamageSource.Spell))
                inc += attrs.GetOrDefault("Supported Triggered Spells have #% increased Spell Damage");
            if (inc != 0)
                damage.Increase(inc);

            // Apply all less multipliers.
            float mul = 1;
            foreach (Damage.More more in mores.FindAll(m => m.IsLess && damage.Matches(m)))
                mul *= (100 + more.Percent) / 100;
            if (mul != 1)
                damage.Multiply(RoundHalfDownValue(mul, 2));
            // Apply all more multipliers.
            mul = 1;
            foreach (Damage.More more in mores.FindAll(m => !m.IsLess && damage.Matches(m)))
                mul *= (100 + more.Percent) / 100;
            if (mul != 1)
                damage.Multiply(RoundHalfDownValue(mul, 2));
        }

        // Returns attacks/casts per second.
        public float AttacksPerSecond()
        {
            return Nature.Is(DamageSource.Attack) && Compute.IsDualWielding ? (Sources[0].APS + Sources[1].APS) / 2 : Sources[0].APS;
        }

        // Returns average hit including critical strikes.
        public float AverageHit()
        {
            return Nature.Is(DamageSource.Attack) && Compute.IsDualWielding ? (Sources[0].AverageHit() + Sources[1].AverageHit()) / 2 : Sources[0].AverageHit();
        }

        // Returns true if skill can be used with weapon.
        public bool CanUse(Weapon weapon)
        {
            return (weapon.IsWeapon() || weapon.IsUnarmed()) && Nature.Is(weapon.Nature.WeaponType) && GemDB.Instance.CanUse(Gem, weapon, Compute);
        }

        // Returns chance to hit.
        public float ChanceToHit()
        {
            return Nature.Is(DamageSource.Attack) && Compute.IsDualWielding ? (Sources[0].ChanceToHit() + Sources[1].ChanceToHit()) / 2 : Sources[0].ChanceToHit();
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
                foreach (Damage.Gained gain in Gains.FindAll(g => g.Type == type))
                    gain.Apply(convert, output);

                // Remove processed damage and append generated output.
                deals.RemoveAll(d => d.Is(type));
                deals.AddRange(output);
            }
        }

        // Creates attack from gem.
        public static AttackSkill Create(Item gem, Computation compute)
        {
            return new AttackSkill(gem, compute);
        }

        // Creates sources of attack skill (spell, main hand and/or off hand).
        // XXX: "Uses both hand slots" mod on one-handed weapon locks off-hand slot, @see http://pathofexile.gamepedia.com/The_Goddess_Scorned
        public void CreateSources()
        {
            Sources = new List<AttackSource>();

            if (Nature.Is(DamageSource.Attack))
            {
                if (MainHand.IsWeapon() || MainHand.IsUnarmed())
                    Sources.Add(new AttackSource("Main Hand", this, MainHand, Compute));

                if (OffHand.IsWeapon())
                    Sources.Add(new AttackSource("Off Hand", this, OffHand, Compute));

                // Skill can't be used with any hand, flag it as unuseable.
                if (!CanUse(MainHand) && !CanUse(OffHand))
                    IsUseable = false;
            }
            else if (Nature.Is(DamageSource.Spell))
            {
                Sources.Add(new AttackSource("Spell", this, null, Compute));
            }
            else // Cast
            {
                Sources.Add(new AttackSource("", this, null, Compute));
            }
        }

        // Returns damage per second.
        public float DamagePerSecond()
        {
            float dps = AverageHit() * (IsDamageOnUse() ? 1 : AttacksPerSecond()) * RoundValue(ChanceToHit(), 0) / 100;

            dps *= HitsPerAttack;

            // XXX: If skill doesn't alternate weapons while dual wielding (i.e. strikes with both weapons at once), then DPS is doubled.
            return Compute.IsDualWielding && IsStrikingWithBothWeaponsAtOnce ? dps * 2 : dps;
        }

        // Returns true if gem is an attack skill, false otherwise.
        public static bool IsAttackSkill(Item gem)
        {
            // A gem is an attack if it has Attack, Cast or Spell keyword with damage dealing mod.
            return (gem.Keywords.Contains("Attack") // It's Attack.
                    || (gem.Keywords.Contains("Spell") || gem.Keywords.Contains("Cast")) && gem.Mods.Any(mod => mod.Attribute.StartsWith("Deals"))) // It's Spell or Cast buff which deals damage.
                   && !gem.Keywords.Contains("Trap") && !gem.Keywords.Contains("Mine") // No traps & mines.
                   && !gem.Keywords.Contains("Support"); // Not a support gem.
        }

        // Returns true if damage isn't affected by attack/cast speed, false otherwise.
        public bool IsDamageOnUse()
        {
            return Nature.Is(DamageForm.OnUse);
        }

        // Links support gems.
        // TODO: In case of same gems slotted only highest level one is used.
        public void Link(List<Item> gems, Item item)
        {
            // Check for gem support from item modifier.
            foreach (ItemMod mod in item.Mods.Where(m => ReGemSupportFromItem.IsMatch(m.Attribute)))
            {
                Match m = ReGemSupportFromItem.Match(mod.Attribute);
                string gemName = m.Groups[1].Value;
                int level = (int)mod.Value[0];

                if (!GemDB.Instance.CanSupport(this, gemName)) continue;
                Local.Add(GemDB.Instance.AttributesOf(gemName, level, 0));
            }

            foreach (Item gem in gems)
            {
                if (!gem.Keywords.Contains("Support")) continue; // Skip non-support gems.
                if (!GemDB.Instance.CanSupport(this, gem)) continue; // Check whether gem can support our skill gem.

                // XXX: Spells linked to Cast on/when are treated as cast on use spells (i.e. their cast speed is ignored).
                if ((gem.Name.StartsWith("Cast On") || gem.Name.StartsWith("Cast on") || gem.Name.StartsWith("Cast when"))
                    && Nature.Is(DamageSource.Spell))
                    Nature.Form |= DamageForm.OnUse;

                // Add support gem attributes.
                Local.Add(GemDB.Instance.AttributesOf(gem, item));
            }
        }

        // Return list group of this attack.
        public ListGroup ToListGroup()
        {
            AttributeSet props = new AttributeSet();

            props.Add(IsDamageOnUse() ? "Damage per Use: #" : "Damage per Second: #", new List<float> { RoundHalfDownValue(DamagePerSecond(), 1) });

            if (Nature.Is(DamageSource.Attack))
            {
                props.Add("Chance to Hit: #%", new List<float> { RoundValue(ChanceToHit(), 0) });
                props.Add("Attacks per Second: #", new List<float> { RoundHalfDownValue(AttacksPerSecond(), 1) });
            }
            else
                props.Add("Casts per Second: #", new List<float> { RoundHalfDownValue(AttacksPerSecond(), 1) });

            foreach (AttackSource source in Sources)
            {
                string sourcePrefix = source.Name.Length == 0 ? "" : source.Name + " ";

                foreach (DamageType type in DamageTypes)
                {
                    Damage damage = source.Deals.Find(d => d.Is(type));
                    if (damage != null)
                        props.Add(sourcePrefix + damage.ToAttribute(), damage.ToValue());
                }

                if (source.Nature.Is(DamageSource.Attack))
                    props.Add(sourcePrefix + "Accuracy Rating: #", new List<float> { RoundValue(source.Accuracy, 0) });

                if (source.CriticalChance > 0)
                {
                    // XXX: Different rounding style for spells and attacks. Really?
                    props.Add(sourcePrefix + "Critical Strike Chance: #%", new List<float> { Nature.Is(DamageSource.Spell) ? RoundValue(source.CriticalChance, 1) : RoundHalfDownValue(source.CriticalChance, 1) });
                    props.Add(sourcePrefix + "Critical Strike Multiplier: #%", new List<float> { RoundValue(source.CriticalMultiplier, 0) });
                }
            }

            return new ListGroup(Name + (IsUseable ? "" : " (Unuseable)"), props);
        }
    }
}