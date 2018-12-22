using System.Linq;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    public class SupportSkillGeneralParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly IMetaStatBuilders _metaStatBuilders;

        private ModifierCollection _parsedModifiers;
        private SkillPreParseResult _preParseResult;

        public SupportSkillGeneralParser(IBuilderFactories builderFactories, IMetaStatBuilders metaStatBuilders)
            => (_builderFactories, _metaStatBuilders) = (builderFactories, metaStatBuilders);

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            _parsedModifiers = new ModifierCollection(_builderFactories, preParseResult.LocalSource);
            _preParseResult = preParseResult;
            var isActiveSkill = preParseResult.IsActiveSkill;

            _parsedModifiers.AddGlobal(_metaStatBuilders.ActiveSkillItemSlot(parsedSkill.Id),
                Form.BaseSet, (double) parsedSkill.ItemSlot, isActiveSkill);
            _parsedModifiers.AddGlobal(_metaStatBuilders.ActiveSkillSocketIndex(parsedSkill.Id),
                Form.BaseSet, parsedSkill.SocketIndex, isActiveSkill);
            AddInstanceModifiers();

            var result = new PartialSkillParseResult(_parsedModifiers, new UntranslatedStat[0]);
            _parsedModifiers = null;
            return result;
        }

        private void AddInstanceModifiers()
        {
            var addedKeywords = _preParseResult.SkillDefinition.SupportSkill.AddedKeywords
                .Where(k => !_preParseResult.MainSkillDefinition.ActiveSkill.Keywords.Contains(k));
            foreach (var keyword in addedKeywords)
            {
                var keywordBuilder = _builderFactories.KeywordBuilders.From(keyword);
                _parsedModifiers.AddGlobal(_builderFactories.SkillBuilders[keywordBuilder].CombinedInstances,
                    Form.BaseAdd, 1, _preParseResult.IsActiveSkill);
            }
        }
    }
}