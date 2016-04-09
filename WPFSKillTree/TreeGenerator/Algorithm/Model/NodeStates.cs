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
        /// Gets the node indices currently marked as fixed target nodes.
        /// </summary>
        IEnumerable<int> FixedTargetNodeIndices { get; }

        /// <summary>
        /// Gets the number of nodes currently marked as fixed target nodes.
        /// </summary>
        int FixedTargetNodeCount { get; }

        /// <summary>
        /// Gets the number of nodes currently marked as variable target nodes.
        /// </summary>
        int VariableTargetNodeCount { get; }

        /// <summary>
        /// Gets the number of nodes currently marked as fixed or variable target nodes.
        /// </summary>
        int TargetNodeCount { get; }

        /// <summary>
        /// Returns true iff the node with index i is currently marked as a fixed target node.
        /// </summary>
        bool IsFixedTarget(int i);

        /// <summary>
        /// Returns true iff the node with index i is currently marked as a fixed or variable target node.
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
    /// Implementation of <see cref="INodeStates"/>. SearchSpace has to be set for all methods to be available.
    /// </summary>
    public class NodeStates : INodeStates
    {
        /// <summary>
        /// The list of GraphNodes that represent the problem at its current reduced stage.
        /// </summary>
        private IReadOnlyList<GraphNode> _searchSpace;

        /// <summary>
        /// Sets the search space. Recalculates the fields used for <see cref="IsFixedTarget(int)"/>, 
        /// <see cref="IsTarget(int)"/> and <see cref="IsRemoved"/> based on the new search space indices.
        /// </summary>
        public IReadOnlyList<GraphNode> SearchSpace
        {
            get { return _searchSpace; }
            set
            {
                _searchSpace = new List<GraphNode>(value);
                ComputeFields();
            }
        }

        /// <summary>
        /// Gets the number of nodes currently contained in <see cref="SearchSpace"/>.
        /// </summary>
        public int SearchSpaceSize { get { return _searchSpace.Count; } }

        private readonly HashSet<GraphNode> _fixedTargetNodes;

        /// <summary>
        /// Gets the set of GraphNodes of the search space which must be included in every solution to the problem
        /// instance. These are the nodes for which the minimum Steiner tree has to be found.
        /// </summary>
        public IEnumerable<GraphNode> FixedTargetNodes
        {
            get { return _fixedTargetNodes; }
        }

        private HashSet<int> _fixedTargetNodeIndexIndices;
        
        public IEnumerable<int> FixedTargetNodeIndices
        {
            get { return _fixedTargetNodeIndexIndices; }
        }
        
        public int FixedTargetNodeCount
        {
            get { return _fixedTargetNodes.Count; }
        }

        /// <summary>
        /// The set of GraphNodes of the search space which may be set as included in solutions. Finding the subset of
        /// these nodes which has optimal constraint satisfaction is part of the extended Steiner problem solved by
        /// the AdvancedSolver.
        /// 
        /// The basic Steiner problem does not have any of these.
        /// </summary>
        private readonly HashSet<GraphNode> _variableTargetNodes;

        public int VariableTargetNodeCount
        {
            get { return _variableTargetNodes.Count; }
        }

        /// <summary>
        /// The set of GraphNode either in _fixedTargetNodes or in _variableTargetNodes.
        /// </summary>
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
        /// Creates a new instance. The parameters are copied.
        /// </summary>
        public NodeStates(IEnumerable<GraphNode> searchSpace, IEnumerable<GraphNode> fixedTargetNodes, IEnumerable<GraphNode> variableTargetNodes)
        {
            _searchSpace = new List<GraphNode>(searchSpace);
            _fixedTargetNodes = new HashSet<GraphNode>(fixedTargetNodes);
            _variableTargetNodes = new HashSet<GraphNode>(variableTargetNodes);
            _allTargetNodes = new HashSet<GraphNode>(_variableTargetNodes.Union(_fixedTargetNodes));
            ComputeFields();
        }

        /// <summary>
        /// Recomputes the fields that rely on distance indices. Is called each time the search space is set.
        /// 
        /// Has to be called if there are changes to the distance indices without the search space being set.
        /// For example, if the distances were not calculated on object creation, this has to be called after
        /// the distances were calculated.
        /// </summary>
        public void ComputeFields()
        {
            var searchSpaceIndexes = Enumerable.Range(0, _searchSpace.Count).ToList();
            _isFixedTarget =
                searchSpaceIndexes.Select(i => _fixedTargetNodes.Contains(_searchSpace[i])).ToArray();
            _isVariableTarget =
                searchSpaceIndexes.Select(i => _variableTargetNodes.Contains(_searchSpace[i])).ToArray();
            _isTarget = searchSpaceIndexes.Select(i => _isFixedTarget[i] || _isVariableTarget[i]).ToArray();
            _isRemoved = new bool[_searchSpace.Count];
            _fixedTargetNodeIndexIndices = new HashSet<int>(_fixedTargetNodes.Select(n => n.DistancesIndex));
        }

        public bool IsFixedTarget(int i)
        {
            return _isFixedTarget[i];
        }

        /// <summary>
        /// Returns true iff the given node is currently marked as a fixed target node.
        /// </summary>
        public bool IsFixedTarget(GraphNode n)
        {
            return _fixedTargetNodes.Contains(n);
        }

        public bool IsTarget(int i)
        {
            return _isTarget[i];
        }

        /// <summary>
        /// Returns true iff the given node is currently marked as a fixed or variable target node.
        /// </summary>
        public bool IsTarget(GraphNode n)
        {
            return _allTargetNodes.Contains(n);
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
                    _fixedTargetNodeIndexIndices.Remove(i);
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