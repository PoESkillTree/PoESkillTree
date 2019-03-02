using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// The context for accessing the values of other calculation graph nodes in <see cref="IValue.Calculate"/>.
    /// </summary>
    public interface IValueCalculationContext
    {
        /// <summary>
        /// The path for which the value should be calculated.
        /// </summary>
        PathDefinition CurrentPath { get; }

        /// <summary>
        /// Returns all paths of the given stat.
        /// </summary>
        IReadOnlyCollection<PathDefinition> GetPaths(IStat stat);
        
        /// <summary>
        /// Returns the value of the specified node.
        /// </summary>
        NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path);

        /// <summary>
        /// Returns the values of all form nodes of the given form for the given stat-path combinations.
        /// The value of each form node will only be evaluated and returned at most once.
        /// </summary>
        List<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths);
    }


    public static class ValueCalculationContextExtensions
    {
        public static NodeValue? GetValue(
            this IValueCalculationContext context, IStat stat, NodeType nodeType = NodeType.Total)
            => context.GetValue(stat, nodeType, PathDefinition.MainPath);

        /// <summary>
        /// Returns the values of the nodes of all paths of the given type in the given stat's subgraph.
        /// </summary>
        public static List<NodeValue?> GetValues(
            this IValueCalculationContext context, IStat stat, NodeType nodeType)
        {
            var paths = context.GetPaths(stat);
            var values = new List<NodeValue?>(paths.Count);
            foreach (var path in paths)
            {
                values.Add(context.GetValue(stat, nodeType, path));
            }
            return values;
        }

        public static List<NodeValue?> GetValues(
            this IValueCalculationContext context, Form form, IStat stat)
            => context.GetValues(form, stat, PathDefinition.MainPath);

        public static List<NodeValue?> GetValues(
            this IValueCalculationContext context, Form form, IStat stat, PathDefinition path)
            => context.GetValues(form, new[] { (stat, path) });

        public static List<NodeValue?> GetValues(
            this IValueCalculationContext context, Form form, IEnumerable<IStat> stats, PathDefinition path)
            => context.GetValues(form, stats.Select(s => (s, path)));
    }
}