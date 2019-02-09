using System.Collections.Generic;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Behaviors
{
    internal class ModifiedValueCalculationContext : IValueCalculationContext
    {
        private readonly IValueCalculationContext _originalContext;
        private readonly GetPathsDelegate _getPaths;
        private readonly GetValueDelegate _getValue;
        private readonly GetValuesDelegate _getValues;

        public delegate IEnumerable<PathDefinition> GetPathsDelegate(
            IValueCalculationContext context, IStat stat);

        public delegate NodeValue? GetValueDelegate(
            IValueCalculationContext context, IStat stat, NodeType nodeType, PathDefinition path);

        public delegate IEnumerable<NodeValue?> GetValuesDelegate(
            IValueCalculationContext context, Form form, IEnumerable<(IStat stat, PathDefinition path)> paths);

        public ModifiedValueCalculationContext(IValueCalculationContext originalContext,
            GetPathsDelegate getPaths = null, GetValueDelegate getValue = null, GetValuesDelegate getValues = null)
        {
            _originalContext = originalContext;
            _getPaths = getPaths;
            _getValue = getValue;
            _getValues = getValues;
        }

        public PathDefinition CurrentPath => _originalContext.CurrentPath;

        public IEnumerable<PathDefinition> GetPaths(IStat stat) =>
            _getPaths is null
                ? _originalContext.GetPaths(stat)
                : _getPaths(_originalContext, stat);

        public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path) =>
            _getValue is null
                ? _originalContext.GetValue(stat, nodeType, path)
                : _getValue(_originalContext, stat, nodeType, path);

        public IEnumerable<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths) =>
            _getValues is null
                ? _originalContext.GetValues(form, paths)
                : _getValues(_originalContext, form, paths);
    }
}