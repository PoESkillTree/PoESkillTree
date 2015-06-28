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
            FromSearch = 1, FromAttrib = 2, FromNode = 4, Crossed = 8,
            All = FromSearch | FromAttrib | FromNode | Crossed
        }

        public Dictionary<SkillNode, HighlightState> nodeHighlights = new Dictionary<SkillNode, HighlightState>();

        public bool NodeHasHighlights(SkillNode node, HighlightState flags)
        {
            if (nodeHighlights.ContainsKey(node))
            {
                return nodeHighlights[node].HasFlag(flags);
            }
            return false;
        }

        public void ToggleHighlightNode(SkillNode node, HighlightState toggleFlags)
        {
            if (toggleFlags == 0) return;
            if (nodeHighlights.ContainsKey(node))
            {
                nodeHighlights[node] ^= toggleFlags;
                if (nodeHighlights[node] == 0) nodeHighlights.Remove(node);
            }
            else nodeHighlights.Add(node, toggleFlags);
        }

        public void HighlightNode(SkillNode node, HighlightState newFlags)
        {
            if (newFlags == 0) return;
            if (nodeHighlights.ContainsKey(node))
                nodeHighlights[node] |= newFlags;
            else nodeHighlights.Add(node, newFlags);
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

        public void HighlightNodes(List<SkillNode> nodes, HighlightState newFlags)
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

        public void ReplaceHighlights(List<SkillNode> newNodes, HighlightState replaceFlags)
        {
            UnhighlightAllNodes(replaceFlags);
            HighlightNodes(newNodes, replaceFlags);
        }
    }
}
