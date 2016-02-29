using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Algorithm.Model
{
    /// <summary>
    /// Interface for information about what nodes are fixed/variable target nodes or are
    /// removed from the current reduced skill tree.
    /// </summary>
    /// <remarks>
    /// - Fixed target nodes: Have to be included in the final solution.
    /// - Variable target nodes: Could have to be included in the final solution
    /// (currently used by advanced solver: finding the best set of variable target nodes).
    /// - Removed nodes: will be removed the next time the search space is shrunk.
    /// </remarks>
    public interface INodeStates
    {
        /// <summary>
        /// Gets the nodes marked as fixed target nodes.
        /// </summary>
        IEnumerable<GraphNode> FixedTargetNodes { get; }

        /// <summary>
        /// Gets the number of nodes marked as fixed target nodes.
        /// </summary>
        int FixedTargetNodeCount { get; }

        /// <summary>
        /// Gets the number of nodes marked as variable target nodes.
        /// </summary>
        int VariableTargetNodeCount { get; }

        /// <summary>
        /// Gets the number of nodes marked as fixed or variable target nodes.
        /// </summary>
        int TargetNodeCount { get; }

        /// <summary>
        /// Returns true iff the node with index i is marked as a fixed target node.
        /// </summary>
        bool IsFixedTarget(int i);

        /// <summary>
        /// Returns true iff the node with index i is marked as a fixed or variable target node.
        /// </summary>
        bool IsTarget(int i);

        /// <summary>
        /// Marks the node with index i as to be removed from the search space.
        /// </summary>
        void MarkNodeAsRemoved(int i);

        /// <summary>
        /// Returns true iff the node with index i was marked as to be removed.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        bool IsRemoved(int i);
    }

    /// <summary>
    /// Implementation of <see cref="INodeStates"/>.
    /// </summary>
    public class NodeStates : INodeStates
    {
        private IReadOnlyList<GraphNode> _searchSpace;

        /// <summary>
        /// Sets the search space. Recalculates the fields used for <see cref="IsFixedTarget"/>, 
        /// <see cref="IsTarget"/> and <see cref="IsRemoved"/> based on the new search space indices.
        /// </summary>
        public IReadOnlyList<GraphNode> SearchSpace
        {
            set
            {
                _searchSpace = value;
                ComputeFields();
            }
        }

        private readonly HashSet<GraphNode> _fixedTargetNodes;

        public IEnumerable<GraphNode> FixedTargetNodes
        {
            get { return _fixedTargetNodes; }
        }

        public int FixedTargetNodeCount
        {
            get { return _fixedTargetNodes.Count; }
        }

        private readonly HashSet<GraphNode> _variableTargetNodes;

        public int VariableTargetNodeCount
        {
            get { return _variableTargetNodes.Count; }
        }

        private readonly HashSet<GraphNode> _allTargetNodes;

        public int TargetNodeCount
        {
            get { return _allTargetNodes.Count; }
        }

        private bool[] _isFixedTarget;

        private bool[] _isVariableTarget;

        private bool[] _isTarget;

        private bool[] _isRemoved;

        /// <summary>
        /// Creates a new instance. The target node enumerables are copied. The search space is not.
        /// </summary>
        public NodeStates(IReadOnlyList<GraphNode> searchSpace, IEnumerable<GraphNode> fixedTargetNodes, IEnumerable<GraphNode> variableTargetNodes, IEnumerable<GraphNode> allTargetNodes)
        {
            _fixedTargetNodes = new HashSet<GraphNode>(fixedTargetNodes);
            _variableTargetNodes = new HashSet<GraphNode>(variableTargetNodes);
            _allTargetNodes = new HashSet<GraphNode>(allTargetNodes);
            _searchSpace = searchSpace;
            ComputeFields();
        }

        private void ComputeFields()
        {
            var searchSpaceIndexes = Enumerable.Range(0, _searchSpace.Count).ToList();
            _isFixedTarget =
                searchSpaceIndexes.Select(i => _fixedTargetNodes.Contains(_searchSpace[i])).ToArray();
            _isVariableTarget =
                searchSpaceIndexes.Select(i => _variableTargetNodes.Contains(_searchSpace[i])).ToArray();
            _isTarget = searchSpaceIndexes.Select(i => _isFixedTarget[i] || _isVariableTarget[i]).ToArray();
            _isRemoved = new bool[_searchSpace.Count];
        }

        public bool IsFixedTarget(int i)
        {
            return _isFixedTarget[i];
        }

        public bool IsTarget(int i)
        {
            return _isTarget[i];
        }

        public void MarkNodeAsRemoved(int i)
        {
            _isRemoved[i] = true;
            if (_isTarget[i])
            {
                var node = _searchSpace[i];
                _isTarget[i] = false;
                _allTargetNodes.Remove(node);
                if (_isFixedTarget[i])
                {
                    _isFixedTarget[i] = false;
                    _fixedTargetNodes.Remove(node);
                }
                else
                {
                    _isVariableTarget[i] = false;
                    _variableTargetNodes.Remove(node);
                }
            }
        }

        public bool IsRemoved(int i)
        {
            return _isRemoved[i];
        }
    }
}