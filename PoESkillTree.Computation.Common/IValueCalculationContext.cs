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

        // Total, Subtotal, TotalOverride: behavior unchanged
        //  (requires GetValue(IStat, NodeType), GetValues(Form, (IStat, PathDefinition)))
        // UncappedSubtotal outside of paths: Combines UncappedSubtotal of all paths
        //  (requires GetValues(IStat, NodeType))
        // PathTotal in a path: same behavior as UncappedSubtotal before but path-specific
        //  (requires GetValue(IStat, NodeType, PathDefinition))
        // Base with conversion: value is the base value of the first stat in the conversion list with the same path
        //  except removing that from the conversion list.
        //  (requires GetValue(IStat, NodeType, PathDefinition))
        // Base without conversion: same behavior but path-specific
        //  (requires GetValue(IStat, NodeType, PathDefinition))
        // Increase, More: still form aggregating, but with multiple paths:
        //  For each source in currentPath.Source.InfluencingSources:
        //   For each stat in currentPath.Stats:
        //    paths.Add((source, stat))
        //  (requires GetValues(Form, (IStat, PathDefinition)[]))
        // BaseOverride, BaseSet, BaseAdd: only used in paths without conversions. Same behavior except using the path's
        //  form node collection.
        //  (requires GetValues(Form, (IStat, PathDefinition)))
        // -> Values need to know their path
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