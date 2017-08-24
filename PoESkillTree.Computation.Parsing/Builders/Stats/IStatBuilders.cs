namespace PoESkillTree.Computation.Parsing.Builders.Stats
{
    public interface IStatBuilders
    {
        IStatBuilder Armour { get; }
        IEvasionStatBuilder Evasion { get; }

        IStatBuilder Accuracy { get; }

        // these have base values of 1 (so the value results in a multiplier)
        IStatBuilder MovementSpeed { get; }
        IStatBuilder AnimationSpeed { get; }

        // Only used with skills that use the weapons (or unarmed) range
        IStatBuilder Range { get; }

        IStatBuilder TrapTriggerAoE { get; }
        IStatBuilder MineDetonationAoE { get; }

        IStatBuilder ItemQuantity { get; }
        IStatBuilder ItemRarity { get; }

        IStatBuilder PrimordialJewelsSocketed { get; }
        IStatBuilder GrandSpectrumJewelsSocketed { get; }

        IStatBuilder RampageStacks { get; }

        // Stats from sub factories

        IAttributeStatBuilders Attribute { get; }

        IPoolStatBuilders Pool { get; }

        IDodgeStatBuilders Dodge { get; }

        IFlaskStatBuilders Flask { get; }

        IProjectileStatBuilders Projectile { get; }

        IFlagStatBuilders Flag { get; }

        IGemStatBuilders Gem { get; }

        // Methods

        // No "double dipping" if one of the stats is converted to another
        IStatBuilder ApplyOnce(params IStatBuilder[] stats);

        // These don't interact with anything but should still be calculated.
        // Will probably also need section information or something like that.
        // Name may be a regex replacement.
        IStatBuilder Unique(string name = "$0");
    }
}