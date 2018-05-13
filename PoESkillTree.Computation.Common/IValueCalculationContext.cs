using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.Computation.Common
{
    public interface IValueCalculationContext
    {
        IEnumerable<PathDefinition> GetPaths(IStat stat);
        
        NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path);

        IEnumerable<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths);
    }


    public static class ValueCalculationContextExtensions
    {
        public static NodeValue? GetValue(
            this IValueCalculationContext context, IStat stat, NodeType nodeType = NodeType.Total) => 
            context.GetValue(stat, nodeType, PathDefinition.MainPath);
        
        // Returns the values of all paths.
        public static IEnumerable<NodeValue?> GetValues(
            this IValueCalculationContext context, IStat stat, NodeType nodeType) =>
            context.GetPaths(stat).Select(p => context.GetValue(stat, nodeType, p));

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