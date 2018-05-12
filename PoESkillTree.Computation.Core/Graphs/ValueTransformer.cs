using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class ValueTransformer
    {
        private readonly Dictionary<(IStat, NodeSelector), Transformable> _transformables =
            new Dictionary<(IStat, NodeSelector), Transformable>();

        private readonly Dictionary<(IStat, NodeType), List<Transformable>> _transformableLists =
            new Dictionary<(IStat, NodeType), List<Transformable>>();

        private readonly Dictionary<IBehavior, Transformation> _transformations =
            new Dictionary<IBehavior, Transformation>();

        private readonly Dictionary<(IStat, NodeType), List<Transformation>> _transformationLists =
            new Dictionary<(IStat, NodeType), List<Transformation>>();

        public void AddBehaviors(IEnumerable<IBehavior> behaviors)
        {
            foreach (var behavior in behaviors)
            {
                var transformation = new Transformation(behavior.AffectedPaths, behavior.Transformation);
                _transformations[behavior] = transformation;

                foreach (var key in GetAffectedKeys(behavior))
                {
                    _transformationLists
                        .GetOrAdd(key, _ => new List<Transformation>())
                        .Add(transformation);
                    _transformableLists.ApplyIfPresent(key,
                        value => value.ForEach(t => t.Add(transformation)));
                }
            }
        }

        public void RemoveBehaviors(IEnumerable<IBehavior> behaviors)
        {
            foreach (var behavior in behaviors)
            {
                var transformation = _transformations[behavior];
                _transformations.Remove(behavior);

                foreach (var key in GetAffectedKeys(behavior))
                {
                    _transformationLists[key].Remove(transformation);
                    _transformableLists.ApplyIfPresent(key,
                        value => value.ForEach(t => t.Remove(transformation)));
                }
            }
        }

        private static IEnumerable<(IStat, NodeType)> GetAffectedKeys(IBehavior behavior) =>
            from stat in behavior.AffectedStats
            from nodeType in behavior.AffectedNodeTypes
            select (stat, nodeType);

        public void AddTransformable(IStat stat, NodeSelector selector, IValueTransformable transformable)
        {
            var t = new Transformable(selector.Path, transformable);
            _transformables[(stat, selector)] = t;
            AddTransformable(stat, selector.NodeType, t);
        }

        private void AddTransformable(IStat stat, NodeType nodeType, Transformable transformable)
        {
            var key = (stat, nodeType);
            _transformationLists.ApplyIfPresent(key,
                value => value.ForEach(transformable.Add));
            _transformableLists
                .GetOrAdd(key, _ => new List<Transformable>())
                .Add(transformable);
        }

        public void RemoveTransformable(IStat stat, NodeSelector selector)
        {
            var transformable = _transformables[(stat, selector)];
            _transformables.Remove((stat, selector));

            transformable.RemoveAll();
            _transformableLists[(stat, selector.NodeType)].Remove(transformable);
        }


        private class Transformation
        {
            private readonly BehaviorPathInteraction _pathInteraction;
            private readonly IValueTransformation _transformation;

            public Transformation(BehaviorPathInteraction pathInteraction, IValueTransformation transformation)
            {
                _pathInteraction = pathInteraction;
                _transformation = transformation;
            }

            public void ApplyTo(IValueTransformable transformable) =>
                transformable.Add(_transformation);

            public void RemoveFrom(IValueTransformable transformable) =>
                transformable.Remove(_transformation);

            public bool Affects(PathDefinition path) =>
                _pathInteraction == BehaviorPathInteraction.AllPaths || path.IsMainPath;
        }


        private class Transformable
        {
            private readonly PathDefinition _path;
            private readonly IValueTransformable _transformable;

            public Transformable(PathDefinition path, IValueTransformable transformable)
            {
                _path = path;
                _transformable = transformable;
            }

            public void Add(Transformation transformation)
            {
                if (transformation.Affects(_path))
                    transformation.ApplyTo(_transformable);
            }

            public void Remove(Transformation transformation)
            {
                if (transformation.Affects(_path))
                    transformation.RemoveFrom(_transformable);
            }

            public void RemoveAll() =>
                _transformable.RemoveAll();
        }
    }
}