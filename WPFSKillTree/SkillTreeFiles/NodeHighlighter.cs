using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.SkillTreeFiles
{
    public class NodeHighlighter
    {
        [Flags]
        public enum HighlightState
        {
            FromSearch = 1, FromAttrib = 2, Checked = 4, Crossed = 8, FromHover = 16,
            Highlights = FromSearch | FromAttrib | FromHover,
            Tags = Checked | Crossed,
            All = FromSearch | FromAttrib | Checked | Crossed | FromHover
        }

        private readonly Dictionary<SkillNode, HighlightState> _nodeHighlights =
            new Dictionary<SkillNode, HighlightState>();

        public IReadOnlyDictionary<SkillNode, HighlightState> NodeHighlights => _nodeHighlights;

        /// <summary>
        /// Returns flags without HighlightState.Tags if the node is an ascendancy node.
        /// Returns flags unchanged if it is not.
        /// </summary>
        private static HighlightState CleanFlags(SkillNode node, HighlightState flags)
        {
            if (node.ascendancyName == null) return flags;
            return flags & ~HighlightState.Tags;
        }

        public bool NodeHasHighlights(SkillNode node, HighlightState flags)
        {
            if (_nodeHighlights.ContainsKey(node))
            {
                return _nodeHighlights[node].HasFlag(flags);
            }
            return false;
        }

        public void HighlightNode(SkillNode node, HighlightState newFlags)
        {
            var flags = CleanFlags(node, newFlags);
            if (flags == 0) return;
            if (_nodeHighlights.ContainsKey(node))
                _nodeHighlights[node] |= flags;
            else _nodeHighlights.Add(node, flags);
        }

        public void UnhighlightNode(SkillNode node, HighlightState removeFlags)
        {
            if (_nodeHighlights.ContainsKey(node))
            {
                // Each flag only remains set if it's not one of the flags to be removed.
                HighlightState newState = _nodeHighlights[node] & ~removeFlags;
                if (newState == 0) _nodeHighlights.Remove(node);
                else _nodeHighlights[node] = newState;
            }
        }

        public void HighlightNodes(IEnumerable<SkillNode> nodes, HighlightState newFlags)
        {
            foreach (SkillNode node in nodes)
                HighlightNode(node, newFlags);
        }


        public void UnhighlightAllNodes(HighlightState removeFlags)
        {
            // Kludge cast to avoid a "Collection was modified" exception.
            var keys = _nodeHighlights.Keys.ToArray();
            foreach (SkillNode node in keys)
                UnhighlightNode(node, removeFlags);
        }

        /// <summary>
        /// Removes <paramref name="replaceFlags"/> from all nodes and then adds them to all nodes
        /// in <paramref name="newNodes"/>.
        /// </summary>
        public void ResetHighlights(IEnumerable<SkillNode> newNodes, HighlightState replaceFlags)
        {
            UnhighlightAllNodes(replaceFlags);
            HighlightNodes(newNodes, replaceFlags);
        }

        /// <summary>
        /// For all nodes that have at least one of the ifFlags:
        /// Removes flags not in ifFlags, adds newFlags.
        /// </summary>
        /// <returns>All affected nodes.</returns>
        public IEnumerable<SkillNode> HighlightNodesIf(HighlightState newFlags, HighlightState ifFlags)
        {
            var pairs = _nodeHighlights.Where(pair => (pair.Value & ifFlags) > 0).ToArray();
            foreach (var pair in pairs)
            {
                _nodeHighlights[pair.Key] &= ifFlags;
                _nodeHighlights[pair.Key] |= CleanFlags(pair.Key, newFlags);
            }
            return pairs.Select(p => p.Key);
        }
    }
}
