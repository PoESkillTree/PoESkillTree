using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Model;

namespace PoESkillTree.Computation.ViewModels
{
    public class ModifierNodeViewModelFactory
    {
        private readonly ObservableCalculator _calculator;
        private readonly CalculationNodeViewModelFactory _nodeFactory;

        public ModifierNodeViewModelFactory(ObservableCalculator calculator,
            CalculationNodeViewModelFactory nodeFactory)
        {
            _calculator = calculator;
            _nodeFactory = nodeFactory;
        }

        public async Task<IReadOnlyList<ModifierNodeViewModel>> CreateAsync(IStat stat, NodeType nodeType)
        {
            var totalOverrideNodes = await CreateTotalOverrideAsync(stat, nodeType);
            if (totalOverrideNodes.Any())
                return totalOverrideNodes;

            var nodes = new List<ModifierNodeViewModel>();
            nodes.AddRange(await CreateBaseAsync(stat, nodeType));
            nodes.AddRange(await CreateIncreaseMoreAsync(stat, nodeType));

            return nodes.OrderBy(n => n.Form).ThenBy(n => n.ModifierSource.ToString()).ToList();
        }

        private async Task<IReadOnlyList<ModifierNodeViewModel>> CreateTotalOverrideAsync(IStat stat, NodeType nodeType)
        {
            if (nodeType != NodeType.Total && nodeType != NodeType.TotalOverride)
                return new ModifierNodeViewModel[0];

            var totalOverrideValue = await _calculator.GetNodeValueAsync(stat, NodeType.TotalOverride);
            if (totalOverrideValue.HasValue)
                return await CreateFormNodesAsync(stat, Form.TotalOverride, PathDefinition.MainPath);
            return new ModifierNodeViewModel[0];
        }

        private async Task<IReadOnlyList<ModifierNodeViewModel>> CreateBaseAsync(IStat stat, NodeType nodeType)
        {
            if (nodeType == NodeType.Increase || nodeType == NodeType.More)
                return new ModifierNodeViewModel[0];

            var nodes = new List<ModifierNodeViewModel>();
            var paths = (await _calculator.GetPathsAsync(stat)).ToList();
            if (nodeType != NodeType.BaseSet)
            {
                nodes.AddRange(await CreateFormNodesAsync(stat, Form.BaseAdd, paths));
            }
            if (nodeType != NodeType.BaseAdd)
            {
                nodes.AddRange(await CreateFormNodesAsync(stat, Form.BaseSet, paths));
            }
            return nodes;
        }

        private async Task<IReadOnlyList<ModifierNodeViewModel>> CreateIncreaseMoreAsync(IStat stat, NodeType nodeType)
        {
            if (nodeType == NodeType.Base || nodeType == NodeType.BaseSet || nodeType == NodeType.BaseAdd)
                return new ModifierNodeViewModel[0];

            var allPaths = (await _calculator.GetPathsAsync(stat)).ToList();            
            var consideredPaths = new HashSet<PathDefinition>();
            foreach (var path in allPaths)
            {
                if (nodeType != NodeType.Increase && nodeType != NodeType.More)
                {
                    var baseValue = await _calculator.GetNodeValueAsync(stat, NodeType.Base, path);
                    if (baseValue is null)
                        continue;
                }

                foreach (var influencingSource in path.ModifierSource.InfluencingSources)
                {
                    consideredPaths.Add(new PathDefinition(influencingSource));
                }
            }

            var nodes = new List<ModifierNodeViewModel>();
            if (nodeType != NodeType.Increase)
            {
                nodes.AddRange(await CreateFormNodesAsync(stat, Form.More, consideredPaths));
            }
            if (nodeType != NodeType.More)
            {
                nodes.AddRange(await CreateFormNodesAsync(stat, Form.Increase, consideredPaths));
            }
            return nodes;
        }

        private async Task<IReadOnlyList<ModifierNodeViewModel>> CreateFormNodesAsync(
            IStat stat, Form form, IEnumerable<PathDefinition> paths)
        {
            var nodes = new List<ModifierNodeViewModel>();
            foreach (var path in paths)
            {
                nodes.AddRange(await CreateFormNodesAsync(stat, form, path));
            }
            return nodes;
        }

        private async Task<IReadOnlyList<ModifierNodeViewModel>> CreateFormNodesAsync(
            IStat stat, Form form, PathDefinition path)
        {
            var nodes = new List<ModifierNodeViewModel>();
            var formNodes = await _calculator.GetFormNodeCollectionAsync(stat, form, path);
            foreach (var (node, modifier) in formNodes)
            {
                var resultNode = await _nodeFactory.CreateConstantResultAsync(stat, node);
                if (resultNode.Value is null && stat.DataType != typeof(bool))
                    continue;
                nodes.Add(new ModifierNodeViewModel(form, modifier.Source, resultNode));
            }

            if (form == Form.BaseAdd || form == Form.Increase)
            {
                return nodes
                    .GroupBy(n => (n.Form, n.ModifierSource, n.ModifierSource.SourceName))
                    .Select(g => g.Aggregate(Accumulate))
                    .ToList();
            }
            return nodes;

            ModifierNodeViewModel Accumulate(ModifierNodeViewModel l, ModifierNodeViewModel r)
            {
                l.Node.Value = l.Node.Value.SumWhereNotNull(r.Node.Value);
                return l;
            }
        }
    }
}