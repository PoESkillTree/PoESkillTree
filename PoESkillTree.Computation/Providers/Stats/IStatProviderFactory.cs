namespace PoESkillTree.Computation.Providers.Stats
{
    public interface IStatProviderFactory
    {
        IStatProvider Armour { get; }
        IEvasionStatProvider Evasion { get; }

        IStatProvider Accuracy { get; }

        // these have base values of 1 (so the value results in a multiplier)
        IStatProvider MovementSpeed { get; }
        IStatProvider AnimationSpeed { get; }

        // Only used with skills that use the weapons (or unarmed) range
        IStatProvider Range { get; }

        IStatProvider TrapTriggerAoE { get; }
        IStatProvider MineDetonationAoE { get; }

        IStatProvider ItemQuantity { get; }
        IStatProvider ItemRarity { get; }

        IStatProvider PrimordialJewelsSocketed { get; }
        IStatProvider GrandSpectrumJewelsSocketed { get; }

        IStatProvider RampageStacks { get; }

        // Stats from sub factories

        IAttributeStatProviderFactory Attribute { get; }

        IPoolStatProviderFactory Pool { get; }

        IDodgeStatProviderFactory Dodge { get; }

        IFlaskStatProviderFactory Flask { get; }

        IProjectileStatProviderFactory Projectile { get; }

        IFlagStatProviderFactory Flag { get; }

        IGemStatProviderFactory Gem { get; }

        // Methods

        // No "double dipping" if one of the stats is converted to another
        IStatProvider ApplyOnce(params IStatProvider[] stats);
    }
}