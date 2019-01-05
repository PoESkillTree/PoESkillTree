using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using MoreLinq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel;
using PoESkillTree.Utils.Extensions;

namespace POESKillTree.Computation.Model
{
    public class InitialComputation
    {
        private readonly GameData _gameData;
        private readonly IParser _parser;

        public InitialComputation(GameData gameData, IParser parser)
            => (_gameData, _parser) = (gameData, parser);

        public IObservable<CalculatorUpdate> InitialParse()
        {
            var givenResultObservable = _parser.CreateGivenModifierParseDelegates().ToObservable()
                .Select(d => d());
            var passiveNodesObservable = _gameData.PassiveTree.ToObservable()
                .SelectMany(t => t.Nodes)
                .Select(n => _parser.ParsePassiveNode(n.Id).Modifiers);
            return givenResultObservable.Merge(passiveNodesObservable)
                .Buffer(TimeSpan.FromMilliseconds(100))
                .Select(ms => ms.Flatten().ToList())
                .Where(ms => ms.Any())
                .Select(ms => new CalculatorUpdate(ms, new Modifier[0]));
        }

        public IObservable<CalculatorUpdate> ParseSkilledPassiveNodes(IEnumerable<ushort> skilledNodes)
            => skilledNodes.ToObservable()
                .SelectMany(id => _parser.ParseSkilledPassiveNode(id).Modifiers)
                .Aggregate(Enumerable.Empty<Modifier>(), (ms, m) => ms.Append(m))
                .Select(ms => new CalculatorUpdate(ms.ToList(), new Modifier[0]));
    }
}