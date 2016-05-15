using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using POESKillTree.TreeGenerator.Algorithm.Model;

namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    ///     Simple hillclimbing algorithm that tries to improve a set of nodes by swapping
    ///     single nodes in and out until no swap improves the score.
    /// </summary>
    public class HillClimber
    {
        /// <summary>
        /// Maps nodes to arrays of adjacent nodes.
        /// </summary>
        private readonly Dictionary<ushort, ushort[]> _adjacencyMatrix = new Dictionary<ushort, ushort[]>();
        /// <summary>
        /// Function returning the fitness of a set of nodes.
        /// </summary>
        private readonly Func<HashSet<ushort>, double> _fitnessFunc;

        /// <summary>
        /// All nodes of the graph.
        /// </summary>
        private readonly HashSet<ushort> _allNodes;
        /// <summary>
        /// Nodes that can not swapped out of the current set.
        /// </summary>
        private readonly HashSet<ushort> _fixedNodes;
        /// <summary>
        /// Nodes that are in the search space (their GraphNode has a valid DistanceIndex).
        /// </summary>
        private readonly HashSet<ushort> _searchSpaceNodes;

        private readonly object _improvementLock = new object();

        /// <summary>
        /// Fitness value of the current node set.
        /// </summary>
        private double _curFitness;
        /// <summary>
        /// Current set of nodes that build the tree.
        /// These can be swapped out (if not in <see cref="_fixedNodes"/>).
        /// </summary>
        private HashSet<ushort> _current;
        /// <summary>
        /// Set of nodes in <see cref="_allNodes"/> but not in <see cref="_current"/>.
        /// These can be swapped in.
        /// </summary>
        private HashSet<ushort> _notCurrent;
        /// <summary>
        /// If the current iteration over all nodes improved the solution.
        /// If not, there won't be another iteration as the solution did not change.
        /// </summary>
        private bool _improvement;

        /// <summary>
        /// Constructs a new HillClimber and initializes it with the given fitness function,
        /// fixed nodes and collection of all nodes.
        /// </summary>
        /// <param name="fitnessFunc">Function returning the fitness of a set of nodes. (not null)</param>
        /// <param name="fixedNodes">Nodes that can not swapped out of the current set. (not null)</param>
        /// <param name="allNodes">All nodes of the graph. (not null)</param>
        public HillClimber(Func<HashSet<ushort>, double> fitnessFunc, IEnumerable<GraphNode> fixedNodes,
            IEnumerable<GraphNode> allNodes)
        {
            if (fitnessFunc == null) throw new ArgumentNullException("fitnessFunc");
            if (fixedNodes == null) throw new ArgumentNullException("fixedNodes");
            if (allNodes == null) throw new ArgumentNullException("allNodes");

            _fitnessFunc = fitnessFunc;
            _fixedNodes = new HashSet<ushort>(fixedNodes.Select(n => n.Id));
            _allNodes = new HashSet<ushort>();
            _searchSpaceNodes = new HashSet<ushort>();
            foreach (var graphNode in allNodes)
            {
                _allNodes.Add(graphNode.Id);
                _adjacencyMatrix[graphNode.Id] = graphNode.Adjacent.Select(n => n.Id).ToArray();

                // Only nodes that are in the search space can be removed. The adjacency information
                // may be incorrect for nodes that did not survive reductions.
                // In most cases this is not bad because nodes were removed from the search space for
                // being irrelevant to the solution. It may be bad if entire clusters are switched out
                // because the travel nodes to them are not in the search space.
                if (graphNode.DistancesIndex >= 0)
                    _searchSpaceNodes.Add(graphNode.Id);
            }
        }

        /// <summary>
        /// Applies a hill climbing algorithm to the given solution by trying to swap single nodes
        /// in and out until no swap improves it and returns the resulting solutions.
        /// </summary>
        /// <param name="original">The solution to improve. (not null)</param>
        /// <returns>The potentially improved solution.</returns>
        public HashSet<ushort> Improve(IEnumerable<ushort> original)
        {
            if (original == null) throw new ArgumentNullException("original");

            _current = new HashSet<ushort>(original);
            _notCurrent = new HashSet<ushort>(_allNodes);
            _notCurrent.ExceptWith(_current);
            _curFitness = _fitnessFunc(new HashSet<ushort>(_current));
            _improvement = true;

            while (_improvement)
            {
                _improvement = false;

                Parallel.ForEach(_current, (curNode, state) =>
                {
                    // If node is fixed or not in the search space it can't be removed.
                    if (_fixedNodes.Contains(curNode) || !_searchSpaceNodes.Contains(curNode)) return;

                    var newCur = new HashSet<ushort>(_current);
                    var newNot = new HashSet<ushort>(_notCurrent);

                    // Continue if node can't be removed.
                    if (CountNeighborsIn(curNode, newCur) > 1) return;

                    // Remove current node
                    newCur.Remove(curNode);
                    var newFitness = _fitnessFunc(new HashSet<ushort>(newCur));
                    if (newFitness <= _curFitness)
                    {
                        // Swap current node with each of _notCurrent
                        foreach (var notNode in newNot)
                        {
                            // Break if someone else already found a better set.
                            if (state.IsStopped) break;
                            // Continue if node can't be added.
                            if (CountNeighborsIn(notNode, newCur) == 0) continue;

                            newCur.Add(notNode);
                            newFitness = _fitnessFunc(new HashSet<ushort>(newCur));
                            if (newFitness > _curFitness)
                            {
                                newNot.Remove(notNode);
                                break;
                            }
                            newCur.Remove(notNode);
                        }
                    }

                    if (newFitness <= _curFitness || state.IsStopped) return;
                    lock (_improvementLock)
                    {
                        if (newFitness <= _curFitness || state.IsStopped) return;
                        Debug.WriteLine("Improved from " + _curFitness + " to " + newFitness);
                        _current = newCur;
                        newNot.Add(curNode);
                        _notCurrent = newNot;
                        _curFitness = newFitness;
                        _improvement = true;
                        state.Stop();
                    }
                });
            }

            return _current;
        }

        /// <summary>
        /// Counting neigbors is not the optimal way to determine if nodes can be removed.
        /// If it is a graph and no tree, there may very well be nodes that have more than
        /// one neighbor and can be removed. Calculating that, however, would be far more complicated.
        /// </summary>
        private int CountNeighborsIn(ushort node, HashSet<ushort> skilled)
        {
            return _adjacencyMatrix[node].Count(skilled.Contains);
        }
    }
}