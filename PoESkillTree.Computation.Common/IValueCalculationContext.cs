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
        // TODO Check everything using these methods to make sure they actually want the main path

        public static NodeValue? GetValue(
            this IValueCalculationContext context, IStat stat, NodeType nodeType = NodeType.Total) => 
            context.GetValue(stat, nodeType, PathDefinition.MainPath);

        public static IEnumerable<NodeValue?> GetValues(
            this IValueCalculationContext context, Form form, params IStat[] stats) =>
            context.GetValues(form, PathDefinition.MainPath, stats);

        public static IEnumerable<NodeValue?> GetValues(
            this IValueCalculationContext context, Form form, PathDefinition path, params IStat[] stats) =>
            context.GetValues(form, stats.Select(s => (s, path)));

        public static IEnumerable<NodeValue?> GetValues(
            this IValueCalculationContext context, Form form, IStat stat, PathDefinition path) => 
            context.GetValues(form, (stat, path));

        public static IEnumerable<NodeValue?> GetValues(
            this IValueCalculationContext context, Form form, params (IStat, PathDefinition)[] paths) =>
            context.GetValues(form, paths);
    }
}