using System;

namespace PoESkillTree.Computation.Core.Nodes
{
    public interface ICycleGuard
    {
        /// <summary>
        /// Guards against recursive calls.
        /// <para>
        /// If this method is called again before the returned <see cref="IDisposable"/> is disposed, it throws
        /// an <see cref="InvalidOperationException"/>. This prohibits the code between this call and
        /// <see cref="IDisposable.Dispose"/> from recursively calling itself again.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Used to prevent nodes from recursively using their own value again, i.e. prevents cycles from occurring
        /// in the calculation graph.
        /// </remarks>
        IDisposable Guard();
    }
}