using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Z3;
using POESKillTree.SkillTreeFiles;
using POESKillTree.TreeGenerator.Algorithm;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    class SmtRunner : IDisposable
    {
        private readonly Context _c = new Context();

        private readonly Optimize _o;

        private readonly Dictionary<GraphEdge, ArithExpr[]> _edgeDictionary;

        private readonly HashSet<int>[] _adjacencyMatrix;

        private readonly IList<GraphEdge> _edgeList;

        private readonly IntNum _zero;

        private readonly Optimize.Handle _softConstraintsHandle;

        public SmtRunner(IEnumerable<GraphEdge> edges, IEnumerable<int> targetNodes, int totalNodeCount, Func<int, int, uint> weightFunc)
        {
            _o = _c.MkOptimize();
            
            var edgeSet = new HashSet<GraphEdge>(edges.Distinct());
            var targetList = targetNodes.ToList();
            var targetHashSet = new HashSet<int>(targetList);
            
            _adjacencyMatrix = Enumerable.Range(0, totalNodeCount).Select(_ => new HashSet<int>()).ToArray();
            foreach (var edge in edgeSet)
            {
                _adjacencyMatrix[edge.N1].Add(edge.N2);
                _adjacencyMatrix[edge.N2].Add(edge.N1);
            }
            var changed = true;
            while (changed)
            {
                changed = false;
                for (var i = 0; i < totalNodeCount; i++)
                {
                    if (targetHashSet.Contains(i)) continue;
                    var neighbors = _adjacencyMatrix[i];
                    if (neighbors.Count == 1)
                    {
                        var other = neighbors.First();
                        _adjacencyMatrix[other].Remove(i);
                        neighbors.Clear();
                        edgeSet.Remove(new GraphEdge(i, other, 0));

                        changed = true;
                    }
                    else if (neighbors.Count == 2)
                    {
                        var left = neighbors.First();
                        var right = neighbors.Last();
                        _adjacencyMatrix[left].Remove(i);
                        _adjacencyMatrix[right].Remove(i);
                        neighbors.Clear();

                        var tmpLeftEdge = new GraphEdge(i, left, 0);
                        var leftEdge = edgeSet.First(e => tmpLeftEdge.Equals(e));
                        var tmpRightEdge = new GraphEdge(i, right, 0);
                        var rightEdge = edgeSet.First(e => tmpRightEdge.Equals(e));
                        edgeSet.Remove(leftEdge);
                        edgeSet.Remove(rightEdge);

                        if (weightFunc(left, right) >= leftEdge.Weight + rightEdge.Weight)
                        {
                            _adjacencyMatrix[right].Add(left);
                            _adjacencyMatrix[left].Add(right);
                            edgeSet.Add(new GraphEdge(left, right, leftEdge.Weight + rightEdge.Weight));
                        }

                        changed = true;
                    }
                }
            }

            _edgeList = edgeSet.ToList();
            _edgeDictionary = new Dictionary<GraphEdge, ArithExpr[]>(_edgeList.Count);
            foreach (var edge in _edgeList)
            {
                _edgeDictionary[edge] = new ArithExpr[targetList.Count - 1];
            }

            _zero = _c.MkInt(0);
            var one = _c.MkInt(1);
            var two = _c.MkInt(2);

            for (var gi = 0; gi < targetList.Count - 1; gi++)
            {
                foreach (var edge in _edgeList)
                {
                    var edgeConst = _c.MkIntConst("e" + edge.N1 + "-" + edge.N2 + "g" + gi);
                    _edgeDictionary[edge][gi] = edgeConst;
                    _o.Assert(_c.MkAnd(_c.MkGe(edgeConst, _zero), _c.MkLe(edgeConst, one)));
                }
                for (var i = 0; i < totalNodeCount; i++)
                {
                    if (i == targetList[gi] || i == targetList[gi + 1])
                    {
                        _o.Assert(_c.MkEq(one, AddNeighbors(i, gi)));
                    }
                    else if (_adjacencyMatrix[i].Any())
                    {
                        _o.Assert(_c.MkOr(_c.MkEq(_zero, AddNeighbors(i, gi)), _c.MkEq(two, AddNeighbors(i, gi))));
                    }
                }
            }
            
            _softConstraintsHandle = _o.MkMinimize(_c.MkAdd(_edgeList
                .Select(edge => MkEdgeWeightExpr(_edgeDictionary[edge], edge.Weight)).ToArray()));
        }

        private ArithExpr AddNeighbors(int n1, int graph)
        {
            if (!_adjacencyMatrix[n1].Any()) return _zero;
            return _c.MkAdd(_adjacencyMatrix[n1].Select(n2 => _edgeDictionary[new GraphEdge(n1, n2, 0)][graph]).ToArray());
        }

        private ArithExpr MkEdgeWeightExpr(ArithExpr[] es, uint weight)
        {
            return (ArithExpr)_c.MkITE(_c.MkGt(_c.MkAdd(es), _zero), _c.MkInt(weight), _zero);
        }

        public IEnumerable<GraphEdge> Run()
        {
            var status = _o.Check();
            switch (status)
            {
                case Status.UNKNOWN:
                    throw new SmtRunException("Could not calculate a solution: " + _o.getReasonUnknown());
                case Status.UNSATISFIABLE:
                    throw new SmtRunException("This problem instance does not have a solution");
                case Status.SATISFIABLE:
                    var model = _o.Model;
                    Debug.Print(_softConstraintsHandle.Value.ToString());
                    //Debug.Print(model.ToString());
                    return _edgeList.Where(e => _edgeDictionary[e].Any(expr => ((IntNum)model.ConstInterp(expr)).Int > 0));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Dispose()
        {
            _c.Dispose();
        }
    }

    internal class SmtRunException : Exception
    {
        public SmtRunException(string message) : base(message)
        { }
    }

    public class SteinerSmtSolver : ISolver
    {

        private readonly SkillTree _tree;
        private readonly SolverSettings _settings;

        public bool IsConsideredDone
        {
            get { return CurrentStep >= MaxSteps; }
        }

        public int MaxSteps
        {
            get { return 1; }
        }

        public int CurrentStep { get; private set; }

        public IEnumerable<ushort> BestSolution { get; private set; }

        private SmtRunner _runner;

        private IDistanceLookup _distances;

        public SteinerSmtSolver(SkillTree tree, SolverSettings settings)
        {
            if (tree == null) throw new ArgumentNullException("tree");
            if (settings == null) throw new ArgumentNullException("settings");
            _tree = tree;
            _settings = settings;
        }

        public void Initialize()
        {
            var auxiliarySolver = new SteinerSolver(_tree, _settings);
            auxiliarySolver.Initialize();
            BestSolution = auxiliarySolver.BestSolution;
            _distances = auxiliarySolver.Distances;
            var nodes = new HashSet<GraphNode>(
                auxiliarySolver.SearchSpace.Concat(auxiliarySolver.TargetNodes));
            var targets = new HashSet<int>(
                auxiliarySolver.TargetNodes.Select(n => n.DistancesIndex));
            var edges = new List<GraphEdge>();
            foreach (var node in nodes)
            {
                foreach (var neighbor in node.Adjacent)
                {
                    var current = neighbor;
                    var previous = node;
                    var path = new HashSet<ushort>();
                    while (current.Adjacent.Count == 2 && !targets.Contains(current.DistancesIndex))
                    {
                        path.Add(current.Id);
                        var tmp = current;
                        current = current.Adjacent.First(n => n != previous);
                        previous = tmp;
                    }
                    if (current.DistancesIndex >= 0 && node != current && path.SetEquals(_distances.GetShortestPath(node.DistancesIndex, current.DistancesIndex)))
                    {
                        edges.Add(new GraphEdge(node.DistancesIndex, current.DistancesIndex, _distances[node.DistancesIndex, current.DistancesIndex]));
                    }
                }
            }
            _runner = new SmtRunner(edges, targets, nodes.Count, (n1, n2) => _distances[n1, n2]);
        }

        public void Step()
        {
            var edges = _runner.Run();
            var nodes = new HashSet<ushort>();
            foreach (var edge in edges)
            {
                nodes.UnionWith(_distances.IndexToNode(edge.N1).Nodes);
                nodes.UnionWith(_distances.IndexToNode(edge.N2).Nodes);
                nodes.UnionWith(_distances.GetShortestPath(edge.N1, edge.N2));
            }
            BestSolution = nodes;
            _runner.Dispose();
            CurrentStep++;
        }

        public void FinalStep()
        {
        }
    }
}