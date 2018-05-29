using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core.Nodes;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Transforms <see cref="IValueTransformable"/> using <see cref="Behavior"/>s.
    /// </summary>
    public class ValueTransformer
    {
        private readonly Dictionary<(IStat, NodeSelector), Transformable> _transformables =
            new Dictionary<(IStat, NodeSelector), Transformable>();

        private readonly Dictionary<(IStat, NodeType), List<Transformable>> _transformableLists =
            new Dictionary<(IStat, NodeType), List<Transformable>>();

        private readonly Dictionary<Behavior, int> _behaviorMultiSet =
            new Dictionary<Behavior, int>();

        private readonly Dictionary<Behavior, Transformation> _transformations =
            new Dictionary<Behavior, Transformation>();

        private readonly Dictionary<(IStat, NodeType), List<Transformation>> _transformationLists =
            new Dictionary<(IStat, NodeType), List<Transformation>>();

        /// <summary>
        /// Adds the given behaviors and applies their transformations to all matching transformables already stored.
        /// <para>
        /// Only adding a <see cref="Behavior"/> the first times applies its transformations.
        /// </para>
        /// </summary>
        public void AddBehaviors(IEnumerable<Behavior> behaviors)
        {
            foreach (var behavior in behaviors)
            {
                if (_behaviorMultiSet.ContainsKey(behavior))
                {
                    _behaviorMultiSet[behavior]++;
                    continue;
                }

                _behaviorMultiSet[behavior] = 1;
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

        /// <summary>
        /// Removes the given behaviors and their transformations that were already applied.
        /// <para>
        /// If a <see cref="Behavior"/> was added multiple times, its transformations will only be removed when it is
        /// removed as often as it was added.
        /// </para>
        /// </summary>
        /// <param name="behaviors"></param>
        public void RemoveBehaviors(IEnumerable<Behavior> behaviors)
        {
            foreach (var behavior in behaviors)
            {
                if (!_behaviorMultiSet.TryGetValue(behavior, out var count))
                {
                    continue;
                }
                if (count > 1)
                {
                    _behaviorMultiSet[behavior]--;
                    continue;
                }

                _behaviorMultiSet.Remove(behavior);
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

        private static IEnumerable<(IStat, NodeType)> GetAffectedKeys(Behavior behavior) =>
            from stat in behavior.AffectedStats
            from nodeType in behavior.AffectedNodeTypes
            select (stat, nodeType);

        /// <summary>
        /// Adds the given transformable and applies all matching transformations to it that are already saved.
        /// </summary>
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

        /// <summary>
        /// Removes the transformable of the given stat and selector and removes all of its transformations.
        /// </summary>
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

            public bool Affects(PathDefinition path)
            {
                switch (_pathInteraction)
                {
                    case BehaviorPathInteraction.AllPaths:
                        return true;
                    case BehaviorPathInteraction.MainPathOnly:
                        return path.IsMainPath;
                    case BehaviorPathInteraction.ConversionPathsOnly:
                        return path.ConversionStats.Any();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_pathInteraction), _pathInteraction,
                            "Unexpected _pathInteraction value");
                }
            }
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