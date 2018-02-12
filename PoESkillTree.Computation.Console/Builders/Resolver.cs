using PoESkillTree.Computation.Common.Builders.Resolving;

namespace PoESkillTree.Computation.Console.Builders
{
    /// <summary>
    /// Resolves <paramref name="current"/> using the given match context.
    /// </summary>
    /// <remarks>
    /// This is the delegate corresponding to <see cref="IResolvable{T}.Resolve"/> to allow passing resolving functions
    /// as parameters without always repeating the same signature.
    /// </remarks>
    public delegate T Resolver<T>(T current, ResolveContext context);
}