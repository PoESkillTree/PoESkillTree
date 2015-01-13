using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.Model;
using POESKillTree.ViewModels;
using POESKillTree.ViewModels.ItemAttribute;

namespace POESKillTree.SkillTreeFiles
{
    /* Known issues:
     * - No support for unarmed combat.
     * - No support for minions, traps, mines and totems.
     * - No support for Vaal gems.
     * - Mana regeneration shows sometimes incorrect value (0.1 difference from in-game value).
     * - Damage type ranges shows sometimes incorrect value affecting overall DPS (1 difference from in-game value, occurs with damage conversions).
     * - Damage per Second shows sometimes incorrect value (0.1 difference from in-game value).
     * - Spell Critical Strike chance shows sometimes incorrect value (0.1 difference from in-game value).
     * - Estimated chance to Evade Attacks shows sometimes incorrect value.
     * - Cast gems (Herald of Ice, Herald of Thunder) have their quality bonuses applied in inactive state.
     * - Chance to Hit shows sometimes incorrect value affecting overall DPS.
     */
    public class Compute
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

            // Creates attack from gem.
            AttackSkill(Item gem)
            {
                Gem = gem;
                Name = gem.Name;
                Nature = ItemDB.NatureOf(gem);
                HitsPerAttack = ItemDB.HitsPerAttackOf(gem);
                IsStrikingWithBothWeaponsAtOnce = ItemDB.IsStrikingWithBothWeaponsAtOnce(gem);

                Effectiveness = gem.Attributes.ContainsKey("Damage Effectiveness: #%") ? gem.Attributes["Damage Effectiveness: #%"][0] : 100;
            }

            // Applies item modifiers.
            public void Apply(Item item)
            {
                // Add skill gem attributes.
                Local.Add(ItemDB.AttributesOf(Gem, item));

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

                // Iron Grip.
                if (IronGrip || attrs.ContainsKey("Strength's damage bonus applies to Projectile Attacks made with Supported Skills"))
                {
                    // Create projectile attack damage bonus from value of implicit melee physical damage increase.
                    float bonus = Implicit["#% increased Melee Physical Damage"][0];
                    if (attrs.ContainsKey("#% increased Projectile Weapon Damage"))
                        attrs["#% increased Projectile Weapon Damage"][0] += bonus;
                    else
                        attrs.Add("#% increased Projectile Weapon Damage", new List<float> { bonus });
                }

                // Iron Will.
                if (attrs.ContainsKey("Strength's damage bonus applies to Spell Damage as well for Supported Skills"))
                {
                    // Create spell damage bonus from value of implicit melee physical damage increase.
                    float bonus = Implicit["#% increased Melee Physical Damage"][0];
                    if (attrs.ContainsKey("#% increased Spell Damage"))
                        attrs["#% increased Spell Damage"][0] += bonus;
                    else
                        attrs.Add("#% increased Spell Damage", new List<float> { bonus });
                }

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
                        if (added.MatchesExceptType(source.Nature))
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
                        if (IsDualWielding && damage.Matches(PhysicalWeaponDamage) && attrs.ContainsKey("#% increased Physical Weapon Damage while Dual Wielding"))
                            inc += attrs["#% increased Physical Weapon Damage while Dual Wielding"][0];
                        if (source.Nature.Is(DamageSource.Spell) && attrs.ContainsKey("Supported Triggered Spells have #% increased Spell Damage")) // Cast on Melee Kill.
                            inc += attrs["Supported Triggered Spells have #% increased Spell Damage"][0];
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

                    // Avatar of Fire (remove non-Fire damage).
                    if (AvatarOfFire)
                        foreach (Damage damage in new List<Damage>(source.Deals))
                            if (!damage.Is(DamageType.Fire))
                                source.Deals.Remove(damage);

                    // Summarize, round and combine damage dealt.
                    source.Combine();

                    source.AccuracyRating(attrs);

                    source.AttackSpeed(this, attrs);

                    source.CriticalStrike(attrs);
                }
            }

            // Returns attacks/casts per second.
            public float AttacksPerSecond()
            {
                return Nature.Is(DamageSource.Attack) && IsDualWielding ? (Sources[0].APS + Sources[1].APS) / 2 : Sources[0].APS;
            }

            // Returns average hit including critical strikes.
            public float AverageHit()
            {
                return Nature.Is(DamageSource.Attack) && IsDualWielding ? (Sources[0].AverageHit() + Sources[1].AverageHit()) / 2 : Sources[0].AverageHit();
            }

            // Returns true if skill can be used with weapon.
            public bool CanUse(Weapon weapon)
            {
                return weapon.IsWeapon() && Nature.Is(weapon.Nature.WeaponType) && ItemDB.CanUse(Gem, weapon);
            }

            // Returns chance to hit.
            public float ChanceToHit()
            {
                return Nature.Is(DamageSource.Attack) && IsDualWielding ? (Sources[0].ChanceToHit() + Sources[1].ChanceToHit()) / 2 : Sources[0].ChanceToHit();
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
            public static AttackSkill Create(Item gem)
            {
                return new AttackSkill(gem);
            }

            // Creates sources of attack skill (spell, main hand and/or off hand).
            // XXX: "Uses both hand slots" mod on one-handed weapon locks off-hand slot, @see http://pathofexile.gamepedia.com/The_Goddess_Scorned
            public void CreateSources()
            {
                Sources = new List<AttackSource>();

                if (Nature.Is(DamageSource.Attack))
                {
                    if (MainHand.IsWeapon())
                        Sources.Add(new AttackSource("Main Hand", this, MainHand));
                        
                    if (OffHand.IsWeapon())
                        Sources.Add(new AttackSource("Off Hand", this, OffHand));

                    // Skill can't be used with any hand, flag it as unuseable.
                    if (!CanUse(MainHand) && !CanUse(OffHand))
                        IsUseable = false;
                }
                else if (Nature.Is(DamageSource.Spell))
                {
                    Sources.Add(new AttackSource("Spell", this, null));
                }
                else // Cast
                {
                    Sources.Add(new AttackSource("", this, null));
                }
            }

            // Returns damage per second.
            public float DamagePerSecond()
            {
                float dps = AverageHit() * (IsDamageOnUse() ? 1 : AttacksPerSecond()) * RoundValue(ChanceToHit(), 0) / 100;

                dps *= HitsPerAttack;

                // XXX: If skill doesn't alternate weapons while dual wielding (i.e. strikes with both weapons at once), then DPS is doubled.
                return IsDualWielding && IsStrikingWithBothWeaponsAtOnce ? dps * 2 : dps;
            }

            // Returns true if gem is an attack skill, false otherwise.
            public static bool IsAttackSkill(Item gem)
            {
                // A gem is an attack if it has Attack, Cast or Spell keyword with damage dealing mod.
                return (gem.Keywords.Contains("Attack") // It's Attack.
                        || (gem.Keywords.Contains("Spell") || gem.Keywords.Contains("Cast")) && gem.Mods.Exists(mod => mod.Attribute.StartsWith("Deals"))) // It's Spell or Cast buff which deals damage.
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
                foreach (ItemMod mod in item.Mods.FindAll(m => ReGemSupportFromItem.IsMatch(m.Attribute)))
                {
                    Match m = ReGemSupportFromItem.Match(mod.Attribute);
                    string gemName = m.Groups[1].Value;
                    int level = (int)mod.Value[0];

                    if (!ItemDB.CanSupport(this, gemName)) continue;
                    Local.Add(ItemDB.AttributesOf(gemName, level, 0));
                }

                foreach (Item gem in gems)
                {
                    if (!gem.Keywords.Contains("Support")) continue; // Skip non-support gems.
                    if (!ItemDB.CanSupport(this, gem)) continue; // Check whether gem can support our skill gem.

                    // XXX: Spells linked to Cast on/when are treated as cast on use spells (i.e. their cast speed is ignored).
                    if ((gem.Name.StartsWith("Cast On") || gem.Name.StartsWith("Cast on") || gem.Name.StartsWith("Cast when"))
                        && Nature.Is(DamageSource.Spell))
                        Nature.Form |= DamageForm.OnUse;

                    // Add support gem attributes.
                    Local.Add(ItemDB.AttributesOf(gem, item));
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

        public class AttackSource
        {
            // The accuracy rating.
            public float Accuracy;
            // Attacks/casts per second.
            public float APS;
            // Cast time.
            public float CastTime;
            // Critical strike chance (in percent).
            public float CriticalChance;
            // Critical strike multiplier (in percent).
            public float CriticalMultiplier = 150;
            // List of damage dealt by source.
            public List<Damage> Deals = new List<Damage>();
            // Local attributes of weapon.
            public AttributeSet Local;
            // The source name.
            public string Name;
            // The result nature of skill used with weapon.
            public DamageNature Nature;

            // The increased/reduced accuracy rating with weapon type pattern.
            static Regex ReIncreasedAccuracyRatingWithWeaponType = new Regex("#% (increased|reduced) Accuracy Rating with (.+)$");
            // The increased/reduced attack speed patterns.
            static Regex ReIncreasedAttackSpeedWeaponType = new Regex("#% (increased|reduced) (.+) Attack Speed$");
            static Regex ReIncreasedAttackSpeedWithWeaponHandOrType = new Regex("#% (increased|reduced) Attack Speed with (.+)$");
            // The more/less attack speed patterns.
            static Regex ReMoreAttackSpeedType = new Regex("#% (more|less) (.+) Attack Speed$");
            // The form specific increased/reduced critical chance/multiplier patterns.
            static Regex ReIncreasedCriticalChanceForm = new Regex("#% (increased|reduced) (.+) Critical Strike Chance$");
            static Regex ReIncreasedCriticalMultiplierForm = new Regex("#% (increased|reduced) (.+) Critical Strike Multiplier$");
            // The increased/reduced critical chance/multiplier with weapon type patterns.
            static Regex ReIncreasedCriticalChanceWithWeaponType = new Regex("#% (increased|reduced) Critical Strike Chance with (.+)$");
            static Regex ReIncreasedCriticalMultiplierWithWeaponType = new Regex("#% (increased|reduced) Critical Strike Multiplier with (.+)$");

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

                    if (skill.Gem.Attributes.ContainsKey("Cast Time: # sec"))
                    {
                        CastTime = skill.Gem.Attributes["Cast Time: # sec"][0];
                        APS = 1 / CastTime;
                    }
                    else
                        APS = CastTime = 1; // Spell without Cast Time has cast time of 1 second.

                    if (skill.Gem.Attributes.ContainsKey("Critical Strike Chance: #%"))
                        CriticalChance = skill.Gem.Attributes["Critical Strike Chance: #%"][0];
                    else
                        CriticalChance = 0; // Spell without Critical Strike Chance has none.

                    Local = new AttributeSet(); // No local weapon attributes.
                }
                else
                {
                    if ((skill.Nature.WeaponType & weapon.Nature.WeaponType) == 0) // Skill can't be used.
                        // Override weapon type and form of skill with actual weapon (client shows damage of unuseable skills as well).
                        Nature = new DamageNature(skill.Nature) { Form = weapon.Nature.Form, WeaponHand = weapon.Hand, WeaponType = weapon.Nature.WeaponType };
                    else // Narrow down weapon type and form of skill gem to actual weapon (e.g. Frenzy).
                        Nature = new DamageNature(skill.Nature)
                        {
                            Form = skill.Nature.ChooseWeaponForm(weapon.Nature), // XXX: Choose between melee or projectile form according to weapon.
                            WeaponHand = weapon.Hand,
                            WeaponType = skill.Nature.WeaponType & weapon.Nature.WeaponType
                        };

                    // XXX: If source has no form, but skill has form defined, then force form of skill.
                    // This happens in form transition from melee to projectile with skills like Spectral Throw.
                    if (Nature.Form == DamageForm.Any && skill.Nature.Form != DamageForm.Any)
                        Nature.Form = skill.Nature.Form;

                    foreach (Damage damage in weapon.Deals)
                        Deals.Add(new Damage(damage) { Form = Nature.Form, Source = Nature.Source, WeaponHand = Nature.WeaponHand, WeaponType = Nature.WeaponType });

                    foreach (Damage.Added added in weapon.Added)
                        if (weapon.Is(added.Hand)) // Added damage may require specific hand.
                            added.Apply(this, 100);

                    APS = weapon.Attributes["Attacks per Second: #"][0];

                    if (weapon.Attributes.ContainsKey("Critical Strike Chance: #%"))
                        CriticalChance = weapon.Attributes["Critical Strike Chance: #%"][0];
                    else
                        CriticalChance = 0; // Weapon without Critical Strike Chance has none.

                    Local = weapon.Attributes;
                }
            }

            // Computes accuracy.
            public void AccuracyRating(AttributeSet attrs)
            {
                Accuracy = attrs["+# Accuracy Rating"][0];
                // Local weapon accuracy bonus.
                if (Local.ContainsKey("+# to Accuracy Rating"))
                    Accuracy += Local["+# to Accuracy Rating"][0];
                float incAcc = 0;
                // Local weapon accuracy bonus.
                if (Local.ContainsKey("#% increased Accuracy Rating"))
                    incAcc += Local["#% increased Accuracy Rating"][0];
                // Gems & global bonuses.
                if (attrs.ContainsKey("+# to Accuracy Rating"))
                    Accuracy += attrs["+# to Accuracy Rating"][0];
                if (attrs.ContainsKey("#% increased Accuracy Rating"))
                    incAcc += attrs["#% increased Accuracy Rating"][0];
                if (attrs.ContainsKey("#% reduced Accuracy Rating"))
                    incAcc += attrs["#% reduced Accuracy Rating"][0];
                foreach (var attr in attrs.Matches(ReIncreasedAccuracyRatingWithWeaponType))
                {
                    Match m = ReIncreasedAccuracyRatingWithWeaponType.Match(attr.Key);
                    if (Nature.Is(WithWeaponType[m.Groups[2].Value]))
                        incAcc += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
                }
                if (IsDualWielding && attrs.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
                    incAcc += attrs["#% increased Accuracy Rating while Dual Wielding"][0];
                if (incAcc != 0)
                    Accuracy = IncreaseValueByPercentage(Accuracy, incAcc);
            }

            // Computes attacks or casts per second.
            public void AttackSpeed(AttackSkill skill, AttributeSet attrs)
            {
                if (Nature.Is(DamageSource.Attack))
                {
                    // If gem has own Attacks per Second, use it instead of weapon one.
                    if (skill.Local.ContainsKey("Attacks per Second: #"))
                    {
                        APS = skill.Local["Attacks per Second: #"][0];
                        // Apply local increased attack speed of weapon.
                        if (Local.ContainsKey("#% increased Attack Speed"))
                            APS = IncreaseValueByPercentage(APS, Local["#% increased Attack Speed"][0]);
                    }

                    float incAS = 0;
                    if (attrs.ContainsKey("#% increased Attack Speed"))
                        incAS += attrs["#% increased Attack Speed"][0];
                    if (attrs.ContainsKey("#% reduced Attack Speed"))
                        incAS -= attrs["#% reduced Attack Speed"][0];
                    if (attrs.ContainsKey("#% increased Attack and Cast Speed"))
                        incAS += attrs["#% increased Attack and Cast Speed"][0];
                    if (attrs.ContainsKey("#% reduced Attack and Cast Speed"))
                        incAS -= attrs["#% reduced Attack and Cast Speed"][0];
                    foreach (var attr in attrs.MatchesAny(new Regex[] { ReIncreasedAttackSpeedWithWeaponHandOrType, ReIncreasedAttackSpeedWeaponType }))
                    {
                        Match m = ReIncreasedAttackSpeedWithWeaponHandOrType.Match(attr.Key);
                        if (m.Success)
                        {
                            if (WithWeaponHand.ContainsKey(m.Groups[2].Value) && Nature.Is(WithWeaponHand[m.Groups[2].Value]))
                                incAS += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
                            else if (WithWeaponType.ContainsKey(m.Groups[2].Value) && Nature.Is(WithWeaponType[m.Groups[2].Value]))
                                incAS += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
                        }
                        else
                        {
                            m = ReIncreasedAttackSpeedWeaponType.Match(attr.Key);
                            if (m.Success && Weapon.Types.ContainsKey(m.Groups[2].Value) && Nature.Is(Weapon.Types[m.Groups[2].Value]))
                                incAS += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
                        }
                    }
                    if (IsDualWielding && attrs.ContainsKey("#% increased Attack Speed while Dual Wielding"))
                        incAS += attrs["#% increased Attack Speed while Dual Wielding"][0];
                    if (incAS != 0)
                        APS = IncreaseValueByPercentage(APS, incAS);

                    float moreAS = 0;
                    if (attrs.ContainsKey("#% more Attack Speed"))
                        moreAS += attrs["#% more Attack Speed"][0];
                    if (attrs.ContainsKey("#% less Attack Speed"))
                        moreAS -= attrs["#% less Attack Speed"][0];
                    foreach (var attr in attrs.Matches(ReMoreAttackSpeedType))
                    {
                        Match m = ReMoreAttackSpeedType.Match(attr.Key);
                        if (m.Success)
                        {
                            if (Weapon.Types.ContainsKey(m.Groups[2].Value) && Nature.Is(Weapon.Types[m.Groups[2].Value]))
                                moreAS += m.Groups[1].Value == "more" ? attr.Value[0] : -attr.Value[0];
                            else if (DamageNature.Forms.ContainsKey(m.Groups[2].Value) && Nature.Is(DamageNature.Forms[m.Groups[2].Value]))
                                moreAS += m.Groups[1].Value == "more" ? attr.Value[0] : -attr.Value[0];
                        }
                    }
                    if (moreAS != 0)
                        APS = IncreaseValueByPercentage(APS, moreAS);

                    APS = RoundHalfDownEvenValue(APS, 2);
                }
                else // Spell (use Cast Time directly).
                {
                    float incCS = 0;
                    if (attrs.ContainsKey("#% increased Cast Speed"))
                        incCS += attrs["#% increased Cast Speed"][0];
                    if (attrs.ContainsKey("#% reduced Cast Speed"))
                        incCS -= attrs["#% reduced Cast Speed"][0];
                    if (attrs.ContainsKey("#% increased Attack and Cast Speed"))
                        incCS += attrs["#% increased Attack and Cast Speed"][0];
                    if (attrs.ContainsKey("#% reduced Attack and Cast Speed"))
                        incCS -= attrs["#% reduced Attack and Cast Speed"][0];
                    if (IsDualWielding && attrs.ContainsKey("#% increased Cast Speed while Dual Wielding"))
                        incCS += attrs["#% increased Cast Speed while Dual Wielding"][0];
                    if (incCS != 0)
                        CastTime = RoundValue(CastTime / ((100 + incCS) / 100), 3);

                    float moreCS = 0;
                    if (attrs.ContainsKey("#% more Cast Speed"))
                        moreCS += attrs["#% more Cast Speed"][0];
                    if (attrs.ContainsKey("#% less Cast Speed"))
                        moreCS -= attrs["#% less Cast Speed"][0];
                    if (moreCS != 0)
                        CastTime = FloorValue(CastTime / ((100 + moreCS) / 100), 3);

                    APS = RoundValue(1 / CastTime, 2);
                }
            }

            // Returns average hit including critical strikes.
            public float AverageHit()
            {
                Damage total = Deals.Find(d => d.Type == DamageType.Total);

                return total.AverageHit() * (1 + (CriticalChance / 100) * (RoundValue(CriticalMultiplier, 0) - 100) / 100);
            }

            // Combines damage type into total combined damage.
            public void Combine()
            {
                Damage total = new Damage(Nature.Source, DamageType.Total, 0, 0);

                foreach (DamageType type in DamageTypes)
                {
                    List<Damage> deals = Deals.FindAll(d => d.Is(type));
                    if (deals.Count > 0)
                    {
                        if (deals.Count > 1)
                            for (int i = 1; i < deals.Count; ++i)
                            {
                                deals[0].Add(deals[i]);
                                Deals.Remove(deals[i]);
                            }

                        deals[0].Round();
                        total.Add(deals[0]);
                    }
                }

                Deals.Add(total);
            }

            // Returns chance to hit.
            public float ChanceToHit()
            {
                // Chance to hit is always 100% when:
                if (ResoluteTechnique                               // Resolute Technique keystone.
                    || Local.ContainsKey("Hits can't be Evaded")    // Local weapon modifier (Kongor's Undying Rage).
                    || ! Nature.Is(DamageSource.Attack))            // Not an attack (either spell or buff damage).
                    return 100;

                return Compute.ChanceToHit(Level, Accuracy);
            }

            // Computes critical strike chance and multiplier.
            public void CriticalStrike(AttributeSet attrs)
            {
                // Critical chance.
                if (ResoluteTechnique) CriticalChance = 0;
                else
                {
                    if (CriticalChance > 0)
                    {
                        float incCC = 0;
                        if (attrs.ContainsKey("#% increased Critical Strike Chance"))
                            incCC += attrs["#% increased Critical Strike Chance"][0];
                        if (attrs.ContainsKey("#% increased Global Critical Strike Chance"))
                            incCC += attrs["#% increased Global Critical Strike Chance"][0];
                        if (IsWieldingStaff && attrs.ContainsKey("#% increased Global Critical Strike Chance while wielding a Staff"))
                            incCC += attrs["#% increased Global Critical Strike Chance while wielding a Staff"][0];
                        if (Nature.Is(DamageSource.Spell))
                        {
                            if (attrs.ContainsKey("#% increased Critical Strike Chance for Spells"))
                                incCC += attrs["#% increased Critical Strike Chance for Spells"][0];
                            if (attrs.ContainsKey("#% increased Global Critical Strike Chance for Spells"))
                                incCC += attrs["#% increased Global Critical Strike Chance for Spells"][0];
                        }
                        else // Attack
                        {
                            foreach (var attr in attrs.Matches(ReIncreasedCriticalChanceWithWeaponType))
                            {
                                Match m = ReIncreasedCriticalChanceWithWeaponType.Match(attr.Key);
                                if (WithWeaponType.ContainsKey(m.Groups[2].Value) && Nature.Is(WithWeaponType[m.Groups[2].Value]))
                                    incCC += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
                            }
                            if (IsDualWielding && attrs.ContainsKey("#% increased Weapon Critical Strike Chance while Dual Wielding"))
                                incCC += attrs["#% increased Weapon Critical Strike Chance while Dual Wielding"][0];
                        }
                        // Form specific.
                        foreach (var attr in attrs.Matches(ReIncreasedCriticalChanceForm))
                        {
                            Match m = ReIncreasedCriticalChanceForm.Match(attr.Key);
                            if (DamageNature.Forms.ContainsKey(m.Groups[2].Value) && Nature.Is(DamageNature.Forms[m.Groups[2].Value]))
                                incCC += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
                        }
                        if (incCC > 0)
                            CriticalChance = IncreaseValueByPercentage(CriticalChance, incCC);

                        // Critical chance can not be less than 5% nor more than 95%.
                        // @see http://pathofexile.gamepedia.com/Critical_Strike
                        if (CriticalChance < 5) CriticalChance = 5;
                        else if (CriticalChance > 95) CriticalChance = 95;

                        float incCM = 0;
                        if (attrs.ContainsKey("#% increased Critical Strike Multiplier"))
                            incCM += attrs["#% increased Critical Strike Multiplier"][0];
                        if (attrs.ContainsKey("#% increased Global Critical Strike Multiplier"))
                            incCM += attrs["#% increased Global Critical Strike Multiplier"][0];
                        if (IsWieldingStaff && attrs.ContainsKey("#% increased Global Critical Strike Multiplier while wielding a Staff"))
                            incCM += attrs["#% increased Global Critical Strike Multiplier while wielding a Staff"][0];
                        if (Nature.Is(DamageSource.Spell))
                        {
                            if (attrs.ContainsKey("#% increased Critical Strike Multiplier for Spells"))
                                incCM += attrs["#% increased Critical Strike Multiplier for Spells"][0];
                            if (attrs.ContainsKey("#% increased Global Critical Strike Multiplier for Spells"))
                                incCM += attrs["#% increased Global Critical Strike Multiplier for Spells"][0];
                        }
                        else // Attack
                        {
                            foreach (var attr in attrs.Matches(ReIncreasedCriticalMultiplierWithWeaponType))
                            {
                                Match m = ReIncreasedCriticalMultiplierWithWeaponType.Match(attr.Key);
                                if (WithWeaponType.ContainsKey(m.Groups[2].Value) && Nature.Is(WithWeaponType[m.Groups[2].Value]))
                                    incCM += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
                            }
                            if (IsDualWielding && attrs.ContainsKey("#% increased Weapon Critical Strike Multiplier while Dual Wielding"))
                                incCM += attrs["#% increased Weapon Critical Strike Multiplier while Dual Wielding"][0];
                        }
                        // Form specific.
                        foreach (var attr in attrs.Matches(ReIncreasedCriticalMultiplierForm))
                        {
                            Match m = ReIncreasedCriticalMultiplierForm.Match(attr.Key);
                            if (DamageNature.Forms.ContainsKey(m.Groups[2].Value) && Nature.Is(DamageNature.Forms[m.Groups[2].Value]))
                                incCM += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
                        }
                        if (incCM > 0)
                            CriticalMultiplier = IncreaseValueByPercentage(CriticalMultiplier, incCM);
                    }
                }
            }
       }

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
                { "Burning",    DamageForm.DoT }
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

            public DamageNature(List<string> keywords)
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

                static Regex ReAddMod = new Regex("Adds #-# ([^ ]+) Damage$");
                static Regex ReAddInHandMod = new Regex("Adds #-# ([^ ]+) Damage in (Main|Off) Hand$");
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
                public static More Create(KeyValuePair<string, List<float>> attr)
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
                                            if (IsDualWielding && attr.Key == "When Dual Wielding, Deals #% Damage from each Weapon combined")
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

            // The nature from which this damage originated (due to conversion).
            DamageNature Origin;
            // The damage range minimum.
            float Min;
            // The damage range maximum.
            float Max;

            static Regex ReDamageAttribute = new Regex("([^ ]+) Damage: #-#");
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
            public Damage(DamageSource source, DamageType type, float min, float max)
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

            // Returns average hit.
            public float AverageHit()
            {
                return (Min + Max) / 2;
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

            // Returns true if damage or its origin matches nature, false otherwise.
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

            // Multiplies damage by multiplier.
            public void Multiply(float multiplier)
            {
                Min *= multiplier;
                Max *= multiplier;
            }

            public void Round()
            {
                Min = Compute.RoundValue(Min, 0);
                Max = Compute.RoundValue(Max, 0);
            }

            public void RoundHalfDown()
            {
                Min = Compute.RoundHalfDownValue(Min, 0);
                Max = Compute.RoundHalfDownValue(Max, 0);
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

        public class Weapon
        {
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
                { "Wand",               WeaponType.Wand }
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

            public Weapon(Item item)
            {
                if (item != null)
                {
                    Hand = item.Class == Item.ItemClass.MainHand ? WeaponHand.Main : WeaponHand.Off;
                    Item = item;

                    // Get weapon type (damage nature).
                    if (item.Keywords == null) // Quiver or shield.
                    {
                        if (item.Type.Contains("Quiver"))
                            Nature = new DamageNature() { WeaponType = WeaponType.Quiver };
                        else
                            if (item.Type.Contains("Shield") || item.Type.Contains("Buckler") || item.Type == "Spiked Bundle")
                                Nature = new DamageNature() { WeaponType = WeaponType.Shield };
                            else
                                throw new Exception("Unknown weapon type: " + item.Type);
                    }
                    else // Regular weapon.
                        foreach (string keyword in item.Keywords)
                            if (Types.ContainsKey(keyword))
                            {
                                Nature = new DamageNature() { WeaponType = Types[keyword] };
                                break;
                            }

                    // If weapon is melee, it defaults to melee form. If it's ranged then projectile.
                    if (Nature.Is(WeaponType.Melee))
                        Nature.Form = DamageForm.Melee;
                    else
                        if (Nature.Is(WeaponType.Ranged))
                            Nature.Form = DamageForm.Projectile;

                    // Copy attributes and create damage dealt.
                    foreach (var attr in item.Attributes)
                    {
                        Attributes.Add(attr);

                        Damage damage = Damage.Create(Nature, attr);
                        if (damage != null && damage.Type == DamageType.Physical) // Create only physical damage from item properties.
                            Deals.Add(damage);
                    }

                    // Copy local and non-local mods and collect added non-physical damage.
                    foreach (var mod in item.Mods)
                    {
                        if (mod.isLocal)
                        {
                            Damage.Added added = Damage.Added.Create(Nature.Source, mod);
                            if (added != null && added.Type != DamageType.Physical)
                                Added.Add(added);
                        }

                        Attributes.Add(mod);
                    }
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

            // Returns true if weapon is a regular weapon, false otherwise.
            public bool IsWeapon()
            {
                return Nature != null && (Nature.WeaponType & WeaponType.Weapon) != 0;
            }
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
            Melee = Claw | Dagger | OneHandedAxe | OneHandedMace | OneHandedSword | Staff | TwoHandedAxe | TwoHandedMace | TwoHandedSword,
            OneHandedMelee = Claw | Dagger | OneHandedAxe | OneHandedMace | OneHandedSword,
            TwoHandedMelee = Staff | TwoHandedAxe | TwoHandedMace | TwoHandedSword,
            Axe = OneHandedAxe | TwoHandedAxe,
            Mace = OneHandedMace | TwoHandedMace,
            Sword = OneHandedSword | TwoHandedSword,
            Ranged = Bow | Wand,
            Weapon = Melee | Ranged
        }

        // Equipped items.
        public static List<Item> Items;
        // Main hand weapon.
        public static Weapon MainHand;
        // Off hand weapon or quiver/shield.
        public static Weapon OffHand;
        // The flag whether character is dual wielding.
        public static bool IsDualWielding;
        // The flag whether character is wielding a shield.
        public static bool IsWieldingShield;
        // The flag whether character is wielding a staff.
        public static bool IsWieldingStaff;
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
        public static bool IronGrip;
        public static bool IronReflexes;
        public static bool NecromanticAegis;
        public static bool ResoluteTechnique;
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
        // Monster average evasion rating for each level (1 .. 100).
        public static int[] MonsterAverageEvasion = new int[] { 0, // Level 0 placeholder.
              36,     42,     49,     56,     64,
              72,     80,     89,     98,    108,
             118,    128,    140,    151,    164,
             177,    190,    204,    219,    235,
             251,    268,    286,    305,    325,
             345,    367,    389,    412,    437,
             463,    489,    517,    546,    577,
             609,    642,    676,    713,    750,
             790,    831,    873,    918,    964,
            1013,   1063,   1116,   1170,   1227,
            1287,   1349,   1413,   1480,   1550,
            1623,   1698,   1777,   1859,   1944,
            2033,   2125,   2221,   2321,   2425,
            2533,   2645,   2761,   2883,   3009,
            3140,   3276,   3418,   3565,   3717,
            3876,   4041,   4213,   4391,   4576,
            4768,   4967,   5174,   5389,   5613,
            5845,   6085,   6335,   6595,   6864,
            7144,   7434,   7735,   8048,   8372,
            8709,   9058,   9420,   9796,  10186
        };

        // The sorted list of damage types for character sheet.
        static List<DamageType> DamageTypes = new List<DamageType>()
        {
            DamageType.Total, DamageType.Physical, DamageType.Fire, DamageType.Cold, DamageType.Lightning, DamageType.Chaos
        };
        // The dictionary of weapon hands.
        static Dictionary<string, WeaponHand> WithWeaponHand = new Dictionary<string, WeaponHand>()
        {
            { "Main Hand",                  WeaponHand.Main },
            { "Off Hand",                   WeaponHand.Off }
        };
        // The dictionary of weapon types.
        static Dictionary<string, WeaponType> WithWeaponType = new Dictionary<string, WeaponType>()
        {
            { "Bows",                       WeaponType.Bow },
            { "Claws",                      WeaponType.Claw },
            { "Daggers",                    WeaponType.Dagger },
            { "Wands",                      WeaponType.Wand },
            { "One Handed Melee Weapons",   WeaponType.OneHandedMelee },
            { "Two Handed Melee Weapons",   WeaponType.TwoHandedMelee },
            { "Axes",                       WeaponType.Axe },
            { "Maces",                      WeaponType.Mace },
            { "Staves",                     WeaponType.Staff },
            { "Swords",                     WeaponType.Sword }
        };

        // Returns rounded value with all fractional digits after specified precision cut off.
        public static float CeilValue(float value, int precision)
        {
            float coeff = (float)Math.Pow(10, precision);

            return (float)(Math.Ceiling((float)(value * coeff)) / coeff);
        }

        // Chance to Evade = 1 - Attacker's Accuracy / ( Attacker's Accuracy + (Defender's Evasion / 4) ^ 0.8 )
        // Chance to hit can never be lower than 5%, nor higher than 95%.
        // @see http://pathofexile.gamepedia.com/Evasion
        public static float ChanceToEvade(int level, float evasionRating)
        {
            if (Global.ContainsKey("Cannot Evade enemy Attacks")) return 0; // The modifier can be from other source than Unwavering Stance.

            int maa = MonsterAverageAccuracy[level];

            float chance = RoundValue((float)(1 - maa / (maa + Math.Pow(evasionRating / 4, 0.8))) * 100, 0);
            if (chance < 5f) chance = 5f;
            else if (chance > 95f) chance = 95f;

            return chance;
        }

        // Chance to Hit = Attacker's Accuracy / ( Attacker's Accuracy + (Defender's Evasion / 4) ^ 0.8 )
        // Chance to hit can never be lower than 5%, nor higher than 95%.
        // @see http://pathofexile.gamepedia.com/Accuracy
        public static float ChanceToHit(int level, float accuracyRating)
        {
            int mae = MonsterAverageEvasion[level - 1]; // XXX: For some reason this works.

            float chance = (float)(accuracyRating / (accuracyRating + Math.Pow(mae / 4, 0.8))) * 100;
            if (chance < 5f) chance = 5f;
            else if (chance > 95f) chance = 95f;

            return chance;
        }

        // Computes core attributes.
        private static void CoreAttributes ()
        {
            float strength = 0;
            float dexterity = 0;
            float intelligence = 0;

            // Citrine Amulet
            if (Global.ContainsKey("+# to Strength and Dexterity"))
            {
                strength += Global["+# to Strength and Dexterity"][0];
                dexterity += Global["+# to Strength and Dexterity"][0];

                Global.Remove("+# to Strength and Dexterity");
            }

            // Agate Amulet
            if (Global.ContainsKey("+# to Strength and Intelligence"))
            {
                strength += Global["+# to Strength and Intelligence"][0];
                intelligence += Global["+# to Strength and Intelligence"][0];

                Global.Remove("+# to Strength and Intelligence");
            }

            // Turquoise Amulet
            if (Global.ContainsKey("+# to Dexterity and Intelligence"))
            {
                dexterity += Global["+# to Dexterity and Intelligence"][0];
                intelligence += Global["+# to Dexterity and Intelligence"][0];

                Global.Remove("+# to Dexterity and Intelligence");
            }

            // TODO: Onyx Amulet (when hack in POESKillTree.ViewModels.ItemAttribute.ItemMod.CreateMods will be removed).
            /*
            if (Global.ContainsKey("+# to all Attributes"))
            {
                strength += Global["+# to all Attributes"][0];
                dexterity += Global["+# to all Attributes"][0];
                intelligence += Global["+# to all Attributes"][0];

                Global.Remove("+# to all Attributes");
            }
             */

            if (strength != 0)
                Global["+# to Strength"][0] += strength;
            if (dexterity != 0)
                Global["+# to Dexterity"][0] += dexterity;
            if (intelligence != 0)
                Global["+# to Intelligence"][0] += intelligence;
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
            float incES = 0;
            // Add maximum shield from tree.
            if (Global.ContainsKey("+# to maximum Energy Shield"))
                es += Global["+# to maximum Energy Shield"][0];
            // Add maximum shield from items.
            if (Global.ContainsKey("Energy Shield: #"))
                es += Global["Energy Shield: #"][0];
            // Increase % maximum shield from tree, items and intelligence.
            if (Global.ContainsKey("#% increased maximum Energy Shield"))
                incES += Global["#% increased maximum Energy Shield"][0];

            float moreES = 0;
            // More % maximum shield from tree and items.
            if (Global.ContainsKey("#% more maximum Energy Shield"))
                moreES += Global["#% more maximum Energy Shield"][0];

            float lessArmourAndES = 0;
            if (Acrobatics)
                lessArmourAndES += Global["#% Chance to Dodge Attacks. #% less Armour and Energy Shield, #% less Chance to Block Spells and Attacks"][1];

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
                List<float> value = OffHand.GetValues("Armour: #");
                if (value.Count > 0)
                    shieldArmour += PercentOfValue(value[0], incArmourShield + incDefencesShield);

                value = OffHand.GetValues("Evasion Rating: #");
                if (value.Count > 0)
                    shieldEvasion += PercentOfValue(value[0], incDefencesShield);

                value = OffHand.GetValues("Energy Shield: #");
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

            // Evasion Rating from level, tree and items.
            float evasion = Global["Evasion Rating: #"][0];
            if (Global.ContainsKey("+# to Evasion Rating"))
                evasion += Global["+# to Evasion Rating"][0];
            // Increase % from dexterity, tree and items.
            float incEvasion = Global["#% increased Evasion Rating"][0];
            float incEvasionAndArmour = 0;
            if (Global.ContainsKey("#% increased Evasion Rating and Armour"))
                incEvasionAndArmour += Global["#% increased Evasion Rating and Armour"][0];

            float armour = 0;
            float armourProjectile = 0;
            // Armour from items.
            if (Global.ContainsKey("Armour: #"))
                armour += Global["Armour: #"][0];
            if (Global.ContainsKey("+# to Armour"))
                armour += Global["+# to Armour"][0];
            float incArmour = 0;
            float incArmourProjectile = 0;
            if (Global.ContainsKey("#% increased Armour"))
                incArmour += Global["#% increased Armour"][0];
            if (Global.ContainsKey("#% increased Armour against Projectiles"))
                incArmourProjectile += Global["#% increased Armour against Projectiles"][0];
            // Enable armour against projectile calculations once there is some Armour against Projectiles modifier.
            if (incArmourProjectile != 0)
                armourProjectile = armour;

            // Final Armour = Base Evasion * ( 1 + % increased Evasion Rating + % increased Armour + % increased Evasion Rating and Armour )
            //              + Base Armour  * ( 1 + % increased Armour                              + % increased Evasion Rating and Armour )
            // @see http://pathofexile.gamepedia.com/Iron_Reflexes
            if (IronReflexes)
            {
                // Substract "#% increased Evasion Rating" from Dexterity (it's not being applied).
                incEvasion -= Implicit["#% increased Evasion Rating"][0];
                armour = IncreaseValueByPercentage(armour, incArmour + incEvasionAndArmour) + IncreaseValueByPercentage(evasion, incEvasion + incArmour + incEvasionAndArmour);
                armour += shieldArmour + shieldEvasion;
                if (armourProjectile > 0)
                {
                    armourProjectile = IncreaseValueByPercentage(armourProjectile, incArmour + incArmourProjectile + incEvasionAndArmour) + IncreaseValueByPercentage(evasion, incEvasion + incArmour + incEvasionAndArmour);
                    armourProjectile += shieldArmour + shieldEvasion;
                }
                evasion = 0;
            }
            else
            {
                evasion = IncreaseValueByPercentage(evasion, incEvasion + incEvasionAndArmour) + shieldEvasion;
                armour = IncreaseValueByPercentage(armour, incArmour + incEvasionAndArmour) + shieldArmour;
                if (armourProjectile > 0)
                    armourProjectile = IncreaseValueByPercentage(armourProjectile, incArmour + incArmourProjectile + incEvasionAndArmour) + shieldArmour;
            }
            if (lessArmourAndES > 0)
            {
                armour = IncreaseValueByPercentage(armour, -lessArmourAndES);
                if (armourProjectile > 0)
                    armourProjectile = IncreaseValueByPercentage(armourProjectile, -lessArmourAndES);
            }

            if (armour > 0)
            {
                def["Armour: #"] = new List<float>() { RoundValue(armour, 0) };
                def["Estimated Physical Damage reduction: #%"] = new List<float>() { RoundValue(PhysicalDamageReduction(Level, RoundValue(armour, 0)), 0) };
            }
            if (armourProjectile > 0)
            {
                def["Armour against Projectiles: #"] = new List<float>() { RoundValue(armourProjectile, 0) };
                def["Estimated Physical Damage reduction against Projectiles: #%"] = new List<float>() { RoundValue(PhysicalDamageReduction(Level, RoundValue(armourProjectile, 0)), 0) };
            }
            if (evasion > 0)
                def["Evasion Rating: #"] = new List<float>() { RoundValue(evasion, 0) };
            def["Estimated chance to Evade Attacks: #%"] = new List<float>() { ChanceToEvade(Level, RoundValue(evasion, 0)) };

            // Dodge Attacks and Spells.
            float chanceToDodgeAttacks = 0;
            float chanceToDodgeSpells = 0;
            if (Acrobatics)
                chanceToDodgeAttacks += Global["#% Chance to Dodge Attacks. #% less Armour and Energy Shield, #% less Chance to Block Spells and Attacks"][0];
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
                float esOccurrence = 0;
                if (Global.ContainsKey("#% faster start of Energy Shield Recharge"))
                    esOccurrence += Global["#% faster start of Energy Shield Recharge"][0];
                if (Global.ContainsKey("#% slower start of Energy Shield Recharge"))
                    esOccurrence -= Global["#% slower start of Energy Shield Recharge"][0];
                esDelay = esDelay * 100 / (100 + esOccurrence);
                if (esOccurrence != 0)
                    def["Energy Shield Recharge Occurrence modifier: " + (esOccurrence > 0 ? "+" : "") + "#%"] = new List<float>() { esOccurrence };
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
            float maxChanceBlockProjectiles = 75;
            float chanceBlockAttacks = 0;
            float chanceBlockSpells = 0;
            float chanceBlockProjectiles = 0;
            if (Global.ContainsKey("+#% to maximum Block Chance"))
            {
                maxChanceBlockAttacks += Global["+#% to maximum Block Chance"][0];
                maxChanceBlockSpells += Global["+#% to maximum Block Chance"][0];
                maxChanceBlockProjectiles += Global["+#% to maximum Block Chance"][0];
            }
            if (hasShield)
            {
                List<float> values = OffHand.GetValues("Chance to Block: #%");
                if (values.Count > 0) chanceBlockAttacks += values[0];
            }
            else if (IsWieldingStaff)
            {
                List<float> values = MainHand.GetValues("#% Chance to Block");
                if (values.Count > 0) chanceBlockAttacks += values[0];
            }
            else if (IsDualWielding)
                chanceBlockAttacks += 15; // When dual wielding, the base chance to block is 15% no matter which weapons are used.
            if (hasShield && Global.ContainsKey("#% additional Chance to Block with Shields"))
                chanceBlockAttacks += Global["#% additional Chance to Block with Shields"][0];
            if (IsWieldingStaff && Global.ContainsKey("#% additional Block Chance With Staves"))
                chanceBlockAttacks += Global["#% additional Block Chance With Staves"][0];
            if (IsDualWielding && Global.ContainsKey("#% additional Chance to Block while Dual Wielding"))
                chanceBlockAttacks += Global["#% additional Chance to Block while Dual Wielding"][0];
            if ((IsDualWielding || hasShield) && Global.ContainsKey("#% additional Chance to Block while Dual Wielding or holding a Shield"))
                chanceBlockAttacks += Global["#% additional Chance to Block while Dual Wielding or holding a Shield"][0];
            if (Global.ContainsKey("#% of Block Chance applied to Spells"))
                chanceBlockSpells = PercentOfValue(chanceBlockAttacks, Global["#% of Block Chance applied to Spells"][0]);
            if (hasShield && Global.ContainsKey("#% additional Chance to Block Spells with Shields"))
                chanceBlockSpells += Global["#% additional Chance to Block Spells with Shields"][0];
            if (Global.ContainsKey("+#% additional Block Chance against Projectiles"))
                chanceBlockProjectiles = chanceBlockAttacks + Global["+#% additional Block Chance against Projectiles"][0];
            if (Acrobatics)
            {
                float lessChanceBlock = Global["#% Chance to Dodge Attacks. #% less Armour and Energy Shield, #% less Chance to Block Spells and Attacks"][2];
                chanceBlockAttacks = IncreaseValueByPercentage(chanceBlockAttacks, -lessChanceBlock);
                chanceBlockSpells = IncreaseValueByPercentage(chanceBlockSpells, -lessChanceBlock);
            }
            if (chanceBlockAttacks > 0)
                def["Chance to Block Attacks: #%"] = new List<float>() { MaximumValue(RoundValue(chanceBlockAttacks, 0), maxChanceBlockAttacks) };
            if (chanceBlockSpells > 0)
                def["Chance to Block Spells: #%"] = new List<float>() { MaximumValue(RoundValue(chanceBlockSpells, 0), maxChanceBlockSpells) };
            if (chanceBlockProjectiles > 0)
                def["Chance to Block Projectile Attacks: #%"] = new List<float>() { MaximumValue(RoundValue(chanceBlockProjectiles, 0), maxChanceBlockProjectiles) };

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

        // Returns rounded value with all fractional digits after specified precision cut off.
        public static float FloorValue(float value, int precision)
        {
            float coeff = (float)Math.Pow(10, precision);

            return (float)(Math.Floor((float)(value * coeff)) / coeff);
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

            // If main hand weapon has Counts as Dual Wielding modifier, then clone weapon to off hand.
            // @see http://pathofexile.gamepedia.com/Wings_of_Entropy
            if (MainHand.Attributes.ContainsKey("Counts as Dual Wielding"))
                OffHand = MainHand.Clone(WeaponHand.Off);

            IsDualWielding = MainHand.IsWeapon() && OffHand.IsWeapon();
            if (IsDualWielding)
            {
                // Set dual wielded bit on weapons.
                MainHand.Hand |= WeaponHand.DualWielded;
                OffHand.Hand |= WeaponHand.DualWielded;
            }
            IsWieldingShield = MainHand.Is(WeaponType.Shield) || OffHand.Is(WeaponType.Shield);
            IsWieldingStaff = MainHand.Is(WeaponType.Staff);

            Level = skillTree.Level;
            if (Level < 1) Level = 1;
            else if (Level > 100) Level = 100;

            Global = new AttributeSet();

            Tree = new AttributeSet(skillTree.SelectedAttributesWithoutImplicit);
            Global.Add(Tree);

            // Keystones.
            Acrobatics = Tree.ContainsKey("#% Chance to Dodge Attacks. #% less Armour and Energy Shield, #% less Chance to Block Spells and Attacks");
            AvatarOfFire = Tree.ContainsKey("Deal no Non-Fire Damage");
            BloodMagic = Tree.ContainsKey("Removes all mana. Spend Life instead of Mana for Skills");
            ChaosInoculation = Tree.ContainsKey("Maximum Life becomes #, Immune to Chaos Damage");
            EldritchBattery = Tree.ContainsKey("Converts all Energy Shield to Mana");
            IronGrip = Tree.ContainsKey("The increase to Physical Damage from Strength applies to Projectile Attacks as well as Melee Attacks");
            IronReflexes = Tree.ContainsKey("Converts all Evasion Rating to Armour. Dexterity provides no bonus to Evasion Rating");
            NecromanticAegis = Tree.ContainsKey("All bonuses from an equipped Shield apply to your Minions instead of you");
            ResoluteTechnique = Tree.ContainsKey("Never deal Critical Strikes");
            VaalPact = Tree.ContainsKey("Life Leech applies instantly at #% effectiveness. Life Regeneration has no effect.");
            ZealotsOath = Tree.ContainsKey("Life Regeneration applies to Energy Shield instead of Life");

            Equipment = new AttributeSet();
            foreach (ItemAttributes.Attribute attr in itemAttrs.NonLocalMods)
                Equipment.Add(attr.TextAttribute, new List<float>(attr.Value));

            if (NecromanticAegis && OffHand.IsShield())
            {
                // Remove all bonuses of shield from equipment set.
                // @see http://pathofexile.gamepedia.com/Necromantic_Aegis
                foreach (var attr in OffHand.Attributes)
                    Equipment.Remove(attr);
                // Remove all bonuses from shield itself.
                OffHand.Attributes.Clear();
            }

            Global.Add(Equipment);

            CoreAttributes();

            Implicit = new AttributeSet(SkillTree.ImplicitAttributes(Global, Level));
            Global.Add(Implicit);

            // Innate dual wielding bonuses.
            // @see http://pathofexile.gamepedia.com/Dual_wielding
            if (IsDualWielding)
            {
                Global["#% more Attack Speed"] = new List<float>() { 10 };
                Global["#% more Physical Damage with Weapons"] = new List<float>() { 20 };
            }
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

            return RoundValue((float)Math.Pow(a + b * level, c) + d, 0);
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
                        if (item.GetLinkedGems(gem).Find(g => g.Name.Contains("Totem")) != null) continue;

                        AttackSkill attack = AttackSkill.Create(gem);

                        attack.Link(item.GetLinkedGems(gem), item);
                        attack.Apply(item);

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
            float mad = MonsterAverageDamage(level);
            float reduction = RoundValue(armour / (armour + 12 * mad) * 100, 1);
            if (reduction > 90f) reduction = 90f;

            return reduction;
        }

        // Returns rounded value with specified number of fractional digits (round half down if even digit before half).
        public static float RoundHalfDownEvenValue(float value, int precision)
        {
            // Detect half.
            float coeff = (float)Math.Pow(10, precision);
            float half = value * coeff;

            return (half - (int)half == 0.5 || half - (int)half == -0.5) && (int)half % 2 == 0
                   ? (float)((int)half) / coeff
                   : (float)Math.Round((decimal)value, precision, MidpointRounding.AwayFromZero);
        }

        // Returns rounded value with specified number of fractional digits (round half down).
        public static float RoundHalfDownValue(float value, int precision)
        {
            // Detect half.
            float coeff = (float)Math.Pow(10, precision);
            float half = value * coeff;

            return half - (int)half == 0.5 || half - (int)half == -0.5
                   ? (float)((int)half) / coeff
                   : (float)Math.Round((decimal)value, precision, MidpointRounding.AwayFromZero);
        }

        // Returns rounded value with specified number of fractional digits.
        public static float RoundValue(float value, int precision)
        {
            return (float)Math.Round((decimal)value, precision, MidpointRounding.AwayFromZero);
        }
    }
}
