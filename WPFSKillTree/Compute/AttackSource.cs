using System.Collections.Generic;
using System.Text.RegularExpressions;
using POESKillTree.Model;
using POESKillTree.Model.Items.Affixes;
using static POESKillTree.Compute.ComputeGlobal;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Compute
{

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
        public Computation Compute;

        // The increased/reduced accuracy rating with weapon type pattern.
        static Regex ReIncreasedAccuracyRatingWithWeaponType = new Regex("#% (increased|reduced) Accuracy Rating with (.+)$");
        // The increased/reduced attack speed patterns.
        static Regex ReIncreasedAttackSpeedType = new Regex("#% (increased|reduced) (.+) Attack Speed$");
        static Regex ReIncreasedAttackSpeedWithWeaponHandOrType = new Regex("#% (increased|reduced) Attack Speed with (.+)$");
        // The more/less attack speed patterns.
        static Regex ReMoreAttackSpeedType = new Regex("#% (more|less) (.+) Attack Speed$");
        // The form specific increased/reduced critical chance/multiplier patterns.
        static Regex ReIncreasedCriticalChanceForm = new Regex("#% (increased|reduced) (.+) Critical Strike Chance$");
        static Regex ReIncreasedCriticalMultiplierForm = new Regex(@"(\+|-)#% to (.+) Critical Strike Multiplier$");
        // The increased/reduced critical chance/multiplier with weapon type patterns.
        static Regex ReIncreasedCriticalChanceWithWeaponType = new Regex("#% (increased|reduced) Critical Strike Chance with (.+)$");
        static Regex ReIncreasedCriticalMultiplierWithWeaponType = new Regex(@"(\+|-)#% to Critical Strike Multiplier with (.+)$");

        public AttackSource(string name, AttackSkill skill, Weapon weapon, Computation compute)
        {
            Name = name;
            Compute = compute;

            if (weapon == null) // Spells get damage from gem local attributes.
            {
                Nature = new DamageNature(skill.Nature);

                foreach (var attr in skill.Local)
                {
                    Damage damage = Damage.Create(skill.Nature, attr.Key, attr.Value);
                    if (damage != null) Deals.Add(damage);
                }

                if (skill.Gem.Properties.TryGetValue("Cast Time: # sec", 0, out CastTime))
                {
                    APS = 1 / CastTime;
                }
                else
                    APS = CastTime = 1; // Spell without Cast Time has cast time of 1 second.

                if (!skill.Gem.Properties.TryGetValue("Critical Strike Chance: #%", 0, out CriticalChance))
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
                WeaponType weaponType;
                if (WithWeaponType.TryGetValue(m.Groups[2].Value, out weaponType) && Nature.Is(weaponType))
                    incAcc += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
            }
            if (Compute.IsDualWielding && attrs.ContainsKey("#% increased Accuracy Rating while Dual Wielding"))
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
                foreach (var attr in attrs.MatchesAny(new Regex[] { ReIncreasedAttackSpeedWithWeaponHandOrType, ReIncreasedAttackSpeedType }))
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
                        m = ReIncreasedAttackSpeedType.Match(attr.Key);
                        if (m.Success)
                        {
                            // XXX: Not sure there are any mods with WeaponType here (Melee string in mod is DamageForm now, maybe Unarmed should be form as well).
                            if (Weapon.Types.ContainsKey(m.Groups[2].Value) && Nature.Is(Weapon.Types[m.Groups[2].Value]))
                                incAS += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
                            else
                                if (DamageNature.Forms.ContainsKey(m.Groups[2].Value) && Nature.Is(DamageNature.Forms[m.Groups[2].Value]))
                                incAS += m.Groups[1].Value == "increased" ? attr.Value[0] : -attr.Value[0];
                        }
                    }
                }
                if (Compute.IsDualWielding && attrs.ContainsKey("#% increased Attack Speed while Dual Wielding"))
                    incAS += attrs["#% increased Attack Speed while Dual Wielding"][0];
                if (Compute.OffHand.IsShield() && attrs.ContainsKey("#% increased Attack Speed while holding a Shield"))
                    incAS += attrs["#% increased Attack Speed while holding a Shield"][0];
                if (incAS != 0)
                    APS = IncreaseValueByPercentage(APS, incAS);

                float moreAS = 1;
                if (attrs.ContainsKey("#% more Attack Speed"))
                    moreAS *= 1 + attrs["#% more Attack Speed"][0] / 100;
                if (attrs.ContainsKey("#% less Attack Speed"))
                    moreAS *= 1 - attrs["#% less Attack Speed"][0] / 100;
                foreach (var attr in attrs.Matches(ReMoreAttackSpeedType))
                {
                    Match m = ReMoreAttackSpeedType.Match(attr.Key);
                    if (m.Success)
                    {
                        // XXX: Not sure there are any mods with WeaponType here (Melee string in mod is DamageForm now, maybe Unarmed should be form as well).
                        if (Weapon.Types.ContainsKey(m.Groups[2].Value) && Nature.Is(Weapon.Types[m.Groups[2].Value])
                            || DamageNature.Forms.ContainsKey(m.Groups[2].Value) && Nature.Is(DamageNature.Forms[m.Groups[2].Value]))
                            moreAS *= m.Groups[1].Value == "more" ? 1 + attr.Value[0] / 100 : 1 - attr.Value[0] / 100;
                    }
                }
                APS = APS * moreAS;
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
                if (Compute.IsDualWielding && attrs.ContainsKey("#% increased Cast Speed while Dual Wielding"))
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
            if (Compute.ResoluteTechnique                               // Resolute Technique keystone.
                || Local.ContainsKey("Hits can't be Evaded")    // Local weapon modifier (Kongor's Undying Rage).
                || !Nature.Is(DamageSource.Attack))            // Not an attack (either spell or buff damage).
                return 100;

            return Compute.ChanceToHit(Compute.Level, Accuracy);
        }

        // Computes critical strike chance and multiplier.
        public void CriticalStrike(AttributeSet attrs)
        {
            // Critical chance.
            if (Compute.ResoluteTechnique) CriticalChance = 0;
            else
            {
                if (CriticalChance > 0)
                {
                    float incCC = 0;
                    if (attrs.ContainsKey("#% increased Critical Strike Chance"))
                        incCC += attrs["#% increased Critical Strike Chance"][0];
                    if (attrs.ContainsKey("#% increased Global Critical Strike Chance"))
                        incCC += attrs["#% increased Global Critical Strike Chance"][0];
                    if (Compute.IsWieldingStaff && attrs.ContainsKey("#% increased Global Critical Strike Chance while wielding a Staff"))
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
                        if (Compute.IsDualWielding && attrs.ContainsKey("#% increased Weapon Critical Strike Chance while Dual Wielding"))
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
                    if (attrs.ContainsKey("+#% to Critical Strike Multiplier"))
                        incCM += attrs["+#% to Critical Strike Multiplier"][0];
                    if (attrs.ContainsKey("+#% to Global Critical Strike Multiplier"))
                        incCM += attrs["+#% to Global Critical Strike Multiplier"][0];
                    if (Compute.IsWieldingStaff && attrs.ContainsKey("+#% to Global Critical Strike Multiplier while wielding a Staff"))
                        incCM += attrs["+#% to Global Critical Strike Multiplier while wielding a Staff"][0];
                    if (Nature.Is(DamageSource.Spell))
                    {
                        if (attrs.ContainsKey("+#% to Critical Strike Multiplier for Spells"))
                            incCM += attrs["+#% to Critical Strike Multiplier for Spells"][0];
                        if (attrs.ContainsKey("+#% to Global Critical Strike Multiplier for Spells"))
                            incCM += attrs["+#% to Global Critical Strike Multiplier for Spells"][0];
                    }
                    else // Attack
                    {
                        foreach (var attr in attrs.Matches(ReIncreasedCriticalMultiplierWithWeaponType))
                        {
                            Match m = ReIncreasedCriticalMultiplierWithWeaponType.Match(attr.Key);
                            if (WithWeaponType.ContainsKey(m.Groups[2].Value) && Nature.Is(WithWeaponType[m.Groups[2].Value]))
                                incCM += m.Groups[1].Value == "+" ? attr.Value[0] : -attr.Value[0];
                        }
                        if (Compute.IsDualWielding && attrs.ContainsKey("+#% to Weapon Critical Strike Multiplier while Dual Wielding"))
                            incCM += attrs["+#% to Weapon Critical Strike Multiplier while Dual Wielding"][0];
                    }
                    // Form specific.
                    foreach (var attr in attrs.Matches(ReIncreasedCriticalMultiplierForm))
                    {
                        Match m = ReIncreasedCriticalMultiplierForm.Match(attr.Key);
                        if (DamageNature.Forms.ContainsKey(m.Groups[2].Value) && Nature.Is(DamageNature.Forms[m.Groups[2].Value]))
                            incCM += m.Groups[1].Value == "+" ? attr.Value[0] : -attr.Value[0];
                    }
                    CriticalMultiplier += incCM;
                    if (attrs.ContainsKey("No Critical Strike Multiplier"))
                        CriticalMultiplier = 100;
                }
            }
        }
    }
}
