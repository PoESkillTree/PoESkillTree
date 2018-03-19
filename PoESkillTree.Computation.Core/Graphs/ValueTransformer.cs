using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class ValueTransformer
    {
        private readonly Dictionary<(IStat, NodeType), IValueTransformable> _transformables =
            new Dictionary<(IStat, NodeType), IValueTransformable>();

        private readonly Dictionary<(IStat, NodeType), List<IValueTransformation>> _transformations =
            new Dictionary<(IStat, NodeType), List<IValueTransformation>>();

        public void AddBehaviors(IEnumerable<IBehavior> behaviors)
        {
            foreach (var behavior in behaviors)
            {
                var transformation = behavior.Transformation;
                foreach (var key in GetAffectedKeys(behavior))
                {
                    _transformations.GetOrAdd(key, _ => new List<IValueTransformation>())
                        .Add(transformation);
                    _transformables.ApplyIfPresent(key, value => value.Add(transformation));
                }
            }
        }

        public void RemoveBehaviors(IEnumerable<IBehavior> behaviors)
        {
            foreach (var behavior in behaviors)
            {
                var transformation = behavior.Transformation;
                foreach (var key in GetAffectedKeys(behavior))
                {
                    _transformations.ApplyIfPresent(key, value => value.Remove(transformation));
                    _transformables.ApplyIfPresent(key, value => value.Remove(transformation));
                }
            }
        }

        private static IEnumerable<(IStat, NodeType)> GetAffectedKeys(IBehavior behavior) =>
            from stat in behavior.AffectedStats
            from nodeType in behavior.AffectedNodeTypes
            select (stat, nodeType);

        public void AddTransformable(IStat stat, NodeType nodeType, IValueTransformable transformable)
        {
            var key = (stat, nodeType);
            _transformations.ApplyIfPresent(key, value => value.ForEach(transformable.Add));
            _transformables[key] = transformable;
        }

        public void RemoveTransformable(IStat stat, NodeType nodeType)
        {
            var key = (stat, nodeType);
            _transformables.ApplyIfPresent(key, value => value.RemoveAll());
            _transformables.Remove(key);
        }
    }
}