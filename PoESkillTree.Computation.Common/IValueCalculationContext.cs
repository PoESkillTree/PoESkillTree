using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Computation.Common
{
    public interface IValueCalculationContext
    {
        NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path);

        // Returns the values of all paths.
        IEnumerable<NodeValue?> GetValues(IStat stat, NodeType nodeType);

        IEnumerable<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths);
    }


    public static class ValueCalculationContextExtensions
    {
        public static NodeValue? GetValue(
            this IValueCalculationContext context, IStat stat, NodeType nodeType = NodeType.Total) => 
            context.GetValue(stat, nodeType, PathDefinition.MainPath);

        public static IEnumerable<NodeValue?> GetValues(
            this IValueCalculationContext context, Form form, IStat stat) =>
            context.GetValues(form, stat, PathDefinition.MainPath);

        public static IEnumerable<NodeValue?> GetValues(
            this IValueCalculationContext context, Form form, IStat stat, PathDefinition path) =>
            context.GetValues(form, new[] { (stat, path) });

        public static IEnumerable<NodeValue?> GetValues(
            this IValueCalculationContext context, Form form, IEnumerable<IStat> stats, PathDefinition path) =>
            context.GetValues(form, stats.Select(s => (s, path)));
    }
}