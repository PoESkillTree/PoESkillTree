namespace PoESkillTree.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Factory interface for stats.
    /// </summary>
    public interface IStatBuilders
    {
        IStatBuilder Level { get; }

        IStatBuilder Armour { get; }

        IEvasionStatBuilder Evasion { get; }


        IStatBuilder Accuracy { get; }


        /// <summary>
        /// Gets a stat representing the multiplier to Self's movement speed.
        /// </summary>
        IStatBuilder MovementSpeed { get; }

        /// <summary>
        /// Gets a stat representing the multiplier to Self's animation speed.
        /// </summary>
        IStatBuilder AnimationSpeed { get; }


        /// <summary>
        /// Gets a stat representing the main skill's range (only for skills that use the weapon's or the unarmed range)
        /// </summary>
        IStatBuilder Range { get; }


        /// <summary>
        /// Gets a stat representing the trap trigger area of effect.
        /// </summary>
        IStatBuilder TrapTriggerAoE { get; }

        /// <summary>
        /// Gets a stat representing the mine detonation area of effect.
        /// </summary>
        IStatBuilder MineDetonationAoE { get; }


        IStatBuilder ItemQuantity { get; }

        IStatBuilder ItemRarity { get; }


        /// <summary>
        /// Gets a stat representing the number of primordial jewels socketed into the skill tree.
        /// </summary>
        IStatBuilder PrimordialJewelsSocketed { get; }

        /// <summary>
        /// Gets a stat representing the number of grand spectrum jewels socketed into the skill tree.
        /// </summary>
        IStatBuilder GrandSpectrumJewelsSocketed { get; }

        /// <summary>
        /// Gets a stat representing the number of currently active rampage stacks.
        /// </summary>
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

        /// <summary>
        /// Returns a stat whose modifiers apply to all given stats, but only once.
        /// (no multiple application if one of the stats is converted to another)
        /// </summary>
        IStatBuilder ApplyOnce(params IStatBuilder[] stats);

        /// <summary>
        /// Returns a unique stat that can not interact with any other stat. 
        /// These can still be calculated and displayed.
        /// </summary>
        IStatBuilder Unique(string name);
    }
}