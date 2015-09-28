using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace POESKillTree.SkillTreeFiles.SteinerTrees
{
    public class HillClimber
    {
        private readonly Func<HashSet<ushort>, double> _fitnessFunc;

        private readonly HashSet<ushort> _fixedNodes;

        private readonly HashSet<ushort> _allNodes;

        private readonly Dictionary<ushort, ushort[]> _adjacencyMatrix = new Dictionary<ushort, ushort[]>();
        
        public HillClimber(Func<HashSet<ushort>, double> fitnessFunc, IEnumerable<ushort> fixedNodes, IEnumerable<GraphNode> allNodes)
        {
            if (fitnessFunc == null) throw new ArgumentNullException("fitnessFunc");
            if (fixedNodes == null) throw new ArgumentNullException("fixedNodes");
            if (allNodes == null) throw new ArgumentNullException("allNodes");

            _fitnessFunc = fitnessFunc;
            _fixedNodes = new HashSet<ushort>(fixedNodes);
            _allNodes = new HashSet<ushort>();
            foreach (var graphNode in allNodes)
            {
                _allNodes.Add(graphNode.Id);
                _adjacencyMatrix[graphNode.Id] = graphNode.Adjacent.Select(n => n.Id).ToArray();
            }
        }

        private HashSet<ushort> _current;
        private HashSet<ushort> _notCurrent;
        private double _curFitness;
        private bool _improvement;
        private readonly object _improvementLock = new object();

        public HashSet<ushort> Improve(HashSet<ushort> original)
        {
            if (original == null) throw new ArgumentNullException("original");
            
            _current = original;
            _notCurrent = new HashSet<ushort>(_allNodes);
            _notCurrent.ExceptWith(_current);
            _curFitness = _fitnessFunc(new HashSet<ushort>(_current));
            _improvement = true;

            while (_improvement)
            {
                _improvement = false;

                Parallel.ForEach(_current, (curNode, state) =>
                {
                    // If node is fixed it can't be removed.
                    if (_fixedNodes.Contains(curNode)) return;

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

        private int CountNeighborsIn(ushort node, HashSet<ushort> skilled)
        {
            return _adjacencyMatrix[node].Count(skilled.Contains);
        }
    }
}