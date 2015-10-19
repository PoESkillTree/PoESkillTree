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

        private readonly Dictionary<Edge, ArithExpr[]> _edgeDictionary;

        private readonly HashSet<int>[] _adjacencyMatrix;

        private readonly IList<Edge> _edgeList;

        private readonly IntNum _zero;

        private readonly Optimize.Handle _softConstraintsHandle;

        public SmtRunner(IEnumerable<Edge> edges, IEnumerable<int> targetNodes, int totalNodeCount, Func<int, int, uint> weightFunc)
        {
            _o = _c.MkOptimize();
            
            var edgeSet = new HashSet<Edge>(edges.Distinct());
            var sortedTargetNodes = SortNodes(targetNodes);
            var targetHashSet = new HashSet<int>(sortedTargetNodes);
            
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
                        edgeSet.Remove(new Edge(i, other, 0));

                        changed = true;
                    }
                    else if (neighbors.Count == 2)
                    {
                        var left = neighbors.First();
                        var right = neighbors.Last();
                        _adjacencyMatrix[left].Remove(i);
                        _adjacencyMatrix[right].Remove(i);
                        neighbors.Clear();

                        var tmpLeftEdge = new Edge(i, left, 0);
                        var leftEdge = edgeSet.First(e => tmpLeftEdge.Equals(e));
                        var tmpRightEdge = new Edge(i, right, 0);
                        var rightEdge = edgeSet.First(e => tmpRightEdge.Equals(e));
                        edgeSet.Remove(leftEdge);
                        edgeSet.Remove(rightEdge);

                        if (weightFunc(left, right) >= leftEdge.Weight + rightEdge.Weight)
                        {
                            _adjacencyMatrix[right].Add(left);
                            _adjacencyMatrix[left].Add(right);
                            edgeSet.Add(new Edge(left, right, leftEdge.Weight + rightEdge.Weight));
                        }

                        changed = true;
                    }
                }
            }

            _edgeList = edgeSet.ToList();
            _edgeDictionary = new Dictionary<Edge, ArithExpr[]>(_edgeList.Count);
            foreach (var edge in _edgeList)
            {
                _edgeDictionary[edge] = new ArithExpr[sortedTargetNodes.Count - 1];
            }

            _zero = _c.MkInt(0);
            var one = _c.MkInt(1);
            var two = _c.MkInt(2);

            for (var gi = 0; gi < sortedTargetNodes.Count - 1; gi++)
            {
                foreach (var edge in _edgeList)
                {
                    var edgeConst = _c.MkIntConst("e" + edge.N1 + "-" + edge.N2 + "g" + gi);
                    _edgeDictionary[edge][gi] = edgeConst;
                    _o.Assert(_c.MkAnd(_c.MkGe(edgeConst, _zero), _c.MkLe(edgeConst, one)));
                }
                for (var i = 0; i < totalNodeCount; i++)
                {
                    if (i == sortedTargetNodes[gi] || i == sortedTargetNodes[gi + 1])
                    {
                        _o.Assert(_c.MkEq(one, AddNeighbors(i, gi)));
                    }
                    else if (_adjacencyMatrix[i].Any())
                    {
                        _o.Assert(_c.MkOr(_c.MkEq(_zero, AddNeighbors(i, gi)), _c.MkEq(two, AddNeighbors(i, gi))));
                    }
                }
            }

            foreach (var edge in _edgeList)
            {
                _softConstraintsHandle = _o.AssertSoft(_c.MkEq(_zero, _c.MkAdd(_edgeDictionary[edge])), edge.Weight, "a");
            }
        }

        private List<int> SortNodes(IEnumerable<int> nodes)
        {
            // TODO
            return nodes.ToList();
        }

        private ArithExpr AddNeighbors(int n1, int graph)
        {
            if (!_adjacencyMatrix[n1].Any()) return _zero;
            return _c.MkAdd(_adjacencyMatrix[n1].Select(n2 => _edgeDictionary[new Edge(n1, n2, 0)][graph]).ToArray());
        }

        public IEnumerable<Edge> Run()
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

    [DebuggerDisplay("{N1}-{N2}:{Weight}")]
    public class Edge : IEquatable<Edge>
    {
        public readonly int N1, N2;

        public readonly uint Weight;

        public Edge(int n1, int n2, uint weight)
        {
            N1 = n1;
            N2 = n2;
            Weight = weight;
        }

        public bool Equals(Edge other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return N1 == other.N1 && N2 == other.N2
                || N1 == other.N2 && N2 == other.N1;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Edge) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Math.Min(N1, N2) * 397) ^ Math.Max(N1, N2);
            }
        }
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

        private DistanceLookup _distances;

        public SteinerSmtSolver(SkillTree tree, SolverSettings settings)
        {
            if (tree == null) throw new ArgumentNullException("tree");
            if (settings == null) throw new ArgumentNullException("settings");
            _tree = tree;
            _settings = settings;
        }

        public void Initialize()
        {
            TestCase();

            var auxiliarySolver = new SteinerSolver(_tree, _settings);
            auxiliarySolver.Initialize();
            BestSolution = auxiliarySolver.BestSolution;
            _distances = auxiliarySolver.Distances;
            var nodes = new HashSet<GraphNode>(
                auxiliarySolver.SearchSpace.Concat(auxiliarySolver.TargetNodes)
                    .Concat(new[] { auxiliarySolver.StartNodes }));
            var targets = new HashSet<int>(
                auxiliarySolver.TargetNodes.Concat(new[] { auxiliarySolver.StartNodes }).Select(n => n.DistancesIndex));
            var edges = new List<Edge>();
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
                    if (current.DistancesIndex >= 0 && node != current && path.SetEquals(_distances.GetShortestPath(node, current)))
                    {
                        edges.Add(new Edge(node.DistancesIndex, current.DistancesIndex, (uint)_distances[node, current]));
                    }
                }
            }
            _runner = new SmtRunner(edges, targets, nodes.Count, (n1, n2) => (uint)_distances[n1, n2]);
        }

        private static void TestCase()
        {
            using (var c = new Context())
            {
                var o = c.MkOptimize();
                var e01G0 = c.MkIntConst("e0-1g0");
                var e05G0 = c.MkIntConst("e0-5g0");
                var e02G0 = c.MkIntConst("e0-2g0");
                var e13G0 = c.MkIntConst("e1-3g0");
                var e23G0 = c.MkIntConst("e2-3g0");
                var e34G0 = c.MkIntConst("e3-4g0");
                var e01G1 = c.MkIntConst("e0-1g1");
                var e05G1 = c.MkIntConst("e0-5g1");
                var e02G1 = c.MkIntConst("e0-2g1");
                var e13G1 = c.MkIntConst("e1-3g1");
                var e23G1 = c.MkIntConst("e2-3g1");
                var e34G1 = c.MkIntConst("e3-4g1");
                var zero = c.MkInt(0);
                var assertions = c.ParseSMTLIB2String(@"
(declare-fun e0-1g0 () Int)
(declare-fun e0-5g0 () Int)
(declare-fun e0-2g0 () Int)
(declare-fun e1-3g0 () Int)
(declare-fun e2-3g0 () Int)
(declare-fun e3-4g0 () Int)
(declare-fun e0-1g1 () Int)
(declare-fun e0-5g1 () Int)
(declare-fun e0-2g1 () Int)
(declare-fun e1-3g1 () Int)
(declare-fun e2-3g1 () Int)
(declare-fun e3-4g1 () Int)
(assert (and (>= e0-1g0 0) (<= e0-1g0 1)))
(assert (and (>= e0-5g0 0) (<= e0-5g0 1)))
(assert (and (>= e0-2g0 0) (<= e0-2g0 1)))
(assert (and (>= e1-3g0 0) (<= e1-3g0 1)))
(assert (and (>= e2-3g0 0) (<= e2-3g0 1)))
(assert (and (>= e3-4g0 0) (<= e3-4g0 1)))
(assert (or (= 0 (+ e0-1g0 e0-5g0 e0-2g0)) (= 2 (+ e0-1g0 e0-5g0 e0-2g0))))
(assert (or (= 0 (+ e0-1g0 e1-3g0)) (= 2 (+ e0-1g0 e1-3g0))))
(assert (or (= 0 (+ e0-2g0 e2-3g0)) (= 2 (+ e0-2g0 e2-3g0))))
(assert (= 1 (+ e1-3g0 e2-3g0 e3-4g0)))
(assert (= 1 (+ e3-4g0)))
(assert (or (= 0 (+ e0-5g0)) (= 2 (+ e0-5g0))))
(assert (and (>= e0-1g1 0) (<= e0-1g1 1)))
(assert (and (>= e0-5g1 0) (<= e0-5g1 1)))
(assert (and (>= e0-2g1 0) (<= e0-2g1 1)))
(assert (and (>= e1-3g1 0) (<= e1-3g1 1)))
(assert (and (>= e2-3g1 0) (<= e2-3g1 1)))
(assert (and (>= e3-4g1 0) (<= e3-4g1 1)))
(assert (or (= 0 (+ e0-1g1 e0-5g1 e0-2g1)) (= 2 (+ e0-1g1 e0-5g1 e0-2g1))))
(assert (or (= 0 (+ e0-1g1 e1-3g1)) (= 2 (+ e0-1g1 e1-3g1))))
(assert (or (= 0 (+ e0-2g1 e2-3g1)) (= 2 (+ e0-2g1 e2-3g1))))
(assert (or (= 0 (+ e1-3g1 e2-3g1 e3-4g1)) (= 2 (+ e1-3g1 e2-3g1 e3-4g1))))
(assert (= 1 (+ e3-4g1)))
(assert (= 1 (+ e0-5g1)))
;(assert-soft (= 0 (+ e0-1g0 e0-1g1)) :weight 3 :id a)
;(assert-soft (= 0 (+ e0-5g0 e0-5g1)) :weight 1 :id a)
;(assert-soft (= 0 (+ e0-2g0 e0-2g1)) :weight 1 :id a)
;(assert-soft (= 0 (+ e1-3g0 e1-3g1)) :weight 1 :id a)
;(assert-soft (= 0 (+ e2-3g0 e2-3g1)) :weight 4 :id a)
;(assert-soft (= 0 (+ e3-4g0 e3-4g1)) :weight 1 :id a)
");
                o.Assert(assertions);
                o.AssertSoft(c.MkEq(zero, c.MkAdd(e01G0, e01G1)), 3, "a");
                o.AssertSoft(c.MkEq(zero, c.MkAdd(e05G0, e05G1)), 1, "a");
                o.AssertSoft(c.MkEq(zero, c.MkAdd(e02G0, e02G1)), 1, "a");
                o.AssertSoft(c.MkEq(zero, c.MkAdd(e13G0, e13G1)), 1, "a");
                o.AssertSoft(c.MkEq(zero, c.MkAdd(e23G0, e23G1)), 4, "a");
                var handle = o.AssertSoft(c.MkEq(zero, c.MkAdd(e34G0, e34G1)), 1, "a");
                // TODO model doesn't represent optimization run at all
                o.Check();
                Debug.Print(handle.Value.ToString());
                Debug.Print(o.Model.ToString());
                Debug.Assert(new[] {e13G0, e13G1, e01G0, e01G1}.All(e => ((IntNum) o.Model.ConstInterp(e)).Int > 0));
                Debug.Assert(new[] {e23G0, e23G1, e02G0, e02G1}.All(e => ((IntNum) o.Model.ConstInterp(e)).Int == 0));
            }
        }

        public void Step()
        {
            var edges = _runner.Run();
            var nodes = new HashSet<ushort>();
            foreach (var edge in edges)
            {
                nodes.Add(_distances.IndexToNode(edge.N1).Id);
                nodes.Add(_distances.IndexToNode(edge.N2).Id);
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