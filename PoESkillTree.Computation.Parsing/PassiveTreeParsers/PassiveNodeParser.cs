using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;

namespace PoESkillTree.Computation.Parsing.PassiveTreeParsers
{
    public class PassiveNodeParser : IParser<ushort>
    {
        private readonly PassiveTreeDefinition _passiveTreeDefinition;
        private readonly IBuilderFactories _builderFactories;
        private readonly ICoreParser _coreParser;

        public PassiveNodeParser(
            PassiveTreeDefinition passiveTreeDefinition, IBuilderFactories builderFactories, ICoreParser coreParser)
            => (_passiveTreeDefinition, _builderFactories, _coreParser) =
                (passiveTreeDefinition, builderFactories, coreParser);

        public ParseResult Parse(ushort nodeId)
        {
            var nodeDefinition = _passiveTreeDefinition.GetNodeById(nodeId);
            var localSource = new ModifierSource.Local.Tree(nodeDefinition.Name);
            var globalSource = new ModifierSource.Global();
            var skilledCondition = _builderFactories.StatBuilders.PassiveNodeSkilled(nodeId).IsSet;

            var results = nodeDefinition.Modifiers
                .Select(s => Parse(s, globalSource))
                .Select(r => r.ApplyCondition(skilledCondition.Build))
                .ToList();

            if (nodeDefinition.Type == PassiveNodeType.Keystone)
            {
                var modifiers = new ModifierCollection(_builderFactories, localSource);
                modifiers.AddGlobal(_builderFactories.StatBuilders.KeystoneSkilled(nodeDefinition.Name),
                    Form.TotalOverride, 1, skilledCondition);
                results.Add(ParseResult.Success(modifiers.ToList()));
            }

            return ParseResult.Aggregate(results);
        }

        private ParseResult Parse(string modifierLine, ModifierSource modifierSource)
            => _coreParser.Parse(modifierLine, modifierSource, Entity.Character);
    }
}