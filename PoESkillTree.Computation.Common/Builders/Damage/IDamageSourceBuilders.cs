namespace PoESkillTree.Computation.Common.Builders.Damage
{
    /// <summary>
    /// Factory interface for damage sources.
    /// </summary>
    public interface IDamageSourceBuilders
    {
        IDamageSourceBuilder From(DamageSource source);
    }


    public static class DamageSourceBuilderExtensions
    {
        public static IDamageSourceBuilder Attack(this IDamageSourceBuilders @this) => 
            @this.From(DamageSource.Attack);
        
        public static IDamageSourceBuilder Spell(this IDamageSourceBuilders @this) => 
            @this.From(DamageSource.Spell);
        
        public static IDamageSourceBuilder Secondary(this IDamageSourceBuilders @this) => 
            @this.From(DamageSource.Secondary);
        
        public static IDamageSourceBuilder OverTime(this IDamageSourceBuilders @this) => 
            @this.From(DamageSource.OverTime);
    }
}