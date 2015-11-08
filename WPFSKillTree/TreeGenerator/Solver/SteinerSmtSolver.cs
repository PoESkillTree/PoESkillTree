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

        private readonly IReadOnlyGraphEdgeSet _edges;

        private readonly IntNum _zero;

        private readonly Optimize.Handle _softConstraintsHandle;

        public SmtRunner(IReadOnlyGraphEdgeSet edges, IEnumerable<int> targetNodes, int totalNodeCount)
        {
            _o = _c.MkOptimize();
            
            var targetList = targetNodes.ToList();
            _edges = edges;
            _edgeDictionary = new Dictionary<GraphEdge, ArithExpr[]>(_edges.Count);
            foreach (var edge in edges)
            {
                _edgeDictionary[edge] = new ArithExpr[targetList.Count - 1];
            }

            _zero = _c.MkInt(0);
            var one = _c.MkInt(1);
            var two = _c.MkInt(2);

            for (var gi = 0; gi < targetList.Count - 1; gi++)
            {
                foreach (var edge in _edges)
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
                    else if (_edges.HasNeighbors(i))
                    {
                        _o.Assert(_c.MkOr(_c.MkEq(_zero, AddNeighbors(i, gi)), _c.MkEq(two, AddNeighbors(i, gi))));
                    }
                }
            }

            _softConstraintsHandle = _o.MkMinimize(_c.MkAdd(_edges
                .Select(edge => MkEdgeWeightExpr(_edgeDictionary[edge], edge.Weight)).ToArray()));
        }

        private ArithExpr AddNeighbors(int n1, int graph)
        {
            return !_edges.HasNeighbors(n1)
                ? _zero
                : _c.MkAdd(_edges.NeighborEdges(n1).Select(edge => _edgeDictionary[edge][graph]).ToArray());
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
                    return _edges.Where(e => _edgeDictionary[e].Any(expr => ((IntNum)model.ConstInterp(expr)).Int > 0));
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

    public class SteinerSmtSolver : AbstractSolver<SolverSettings>
    {
        
        public override int MaxSteps
        {
            get { return 1; }
        }

        public override int CurrentStep { get { return _currentStep; } }
        private int _currentStep;

        private SmtRunner _runner;

        public SteinerSmtSolver(SkillTree tree, SolverSettings settings)
            : base(tree, settings)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            if (SearchSpaceEdgeSet.Count > 0)
            {
                _runner = new SmtRunner(SearchSpaceEdgeSet, TargetNodes.Select(n => n.DistancesIndex), Distances.CacheSize);
            }
        }

        public override void Step()
        {
            if (SearchSpaceEdgeSet.Count == 0)
            {
                BestSolution = TargetNodes.SelectMany(n => n.Nodes);
                _currentStep++;
                return;
            }

            var edges = _runner.Run();
            var nodes = new HashSet<ushort>();
            foreach (var edge in edges)
            {
                nodes.UnionWith(Distances.IndexToNode(edge.N1).Nodes);
                nodes.UnionWith(Distances.IndexToNode(edge.N2).Nodes);
                nodes.UnionWith(Distances.GetShortestPath(edge.N1, edge.N2));
            }
            BestSolution = nodes;
            _runner.Dispose();
            _currentStep++;
        }

        public override void FinalStep()
        {
        }
    }
}