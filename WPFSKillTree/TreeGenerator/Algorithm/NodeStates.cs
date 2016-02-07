using System.Collections.Generic;
using System.Linq;

namespace POESKillTree.TreeGenerator.Algorithm
{
    public interface INodeStates
    {
        IEnumerable<GraphNode> FixedTargetNodes { get; }
        int FixedTargetNodeCount { get; }
        int VariableTargetNodeCount { get; }
        void SetFixedTarget(int i);
        bool IsFixedTarget(int i);
        bool IsTarget(int i);
        void MarkNodeAsRemoved(int i);
        bool IsRemoved(int i);
    }

    public class NodeStates : INodeStates
    {
        private IReadOnlyList<GraphNode> _searchSpace;

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

        private bool[] _isFixedTarget;

        private bool[] _isVariableTarget;

        private bool[] _isTarget;

        private bool[] _isRemoved;

        public NodeStates(IReadOnlyList<GraphNode> searchSpace, HashSet<GraphNode> fixedTargetNodes, HashSet<GraphNode> variableTargetNodes, HashSet<GraphNode> allTargetNodes)
        {
            _fixedTargetNodes = fixedTargetNodes;
            _variableTargetNodes = variableTargetNodes;
            _allTargetNodes = allTargetNodes;
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

        public void SetFixedTarget(int i)
        {
            if (_isFixedTarget[i]) return;
            var node = _searchSpace[i];
            if (_isVariableTarget[i])
            {
                _isVariableTarget[i] = false;
                _variableTargetNodes.Remove(node);
            }
            else
            {
                _isTarget[i] = true;
            }
            _isFixedTarget[i] = true;
            _fixedTargetNodes.Add(node);
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