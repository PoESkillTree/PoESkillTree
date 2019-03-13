using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing
{
    /// <summary>
    /// Interface for the parser that consolidates all <see cref="IParser{TParameter}"/> implementations.
    /// This is the main API of Computation.Parsing.
    /// </summary>
    public interface IParser
    {
        ParseResult ParseRawModifier(string modifierLine, ModifierSource modifierSource, Entity modifierSourceEntity);

        ParseResult ParsePassiveNode(ushort nodeId);
        ParseResult ParseSkilledPassiveNode(ushort nodeId);

        ParseResult ParseItem(Item item, ItemSlot itemSlot);

        ParseResult ParseJewelSocketedInItem(Item item, ItemSlot itemSlot);
        
        ParseResult ParseSkills(IReadOnlyList<Skill> skills);
        ParseResult ParseActiveSkill(Skill activeSkill);
        ParseResult ParseSupportSkill(Skill activeSkill, Skill supportSkill);

        IReadOnlyList<Modifier> ParseGivenModifiers();
        // This method looks weird, but the delegates are necessary for caller-defined concurrency
        IEnumerable<Func<IReadOnlyList<Modifier>>> CreateGivenModifierParseDelegates();
    }

    /// <summary>
    /// Generic interface for parsers that use <see cref="ParseResult"/> as result type.
    /// </summary>
    /// <typeparam name="TParameter">Parsing parameter type</typeparam>
    public interface IParser<in TParameter>
    {
        /// <summary>
        /// Parses the given parameter into <see cref="ParseResult"/>.
        /// </summary>
        ParseResult Parse(TParameter parameter);
    }

    public static class ParserExtensions
    {
        public static void Initialize(this IParser @this)
            => @this.ParseRawModifier("", new ModifierSource.Global(), default);
    }
}