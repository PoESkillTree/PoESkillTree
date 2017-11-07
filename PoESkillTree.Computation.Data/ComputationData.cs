// No code here, just design thoughts that I needed to write down somewhere.
// TODO find a place for/solve everything currently in here and remove the file

// most local stats are already added to properties and don't need to be handled here
// (those stats are handled elsewhere and don't even get here)
// locality information for local stats not handled elsewhere gets passed to Computation

/* TODO not handled yet
 * - unique complex value conversion
each Spectral Throw Projectile gains 5% increased Damage each time it Hits
 * - unique complex condition
 * - adds new sub skill
can summon up to 3 Skeleton Mages with Summon Skeletons
Animate Weapon can Animate up to 8 Ranged Weapons
Minions explode when reduced to Low Life, dealing 33% of their maximum Life as Fire Damage to surrounding Enemies
Your Minions spread Caustic Cloud on Death, dealing #% of their maximum Life as Chaos Damage per second
 * - adds new parts for skills
Barrage fires an additional 2 projectiles simultaneously on the first and final attacks
Burning Arrow has a 10% chance to spread Burning Ground if it Ignites an Enemy
 * - adds new parts for skills: splash damage
Dual Strike deals Off-Hand Splash Damage to surrounding targets
Glacial Hammer deals Cold-only Splash Damage to surrounding targets
Single-target Melee attacks deal Splash Damage to surrounding targets
#% less Damage to surrounding targets
 * - modifies skills
Desecrate creates # additional Corpses
 */

/* not handled: (no useful usage)
 * Jewels:
Your Golems are aggressive
#% chance to gain an additional Vaal Soul per Enemy Shattered
#% chance to gain an additional Vaal Soul on Kill
Frostbolt Projectiles gain 40% increased Projectile Speed per second
Ethereal Knives fires Projectiles in a Nova
#% increased Experience Gain for Corrupted Gems
Fireball Projectiles gain Radius as they travel farther, up to +4 Radius
Raised Spectres have a 50% chance to gain Soul Eater for 30 seconds on Kill
Ground Slam has a 35% increased angle
Vigilant Strike Fortifies you and Nearby Allies for 12 seconds
Burning Arrow has a 10% chance to spread Tar if it does not Ignite an Enemy
#% additional Chance to receive a Critical Strike
#% increased Rarity of Items dropped by Enemies Shattered by Glacial Hammer
Magma Orb has 10% increased Area of Effect per Chain
 * passive skills:
#% Chance for Traps to Trigger an additional time
Spend Energy Shield before Mana for Skill Costs
Spend Life instead of Mana for Skills
Share Endurance, Frenzy and Power Charges with nearby party members
You can't deal Damage with your Skills yourself
#% chance to gain a Power, Frenzy or Endurance Charge on Kill
Enemies Become Chilled as they Unfreeze
Light Radius is based on Energy Shield instead of Life
Traps cannot be Damaged for 5 seconds after being Thrown
Mines cannot be Damaged for 5 seconds after being Placed
#% chance to Steal Power, Frenzy and Endurance Charges on Hit with Claws
Enemies Cannot Leech Life From You
Hits that Stun Enemies have Culling Strike
 * Ascendancies:
Bleeding Enemies you Kill Explode, dealing #% of their Maximum Life as Physical Damage
Elemental Ailments are removed when you reach Low Life
Enemies you Taunt deal #% less Damage against other targets
Kill Enemies that have #% or lower Life when Hit by your Skills
#% of Overkill Damage is Leeched as Life
Life Leech effects are not removed at Full Life
Remove Bleeding on Flask use
Gain a Flask Charge when you deal a Critical Strike
#% chance to gain a Flask Charge when you deal a Critical Strike
Flasks gain a Charge every 3 seconds
#% chance for your Flasks to not consume Charges
Enemies you Kill that are affected by Elemental Ailments grant 100% increased Flask Charges
Attack Projectiles Return to You after hitting targets
Moving while Bleeding doesn't cause you to take extra Damage
When your Traps Trigger, your nearby Traps also Trigger
#% chance when Placing Mines to Place an additional Mine
#% increased Mine Arming Speed
Cannot be Blinded
#% chance to create a Smoke Cloud when Hit
#% chance to create a Smoke Cloud when you place a Mine or throw a Trap
#% chance to Recover #% of Maximum Mana when you use a Skill
Critical Strikes have Culling Strike
Elemental Ailments caused by your Skills spread to other nearby Enemies Radius: #
Energy Shield Recharge is not interrupted by Damage if Recharge began Recently
#% reduced life regeneration rate
Cursed Enemies you Kill Explode, dealing a quarter of their maximum Life as Chaos Damage
Nearby Enemies cannot gain Power, Frenzy or Endurance Charges
You and Nearby Party Members Share Power, Frenzy and Endurance Charges with each other
Every # seconds, remove Curses and Elemental Ailments from you
Every # seconds, #% of Maximum Life Regenerated over one second
#% additional Block Chance for # second every # seconds
#% chance to create Consecrated Ground when Hit, lasting # seconds
#% chance to create Consecrated Ground on Kill, lasting # seconds
When you or your Totems Kill a Burning Enemy, #% chance for you and your Totems to each gain an Endurance Charge
Totems Reflect #% of their maximum Life as Fire Damage to nearby Enemies when Hit
Recover #% of Life and Mana when you use a Warcry
#% chance that if you would gain Endurance Charges, you instead gain up to your maximum number of Endurance Charges
 */

/* Order of matcher referencing:
 * 
 * Can be referenced by everything else: Action-, Ailment-, ChargeType-, Damage-, Flag-, ItemSlot-
 * and KeywordMatchers
 * 
 * "StatMatchers" implies that General-, Damage- and PoolStatMatchers can be referenced
 * 
 * SpecialMatchers: can also reference StatMatchers
 * FormAndStatMatchers: can also reference StatMatchers
 * ValueConversionMatchers: can also reference StatMatchers
 * GeneralStatMatchers: can also reference DamageStatMatchers and PoolStatMatchers
 * 
 * Other matchers can be set referenceable by overriding their "ReferenceNames" property. This is
 * only allowed as long as none of their matchers contains values and as long as no cyclical
 * reference chains are created.
 */

/* Keystones:
 * - stats are automatically added to EffectMatchers (split at line breaks)
 * - a IFlagStatBuilder is automatically created and added to FormAndStatMatchers 
 *   in the form:
 *   { "keystone name", BaseOverride, KeystoneStat, 1 }
 */

// "{SkillMatchers}" mostly matches skills by name, which does not need to be implemented 
// in ComputationData
// TODO something needs to be done to match things like below (specify aliases for skills?)
//      and parts need to be defined somewhere

// Skills can be hierarchical, e.g.:
// Raised Zombie -> default attack (-> default part)
//               -> slam attack    (-> default part)
// Shrapnel Shot -> arrow part
//               -> cone part
// Frenzy       (-> default part)
// skill [-> sub skill -> sub skill -> ...] -> part
// Stats can apply to specific levels and may get inherited by lower levels, e.g. 
// "Raise Zombie deals 10% increased Damage" applies to both attacks,
// but "Raised Zombies' Slam Attack deals 10% increased Damage" only applies to the slam attack.
// SkillMatchers and ISkillBuilder can represent any level in a skill's hierarchy.
// Each specific level has names it can be referenced by, e.g. 
// "Raised Zombies' Slam Attack" and  "Shrapnel Shot's cone".