using System.Collections.Generic;
using System.Linq;
using POESKillTree.SkillTreeFiles;
using POESKillTree.SkillTreeFiles.SteinerTrees;
using POESKillTree.TreeGenerator.Settings;

namespace POESKillTree.TreeGenerator.Solver
{
    public class SteinerSolver : AbstractSolver<SolverSettings>
    {
        private const double GenMultiplier = 0.3;

        private const double PopMultiplier = 1.5;

        private const int ConstRuntimeEndpoint = 150;

        private int _maxEdgeDistance;

        protected override GeneticAlgorithmParameters GaParameters
        {
            // up to ConstRuntimeEndpoint: maxGeneration * populationSize = GenMultiplier * PopMultiplier * ConstRuntimeEndpoint^2
            // after that: maxGeneration * populationSize = GenMultiplier * PopMultiplier * SearchSpace.Count^2
            get
            {
                return new GeneticAlgorithmParameters(
                    SearchSpace.Count == 0 ? 0
                        : (int)(GenMultiplier * (SearchSpace.Count < ConstRuntimeEndpoint ? (ConstRuntimeEndpoint*ConstRuntimeEndpoint) / SearchSpace.Count : SearchSpace.Count)),
                    (int)(PopMultiplier * SearchSpace.Count),
                    SearchSpace.Count, 6, 1);
            }
        }

        protected override bool FinalHillClimbEnabled
        {
            get { return false; }
        }

        public SteinerSolver(SkillTree tree, SolverSettings settings)
            : base(tree, settings)
        {
        }

        protected override bool IncludeNode(GraphNode node)
        {
            // Steiner nodes need to have at least 2 neighbors.
            return node.Adjacent.Count > 2 && node != StartNodes && !TargetNodes.Contains(node);
        }

        protected override void OnLeastSolutionCreated(MinimalSpanningTree leastSolution)
        {
            _maxEdgeDistance = TargetNodes.Count == 0
                ? -1
                : leastSolution.SpanningEdges.Max(edge => Distances[edge.Inside, edge.Outside]);
        }

        protected override bool IncludeNodeUsingDistances(GraphNode node)
        {
            // Find potential steiner points that are in reasonable vicinity.
            return _maxEdgeDistance >= 0 && TargetNodes.Any(targetNode => Distances[targetNode, node] < _maxEdgeDistance);
        }

        protected override double FitnessFunction(HashSet<ushort> skilledNodes)
        {
            return 1500 - skilledNodes.Count;
        }
    }
}