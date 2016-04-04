using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POESKillTree.SkillTreeFiles
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

        public Dictionary<SkillNode, HighlightState> nodeHighlights = new Dictionary<SkillNode, HighlightState>();

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
            if (nodeHighlights.ContainsKey(node))
            {
                return nodeHighlights[node].HasFlag(flags);
            }
            return false;
        }

        public void HighlightNode(SkillNode node, HighlightState newFlags)
        {
            var flags = CleanFlags(node, newFlags);
            if (flags == 0) return;
            if (nodeHighlights.ContainsKey(node))
                nodeHighlights[node] |= flags;
            else nodeHighlights.Add(node, flags);
        }

        public void UnhighlightNode(SkillNode node, HighlightState removeFlags)
        {
            if (nodeHighlights.ContainsKey(node))
            {
                // Each flag only remains set if it's not one of the flags to be removed.
                HighlightState newState = nodeHighlights[node] & ~removeFlags;
                if (newState == 0) nodeHighlights.Remove(node);
                else nodeHighlights[node] = newState;
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
            var keys = nodeHighlights.Keys.ToArray();
            foreach (SkillNode node in keys)
                UnhighlightNode(node, removeFlags);
        }

        public void ReplaceHighlights(IEnumerable<SkillNode> newNodes, HighlightState replaceFlags)
        {
            UnhighlightAllNodes(replaceFlags);
            HighlightNodes(newNodes, replaceFlags);
        }

        /// <summary>
        /// For all nodes that have at least one of the ifFlags:
        /// Removes flags not in ifFlags, adds newFlags.
        /// </summary>
        public void HighlightNodesIf(HighlightState newFlags, HighlightState ifFlags)
        {
            var pairs = nodeHighlights.Where(pair => (pair.Value & ifFlags) > 0).ToArray();
            foreach (var pair in pairs)
            {
                nodeHighlights[pair.Key] &= ifFlags;
                nodeHighlights[pair.Key] |= CleanFlags(pair.Key, newFlags);
            }
        }
    }
}
