using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.GameModel.PassiveTree;
using static PoESkillTree.Computation.Common.Helper;

namespace PoESkillTree.Computation.Parsing.JewelParsers
{
    [TestFixture]
    public class TransformationJewelParserTest
    {
        [Test]
        public void ColdSteelIsTransformationJewel()
        {
            var sut = CreateSut();

            var actual = sut.IsTransformationJewelModifier(ColdSteelModifier);

            actual.Should().BeTrue();
        }

        [Test]
        public void AddedDexterityIsNoTransformationJewelModifier()
        {
            var sut = CreateSut();

            var actual = sut.IsTransformationJewelModifier("+1 to Dexterity");

            actual.Should().BeFalse();
        }

        [Test]
        public void ReturnsEmptyEnumerableGivenJewelIsNoTransformationJewel()
        {
            var nodesInRadius = new[] { CreateNode(0, "+1 to Dexterity") };
            var sut = CreateSut();

            var actual = sut.ApplyTransformation("+1 to Strength", nodesInRadius);

            actual.Should().BeEmpty();
        }

        [Test]
        public void TransformsModifiersGivenColdSteel()
        {
            var expected = new[]
            {
                "1% increased physical weapon damage",
                "1% increased Cold weapon damage",
                "3% reduced physical damage",
                "3% reduced Cold damage",
            };
            var nodesInRadius = new[]
            {
                CreateNode(0, "1% increased physical weapon damage"),
                CreateNode(1, "2% more cold damage"),
                CreateNode(2, "3% reduced physical damage"),
                CreateNode(3, "4% increased fire damage"),
            };
            var sut = CreateSut();

            var actual = sut.ApplyTransformation(ColdSteelModifier, nodesInRadius);

            actual.Should().Equal(expected, (a, e) => a.Modifier == e);
        }

        [Test]
        public void ReturnsNodeSkilledConditionsGivenColdSteel()
        {
            var nodeConditions = MockMany<IConditionBuilder>(2);
            var expected = nodeConditions.SelectMany(b => Enumerable.Repeat(b, 2));
            var nodesInRadius = new[]
            {
                CreateNode(0, "1% increased physical weapon damage"),
                CreateNode(1, "3% reduced physical damage"),
            };
            var sut = CreateSut(i => nodeConditions[i]);

            var actual = sut.ApplyTransformation(ColdSteelModifier, nodesInRadius);

            actual.Should().Equal(expected, (a, e) => a.Condition == e);
        }

        [Test]
        public void ReturnsNegativeAndSameValueMultipliersGivenColdSteel()
        {
            var expected = new[]
            {
                new Constant(-1),
                new Constant(1),
            };
            var nodesInRadius = new[]
            {
                CreateNode(0, "1% increased physical weapon damage"),
            };
            var sut = CreateSut();

            var actual = sut.ApplyTransformation(ColdSteelModifier, nodesInRadius);

            actual.Should().Equal(expected, (a, e) => a.ValueMultiplier.Equals(e));
        }

        [Test]
        public void NormalizesWhitespace()
        {
            var expected = new[]
            {
                "1% increased physical weapon damage",
                "1% increased Cold weapon damage",
            };
            var jewelModifier =
                "Increases and Reductions\nto Physical Damage in Radius are Transformed to apply to Cold Damage";
            var nodesInRadius = new[]
            {
                CreateNode(0, "1% increased\tphysical weapon damage"),
            };
            var sut = CreateSut();

            var actual = sut.ApplyTransformation(jewelModifier, nodesInRadius);

            actual.Should().Equal(expected, (a, e) => a.Modifier == e);
        }

        [Test]
        public void ReturnsNegativeAndMultipliedValueMultipliersGivenEnergisedArmour()
        {
            var expected = new[]
            {
                new Constant(-1),
                new Constant(2),
            };
            var nodesInRadius = new[]
            {
                CreateNode(0, "1% increased energy shield"),
            };
            var data = new TransformationJewelParserData.GenericTransformation();
            var sut = CreateSut(data: data);

            var actual = sut.ApplyTransformation(EnergisedArmourModifier, nodesInRadius);

            actual.Should().Equal(expected, (a, e) => a.ValueMultiplier.Equals(e));
        }

        [Test]
        public void DoesNotCancelOutOriginalModifiersGivenTheBlueDream()
        {
            var expected = new[]
            {
                "+1% chance to gain a Power Charge on Kill",
            };
            var nodesInRadius = new[]
            {
                CreateNode(0, "+1% to lightning resistance"),
            };
            var data = new TransformationJewelParserData.DreamTransformation();
            var sut = CreateSut(data: data);

            var actual = sut.ApplyTransformation(TheBlueDreamModifier, nodesInRadius);

            actual.Should().Equal(expected, (a, e) => a.Modifier == e);
        }

        [Test]
        public void OnlyUsesFirstNodeModifierMatchGivenLioneyesFall()
        {
            var expected = new[]
            {
                "1% increased damage with one handed melee weapons",
                "1% increased damage with bows",
            };
            var nodesInRadius = new[]
            {
                CreateNode(0, "1% increased damage with one handed melee weapons"),
            };
            var data = new TransformationJewelParserData.LioneyesFallTransformation();
            var sut = CreateSut(data: data);

            var actual = sut.ApplyTransformation(LioneyesFallModifier, nodesInRadius);

            actual.Should().Equal(expected, (a, e) => a.Modifier == e);
        }

        private const string ColdSteelModifier =
            "Increases and Reductions to Physical Damage in Radius are Transformed to apply to Cold Damage";

        private const string EnergisedArmourModifier =
            "Increases and Reductions to Energy Shield in Radius are Transformed to apply to Armour at 200% of their value";

        private const string TheBlueDreamModifier =
            "Passives granting Lightning Resistance or all Elemental Resistances in Radius also grant an equal chance to gain a Power Charge on Kill";

        private const string LioneyesFallModifier =
            "Melee and Melee Weapon Type modifiers in Radius are Transformed to Bow Modifiers";

        private static TransformationJewelParser CreateSut(
            Func<ushort, IConditionBuilder> createIsSkilledConditionForNode = null,
            TransformationJewelParserData data = null)
            => new TransformationJewelParser(
                createIsSkilledConditionForNode ?? (_ => Mock.Of<IConditionBuilder>()),
                data ?? new TransformationJewelParserData.SingleDamageTypeTransformation());

        private static PassiveNodeDefinition CreateNode(ushort id, params string[] modifiers)
            => new PassiveNodeDefinition(id, default, default, default,
                default, default, default, modifiers);
    }
}