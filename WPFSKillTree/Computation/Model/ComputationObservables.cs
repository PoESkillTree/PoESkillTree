using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel;
using PoESkillTree.Utils.Extensions;
using POESKillTree.SkillTreeFiles;
using POESKillTree.Utils;

namespace POESKillTree.Computation.Model
{
    public class ComputationObservables
    {
        private readonly GameData _gameData;
        private readonly IParser _parser;

        public ComputationObservables(GameData gameData, IParser parser)
            => (_gameData, _parser) = (gameData, parser);

        public IObservable<CalculatorUpdate> InitialParse()
        {
            var givenResultObservable = _parser.CreateGivenModifierParseDelegates().ToObservable()
                .Select(d => d());
            var passiveNodesObservable = _gameData.PassiveTree.ToObservable()
                .SelectMany(t => t.Nodes)
                .Select(n => _parser.ParsePassiveNode(n.Id).Modifiers);
            return givenResultObservable.Merge(passiveNodesObservable)
                .Buffer(TimeSpan.FromMilliseconds(200))
                .Select(ms => ms.Flatten().ToList())
                .Where(ms => ms.Any())
                .Select(ms => new CalculatorUpdate(ms, new Modifier[0]));
        }

        public IObservable<CalculatorUpdate> ParseSkilledPassiveNodes(IEnumerable<SkillNode> skilledNodes)
            => skilledNodes.Select(n => n.Id).ToObservable()
                .SelectMany(ParseSkilledNode)
                .Aggregate(Enumerable.Empty<Modifier>(), (ms, m) => ms.Append(m))
                .Select(ms => new CalculatorUpdate(ms.ToList(), new Modifier[0]));

        public IObservable<CalculatorUpdate> ObserveSkilledPassiveNodes(ObservableSet<SkillNode> skilledNodes)
        {
            return Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => skilledNodes.CollectionChanged += h,
                    h => skilledNodes.CollectionChanged -= h)
                .Select(p =>
                {
                    if (p.EventArgs.Action == NotifyCollectionChangedAction.Reset)
                        throw new NotSupportedException("Reset action is not supported");
                    return p.EventArgs;
                })
                .Select(args => new CalculatorUpdate(Parse(args.NewItems), Parse(args.OldItems)));

            IReadOnlyList<Modifier> Parse(IList changedItems)
                => SelectNodeIds(changedItems).SelectMany(ParseSkilledNode).ToList();

            IEnumerable<ushort> SelectNodeIds(IList changedItems)
                => changedItems?.Cast<SkillNode>().Select(n => n.Id) ?? new ushort[0];
        }

        private IReadOnlyList<Modifier> ParseSkilledNode(ushort nodeId)
            => _parser.ParseSkilledPassiveNode(nodeId).Modifiers;
    }
}