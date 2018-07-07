using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Actions;
using PoESkillTree.Computation.Builders.Damage;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Tests.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;

namespace PoESkillTree.Computation.Builders.Tests.Actions
{
    [TestFixture]
    public class ActionBuildersTest
    {
        [Test]
        public void HitWithOnBuildsToCorrectResult()
        {
            var damageTypeBuilder = new DamageTypeBuilder(StatFactory, DamageType.Fire);
            var sut = CreateSut();

            var result = sut.HitWith(damageTypeBuilder).On.Build();
            var stat = result.StatConverter(InputStat).BuildToSingleStat();

            Assert.AreEqual("stat.On(FireHit).By(Character)", stat.Identity);
        }

        [Test]
        public void HitWithMultipleOnThrows()
        {
            var damageTypes = new[] { DamageType.Chaos, DamageType.Fire };
            var damageTypeBuilder = Mock.Of<IDamageTypeBuilder>(b => b.BuildDamageTypes() == damageTypes);
            var sut = CreateSut();

            var result = sut.HitWith(damageTypeBuilder).On.Build();
            var statBuilder = result.StatConverter(InputStat);

            Assert.Throws<ParseException>(() => statBuilder.Build(default, null).Consume());
        }

        [Test]
        public void SpendManaOnBuildsToCorrectResult()
        {
            var valueBuilder = new ValueBuilderImpl(42);
            var sut = CreateSut();

            var result = sut.SpendMana(valueBuilder).On.Build();
            var stat = result.StatConverter(InputStat).BuildToSingleStat();

            Assert.AreEqual("stat.On(Spend42Mana).By(Character)", stat.Identity);
        }

        [Test]
        public void SpendManaOnBuildThrowsWithNonConstantValue()
        {
            var stat = new Stat("");
            var valueBuilder = new ValueBuilderImpl(new StatValue(stat));
            var sut = CreateSut();

            var result = sut.SpendMana(valueBuilder).On.Build();
            var statBuilder = result.StatConverter(InputStat);

            Assert.Throws<ParseException>(() => statBuilder.Build(default, null).Consume());
        }

        private static ActionBuilders CreateSut() =>
            new ActionBuilders(StatFactory);

        private static readonly IStatBuilder InputStat = StatBuilderUtils.FromIdentity(new StatFactory(), "stat", null);
        private static readonly IStatFactory StatFactory = new StatFactory();
    }
}