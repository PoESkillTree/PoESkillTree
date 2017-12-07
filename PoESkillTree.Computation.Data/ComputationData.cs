// No code here, just design thoughts that I needed to write down somewhere.
// TODO find a place for/solve everything currently in here and remove the file

// most local stats are already added to properties and don't need to be handled here
// (those stats are handled elsewhere and don't even get here)
// locality information for local stats not handled elsewhere gets passed to Computation

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